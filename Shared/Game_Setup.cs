/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        #region AwaitingPlayers

        private void EnterPhaseAwaitingPlayers()
        {
            Players = new List<Player>();
            CurrentTurn = 0;
            Enter(Phase.AwaitingPlayers);
        }

        public List<Faction> FactionsInPlay;

        public void HandleEvent(EstablishPlayers e)
        {
            CurrentMainPhase = MainPhase.Setup;
            CurrentReport = new Report(MainPhase.Setup);

            Seed = e.Seed;
            Name = e.GameName;
            Random = new Random(Seed);

            AllRules = e.ApplicableRules;
            Rules = e.ApplicableRules.Where(r => GetRuleGroup(r) != RuleGroup.Bots).ToArray();
            RulesForBots = e.ApplicableRules.Where(r => GetRuleGroup(r) == RuleGroup.Bots).ToArray();

            var usedRuleset = Ruleset;
            CurrentReport.Add("Ruleset: {0}.",
                usedRuleset == Ruleset.Custom ?
                string.Format("Custom ({0})", string.Join(", ", Rules.Select(r => Skin.Current.Describe(r)))) :
                Skin.Current.Describe(usedRuleset));

            ResourceCardDeck = CreateAndShuffleResourceCardDeck();
            TreacheryDeck = TreacheryCardManager.CreateAndShuffleTreacheryDeck(this, Random);
            TreacheryDiscardPile = new Deck<TreacheryCard>(Random);
            ResourceCardDiscardPileA = new Deck<ResourceCard>(Random);
            ResourceCardDiscardPileB = new Deck<ResourceCard>(Random);

            OrangeAllyMayShipAsGuild = true;
            PurpleAllyMayReviveAsPurple = true;
            GreyAllyMayReplaceCards = true;
            RedWillPayForExtraRevival = 0;
            YellowWillProtectFromShaiHulud = true;
            YellowAllowsThreeFreeRevivals = true;
            YellowSharesPrescience = Version > 78;
            GreenSharesPrescience = Version > 78;
            BlueAllyMayUseVoice = Version > 78;

            MaximumNumberOfTurns = e.MaximumTurns;
            MaximumNumberOfPlayers = e.MaximumNumberOfPlayers;

            CurrentReport.Add("The maximum number of turns is: {0}.", MaximumNumberOfTurns);

            FactionsInPlay = e.FactionsInPlay;

            AddPlayersToGame(e);
            RecentMilestones.Add(Milestone.GameStarted);

            RemoveClaimedFactions();

            Enter(Applicable(Rule.PlayersChooseFactions), Phase.SelectingFactions, AssignFactionsAndEnterFactionTrade);
        }

        private void RemoveClaimedFactions()
        {
            foreach (var f in Players.Where(p => p.Faction != Faction.None).Select(p => p.Faction))
            {
                FactionsInPlay.Remove(f);
            }
        }

        public Deck<ResourceCard> CreateAndShuffleResourceCardDeck()
        {
            var result = new Deck<ResourceCard>(Random);
            foreach (var c in Map.GetResourceCardsInAndOutsidePlay(Map).Where(c => !c.IsSandTrout || Applicable(Rule.GreyAndPurpleExpansionSandTrout)))
            {
                result.PutOnTop(c);
            }

            RecentMilestones.Add(Milestone.Shuffled);
            result.Shuffle();
            return result;
        }

        private void AddPlayersToGame(EstablishPlayers e)
        {
            AddBots();

            foreach (var newPlayer in e.Players)
            {
                var p = new Player(this, newPlayer);
                Players.Add(p);
                CurrentReport.Add("{0} joined the game.", p.Name);
            }

            FillEmptySeatsWithBots();
        }

        private void AddBots()
        {
            if (Applicable(Rule.OrangeBot)) Players.Add(new Player(this, UniquePlayerName("Edric*"), Faction.Orange, true));
            if (Applicable(Rule.RedBot)) Players.Add(new Player(this, UniquePlayerName("Shaddam IV*"), Faction.Red, true));
            if (Applicable(Rule.BlackBot)) Players.Add(new Player(this, UniquePlayerName("The Baron*"), Faction.Black, true));
            if (Applicable(Rule.PurpleBot)) Players.Add(new Player(this, UniquePlayerName("Scytale*"), Faction.Purple, true));
            if (Applicable(Rule.BlueBot)) Players.Add(new Player(this, UniquePlayerName("Mother Mohiam*"), Faction.Blue, true));
            if (Applicable(Rule.GreenBot)) Players.Add(new Player(this, UniquePlayerName("Paul Atreides*"), Faction.Green, true));
            if (Applicable(Rule.YellowBot)) Players.Add(new Player(this, UniquePlayerName("Liet Kynes*"), Faction.Yellow, true));
            if (Applicable(Rule.GreyBot)) Players.Add(new Player(this, UniquePlayerName("Prince Rhombur*"), Faction.Grey, true));
            //if (Applicable(Rule.BrownBot)) Players.Add(new Player(this, UniquePlayerName("Brown*"), Faction.Brown, true));
            //if (Applicable(Rule.WhiteBot)) Players.Add(new Player(this, UniquePlayerName("White*"), Faction.White, true));
        }

        private string UniquePlayerName(string name)
        {
            var result = name;
            while (Players.Any(p => p.Name == result))
            {
                result += "'";
            }
            return result;
        }

        private void FillEmptySeatsWithBots()
        {
            if (Applicable(Rule.FillWithBots))
            {
                var available = new Deck<Faction>(FactionsInPlay.Where(f => Version <= 95 || !IsPlaying(f)), Random);
                available.Shuffle();

                while (Players.Count < MaximumNumberOfPlayers)
                {
                    var bot = available.Draw() switch
                    {
                        Faction.Black => new Player(this, "The Baron*", Faction.Black, true),
                        Faction.Blue => new Player(this, "Mother Mohiam*", Faction.Blue, true),
                        Faction.Green => new Player(this, "Paul Atreides*", Faction.Green, true),
                        Faction.Yellow => new Player(this, "Liet Kynes*", Faction.Yellow, true),
                        Faction.Red => new Player(this, "Shaddam IV*", Faction.Red, true),
                        Faction.Orange => new Player(this, "Edric*", Faction.Orange, true),
                        Faction.Grey => new Player(this, "Prince Rhombur*", Faction.Grey, true),
                        Faction.Purple => new Player(this, "Scytale*", Faction.Purple, true),
                        Faction.Brown => new Player(this, "Brown*", Faction.Brown, true),
                        Faction.White => new Player(this, "White*", Faction.White, true),
                        Faction.Pink => new Player(this, "Pink*", Faction.Pink, true),
                        Faction.Cyan => new Player(this, "Cyan*", Faction.Cyan, true),
                        _ => new Player(this, "?*", Faction.Black, true)
                    };

                    Players.Add(bot);
                }
            }
        }

        public void HandleEvent(FactionSelected e)
        {
            var initiator = Players.FirstOrDefault(p => p.Name == e.InitiatorPlayerName);
            if (initiator != null && FactionsInPlay.Contains(e.Faction))
            {
                initiator.Faction = e.Faction;
                FactionsInPlay.Remove(e.Faction);
                CurrentReport.Add(e);
            }
        }

        private void AssignFactionsAndEnterFactionTrade()
        {
            var inPlay = new Deck<Faction>(FactionsInPlay, Random);
            RecentMilestones.Add(Milestone.Shuffled);
            inPlay.Shuffle();

            foreach (var p in Players.Where(p => p.Faction == Faction.None))
            {
                p.Faction = inPlay.Draw();
            }

            if (IsPlaying(Faction.White))
            {
                WhiteCache = TreacheryCardManager.GetWhiteCards(this);
            }

            DeterminePositionsAtTable();
            BidSequence = new PlayerSequence(this, Players);
            ShipmentAndMoveSequence = new PlayerSequence(this, Players);
            BattleSequence = new PlayerSequence(this, Players);
            CheckWinSequence = new PlayerSequence(this, Players);
            TechTokenSequence = new PlayerSequence(this, Players);
            CurrentReport.Add("Game started.");
            EnterPhaseTradingFactions();
        }

        private void DeterminePositionsAtTable()
        {
            if (Players.Count <= MaximumNumberOfPlayers)
            {
                var positions = new Deck<int>(Random);
                for (int i = 0; i < MaximumNumberOfPlayers; i++)
                {
                    positions.PutOnTop(i);
                }
                positions.Shuffle();

                foreach (var p in Players)
                {
                    p.PositionAtTable = positions.Draw();
                }
            }
            else
            {
                throw new ArgumentException("Number of players cannot exceed number of positions at the table.");
            }
        }
        #endregion

        #region TradingFactions
        public readonly IList<FactionTradeOffered> CurrentTradeOffers = new List<FactionTradeOffered>();

        private void EnterPhaseTradingFactions()
        {
            CurrentTradeOffers.Clear();
            Enter(Phase.TradingFactions);
        }

        public void HandleEvent(FactionTradeOffered thisOffer)
        {
            var match = CurrentTradeOffers.SingleOrDefault(matchingOffer => matchingOffer.Initiator == thisOffer.Target && matchingOffer.Target == thisOffer.Initiator);
            if (match != null)
            {
                CurrentReport.Add("{0} and {1} traded factions.", thisOffer.Initiator, match.Initiator);
                var initiator = GetPlayer(thisOffer.Initiator);
                var target = GetPlayer(thisOffer.Target);
                var initiatorsFaction = initiator.Faction;
                initiator.Faction = target.Faction;
                target.Faction = initiatorsFaction;

                FactionTradeOffered invalidOffer;
                while ((invalidOffer = CurrentTradeOffers.FirstOrDefault(x => x.Initiator == thisOffer.Initiator || x.Initiator == thisOffer.Target)) != null)
                {
                    CurrentTradeOffers.Remove(invalidOffer);
                }
            }
            else
            {
                CurrentReport.Add(thisOffer.GetMessage());
                if (!CurrentTradeOffers.Any(o => o.Initiator == thisOffer.Initiator && o.Target == thisOffer.Target))
                {
                    CurrentTradeOffers.Add(thisOffer);
                }
            }
        }

        #endregion TradingFactions

        #region SettingUp

        private void EnterSetupPhase()
        {
            CurrentTradeOffers.Clear();

            HasBattleWheel.Add(Players.OrderBy(p => p.PositionAtTable).First().Faction);
            HasBattleWheel.Add(Players.OrderBy(p => p.PositionAtTable).Last().Faction);

            foreach (var p in Players)
            {
                p.AssignLeaders(this);
            }

            Enter(IsPlaying(Faction.Blue), Phase.BluePredicting, DealTraitorCards);
        }

        public void HandleEvent(BluePrediction e)
        {
            GetPlayer(e.Initiator).PredictedFaction = e.ToWin;
            GetPlayer(e.Initiator).PredictedTurn = e.Turn;
            CurrentReport.Add(e);
            DealTraitorCards();
        }

        private Deck<IHero> TraitorDeck { get; set; }
        private void DealTraitorCards()
        {
            RecentMilestones.Add(Milestone.Shuffled);
            TraitorDeck = CreateAndShuffleTraitorDeck(Players.Select(p => p.Faction), Random);

            if (Applicable(Rule.BlackMulligan) && IsPlaying(Faction.Black))
            {
                DealBlackTraitorCards();
                Enter(Phase.BlackMulligan);
            }
            else
            {
                for (int i = 1; i <= 4; i++)
                {
                    foreach (var p in Players.Where(p => p.Faction != Faction.Purple))
                    {
                        p.Traitors.Add(TraitorDeck.Draw());
                    }
                }

                EnterSelectTraitors();
            }
        }
        public IEnumerable<IHero> TraitorsInPlay
        {
            get
            {
                var result = new List<IHero>();

                result.AddRange(LeaderManager.Leaders.Where(l => Players.Select(p => p.Faction).Any(f => f == l.Faction)));

                if (Applicable(Rule.GreyAndPurpleExpansionCheapHeroTraitor))
                {
                    result.Add(TreacheryCardManager.GetCardsInPlay(this).First(c => c.Type == TreacheryCardType.Mercenary));
                }

                return result;
            }
        }

        private Deck<IHero> CreateAndShuffleTraitorDeck(IEnumerable<Faction> factions, Random random)
        {
            var result = new Deck<IHero>(TraitorsInPlay, random);
            result.Shuffle();
            return result;
        }

        private void DealBlackTraitorCards()
        {
            var black = GetPlayer(Faction.Black);
            for (int i = 1; i <= 4; i++)
            {
                black.Traitors.Add(TraitorDeck.Draw());
            }
        }

        private void DealNonBlackTraitorCards()
        {
            for (int i = 1; i <= 4; i++)
            {
                foreach (var p in Players.Where(p => p.Faction != Faction.Black && (Version <= 90 || p.Faction != Faction.Purple)))
                {
                    p.Traitors.Add(TraitorDeck.Draw());
                }
            }
        }

        public void HandleEvent(MulliganPerformed e)
        {
            if (!e.Passed)
            {
                var initiator = GetPlayer(e.Initiator);
                TraitorDeck.Items.AddRange(initiator.Traitors);
                initiator.Traitors.Clear();
                TraitorDeck.Shuffle();
                RecentMilestones.Add(Milestone.Shuffled);
                DealBlackTraitorCards();
                Enter(Phase.BlackMulligan);
            }
            else
            {
                DealNonBlackTraitorCards();
                EnterSelectTraitors();
            }

            CurrentReport.Add(e);
        }

        private void EnterSelectTraitors()
        {
            HasActedOrPassed.Clear();

            if (IsPlaying(Faction.Black))
            {
                HasActedOrPassed.Add(Faction.Black);
            }

            if (IsPlaying(Faction.Purple))
            {
                HasActedOrPassed.Add(Faction.Purple);
            }

            Enter(Players.Any(p => !(p.Is(Faction.Black) || p.Is(Faction.Purple))), Phase.SelectingTraitors, AssignFaceDancers);
        }

        public void HandleEvent(TraitorsSelected e)
        {
            var initiator = GetPlayer(e.Initiator);
            var toRemove = initiator.Traitors.Where(l => !l.Equals(e.SelectedTraitor)).ToList();

            foreach (var l in toRemove)
            {
                TraitorDeck.Items.Add(l);
                initiator.Traitors.Remove(l);
                initiator.DiscardedTraitors.Add(l);
                initiator.KnownNonTraitors.Add(l);
            }

            HasActedOrPassed.Add(e.Initiator);
            CurrentReport.Add(e);

            if (EveryoneActedOrPassed)
            {
                AssignFaceDancers();
            }
        }

        private void AssignFaceDancers()
        {
            var purple = GetPlayer(Faction.Purple);
            if (purple != null)
            {
                TraitorDeck.Shuffle();
                for (int i = 0; i < 3; i++)
                {
                    var leader = TraitorDeck.Draw();
                    purple.FaceDancers.Add(leader);
                    purple.KnownNonTraitors.Add(leader);
                }
            }

            AssignLeaderSkills();
        }

        private void AssignLeaderSkills()
        {
            if (Applicable(Rule.BrownAndWhiteLeaderSkills))
            {
                SkillDeck = new Deck<LeaderSkill>(Enumerations.GetValuesExceptDefault(typeof(LeaderSkill), LeaderSkill.None), Random);
                SkillDeck.Shuffle();
                RecentMilestones.Add(Milestone.Shuffled);

                int nrOfSkillsToAssign = Players.Count <= 7 ? 2 : 1;
                for (int i = 0; i < nrOfSkillsToAssign; i++)
                {
                    foreach (var p in Players)
                    {
                        p.SkillsToChooseFrom.Add(SkillDeck.Draw());
                    }
                }

                Enter(Phase.AssigningInitialSkills);
            }
            else
            {
                SetupSpiceAndForces();
            }
        }

        private Phase PhaseBeforeSkillAssignment;
        public void HandleEvent(SkillAssigned e)
        {

            CurrentReport.Add(e);
            SetSkill(e.Leader, e.Skill);
            e.Player.SkillsToChooseFrom.Remove(e.Skill);
            SetInFrontOfShield(e.Leader, true);
            SkillDeck.PutOnTop(e.Player.SkillsToChooseFrom);
            e.Player.SkillsToChooseFrom.Clear();

            if (!Players.Any(p => p.SkillsToChooseFrom.Any()))
            {
                SkillDeck.Shuffle();
                Enter(CurrentPhase != Phase.AssigningInitialSkills, PhaseBeforeSkillAssignment, SetupSpiceAndForces);
            }
        }

        private void SetupSpiceAndForces()
        {
            if (Applicable(Rule.CustomInitialForcesAndResources))
            {
                foreach (var p in Players)
                {
                    SetupPlayerForcesInReserveOnly(p);
                }

                HasActedOrPassed.Clear();
                Enter(Phase.PerformCustomSetup);
            }
            else
            {
                foreach (var p in Players)
                {
                    SetupPlayerSpiceAndForces(p);
                }

                Enter(
                    IsPlaying(Faction.Yellow), Phase.YellowSettingUp,
                    IsPlaying(Faction.Blue) && Applicable(Rule.BlueFirstForceInAnyTerritory), Phase.BlueSettingUp,
                    DrawStartingTreacheryCards);
            }
        }

        private void SetupPlayerForcesInReserveOnly(Player p)
        {
            switch (p.Faction)
            {
                case Faction.Yellow:
                    p.ForcesInReserve = Applicable(Rule.YellowSpecialForces) ? 17 : 20;
                    p.SpecialForcesInReserve = Applicable(Rule.YellowSpecialForces) ? 3 : 0;
                    break;
                case Faction.Green:
                    p.ForcesInReserve = 20;
                    break;
                case Faction.Black:
                    p.ForcesInReserve = 20;
                    break;
                case Faction.Red:
                    p.ForcesInReserve = Applicable(Rule.RedSpecialForces) ? 15 : 20; ;
                    p.SpecialForcesInReserve = Applicable(Rule.RedSpecialForces) ? 5 : 0;
                    break;
                case Faction.Orange:
                    p.ForcesInReserve = 20;
                    break;
                case Faction.Blue:
                    p.ForcesInReserve = 20;
                    break;
                case Faction.Grey:
                    p.ForcesInReserve = 13;
                    p.SpecialForcesInReserve = 7;
                    break;
                case Faction.Purple:
                    p.ForcesInReserve = 20;
                    break;

                case Faction.Brown:
                    p.ForcesInReserve = 20;
                    break;
                case Faction.White:
                    p.ForcesInReserve = 20;
                    break;

                case Faction.Pink:
                    p.ForcesInReserve = 20;
                    break;
                case Faction.Cyan:
                    p.ForcesInReserve = 20;
                    break;

            }
        }

        private void SetupPlayerSpiceAndForces(Player p)
        {
            switch (p.Faction)
            {
                case Faction.Yellow:
                    p.Resources = 3;
                    p.ForcesInReserve = Applicable(Rule.YellowSpecialForces) ? 17 : 20;
                    p.SpecialForcesInReserve = Applicable(Rule.YellowSpecialForces) ? 3 : 0;
                    break;
                case Faction.Green:
                    p.Resources = 10;
                    p.ChangeForces(Map.Arrakeen, 10);
                    p.ForcesInReserve = 10;
                    break;
                case Faction.Black:
                    p.Resources = 10;
                    p.ChangeForces(Map.Carthag, 10);
                    p.ForcesInReserve = 10;
                    break;
                case Faction.Red:
                    p.Resources = 10;
                    p.ForcesInReserve = Applicable(Rule.RedSpecialForces) ? 15 : 20;
                    p.SpecialForcesInReserve = Applicable(Rule.RedSpecialForces) ? 5 : 0;
                    break;
                case Faction.Orange:
                    p.Resources = 5;
                    p.ChangeForces(Map.TueksSietch, 5);
                    p.ForcesInReserve = 15;
                    break;
                case Faction.Blue:
                    p.Resources = 5;
                    if (Applicable(Rule.BlueFirstForceInAnyTerritory))
                    {
                        p.ForcesInReserve = 20;
                    }
                    else
                    {
                        p.ChangeForces(Map.PolarSink, 1);
                        p.ForcesInReserve = 19;
                    }
                    break;
                case Faction.Grey:
                    p.Resources = 10;
                    p.ForcesInReserve = 10;
                    p.SpecialForcesInReserve = 4;
                    p.ChangeForces(Map.HiddenMobileStronghold, 3);
                    p.ChangeSpecialForces(Map.HiddenMobileStronghold, 3);
                    break;
                case Faction.Purple:
                    p.Resources = 5;
                    p.ForcesInReserve = 20;
                    break;

                case Faction.Brown:
                    p.Resources = 2;
                    p.ForcesInReserve = 20;
                    break;

                case Faction.White:
                    p.Resources = 5;
                    p.ForcesInReserve = 20;
                    break;

                case Faction.Pink:
                    p.Resources = 13;
                    p.ForcesInReserve = 17;
                    p.ChangeForces(Map.ImperialBasin.MiddleLocation, 3);
                    break;

                case Faction.Cyan:
                    p.Resources = 12;
                    p.ForcesInReserve = 20;
                    break;

            }
        }

        public Faction NextFactionToPerformCustomSetup
        {
            get
            {
                return Players.Select(p => p.Faction).Where(f => !HasActedOrPassed.Contains(f)).FirstOrDefault();
            }
        }

        public void HandleEvent(PerformSetup e)
        {
            var faction = NextFactionToPerformCustomSetup;
            var player = GetPlayer(faction);

            foreach (var fl in e.ForceLocations)
            {
                var location = fl.Key;
                player.ShipForces(location, fl.Value.AmountOfForces);
                player.ShipSpecialForces(location, fl.Value.AmountOfSpecialForces);
            }

            player.Resources = e.Resources;

            CurrentReport.Add(faction, "{0} initial positions and {1} ({2}) determined.", faction, Concept.Resource, e.Resources);
            HasActedOrPassed.Add(faction);

            if (Players.Count == HasActedOrPassed.Count)
            {
                DrawStartingTreacheryCards();
            }
        }

        public void HandleEvent(PerformYellowSetup e)
        {
            var initiator = GetPlayer(e.Initiator);

            foreach (var fl in e.ForceLocations)
            {
                var location = fl.Key;
                initiator.ShipForces(location, fl.Value.AmountOfForces);
                initiator.ShipSpecialForces(location, fl.Value.AmountOfSpecialForces);
            }

            CurrentReport.Add(e);
            Enter(IsPlaying(Faction.Blue) && Applicable(Rule.BlueFirstForceInAnyTerritory), Phase.BlueSettingUp, DrawStartingTreacheryCards);
        }

        public void HandleEvent(PerformBluePlacement e)
        {
            var player = GetPlayer(e.Initiator);
            if (IsOccupied(e.Target))
            {
                player.ShipAdvisors(e.Target, 1);
            }
            else
            {
                player.ShipForces(e.Target, 1);
            }

            CurrentReport.Add(e);
            DrawStartingTreacheryCards();
        }

        private void FlipBeneGesseritWhenAlone()
        {
            var bg = GetPlayer(Faction.Blue);
            if (bg != null)
            {
                var territoriesWhereAdvisorsAreAlone = Map.Territories.Where(t => bg.SpecialForcesIn(t) > 0 && !Players.Any(p => p.Faction != Faction.Blue && p.AnyForcesIn(t) > 0));
                foreach (var t in territoriesWhereAdvisorsAreAlone)
                {
                    bg.FlipForces(t, false);
                    CurrentReport.Add(Faction.Blue, "{0} are alone and flip to fighters in {1}", Faction.Blue, t);
                }
            }
        }

        public Deck<TreacheryCard> StartingTreacheryCards;
        private TreacheryCard ExtraStartingCardForBlack = null;

        private void DrawStartingTreacheryCards()
        {
            StartingTreacheryCards = new Deck<TreacheryCard>(Random);
            foreach (var p in Players)
            {
                StartingTreacheryCards.Items.Add(TreacheryDeck.Draw());

                if (p.Is(Faction.Black))
                {
                    ExtraStartingCardForBlack = TreacheryDeck.Draw();
                }
            }

            Enter(IsPlaying(Faction.Grey), Phase.GreySelectingCard, DealRemainingStartingTreacheryCardsToNonGrey);
        }

        public void HandleEvent(GreySelectedStartingCard e)
        {
            GetPlayer(e.Initiator).TreacheryCards.Add(e.Card);
            StartingTreacheryCards.Items.Remove(e.Card);
            CurrentReport.Add(e);
            StartingTreacheryCards.Shuffle();
            RecentMilestones.Add(Milestone.Shuffled);
            DealRemainingStartingTreacheryCardsToNonGrey();
        }

        private void DealRemainingStartingTreacheryCardsToNonGrey()
        {
            foreach (var p in Players.Where(p => p.Faction != Faction.Grey))
            {
                var card = StartingTreacheryCards.Draw();
                p.TreacheryCards.Add(card);
                CurrentReport.Add(Faction.None, p.Faction, "You were dealt a starting treachery card.");

                if (p.Is(Faction.Black))
                {
                    p.TreacheryCards.Add(ExtraStartingCardForBlack);
                    CurrentReport.Add(Faction.None, Faction.Black, "Your extra card is: {0}.", ExtraStartingCardForBlack);
                }
            }

            EnterStormPhase();
        }

        #endregion SettingUp
    }
}
