/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
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

        #region Construction

        internal void StartCollection()
        {
            CollectResourcesFromTerritories();
            CollectResourcesFromStrongholdsAndHomeworlds();
            Enter(CollectedResourcesToBeDivided.Any(), Phase.DividingCollectedResources, EndCollectionMainPhase);
        }

        internal void DivideResourcesFromCollection(bool divisionWasAgreed)
        {
            var toBeDivided = DivideResources.GetResourcesToBeDivided(this);

            int gainedByFirstFaction = DivideResources.GainedByFirstFaction(toBeDivided, divisionWasAgreed, CurrentDivisionProposal.PortionToFirstPlayer);
            int gainedByOtherFaction = DivideResources.GainedByOtherFaction(toBeDivided, divisionWasAgreed, CurrentDivisionProposal.PortionToFirstPlayer);

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

        internal void ModifyIncomeBasedOnThresholdOrOccupation(Player from, ref int receivedAmount)
        {
            if (receivedAmount > 1 && Applicable(Rule.Homeworlds))
            {
                int amountToOccupier = from.HasLowThreshold() ? (int)(0.5f * receivedAmount) : 0;
                receivedAmount -= amountToOccupier;

                var homeworld = from.Homeworlds.First();
                var occupier = OccupierOf(homeworld.World);

                if (occupier != null)
                {
                    occupier.Resources += amountToOccupier;
                    Log(Payment.Of(amountToOccupier), " received by ", from, " goes to ", occupier.Faction);
                }
            }
        }

        private void CollectResourcesFromStrongholdsAndHomeworlds()
        {
            if (Applicable(Rule.IncreasedResourceFlow) || Applicable(Rule.ResourceBonusForStrongholds))
            {
                foreach (var player in Players)
                {
                    CollectResourcesFromStronghold(Map.Arrakeen, player, 2);
                    CollectResourcesFromStronghold(Map.Carthag, player, 2);
                    CollectResourcesFromStronghold(Map.TueksSietch, player, 1);
                    CollectResourcesFromStronghold(Map.Cistern, player, 2);
                }
            }

            foreach (var homeworld in Map.Homeworlds)
            {
                var occupier = OccupierOf(homeworld.World);
                if (occupier != null)
                {
                    GainCollectedResources(occupier.Faction, homeworld.Territory, homeworld.ResourceAmount);
                    occupier.TransferrableResources += homeworld.ResourceAmount;
                }
            }
        }

        private void CollectResourcesFromStronghold(Location stronghold, Player player, int amount)
        {
            if (player.Controls(this, stronghold, Applicable(Rule.ContestedStongholdsCountAsOccupied)) &&
                !(player.Is(Faction.Pink) && Prevented(FactionAdvantage.PinkCollection) && player.HasAlly && !player.AlliedPlayer.Controls(this, stronghold, Applicable(Rule.ContestedStongholdsCountAsOccupied))))
            {
                GainCollectedResources(player.Faction, stronghold.Territory, amount);
            }
        }

        private void CollectResourcesFromTerritories()
        {
            foreach (var l in ResourcesOnPlanet.Where(x => x.Value > 0).ToList())
            {
                var thief = Players.FirstOrDefault(p => p.Occupies(Map.ProcessingStation));
                var playersToCollect = Players.Where(y => y.Occupies(l.Key)).ToArray();
                int totalCollectedAmount = 0;
                var spiceLeft = l.Value;

                foreach (var p in playersToCollect)
                {
                    int collectionRate = ResourceCollectionRate(p);
                    int forcesCollectingDefaultAmountOfSpice = p.Faction != Faction.Grey ? p.OccupyingForces(l.Key) : p.ForcesIn(l.Key);
                    int forcesCollecting3Spice = p.Is(Faction.Grey) ? p.SpecialForcesIn(l.Key) : 0;
                    int maximumSpiceThatCanBeCollected = forcesCollectingDefaultAmountOfSpice * collectionRate + forcesCollecting3Spice * 3;
                    int collectedAmountByThisPlayer = Math.Min(spiceLeft, maximumSpiceThatCanBeCollected);
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
                    {
                        GainCollectedResources(p.Faction, l.Key.Territory, collectedAmountByThisPlayer);
                    }
                    else
                    {
                        totalCollectedAmount += collectedAmountByThisPlayer;
                    }

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
            int receivedAmountByYellow = ResourcesCollectedByYellow;
            ModifyIncomeBasedOnThresholdOrOccupation(GetPlayer(Faction.Yellow), ref receivedAmountByYellow);

            var black = GetPlayer(Faction.Black);
            if (ResourcesCollectedByBlackFromDesertOrHomeworld != 0 && black.HasHighThreshold())
            {
                black.Resources += 2;
                Log(Faction.Black, " get ", Payment.Of(2), " extra as they collected from desert or homeworld");
            }

            MainPhaseEnd();
            Enter(Version >= 103, Phase.CollectionReport, EnterContemplatePhase);
        }


        #endregion Construction

        #region Information

        public int ResourceCollectionRate(Player p)
        {
            return p.Controls(this, Map.Arrakeen, Applicable(Rule.ContestedStongholdsCountAsOccupied)) || p.Controls(this, Map.Carthag, Applicable(Rule.ContestedStongholdsCountAsOccupied)) ? 3 : 2;
        }

        #endregion Information
    }
}
