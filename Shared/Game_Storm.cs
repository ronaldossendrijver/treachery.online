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
        public int HmsMovesLeft { get; internal set; }
        public List<Faction> HasBattleWheel { get; private set; } = new List<Faction>();

        internal readonly List<int> Dials = new();

        internal void EnterStormPhase()
        {
            CurrentTurn++;
            MainPhaseStart(MainPhase.Storm, CurrentTurn > 1);
            FactionsThatRevivedSpecialForcesThisTurn.Clear();
            StormLossesToTake.Clear();
            CurrentTestingStationUsed = false;

            if (CurrentTurn == 1)
            {
                Dials.Clear();
                HasActedOrPassed.Clear();
                Enter(Phase.DiallingStorm);
            }
            else
            {
                if (JustRevealedDiscoveryStrongholds.Any())
                {
                    Enter(Phase.BeginningOfStorm);
                }
                else
                {
                    MoveHMSBeforeDiallingStorm();
                }
            }

            DetermineOccupationAtStartOrEndOfTurn();
        }

        

        internal void MoveHMSBeforeDiallingStorm()
        {
            if (IsPlaying(Faction.Grey) && GetPlayer(Faction.Grey).AnyForcesIn(Map.HiddenMobileStronghold) > 0)
            {
                if (Prevented(FactionAdvantage.GreyMovingHMS))
                {
                    LogPreventionByKarma(FactionAdvantage.GreyMovingHMS);
                    if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.GreyMovingHMS);
                    DetermineStorm();
                }
                else if (HasLowThreshold(Faction.Grey))
                {
                    LogPreventionByLowThreshold(FactionAdvantage.GreyMovingHMS);
                    DetermineStorm();
                }
                else
                {
                    if (Map.HiddenMobileStronghold.AttachedToLocation.Sector == SectorInStorm)
                    {
                        Log("The storm prevents ", Map.HiddenMobileStronghold, " from moving");
                        DetermineStorm();
                    }
                    else
                    {
                        HmsMovesLeft = 3;
                        Enter(Phase.HmsMovement);
                    }
                }
            }
            else
            {
                DetermineStorm();
            }
        }

        internal int DetermineLaterStormWithStormDeck()
        {
            return Random.Next(6) + 1;
        }

        internal void DetermineStorm()
        {
            if (UseStormDeck)
            {
                RevealStorm();
            }
            else
            {
                Dials.Clear();
                HasActedOrPassed.Clear();
                Enter(Phase.DiallingStorm);
            }
        }

        internal void RevealStorm()
        {
            HasActedOrPassed.Clear();
            Log("The storm will move ", NextStormMoves, " sectors...");
            Enter(Phase.MetheorAndStormSpell);
        }

        internal void CollectSpiceFrom(Faction faction, Location l, int maximumAmount)
        {
            if (ResourcesOnPlanet.TryGetValue(l, out int value))
            {
                int collected = Math.Min(value, maximumAmount);
                ChangeResourcesOnPlanet(l, -collected);
                var collector = GetPlayer(faction);
                collector.Resources += collected;
                Log(faction, " collect ", Payment.Of(collected), " from ", l);
            }
        }

        
        public bool CurrentTestingStationUsed { get; private set; }
        public void HandleEvent(TestingStationUsed e)
        {
            Log(e);
            Stone(Milestone.WeatherControlled);
            CurrentTestingStationUsed = true;
            NextStormMoves += e.ValueAdded;
        }

        

        internal void MoveStormAndDetermineNext(int amount)
        {
            if (amount == 0)
            {
                PerformStorm();
            }

            for (int i = 0; i < amount; i++)
            {
                SectorInStorm = (SectorInStorm + 1) % Map.NUMBER_OF_SECTORS;
                PerformStorm();
            }

            if (UseStormDeck)
            {
                NextStormMoves = DetermineLaterStormWithStormDeck();
            }

            Stone(Milestone.Storm);
        }

        internal void PerformStorm()
        {
            foreach (var l in Forces(false).Where(f => f.Key.Sector == SectorInStorm && !IsProtectedFromStorm(f.Key)))
            {
                foreach (var battalion in l.Value)
                {
                    var player = GetPlayer(battalion.Faction);

                    int killCount = 0;
                    if (battalion.Is(Faction.Yellow) && Applicable(Rule.YellowStormLosses))
                    {
                        if (!Prevented(FactionAdvantage.YellowProtectedFromStorm))
                        {
                            StormLossesToTake.Add(new LossToTake() { Location = l.Key, Amount = TakeLosses.HalfOf(battalion.AmountOfForces, battalion.AmountOfSpecialForces), Faction = battalion.Faction });
                        }
                        else
                        {
                            killCount += player.AnyForcesIn(l.Key);
                            player.KillAllForces(l.Key, false);
                            if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.YellowProtectedFromStorm);
                        }
                    }
                    else if (battalion.Is(Faction.Brown) && (!Prevented(FactionAdvantage.BrownDiscarding) || Applicable(Rule.NexusCards)))
                    {
                        StormLossesToTake.Add(new LossToTake() { Location = l.Key, Amount = battalion.TotalAmountOfForces, Faction = battalion.Faction });
                    }
                    else
                    {
                        RevealCurrentNoField(player, l.Key);
                        killCount += player.AnyForcesIn(l.Key);
                        player.KillAllForces(l.Key, false);
                    }

                    if (killCount > 0)
                    {
                        Log("The storm kills ", killCount, battalion.Faction, " forces in ", l.Key);
                    }
                }
            }


            foreach (var t in Strongholds.Where(s => s.Locations.Any(l => l.Sector == SectorInStorm && !IsProtectedFromStorm(l))))
            {
                var ambassador = AmbassadorIn(t);
                if (ambassador != Ambassador.None)
                {
                    var pink = GetPlayer(Faction.Pink);
                    bool succ = AmbassadorsOnPlanet.Remove(t);
                    pink.Ambassadors.Add(ambassador);
                    Log("The ambassador in ", t, " returns to ", Faction.Pink);
                }
            }

            foreach (var l in Map.Locations(false).Where(l => l.Sector == SectorInStorm))
            {
                int removed = RemoveResources(l);
                if (removed > 0)
                {
                    Log("The storm destroys ", Payment.Of(removed), " in ", l);
                }
            }
        }

        internal Phase PhaseBeforeStormLoss { get; set; }

        public List<LossToTake> StormLossesToTake { get; private set; } = new List<LossToTake>();
        public bool UseStormDeck => Applicable(Rule.StormDeckWithoutYellow) || IsPlaying(Faction.Yellow) && Applicable(Rule.YellowSeesStorm);

        public bool IsProtectedFromStorm(Location l)
        {
            if (!ShieldWallDestroyed)
            {
                return l.Territory.IsProtectedFromStorm;
            }
            else
            {
                return l.Territory.IsProtectedFromStorm && !(l == Map.Arrakeen || l.Territory == Map.ImperialBasin || l == Map.Carthag);
            }
        }

        internal void EndStormPhase()
        {
            JustRevealedDiscoveryStrongholds.Clear();

            MainPhaseEnd();

            if (StormLossesToTake.Count > 0)
            {
                PhaseBeforeStormLoss = Phase.BlowA;
                Enter(Phase.StormLosses);
            }
            else
            {
                if (Version >= 103)
                {
                    if (Version >= 132) MainPhaseEnd();
                    Enter(Phase.StormReport);
                }
                else
                {
                    EnterSpiceBlowPhase();
                }
            }
        }
    }

    public class LossToTake
    {
        public Faction Faction;
        public Location Location;
        public int Amount;
    }
}
