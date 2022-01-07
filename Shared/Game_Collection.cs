/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System;
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
            MainPhaseEnd();
            Enter(Version >= 103, Phase.CollectionReport, EnterMentatPhase);
        }

        private void CollectResourcesFromStrongholds()
        {
            if (Applicable(Rule.IncreasedResourceFlow) || Applicable(Rule.ResourceBonusForStrongholds))
            {
                foreach (var playerInArrakeen in Players.Where(p => p.Controls(this, Map.Arrakeen, Applicable(Rule.ContestedStongholdsCountAsOccupied))))
                {
                    CurrentReport.Express(playerInArrakeen.Faction, " collect ", Payment(2), " for ", Map.Arrakeen);
                    playerInArrakeen.Resources += 2;
                }

                foreach (var playerInCarthag in Players.Where(p => p.Controls(this, Map.Carthag, Applicable(Rule.ContestedStongholdsCountAsOccupied))))
                {
                    CurrentReport.Express(playerInCarthag.Faction, " collect ", Payment(2), " for ", Map.Carthag);
                    playerInCarthag.Resources += 2;
                }

                foreach (var playerInTueksSietch in Players.Where(p => p.Controls(this, Map.TueksSietch, Applicable(Rule.ContestedStongholdsCountAsOccupied))))
                {
                    CurrentReport.Express(playerInTueksSietch.Faction, " collect ", Payment(1), " for ", Map.TueksSietch);
                    playerInTueksSietch.Resources += 1;
                }
            }
        }

        private void CollectResourcesFromTerritories()
        {
            foreach (var l in ResourcesOnPlanet.Where(x => x.Value > 0).ToList())
            {
                foreach (var p in Players.Where(y => y.Occupies(l.Key)))
                {
                    int collectionRate = ResourceCollectionRate(p);
                    int forcesCollectingDefaultAmountOfSpice = p.Faction != Faction.Grey ? p.OccupyingForces(l.Key) : p.ForcesIn(l.Key);
                    int forcesCollecting3Spice = p.Is(Faction.Grey) ? p.SpecialForcesIn(l.Key) : 0;
                    int maximumSpiceThatCanBeCollected = forcesCollectingDefaultAmountOfSpice * collectionRate + forcesCollecting3Spice * 3;
                    int collectedAmount = Math.Min(l.Value, maximumSpiceThatCanBeCollected);
                    ChangeResourcesOnPlanet(l.Key, -collectedAmount);
                    CurrentReport.Express(p.Faction, " collect ", Payment(collectedAmount), " from ", l.Key);
                    p.Resources += collectedAmount;
                }
            }
        }

        public int ResourceCollectionRate(Player p)
        {
            return (p.Controls(this, Map.Arrakeen, Applicable(Rule.ContestedStongholdsCountAsOccupied)) || p.Controls(this, Map.Carthag, Applicable(Rule.ContestedStongholdsCountAsOccupied))) ? 3 : 2;
        }
    }
}
