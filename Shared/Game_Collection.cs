/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        private void EnterSpiceCollectionPhase()
        {
            MainPhaseStart(MainPhase.Collection);
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

        public void HandleEvent(DivideResources e)
        {
            var toBeDivided = DivideResources.GetResourcesToBeDivided(this);

            int gainedByOtherFaction = e.GainedByOtherFaction(this);
            int gainedByFirstFaction = e.GainedByFirstFaction(this);

            Log(toBeDivided.FirstFaction, " collect ", Payment(gainedByFirstFaction), " from ", toBeDivided.Territory);
            GetPlayer(toBeDivided.FirstFaction).Resources += gainedByFirstFaction;

            Log(toBeDivided.OtherFaction, " collect ", Payment(gainedByOtherFaction), " from ", toBeDivided.Territory);
            GetPlayer(toBeDivided.OtherFaction).Resources += gainedByOtherFaction;

            CollectedResourcesToBeDivided.Remove(toBeDivided);
            Enter(CollectedResourcesToBeDivided.Any(), Phase.DividingCollectedResources, EndCollectionMainPhase);
        }

        private void EndCollectionMainPhase()
        {
            MainPhaseEnd();
            Enter(Version >= 103, Phase.CollectionReport, EnterMentatPhase);
        }

        private void CollectResourcesFromStrongholds()
        {
            if (Applicable(Rule.IncreasedResourceFlow) || Applicable(Rule.ResourceBonusForStrongholds))
            {
                foreach (var playerInArrakeen in Players.Where(p => p.Controls(this, Map.Arrakeen, Applicable(Rule.ContestedStongholdsCountAsOccupied))))
                {
                    Log(playerInArrakeen.Faction, " collect ", Payment(2), " from ", Map.Arrakeen);
                    playerInArrakeen.Resources += 2;
                }

                foreach (var playerInCarthag in Players.Where(p => p.Controls(this, Map.Carthag, Applicable(Rule.ContestedStongholdsCountAsOccupied))))
                {
                    Log(playerInCarthag.Faction, " collect ", Payment(2), " from ", Map.Carthag);
                    playerInCarthag.Resources += 2;
                }

                foreach (var playerInTueksSietch in Players.Where(p => p.Controls(this, Map.TueksSietch, Applicable(Rule.ContestedStongholdsCountAsOccupied))))
                {
                    Log(playerInTueksSietch.Faction, " collect ", Payment(1), " from ", Map.TueksSietch);
                    playerInTueksSietch.Resources += 1;
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

                foreach (var p in playersToCollect)
                {
                    int collectionRate = ResourceCollectionRate(p);
                    int forcesCollectingDefaultAmountOfSpice = p.Faction != Faction.Grey ? p.OccupyingForces(l.Key) : p.ForcesIn(l.Key);
                    int forcesCollecting3Spice = p.Is(Faction.Grey) ? p.SpecialForcesIn(l.Key) : 0;
                    int maximumSpiceThatCanBeCollected = forcesCollectingDefaultAmountOfSpice * collectionRate + forcesCollecting3Spice * 3;
                    int collectedAmountByThisPlayer = Math.Min(l.Value, maximumSpiceThatCanBeCollected);
                    ChangeResourcesOnPlanet(l.Key, -collectedAmountByThisPlayer);

                    if (playersToCollect.Length == 1)
                    {
                        Log(p.Faction, " collect ", Payment(collectedAmountByThisPlayer), " from ", l.Key.Territory);
                        p.Resources += collectedAmountByThisPlayer;
                    }
                    else
                    {
                        totalCollectedAmount += collectedAmountByThisPlayer;
                    }
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
                }
            }
        }

        



        public int ResourceCollectionRate(Player p)
        {
            return (p.Controls(this, Map.Arrakeen, Applicable(Rule.ContestedStongholdsCountAsOccupied)) || p.Controls(this, Map.Carthag, Applicable(Rule.ContestedStongholdsCountAsOccupied))) ? 3 : 2;
        }
    }
}
