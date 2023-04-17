/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        #region State

        internal bool ExtortionToBeReturned { get; set; }
        public Dictionary<Location, Faction> StrongholdOwnership { get; private set; } = new();
        public bool CyanHasPlantedTerror { get; internal set; } = false;
        public List<Player> Winners { get; } = new(); 
        public WinMethod WinMethod { get; set; }

        #endregion State

        #region Contemplate

        internal void EnterContemplatePhase()
        {
            foreach (var player in Players)
            {
                player.TransferrableResources = 0;
            }

            MainPhaseStart(MainPhase.Contemplate, Version >= 103);
            AllowAllPreventedFactionAdvantages(null);
            HandleEconomics();
            if (Version >= 108) AddBribesToPlayerResources();
            CyanHasPlantedTerror = false;

            foreach (var p in Players)
            {
                if (p.BankedResources > 0)
                {
                    p.Resources += p.BankedResources;
                    Log(p.Faction, " add ", Payment.Of(p.BankedResources), " received as ", LeaderSkill.Banker, " to their reserves");
                    p.BankedResources = 0;
                }
            }

            Enter(Version >= 103, EnterMentatPause, ContinueMentatPhase);
        }

        private void HandleEconomics()
        {
            switch (EconomicsStatus)
            {
                case BrownEconomicsStatus.Double:
                    EconomicsStatus = BrownEconomicsStatus.CancelFlipped;
                    Log(Faction.Brown, " The Economics Token flips to Cancel.");
                    Stone(Milestone.Economics);
                    break;

                case BrownEconomicsStatus.Cancel:
                    EconomicsStatus = BrownEconomicsStatus.DoubleFlipped;
                    Log(Faction.Brown, " The Economics Token flips to Double.");
                    Stone(Milestone.Economics);
                    break;

                case BrownEconomicsStatus.DoubleFlipped:
                case BrownEconomicsStatus.CancelFlipped:
                    EconomicsStatus = BrownEconomicsStatus.RemovedFromGame;
                    Log(Faction.Brown, " The Economics Token has been removed from the game.");
                    Stone(Milestone.Economics);
                    break;
            }
        }

        private void EnterMentatPause()
        {
            DetermineIfCyanDrawsNewTraitor();
            ExtortionToBeReturned = Players.Any(p => p.Extortion > 0);
            GainExtortions();

            if (BlackTraitorWasCancelled)
            {
                BlackTraitorWasCancelled = false;
                GetPlayer(Faction.Black).Traitors.Add(TraitorDeck.Draw());
                Log(Faction.Black, " drew a new traitor after having been betrayed");
            }

            Enter(ExtortionToBeReturned, Phase.Extortion, EndContemplatePause);
        }

        private void DetermineIfCyanDrawsNewTraitor()
        {
            var cyan = GetPlayer(Faction.Cyan);
            if (cyan != null)
            {
                foreach (var l in Assassinated.Where(l => cyan.RevealedTraitors.Contains(l)).ToList())
                {

                    Log(Faction.Cyan, " draw a new traitor to replace ", l);
                    cyan.Traitors.Remove(l);
                    cyan.RevealedTraitors.Remove(l);
                    cyan.Traitors.Add(TraitorDeck.Draw());

                }
            }
        }

        internal void EndContemplatePause()
        {
            if (ExtortionToBeReturned)
            {
                Log(Faction.Cyan, " regain their ", TerrorType.Extortion, " token");
                UnplacedTerrorTokens.Add(TerrorType.Extortion);
            }

            Enter(Phase.Contemplate);
            DetermineOccupationAtStartOrEndOfTurn();
        }

        private void GainExtortions()
        {
            foreach (var p in Players.Where(p => p.Extortion > 0).ToList())
            {
                p.Resources += p.Extortion;
                Log(p.Faction, " add ", Payment.Of(p.Extortion), " gained from ", TerrorType.Extortion, " to their reserves");
                p.Extortion = 0;
            }
        }

        internal void ContinueMentatPhase()
        {
            CheckNormalWin();
            CheckBeneGesseritPrediction();
            CheckFinalTurnWin();

            if (Winners.Count > 0 || CurrentTurn == MaximumNumberOfTurns)
            {
                CurrentMainPhase = MainPhase.Ended;
                Enter(Phase.GameEnded);
                Stone(Milestone.GameWon);
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

            if (WhenToSetAsideVidal == VidalMoment.EndOfTurn)
            {
                SetAsideVidal();
            }

            MainPhaseEnd();
        }

        internal void AddBribesToPlayerResources()
        {
            foreach (var p in Players)
            {
                if (p.Bribes > 0)
                {
                    p.Resources += p.Bribes;
                    Log(p.Faction, " add ", Payment.Of(p.Bribes), " from bribes to their reserves");
                    p.Bribes = 0;
                }
            }
        }

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

        private void DetermineStrongholdOwnership(Location location)
        {
            var currentOwner = StrongholdOwnership.TryGetValue(location, out Faction value) ? value : Faction.None;
            var newOwningPlayer = Players.FirstOrDefault(p => p.Controls(this, location, Applicable(Rule.ContestedStongholdsCountAsOccupied)));
            var newOwner = newOwningPlayer != null ? newOwningPlayer.Faction : Faction.None;

            if (currentOwner != newOwner)
            {
                if (currentOwner != Faction.None && newOwner != Faction.None)
                {
                    StrongholdOwnership.Remove(location);
                    StrongholdOwnership.Add(location, newOwner);
                    Log(newOwner, " take control over ", location, " from ", currentOwner);
                }
                else if (currentOwner != Faction.None)
                {
                    StrongholdOwnership.Remove(location);
                    Log(currentOwner, " lose control over ", location);
                }
                else if (newOwner != Faction.None)
                {
                    StrongholdOwnership.Add(location, newOwner);
                    Log(newOwner, " take control over ", location);
                }
            }
        }

        #endregion Contemplate

        #region Victory

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
            return
                MeetsBasicPinkVictoryCondition(p, contestedStongholdsCountAsOccupied) ||
                MeetsHighThresholdPinkVictoryCondition(p, contestedStongholdsCountAsOccupied) ||
                NumberOfVictoryPoints(p, contestedStongholdsCountAsOccupied) >= TresholdForWin(p);
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

        private IEnumerable<Territory> Strongholds => Map.Territories(false).Where(t => t.IsStronghold || IsSpecialStronghold(t));

        public bool MeetsBasicPinkVictoryCondition(Player p, bool contestedStongholdsCountAsOccupied) =>
            (p.Is(Faction.Pink) && p.HasAlly || p.Ally == Faction.Pink) &&
            Strongholds.Count(l => p.Controls(this, l, contestedStongholdsCountAsOccupied) && p.AlliedPlayer.Controls(this, l, contestedStongholdsCountAsOccupied)) >= 3;

        public bool MeetsHighThresholdPinkVictoryCondition(Player p, bool contestedStongholdsCountAsOccupied) =>
            (p.Is(Faction.Pink) && p.HasAlly || p.Ally == Faction.Pink) &&
            HasHighThreshold(Faction.Pink) &&
            Strongholds.Any(l => p.Controls(this, l, contestedStongholdsCountAsOccupied) && p.AlliedPlayer.Controls(this, l, contestedStongholdsCountAsOccupied)) &&
            HomeworldOccupation.Values.Count(f => f == p.Faction || f == p.Ally) >= 2;

        public int NumberOfVictoryPoints(Player p, bool contestedStongholdsCountAsOccupied)
        {
            var ally = GetPlayer(p.Ally);

            if (ally != null)
            {
                int techTokenPoint = p.TechTokens.Count == 3 || p.AlliedPlayer.TechTokens.Count == 3 ? 1 : 0;
                return techTokenPoint + (Map.Territories(false).Where(t => t.IsStronghold || IsSpecialStronghold(t)).Count(l => p.Controls(this, l, contestedStongholdsCountAsOccupied) || ally.Controls(this, l, contestedStongholdsCountAsOccupied)));
            }
            else
            {
                int techTokenPoint = p.TechTokens.Count == 3 ? 1 : 0;
                return techTokenPoint + NumberOfOccupiedStrongholds(p, contestedStongholdsCountAsOccupied);
            }
        }

        public int NumberOfOccupiedStrongholds(Player p, bool contestedStongholdsCountAsOccupied) => Map.Territories(false).Where(t => t.IsStronghold || IsSpecialStronghold(t)).Count(l => p.Controls(this, l, contestedStongholdsCountAsOccupied));

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

        #endregion Victory

        #region Information

        public bool HasStrongholdAdvantage(Faction f, StrongholdAdvantage advantage, Territory battleTerritory)
        {
            if (battleTerritory == Map.HiddenMobileStronghold.Territory && OwnsStronghold(f, Map.HiddenMobileStronghold) && ChosenHMSAdvantage == advantage)
            {
                return true;
            }

            return battleTerritory.Advantage == advantage && battleTerritory.Locations.Any(l => OwnsStronghold(f, l));
        }

        public bool OwnsStronghold(Faction f, Location stronghold)
        {
            return StrongholdOwnership.TryGetValue(stronghold, out Faction owner) && owner == f;
        }

        public bool IsSpecialStronghold(Territory t)
        {
            return t == Map.ShieldWall && Applicable(Rule.SSW) && NumberOfMonsters >= 4;
        }

        public IEnumerable<TerrorType> TerrorIn(Territory t) => TerrorOnPlanet.Where(kvp => kvp.Value == t).Select(kvp => kvp.Key);

        #endregion Information
    }
}
