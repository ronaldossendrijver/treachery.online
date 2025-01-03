/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;

namespace Treachery.Shared;

public partial class Game
{
    #region State

    internal int ResourcesCollectedByYellow { get; set; }
    internal int ResourcesCollectedByBlackFromDesertOrHomeworld { get; set; }
    public IList<DiscoveryToken> PendingDiscoveries { get; set; }
    public List<ResourcesToBeDivided> CollectedResourcesToBeDivided { get; } = new();
    public DivideResources CurrentDivisionProposal { get; internal set; }
    public Faction OwnerOfFlightDiscovery { get; internal set; }
    public List<DiscoveredLocation> JustRevealedDiscoveryStrongholds { get; } = new();

    #endregion State

    #region Collection

    internal void StartCollection()
    {
        CollectResourcesFromTerritories();
        CollectResourcesFromStrongholdsAndHomeworlds();
        Enter(CollectedResourcesToBeDivided.Any(), Phase.DividingCollectedResources, EndCollectionMainPhase);
    }

    internal void DivideResourcesFromCollection(bool divisionWasAgreed)
    {
        var toBeDivided = DivideResources.GetResourcesToBeDivided(this);

        var gainedByFirstFaction = DivideResources.GainedByFirstFaction(toBeDivided, divisionWasAgreed, CurrentDivisionProposal.PortionToFirstPlayer);
        var gainedByOtherFaction = DivideResources.GainedByOtherFaction(toBeDivided, divisionWasAgreed, CurrentDivisionProposal.PortionToFirstPlayer);

        GainCollectedResources(toBeDivided.FirstFaction, toBeDivided.Territory, gainedByFirstFaction);
        GainCollectedResources(toBeDivided.OtherFaction, toBeDivided.Territory, gainedByOtherFaction);

        CollectedResourcesToBeDivided.Remove(toBeDivided);

        CurrentDivisionProposal = null;
    }

    private void GainCollectedResources(Faction faction, Territory from, int amount)
    {
        Log(faction, " collect ", Payment.Of(amount), " from ", from);
        GetPlayer(faction).Resources += amount;
        if (faction == Faction.Yellow) ResourcesCollectedByYellow += amount;
        if (faction == Faction.Black && !from.IsStronghold && !from.IsProtectedFromWorm) ResourcesCollectedByBlackFromDesertOrHomeworld += amount;
    }

    internal void ModifyIncomeBecauseOfLowThresholdOrOccupation(Player from, ref int receivedAmount)
    {
        if (receivedAmount > 1 && Applicable(Rule.Homeworlds))
        {
            var homeworld = Version >= 166 ? from.PrimaryHomeworld : from.HomeWorlds.First();
            var occupier = OccupierOf(homeworld.World);

            bool halfIsLost = false;
            if (Version >= 169)
            {
                halfIsLost = (occupier != null || from.Is(Faction.Red) || from.Is(Faction.Orange)) &&
                             from.HasLowThreshold(homeworld.World);
            }
            else
            {
                halfIsLost = (Version) switch
                {
                    >= 166 when (occupier != null || from.Is(Faction.Red) || from.Is(Faction.Orange)) &&
                                from.HasLowThreshold(homeworld.World) => true,
                    >= 164 when (occupier != null || from.Is(Faction.Red) || from.Is(Faction.Orange)) &&
                                from.HasLowThreshold() => true,
                    <  164 when (!from.Is(Faction.White) || occupier != null) && from.HasLowThreshold() => true,
                    _ => false
                };
            }

            var amountLost = halfIsLost ? (int)(0.5f * receivedAmount) : 0;

            receivedAmount -= amountLost;

            if (occupier != null)
            {
                occupier.Resources += amountLost;
                Log(Payment.Of(amountLost), " received by ", from.Faction, " goes to ", occupier.Faction, " due to occupation");
            }
        }
    }

    private void CollectResourcesFromStrongholdsAndHomeworlds()
    {
        if (Applicable(Rule.IncreasedResourceFlow) || Applicable(Rule.ResourceBonusForStrongholds))
            foreach (var player in Players)
            {
                CollectResourcesFromStronghold(Map.Arrakeen, player, 2);
                CollectResourcesFromStronghold(Map.Carthag, player, 2);
                CollectResourcesFromStronghold(Map.TueksSietch, player, 1);
                CollectResourcesFromStronghold(Map.Cistern, player, 2);
            }

        foreach (var homeworld in Map.Homeworlds.Where(hw => hw.ResourceAmount != 0))
        {
            var occupier = OccupierOf(homeworld.World);
            if (occupier != null)
            {
                GainCollectedResources(occupier.Faction, homeworld.Territory, homeworld.ResourceAmount);
                occupier.TransferableResources += homeworld.ResourceAmount;
            }
        }
    }

