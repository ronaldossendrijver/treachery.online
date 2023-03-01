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
        private int ResourcesCollectedByYellow { get; set; }
        private int ResourcesCollectedByBlackFromDesertOrHomeworld { get; set; }

        private void EnterSpiceCollectionPhase()
        {
            MainPhaseStart(MainPhase.Collection);
            ResourcesCollectedByYellow = 0;
            ResourcesCollectedByBlackFromDesertOrHomeworld = 0;
            CallHeroesHome();

            if (Version < 122)
            {
                StartCollection();
            }
            else
            {
                Enter(Phase.BeginningOfCollection);
            }
        }

        private void StartCollection()
        {
            CollectResourcesFromTerritories();
            CollectResourcesFromStrongholds();
            Enter(CollectedResourcesToBeDivided.Any(), Phase.DividingCollectedResources, EndCollectionMainPhase);
        }

        public DivideResources CurrentDivideResources { get; private set; }

        public void HandleEvent(DivideResources e)
        {
            CurrentDivideResources = e;

            if (e.Passed)
            {
                DivideResourcesFromCollection(false);
                Enter(CollectedResourcesToBeDivided.Any(), Phase.DividingCollectedResources, EndCollectionMainPhase);
            }
            else
            {
                var toBeDivided = DivideResources.GetResourcesToBeDivided(this);
                int gainedByOtherFaction = DivideResources.GainedByOtherFaction(this, true, e.PortionToFirstPlayer);
                Log(e.Initiator, " propose that they take ", Payment(e.PortionToFirstPlayer), " and ", toBeDivided.OtherFaction, " take ", Payment(gainedByOtherFaction));
                Enter(Phase.AcceptingResourceDivision);
            }
        }

        public void HandleEvent(DivideResourcesAccepted e)
        {
            DivideResourcesFromCollection(!e.Passed);
            Enter(CollectedResourcesToBeDivided.Any(), Phase.DividingCollectedResources, EndCollectionMainPhase);
        }

        private void DivideResourcesFromCollection(bool divisionWasAgreed)
        {
            var toBeDivided = DivideResources.GetResourcesToBeDivided(this);

            int gainedByFirstFaction = DivideResources.GainedByFirstFaction(this, !divisionWasAgreed, CurrentDivideResources.PortionToFirstPlayer);
            int gainedByOtherFaction = DivideResources.GainedByOtherFaction(this, !divisionWasAgreed, CurrentDivideResources.PortionToFirstPlayer);

            Collect(toBeDivided.FirstFaction, toBeDivided.Territory, gainedByFirstFaction);
            Collect(toBeDivided.OtherFaction, toBeDivided.Territory, gainedByOtherFaction);

            CollectedResourcesToBeDivided.Remove(toBeDivided);

            CurrentDivideResources = null;
        }

        private void Collect(Faction faction, Territory from, int amount)
        {
            Log(faction, " collect ", Payment(amount), " from ", from);
            GetPlayer(faction).Resources += amount;
            if (faction == Faction.Yellow) ResourcesCollectedByYellow += amount;
            if (faction == Faction.Black && !from.IsStronghold && !from.IsProtectedFromWorm) ResourcesCollectedByBlackFromDesertOrHomeworld += amount;
        }

        private void EndCollectionMainPhase()
        {
            int receivedAmountByYellow = ResourcesCollectedByYellow;
            ModifyIncomeBasedOnThresholdOrOccupation(GetPlayer(Faction.Yellow), ref receivedAmountByYellow, out Player occupier, out int amountToOccupier);
            if (occupier != null && occupier.Is(Faction.Black)) ResourcesCollectedByBlackFromDesertOrHomeworld += amountToOccupier;

            var black = GetPlayer(Faction.Black);
            if (ResourcesCollectedByBlackFromDesertOrHomeworld != 0 && black.HasHighThreshold())
            {
                black.Resources += 2;
                Log(Faction.Black, " get ", Payment(2), " from ", black.PrimaryHomeworld);
            }

            MainPhaseEnd();
            Enter(Version >= 103, Phase.CollectionReport, EnterMentatPhase);
        }

        private void ModifyIncomeBasedOnThresholdOrOccupation(Player from, ref int receivedAmount) => ModifyIncomeBasedOnThresholdOrOccupation(from, ref receivedAmount, out _, out _);


        private void ModifyIncomeBasedOnThresholdOrOccupation(Player from, ref int receivedAmount, out Player occupier, out int amountToOccupier)
        {
            occupier = null;
            amountToOccupier = 0;

            if (receivedAmount > 1 && Applicable(Rule.Homeworlds))
            {
                amountToOccupier = from.HasLowThreshold() ? (int)(0.5f * receivedAmount) : 0;
                receivedAmount -= amountToOccupier;

                var homeworld = from.Homeworlds.First();
                occupier = OccupierOf(homeworld.World);

                if (occupier != null)
                {
                    occupier.Resources += amountToOccupier;
                    Log(Payment(amountToOccupier), " received by ", from, " goes to ", occupier.Faction);
                }
            }
        }

        private void CollectResourcesFromStrongholds()
        {
            if (Applicable(Rule.IncreasedResourceFlow) || Applicable(Rule.ResourceBonusForStrongholds))
            {
                foreach (var player in Players)
                {
                    GetResourcesFrom(Map.Arrakeen, player, 2);
                    GetResourcesFrom(Map.Carthag, player, 2);
                    GetResourcesFrom(Map.TueksSietch, player, 1);
                }
            }
        }

        private void GetResourcesFrom(Location stronghold, Player player, int amount)
        {
            if (player.Controls(this, stronghold, Applicable(Rule.ContestedStongholdsCountAsOccupied)) && 
                !(player.Is(Faction.Pink) && Prevented(FactionAdvantage.PinkCollection) && player.HasAlly && !player.AlliedPlayer.Controls(this, Map.Arrakeen, Applicable(Rule.ContestedStongholdsCountAsOccupied))))
            {
                Collect(player.Faction, Map.Arrakeen.Territory, amount);
            }
        }

        public List<ResourcesToBeDivided> CollectedResourcesToBeDivided { get; private set; } = new();
        private void CollectResourcesFromTerritories()
        {
            foreach (var l in ResourcesOnPlanet.Where(x => x.Value > 0).ToList())
            {
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

                    if (playersToCollect.Length == 1)
                    {
                        Collect(p.Faction, l.Key.Territory, collectedAmountByThisPlayer);
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
                    Log(toBeDivided.FirstFaction, " and ", toBeDivided.OtherFaction, " will have to share ", Payment(totalCollectedAmount), " from ", l.Key.Territory);
                }
            }
        }





        public int ResourceCollectionRate(Player p)
        {
            return (p.Controls(this, Map.Arrakeen, Applicable(Rule.ContestedStongholdsCountAsOccupied)) || p.Controls(this, Map.Carthag, Applicable(Rule.ContestedStongholdsCountAsOccupied))) ? 3 : 2;
        }
    }
}
