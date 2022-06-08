/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        #region Mentat

        public List<Player> Winners { get; private set; } = new List<Player>();

        private void EnterMentatPhase()
        {
            MainPhaseStart(MainPhase.Contemplate, Version >= 103);
            AllowAllPreventedFactionAdvantages(null);
            HandleEconomics();
            if (Version >= 108) AddBribesToPlayerResources();

            foreach (var p in Players)
            {
                if (p.BankedResources > 0)
                {
                    p.Resources += p.BankedResources;
                    Log(p.Faction, " add ", Payment(p.BankedResources), " received as ", LeaderSkill.Banker, " to their reserves");
                    p.BankedResources = 0;
                }
            }

            Enter(Version >= 103, Phase.Contemplate, ContinueMentatPhase);
        }

        private void ContinueMentatPhase()
        {
            CheckNormalWin();
            CheckBeneGesseritPrediction();
            CheckFinalTurnWin();

            if (Winners.Count > 0 || CurrentTurn == MaximumNumberOfTurns)
            {
                CurrentMainPhase = MainPhase.Ended;
                Enter(Phase.GameEnded);
                RecentMilestones.Add(Milestone.GameWon);
                Log("The game has ended.");

                foreach (var w in Winners)
                {
                    Log(w.Faction, " win!");
                }
            }
            else
            {
                Enter(IsPlaying(Faction.Purple) && (Version < 113 || !Prevented(FactionAdvantage.PurpleReplacingFaceDancer)), Phase.ReplacingFaceDancer, Phase.TurnConcluded);
            }

            DetermineStrongholdOwnership();
            MainPhaseEnd();
        }

        private void AddBribesToPlayerResources()
        {
            foreach (var p in Players)
            {
                if (p.Bribes > 0)
                {
                    p.Resources += p.Bribes;
                    Log(p.Faction, " add ", Payment(p.Bribes), " from bribes to their reserves");
                    p.Bribes = 0;
                }
            }
        }

        #endregion

        #region StrongholdOwnership

        public Dictionary<Location, Faction> StrongholdOwnership { get; private set; } = new Dictionary<Location, Faction>();

        private void DetermineStrongholdOwnership()
        {
            if (Applicable(Rule.StrongholdBonus))
            {
                DetermineStrongholdOwnership(Map.Arrakeen);
                DetermineStrongholdOwnership(Map.Carthag);
                DetermineStrongholdOwnership(Map.SietchTabr);
                DetermineStrongholdOwnership(Map.HabbanyaSietch);
                DetermineStrongholdOwnership(Map.TueksSietch);
                DetermineStrongholdOwnership(Map.HiddenMobileStronghold);
            }
        }

        public StrongholdAdvantage ChosenHMSAdvantage { get; private set; } = StrongholdAdvantage.None;

        public bool HasStrongholdAdvantage(Faction f, StrongholdAdvantage advantage, Territory battleTerritory)
        {
            if (battleTerritory == Map.HiddenMobileStronghold.Territory && OwnsStronghold(f, Map.HiddenMobileStronghold) && ChosenHMSAdvantage == advantage)
            {
                return true;
            }

            return advantage switch
            {
                StrongholdAdvantage.FreeResourcesForBattles => battleTerritory == Map.Arrakeen.Territory && OwnsStronghold(f, Map.Arrakeen),
                StrongholdAdvantage.CollectResourcesForDial => battleTerritory == Map.SietchTabr.Territory && OwnsStronghold(f, Map.SietchTabr),
                StrongholdAdvantage.CollectResourcesForUseless => battleTerritory == Map.TueksSietch.Territory && OwnsStronghold(f, Map.TueksSietch),
                StrongholdAdvantage.CountDefensesAsAntidote => battleTerritory == Map.Carthag.Territory && OwnsStronghold(f, Map.Carthag),
                StrongholdAdvantage.WinTies => battleTerritory == Map.HabbanyaSietch.Territory && OwnsStronghold(f, Map.HabbanyaSietch),
                _ => false
            };
        }

        public bool OwnsStronghold(Faction f, Location stronghold)
        {
            return StrongholdOwnership.ContainsKey(stronghold) && StrongholdOwnership[stronghold] == f;
        }

        private void DetermineStrongholdOwnership(Location location)
        {
            var currentOwner = StrongholdOwnership.ContainsKey(location) ? StrongholdOwnership[location] : Faction.None;
            if (currentOwner != Faction.None)
            {
                StrongholdOwnership.Remove(location);
            }

            var newOwner = Players.FirstOrDefault(p => p.Controls(this, location, Applicable(Rule.ContestedStongholdsCountAsOccupied)));

            if (newOwner != null)
            {
                StrongholdOwnership.Add(location, newOwner.Faction);

                if (newOwner.Faction != currentOwner)
                {
                    Log(newOwner.Faction, " take control of ", location);
                }
            }
        }

        #endregion

        #region ChecingForVictories

        public WinMethod WinMethod { get; set; }

        private void CheckFinalTurnWin()
        {
            if (CurrentTurn == MaximumNumberOfTurns)
            {
                if (Winners.Count == 0)
                {
                    CheckSpecialWinConditions();
                }

                if (Winners.Count == 0)
                {
                    CheckOtherWinConditions();
                    CheckBeneGesseritPrediction();
                }
            }
        }

        private void CheckBeneGesseritPrediction()
        {
            var benegesserit = GetPlayer(Faction.Blue);
            if (benegesserit != null && benegesserit.PredictedTurn == CurrentTurn && Winners.Any(w => w.Faction == benegesserit.PredictedFaction))
            {
                Log(Faction.Blue, " predicted ", benegesserit.PredictedFaction, " victory in turn ", benegesserit.PredictedTurn, "! They had everything planned...");
                WinMethod = WinMethod.Prediction;
                Winners.Clear();
                Winners.Add(benegesserit);
            }
        }

        public bool IsSpecialStronghold(Territory t)
        {
            return t == Map.ShieldWall && Applicable(Rule.SSW) && NumberOfMonsters >= 4;
        }

        private void CheckNormalWin()
        {
            var checkWinSequence = new PlayerSequence(this);

            for (int i = 0; i < Players.Count; i++)
            {
                var p = checkWinSequence.CurrentPlayer;

                if (MeetsNormalVictoryCondition(p, Applicable(Rule.ContestedStongholdsCountAsOccupied)))
                {
                    WinMethod = WinMethod.Strongholds;

                    Winners.Add(p);

                    var ally = GetPlayer(p.Ally);
                    if (ally != null)
                    {
                        Winners.Add(ally);
                    }

                    LogNormalWin(p, ally);
                }

                if (Winners.Count > 0) break;

                checkWinSequence.NextPlayer();
            }
        }

        private void LogNormalWin(Player p, Player ally)
        {
            if (Players.Any(p => !Winners.Contains(p) && MeetsNormalVictoryCondition(p, Applicable(Rule.ContestedStongholdsCountAsOccupied))))
            {
                if (ally == null)
                {
                    Log(p.Faction, " are the first in front of the storm with enough victory points to win the game");
                }
                else
                {
                    Log(p.Faction, " and ", p.Ally, " are the first in front of the storm with enough victory points to win the game");
                }
            }
            else
            {
                if (ally == null)
                {
                    Log(p.Faction, " have enough victory points to win the game");
                }
                else
                {
                    Log(p.Faction, " and ", p.Ally, " have enough victory points to win the game");
                }
            }
        }

        public bool MeetsNormalVictoryCondition(Player p, bool contestedStongholdsCountAsOccupied)
        {
            return NumberOfVictoryPoints(p, contestedStongholdsCountAsOccupied) >= TresholdForWin(p);
        }

        public int CountChallengedVictoryPoints(Player p)
        {
            return NumberOfVictoryPoints(p, true) - NumberOfVictoryPoints(p, false);
        }

        public int TresholdForWin(Player p)
        {
            if (p.Ally != Faction.None)
            {
                return 4;
            }
            else
            {
                return (Players.Count <= 2) ? 4 : 3;
            }
        }

        public int NumberOfVictoryPoints(Player p, bool contestedStongholdsCountAsOccupied)
        {
            var ally = GetPlayer(p.Ally);

            if (ally != null)
            {
                int techTokenPoint = p.TechTokens.Count == 3 || p.AlliedPlayer.TechTokens.Count == 3 ? 1 : 0;
                return techTokenPoint + (Map.Locations.Where(l => l.Territory.IsStronghold || IsSpecialStronghold(l.Territory)).Count(l => p.Controls(this, l, contestedStongholdsCountAsOccupied) || ally.Controls(this, l, contestedStongholdsCountAsOccupied)));
            }
            else
            {
                int techTokenPoint = p.TechTokens.Count == 3 ? 1 : 0;
                return techTokenPoint + (Map.Locations.Where(l => l.Territory.IsStronghold || IsSpecialStronghold(l.Territory)).Count(l => p.Controls(this, l, contestedStongholdsCountAsOccupied)));
            }
        }

        private void CheckSpecialWinConditions()
        {
            var fremen = GetPlayer(Faction.Yellow);
            var guild = GetPlayer(Faction.Orange);

            if (fremen != null && YellowVictoryConditionMet)
            {
                Log(Faction.Yellow, " special victory conditions are met!");
                WinMethod = WinMethod.YellowSpecial;
                Winners.Add(fremen);
                if (fremen.Ally != Faction.None) Winners.Add(GetPlayer(fremen.Ally));
            }
            else if (guild != null && !Applicable(Rule.DisableOrangeSpecialVictory))
            {
                Log(Faction.Orange, " special victory conditions are met!");
                WinMethod = WinMethod.OrangeSpecial;
                Winners.Add(guild);
                if (guild.Ally != Faction.None) Winners.Add(GetPlayer(guild.Ally));
            }
            else if (fremen != null && !Applicable(Rule.DisableOrangeSpecialVictory))
            {
                Log(Faction.Yellow, " win because ", Faction.Orange, " are not playing and no one else won");
                WinMethod = WinMethod.OrangeSpecial;
                Winners.Add(fremen);
                if (fremen.Ally != Faction.None) Winners.Add(GetPlayer(fremen.Ally));
            }
        }

        public bool YellowVictoryConditionMet
        {
            get
            {
                bool sietchTabrOccupiedByOtherThanFremen = Players.Any(p => p.Faction != Faction.Yellow && p.Occupies(Map.SietchTabr));
                bool habbanyaSietchOccupiedByOtherThanFremen = Players.Any(p => p.Faction != Faction.Yellow && p.Occupies(Map.HabbanyaSietch));
                bool tueksSietchOccupiedByAtreidesOrHarkonnenOrEmperorOrRichese = Players.Any(p => (p.Is(Faction.Green) || p.Is(Faction.Black) || p.Is(Faction.Red) || p.Is(Faction.White)) && p.Occupies(Map.TueksSietch));

                return CurrentTurn == MaximumNumberOfTurns && !sietchTabrOccupiedByOtherThanFremen && !habbanyaSietchOccupiedByOtherThanFremen && !tueksSietchOccupiedByAtreidesOrHarkonnenOrEmperorOrRichese;
            }
        }

        private void CheckOtherWinConditions()
        {
            Player withMostPoints = null;
            int highestNumberOfPoints = -1;
            var checkWinSequence = new PlayerSequence(this);

            for (int i = 0; i < Players.Count; i++)
            {
                var p = checkWinSequence.CurrentPlayer;

                int pointsOfPlayer = NumberOfVictoryPoints(p, Applicable(Rule.ContestedStongholdsCountAsOccupied));
                if (pointsOfPlayer > highestNumberOfPoints)
                {
                    withMostPoints = p;
                    highestNumberOfPoints = pointsOfPlayer;
                }

                checkWinSequence.NextPlayer();
            }

            if (withMostPoints != null)
            {
                Winners.Add(withMostPoints);
                WinMethod = WinMethod.Strongholds;
                Log(withMostPoints.Faction, " are the first after the storm with most victory points");

                if (withMostPoints.Ally != Faction.None)
                {
                    Winners.Add(withMostPoints.AlliedPlayer);
                }
            }
        }

        #endregion

        #region MentatEvents

        public void HandleEvent(BrownEconomics e)
        {
            EconomicsStatus = e.Status;
            Log(e);
            RecentMilestones.Add(Milestone.Economics);
        }

        public void HandleEvent(BrownRemoveForce e)
        {
            Log(e);
            Discard(e.CardUsed());
            var target = GetPlayer(e.Target);

            if (e.SpecialForce)
            {
                target.SpecialForcesToReserves(e.Location, 1);
            }
            else
            {
                target.ForcesToReserves(e.Location, 1);
            }

            FlipBeneGesseritWhenAlone();
            RecentMilestones.Add(Milestone.SpecialUselessPlayed);
        }

        private void HandleEconomics()
        {
            switch (EconomicsStatus)
            {
                case BrownEconomicsStatus.Double:
                    EconomicsStatus = BrownEconomicsStatus.CancelFlipped;
                    Log(Faction.Brown, " The Economics Token flips to Cancel.");
                    RecentMilestones.Add(Milestone.Economics);
                    break;

                case BrownEconomicsStatus.Cancel:
                    EconomicsStatus = BrownEconomicsStatus.DoubleFlipped;
                    Log(Faction.Brown, " The Economics Token flips to Double.");
                    RecentMilestones.Add(Milestone.Economics);
                    break;

                case BrownEconomicsStatus.DoubleFlipped:
                case BrownEconomicsStatus.CancelFlipped:
                    EconomicsStatus = BrownEconomicsStatus.RemovedFromGame;
                    Log(Faction.Brown, " The Economics Token has been removed from the game.");
                    RecentMilestones.Add(Milestone.Economics);
                    break;
            }


        }

        public void HandleEvent(FaceDancerReplaced e)
        {
            if (!e.Passed)
            {
                var player = GetPlayer(e.Initiator);
                player.FaceDancers.Remove(e.SelectedDancer);
                TraitorDeck.PutOnTop(e.SelectedDancer);
                TraitorDeck.Shuffle();
                RecentMilestones.Add(Milestone.Shuffled);
                var leader = TraitorDeck.Draw();
                player.FaceDancers.Add(leader);
                if (!player.KnownNonTraitors.Contains(leader)) player.KnownNonTraitors.Add(leader);
            }

            Log(e);
            Enter(Phase.TurnConcluded);
        }

        #endregion
    }
}