    private void CollectResourcesFromStronghold(Location stronghold, Player player, int amount)
    {
        if (player.Controls(this, stronghold, Applicable(Rule.ContestedStongholdsCountAsOccupied)) &&
            !(player.Is(Faction.Pink) && Prevented(FactionAdvantage.PinkCollection) && player.HasAlly && !player.AlliedPlayer.Controls(this, stronghold, Applicable(Rule.ContestedStongholdsCountAsOccupied))))
            GainCollectedResources(player.Faction, stronghold.Territory, amount);
    }

    private void CollectResourcesFromTerritories()
    {
        var thief = Players.FirstOrDefault(p => p.Occupies(Map.ProcessingStation));

        foreach (var l in ResourcesOnPlanet.Where(x => x.Value > 0).ToList())
        {
            if (Version >= 159) thief = Players.FirstOrDefault(p => p.Occupies(Map.ProcessingStation));

            var playersToCollect = Players.Where(y => y.Occupies(l.Key)).ToArray();
            var totalCollectedAmount = 0;
            var spiceLeft = l.Value;

            foreach (var p in playersToCollect)
            {
                var collectionRate = ResourceCollectionRate(p);
                var forcesCollectingDefaultAmountOfSpice = p.Faction != Faction.Grey ? p.OccupyingForcesIn(l.Key) : p.ForcesIn(l.Key);
                var forcesCollecting3Spice = p.Is(Faction.Grey) ? p.SpecialForcesIn(l.Key) : 0;
                var maximumSpiceThatCanBeCollected = forcesCollectingDefaultAmountOfSpice * collectionRate + forcesCollecting3Spice * 3;
                var collectedAmountByThisPlayer = Math.Min(spiceLeft, maximumSpiceThatCanBeCollected);
                ChangeResourcesOnPlanet(l.Key, -collectedAmountByThisPlayer);
                spiceLeft -= collectedAmountByThisPlayer;

                if (collectedAmountByThisPlayer > 0 && thief != null && (Version < 159 || p != thief))
                {
                    collectedAmountByThisPlayer -= 1;
                    thief.Resources += 1;
                    Log(thief.Faction, " steal ", Payment.Of(1), " from the ", Concept.Resource, " collected by ", p.Faction, " from ", l.Key.Territory);
                    thief = null;
                }

                if (playersToCollect.Length == 1)
                    GainCollectedResources(p.Faction, l.Key.Territory, collectedAmountByThisPlayer);
                else
                    totalCollectedAmount += collectedAmountByThisPlayer;

                if (spiceLeft <= 0) break;
            }

            if (playersToCollect.Length > 1)
            {
                var toBeDivided = new ResourcesToBeDivided();

                if (playersToCollect.Any(p => p.Is(Faction.Pink)))
                {
                    toBeDivided.FirstFaction = Faction.Pink;
                    toBeDivided.OtherFaction = playersToCollect.First(p => !p.Is(Faction.Pink)).Faction;
                }
                else
                {
                    toBeDivided.FirstFaction = playersToCollect[0].Faction;
                    toBeDivided.OtherFaction = playersToCollect[1].Faction;
                }

                toBeDivided.Amount = totalCollectedAmount;
                toBeDivided.Territory = l.Key.Territory;
                CollectedResourcesToBeDivided.Add(toBeDivided);
                Log(toBeDivided.FirstFaction, " and ", toBeDivided.OtherFaction, " will have to share ", Payment.Of(totalCollectedAmount), " collected from ", l.Key.Territory);
            }
        }
    }

    internal void EndCollectionMainPhase()
    {
        var yellow = GetPlayer(Faction.Yellow);
        if (yellow != null)
        {
            var receivedAmountByYellowAfterTakingIntoAccountOccupation = ResourcesCollectedByYellow;
            ModifyIncomeBecauseOfLowThresholdOrOccupation(yellow, ref receivedAmountByYellowAfterTakingIntoAccountOccupation);

            if (Version >= 162) yellow.Resources -= ResourcesCollectedByYellow - receivedAmountByYellowAfterTakingIntoAccountOccupation;
        }

        var black = GetPlayer(Faction.Black);
        if (ResourcesCollectedByBlackFromDesertOrHomeworld != 0 && black.HasHighThreshold())
        {
            black.Resources += 2;
            Log(Faction.Black, " get ", Payment.Of(2), " extra as they collected from desert or homeworld");
        }

        MainPhaseEnd();
        Enter(Version >= 103, Phase.CollectionReport, EnterContemplatePhase);
    }


    #endregion Collection

    #region Information

    public int ResourceCollectionRate(Player p)
    {
        return p.Controls(this, Map.Arrakeen, Applicable(Rule.ContestedStongholdsCountAsOccupied)) || p.Controls(this, Map.Carthag, Applicable(Rule.ContestedStongholdsCountAsOccupied)) ? 3 : 2;
    }

    #endregion Information
}