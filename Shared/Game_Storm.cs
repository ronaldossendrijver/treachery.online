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
        public int HmsMovesLeft { get; private set; }
        public List<Faction> HasBattleWheel { get; private set; } = new List<Faction>();

        private readonly List<int> Dials = new List<int>();

        private void EnterStormPhase()
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

        public void HandleEvent(StormDialled e)
        {
            Dials.Add(e.Amount);
            HasActedOrPassed.Add(e.Initiator);

            if (HasBattleWheel.All(f => HasActedOrPassed.Contains(f)))
            {
                Log("Storm dial: ", Dials[0], HasActedOrPassed[0], " + ", Dials[1], HasActedOrPassed[1], " = ", Dials[0] + Dials[1]);
                NextStormMoves = Dials.Sum();

                if (CurrentTurn == 1)
                {
                    PositionFirstStorm();
                }
                else
                {
                    RevealStorm();
                }
            }
        }

        private void PositionFirstStorm()
        {
            SectorInStorm = NextStormMoves % Map.NUMBER_OF_SECTORS;
            Log("The first storm moves ", SectorInStorm, " sectors");
            PerformStorm();

            if (Applicable(Rule.TechTokens))
            {
                AssignTechTokens();
            }

            if (UseStormDeck)
            {
                NextStormMoves = DetermineLaterStormWithStormDeck();
            }

            Enter(IsPlaying(Faction.Grey) || Applicable(Rule.HMSwithoutGrey), Phase.HmsPlacement, EndStormPhase);
        }

        private void MoveHMSBeforeDiallingStorm()
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

        private int DetermineLaterStormWithStormDeck()
        {
            return Random.Next(6) + 1;
        }

        private void DetermineStorm()
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

        private void RevealStorm()
        {
            HasActedOrPassed.Clear();
            Log("The storm will move ", NextStormMoves, " sectors...");
            Enter(Phase.MetheorAndStormSpell);
        }

        public void HandleEvent(PerformHmsPlacement e)
        {
            Map.HiddenMobileStronghold.PointAt(e.Target);
            Log(e);
            EndStormPhase();
            Stone(Milestone.HmsMovement);
        }

        public void HandleEvent(PerformHmsMovement e)
        {
            var initiator = GetPlayer(e.Initiator);
            int collectionRate = initiator.AnyForcesIn(Map.HiddenMobileStronghold) * 2;
            Log(e);

            var currentLocation = Map.HiddenMobileStronghold.AttachedToLocation;
            CollectSpiceFrom(e.Initiator, currentLocation, collectionRate);

            if (!e.Passed)
            {
                Map.HiddenMobileStronghold.PointAt(e.Target);
                CollectSpiceFrom(e.Initiator, e.Target, collectionRate);
                HmsMovesLeft--;
                Stone(Milestone.HmsMovement);
            }

            if (e.Passed || HmsMovesLeft == 0)
            {
                DetermineStorm();
            }
        }

        private void CollectSpiceFrom(Faction faction, Location l, int maximumAmount)
        {
            if (ResourcesOnPlanet.ContainsKey(l))
            {
                int collected = Math.Min(ResourcesOnPlanet[l], maximumAmount);
                ChangeResourcesOnPlanet(l, -collected);
                var collector = GetPlayer(faction);
                collector.Resources += collected;
                Log(faction, " collect ", Payment.Of(collected), " from ", l);
            }
        }

        private void AssignTechTokens()
        {
            var techTokensToBeDealt = new List<TechToken>();

            var yellow = GetPlayer(Faction.Yellow);
            if (yellow != null)
            {
                yellow.TechTokens.Add(TechToken.Resources);
                Log(Faction.Yellow, " receive ", TechToken.Resources);
            }
            else
            {
                techTokensToBeDealt.Add(TechToken.Resources);
            }

            var purple = GetPlayer(Faction.Purple);
            if (purple != null)
            {
                purple.TechTokens.Add(TechToken.Graveyard);
                Log(Faction.Purple, " receive ", TechToken.Graveyard);
            }
            else
            {
                techTokensToBeDealt.Add(TechToken.Graveyard);
            }

            var grey = GetPlayer(Faction.Grey);
            if (grey != null)
            {
                grey.TechTokens.Add(TechToken.Ships);
                Log(Faction.Grey, " receive ", TechToken.Ships);
            }
            else
            {
                techTokensToBeDealt.Add(TechToken.Ships);
            }

            var remainingTechTokens = new Deck<TechToken>(techTokensToBeDealt, Random);
            remainingTechTokens.Shuffle();
            Stone(Milestone.Shuffled);

            var techTokenSequence = new PlayerSequence(this);

            while (!remainingTechTokens.IsEmpty && Players.Any(p => p.TechTokens.Count == 0))
            {
                if (techTokenSequence.CurrentPlayer.TechTokens.Count == 0)
                {
                    var token = remainingTechTokens.Draw();
                    techTokenSequence.CurrentPlayer.TechTokens.Add(token);
                    Log(techTokenSequence.CurrentPlayer.Faction, " receive ", token);
                }

                techTokenSequence.NextPlayer();
            }
        }

        public void HandleEvent(MetheorPlayed e)
        {
            var player = GetPlayer(e.Initiator);
            var card = player.Card(TreacheryCardType.Metheor);

            Stone(Milestone.MetheorUsed);
            ShieldWallDestroyed = true;
            player.TreacheryCards.Remove(card);
            RemovedTreacheryCards.Add(card);
            Log(e);

            foreach (var p in Players)
            {
                foreach (var location in Map.ShieldWall.Locations.Where(l => p.AnyForcesIn(l) > 0))
                {
                    RevealCurrentNoField(p, location);
                    p.KillAllForces(location, false);
                }
            }
        }

        public void HandleEvent(StormSpellPlayed e)
        {
            Discard(GetPlayer(e.Initiator), TreacheryCardType.StormSpell);
            Log(e);
            MoveStormAndDetermineNext(e.MoveAmount);
            EndStormPhase();
        }

        public bool CurrentTestingStationUsed { get; private set; }
        public void HandleEvent(TestingStationUsed e)
        {
            Log(e);
            Stone(Milestone.WeatherControlled);
            CurrentTestingStationUsed = true;
            NextStormMoves += e.ValueAdded;
        }

        private void EnterNormalStormPhase()
        {
            Log("The storm moves ", NextStormMoves, " sectors");
            MoveStormAndDetermineNext(NextStormMoves);
            EndStormPhase();
        }

        private void MoveStormAndDetermineNext(int amount)
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

        private void PerformStorm()
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
                            StormLossesToTake.Add(new LossToTake() { Location = l.Key, Amount = LossesToTake(battalion), Faction = battalion.Faction });
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

        private Phase PhaseBeforeStormLoss;

        public void HandleEvent(TakeLosses e)
        {
            var player = GetPlayer(e.Initiator);
            bool mustDiscard = false;

            if (e.UseUselessCard)
            {
                var card = TakeLosses.ValidUselessCardToPreventLosses(this, e.Player);
                if (card == null && NexusPlayed.CanUseCunning(player))
                {
                    PlayNexusCard(player, "Cunning", " prevent losing forces in ", StormLossesToTake[0].Location);
                    mustDiscard = true;
                }
                else
                {
                    Discard(e.Player, card);
                    Log(e.Initiator, " prevent losing forces in ", StormLossesToTake[0].Location);
                }

                StormLossesToTake.RemoveAt(0);
                Stone(Milestone.SpecialUselessPlayed);
            }
            else
            {
                player.KillForces(TakeLosses.LossesToTake(this).Location, e.ForceAmount, e.SpecialForceAmount, false);
                StormLossesToTake.RemoveAt(0);
                Log(e);
            }

            if (PhaseBeforeStormLoss == Phase.BlowA)
            {
                Enter(StormLossesToTake.Count > 0, Phase.StormLosses, EndStormPhase);
            }
            else
            {
                Enter(PhaseBeforeStormLoss);
                DetermineNextShipmentAndMoveSubPhase();
            }

            if (mustDiscard)
            {
                LetPlayerDiscardTreacheryCardOfChoice(e.Initiator);
            }
        }

        private int LossesToTake(Battalion battalion)
        {
            return LossesToTake(battalion.AmountOfForces, battalion.AmountOfSpecialForces);
        }

        private int LossesToTake(int AmountOfForces, int AmountOfSpecialForces)
        {
            return (int)Math.Ceiling(0.5 * (AmountOfForces + AmountOfSpecialForces));
        }

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

        private void EndStormPhase()
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
