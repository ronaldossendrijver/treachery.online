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
        private void EnterSpiceCollectionPhase()
        {
            MainPhaseStart(MainPhase.Collection);
            ResourcesCollectedByYellow = 0;
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

        private void Collect(Faction faction, object from, int amount)
        {
            Log(faction, " collect ", Payment(amount), " from ", from);
            GetPlayer(faction).Resources += amount;
            if (faction == Faction.Yellow) ResourcesCollectedByYellow += amount;
        }

        private void EndCollectionMainPhase()
        {
            if (ResourcesCollectedByYellow > 1) {
                var occupierOfYellowHomeworld = OccupierOf(World.Yellow);
                if (occupierOfYellowHomeworld != null)
                {
                    var amountToOccupier = (int)(0.5f * ResourcesCollectedByYellow);
                    occupierOfYellowHomeworld.Resources += amountToOccupier;
                    GetPlayer(Faction.Yellow).Resources -= amountToOccupier;
                    Log(Payment(amountToOccupier), " collected by ", Faction.Yellow, " is transferred to ", occupierOfYellowHomeworld);
                }
            }

            MainPhaseEnd();
            Enter(Version >= 103, Phase.CollectionReport, EnterMentatPhase);
        }

        private void CollectResourcesFromStrongholds()
        {
            if (Applicable(Rule.IncreasedResourceFlow) || Applicable(Rule.ResourceBonusForStrongholds))
            {
                foreach (var playerInArrakeen in Players.Where(p => p.Controls(this, Map.Arrakeen, Applicable(Rule.ContestedStongholdsCountAsOccupied))))
                {
                    Collect(playerInArrakeen.Faction, Map.Arrakeen, 2);
                }

                foreach (var playerInCarthag in Players.Where(p => p.Controls(this, Map.Carthag, Applicable(Rule.ContestedStongholdsCountAsOccupied))))
                {
                    Collect(playerInCarthag.Faction, Map.Carthag, 2);
                }

                foreach (var playerInTueksSietch in Players.Where(p => p.Controls(this, Map.TueksSietch, Applicable(Rule.ContestedStongholdsCountAsOccupied))))
                {
                    Collect(playerInTueksSietch.Faction, Map.TueksSietch, 1);
                }
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
