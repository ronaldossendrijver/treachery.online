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
        #region AwaitingPlayers

        private void EnterPhaseAwaitingPlayers()
        {
            Players = new List<Player>();
            CurrentTurn = 0;
            Enter(Phase.AwaitingPlayers);
        }

        public List<Faction> FactionsInPlay { get; private set; }

        public void HandleEvent(EstablishPlayers e)
        {
            RecentMilestones.Add(Milestone.GameStarted);

            CurrentMainPhase = MainPhase.Setup;
            CurrentReport = new Report(MainPhase.Setup);

            CurrentReport.Express("Game started!");

            Seed = e.Seed;
            Name = e.GameName;
            Random = new Random(Seed);

            AllRules = e.ApplicableRules.ToList();
            Rules = e.ApplicableRules.Where(r => GetRuleGroup(r) != RuleGroup.Bots).ToList();
            RulesForBots = e.ApplicableRules.Where(r => GetRuleGroup(r) == RuleGroup.Bots).ToList();
            Rules.AddRange(GetRulesInGroup(RuleGroup.CoreBasic));

            Ruleset = DetermineApproximateRuleset(e.FactionsInPlay, Rules);
            CurrentReport.Express("Ruleset: ", Ruleset);
            var customRules = GetCustomRules().ToList();
            CurrentReport.ExpressIf(customRules.Any(), "House rules: ", customRules);

            if (Applicable(Rule.ExpansionTreacheryCards))
            {
                if (!Rules.Contains(Rule.ExpansionTreacheryCardsExceptPBandSSandAmal)) Rules.Add(Rule.ExpansionTreacheryCardsExceptPBandSSandAmal);
                if (!Rules.Contains(Rule.ExpansionTreacheryCardsPBandSS)) Rules.Add(Rule.ExpansionTreacheryCardsPBandSS);
                if (!Rules.Contains(Rule.ExpansionTreacheryCardsAmal)) Rules.Add(Rule.ExpansionTreacheryCardsAmal);
            }

            ResourceCardDeck = CreateAndShuffleResourceCardDeck();
            TreacheryDeck = TreacheryCardManager.CreateTreacheryDeck(this, Random);

            if (!Applicable(Rule.CustomDecks))
            {
                TreacheryDeck.Shuffle();
                RecentMilestones.Add(Milestone.Shuffled);
            }

            TreacheryDiscardPile = new Deck<TreacheryCard>(Random);
            ResourceCardDiscardPileA = new Deck<ResourceCard>(Random);
            ResourceCardDiscardPileB = new Deck<ResourceCard>(Random);

            OrangeAllyMayShipAsGuild = true;
            PurpleAllyMayReviveAsPurple = true;
            GreyAllyMayReplaceCards = true;
            RedWillPayForExtraRevival = 0;
            YellowWillProtectFromShaiHulud = true;
            YellowAllowsThreeFreeRevivals = true;
            YellowSharesPrescience = true;
            GreenSharesPrescience = true;
            BlueAllyMayUseVoice = true;
            WhiteAllyMayUseNoField = true;

            MaximumNumberOfTurns = e.MaximumTurns;
            MaximumNumberOfPlayers = e.MaximumNumberOfPlayers;

            CurrentReport.Express("The maximum number of turns is: ", MaximumNumberOfTurns);

            FactionsInPlay = e.FactionsInPlay;

            AddPlayersToGame(e);

            FillEmptySeatsWithBots();
            RemoveClaimedFactions();
            InitializeTimers();

            Enter(Applicable(Rule.PlayersChooseFactions), Phase.SelectingFactions, AssignFactionsAndEnterFactionTrade);
        }

        private void InitializeTimers()
        {
            foreach (var player in Players)
            {
                var playerTimers = new Dictionary<MainPhase, TimeSpan>();
                foreach (var mainphase in Enumerations.GetValuesExceptDefault(typeof(MainPhase), MainPhase.None))
                {
                    playerTimers.Add(mainphase, TimeSpan.Zero);
                }

                Timers.Add(player, playerTimers);
            }
        }

        private void RemoveClaimedFactions()
        {
            foreach (var f in Players.Where(p => p.Faction != Faction.None).Select(p => p.Faction))
            {
                FactionsInPlay.Remove(f);
            }
        }

        private Deck<ResourceCard> CreateAndShuffleResourceCardDeck()
        {
            var result = new Deck<ResourceCard>(Random);
            foreach (var c in Map.GetResourceCardsInAndOutsidePlay(Map).Where(c => !c.IsSandTrout || Applicable(Rule.SandTrout)))
            {
                result.PutOnTop(c);
            }

            RecentMilestones.Add(Milestone.Shuffled);
            result.Shuffle();
            return result;
        }

        private void AddPlayersToGame(EstablishPlayers e)
        {
            if (Version < 113)
            {
                AddBots();
            }

            foreach (var newPlayer in e.Players)
            {
                var p = new Player(this, newPlayer);
                Players.Add(p);
                CurrentReport.Express(p.Name, " joined the game");
            }
        }

        private void AddBots()
        {
            //Can be removed later, this was replaced by filling empty seats with bots.
            if (Applicable(Rule.OrangeBot)) Players.Add(new Player(this, UniquePlayerName("Edric*"), Faction.Orange, true));
            if (Applicable(Rule.RedBot)) Players.Add(new Player(this, UniquePlayerName("Shaddam IV*"), Faction.Red, true));
            if (Applicable(Rule.BlackBot)) Players.Add(new Player(this, UniquePlayerName("The Baron*"), Faction.Black, true));
            if (Applicable(Rule.PurpleBot)) Players.Add(new Player(this, UniquePlayerName("Scytale*"), Faction.Purple, true));
            if (Applicable(Rule.BlueBot)) Players.Add(new Player(this, UniquePlayerName("Mother Mohiam*"), Faction.Blue, true));
            if (Applicable(Rule.GreenBot)) Players.Add(new Player(this, UniquePlayerName("Paul Atreides*"), Faction.Green, true));
            if (Applicable(Rule.YellowBot)) Players.Add(new Player(this, UniquePlayerName("Liet Kynes*"), Faction.Yellow, true));
            if (Applicable(Rule.GreyBot)) Players.Add(new Player(this, UniquePlayerName("Prince Rhombur*"), Faction.Grey, true));
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
                if (Version <= 125)
                {
                    var available = new Deck<Faction>(FactionsInPlay.Where(f => !IsPlaying(f)), Random);
                    available.Shuffle();

                    while (Players.Count < MaximumNumberOfPlayers)
                    {
                        var bot = available.Draw() switch
                        {
                            Faction.Black => new Player(this, UniquePlayerName("The Baron*"), Faction.Black, true),
                            Faction.Blue => new Player(this, UniquePlayerName("Mother Mohiam*"), Faction.Blue, true),
                            Faction.Green => new Player(this, UniquePlayerName("Paul Atreides*"), Faction.Green, true),
                            Faction.Yellow => new Player(this, UniquePlayerName("Liet Kynes*"), Faction.Yellow, true),
                            Faction.Red => new Player(this, UniquePlayerName("Shaddam IV*"), Faction.Red, true),
                            Faction.Orange => new Player(this, UniquePlayerName("Edric*"), Faction.Orange, true),
                            Faction.Grey => new Player(this, UniquePlayerName("Prince Rhombur*"), Faction.Grey, true),
                            Faction.Purple => new Player(this, UniquePlayerName("Scytale*"), Faction.Purple, true),
                            Faction.Brown => new Player(this, UniquePlayerName("Brown*"), Faction.Brown, true),
                            Faction.White => new Player(this, UniquePlayerName("White*"), Faction.White, true),
                            Faction.Pink => new Player(this, UniquePlayerName("Pink*"), Faction.Pink, true),
                            Faction.Cyan => new Player(this, UniquePlayerName("Cyan*"), Faction.Cyan, true),
                            _ => new Player(this, UniquePlayerName("The Baron*"), Faction.Black, true)
                        };

                        Players.Add(bot);
                    }
                }
                else
                {
                    int botNr = 1;

                    while (Players.Count < MaximumNumberOfPlayers)
                    {
                        Players.Add(new Player(this, UniquePlayerName(string.Format("Bot{0}", botNr++)), Faction.None, true));
                    }
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
                CurrentReport.Express(e);
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

            DeterminePositionsAtTable();

            Enter(Applicable(Rule.CustomDecks) && Version < 134, Phase.CustomizingDecks, EnterPhaseTradingFactions);
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
            if (!IsPlaying(thisOffer.Target))
            {
                CurrentReport.Express(thisOffer.Initiator, " switch to ", thisOffer.Target);
                FactionsInPlay.Add(thisOffer.Initiator);
                thisOffer.Player.Faction = thisOffer.Target;

            }
            else
            {
                var match = CurrentTradeOffers.SingleOrDefault(matchingOffer => matchingOffer.Initiator == thisOffer.Target && matchingOffer.Target == thisOffer.Initiator);
                if (match != null)
                {
                    CurrentReport.Express(thisOffer.Initiator, " and ", match.Initiator, " traded factions");
                    var initiator = GetPlayer(thisOffer.Initiator);
                    var target = GetPlayer(thisOffer.Target);
                    (target.Faction, initiator.Faction) = (initiator.Faction, target.Faction);
                    FactionTradeOffered invalidOffer;
                    while ((invalidOffer = CurrentTradeOffers.FirstOrDefault(x => x.Initiator == thisOffer.Initiator || x.Initiator == thisOffer.Target)) != null)
                    {
                        CurrentTradeOffers.Remove(invalidOffer);
                    }
                }
                else
                {
                    CurrentReport.Express(thisOffer.GetMessage());
                    if (!CurrentTradeOffers.Any(o => o.Initiator == thisOffer.Initiator && o.Target == thisOffer.Target))
                    {
                        CurrentTradeOffers.Add(thisOffer);
                    }
                }
            }
        }

        #endregion TradingFactions

        #region SettingUp
        
        private void EstablishDecks()
        {
            if (IsPlaying(Faction.White))
            {
                WhiteCache = TreacheryCardManager.GetWhiteCards();
            }

            Enter(Applicable(Rule.CustomDecks) && Version >= 134, Phase.CustomizingDecks, EnterSetupPhase);
        }
        public void HandleEvent(CardsDetermined e)
        {
            TreacheryDeck = new Deck<TreacheryCard>(e.TreacheryCards, Random);
            TreacheryDeck.Shuffle();
            RecentMilestones.Add(Milestone.Shuffled);
            WhiteCache = new List<TreacheryCard>(e.WhiteCards);
            CurrentReport.Express(e.GetVerboseMessage());

            Enter(Version < 134, EnterPhaseTradingFactions, EnterSetupPhase);
        }

        private void EnterSetupPhase()
        {
            Enter(Applicable(Rule.CustomDecks) && Version >= 134, Phase.CustomizingDecks, EnterPhaseTradingFactions);

            CurrentTradeOffers.Clear();

            HasBattleWheel.Add(Players.OrderBy(p => p.PositionAtTable).First().Faction);
            HasBattleWheel.Add(Players.OrderBy(p => p.PositionAtTable).Last().Faction);

            foreach (var p in Players)
            {
                p.AssignLeaders(this);
            }

            Enter(IsPlaying(Faction.Blue), Phase.BluePredicting, TreacheryCardsBeforeTraitors, DealStartingTreacheryCards, DealTraitors);
        }

        private bool TreacheryCardsBeforeTraitors => Version >= 121 && Applicable(Rule.LeaderSkills);

        public void HandleEvent(BluePrediction e)
        {
            GetPlayer(e.Initiator).PredictedFaction = e.ToWin;
            GetPlayer(e.Initiator).PredictedTurn = e.Turn;
            CurrentReport.Express(e);
            Enter(TreacheryCardsBeforeTraitors, DealStartingTreacheryCards, DealTraitors);
        }

        private Deck<IHero> TraitorDeck { get; set; }
        private void DealTraitors()
        {
            RecentMilestones.Add(Milestone.Shuffled);
            TraitorDeck = CreateAndShuffleTraitorDeck(Random);

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

                //Leaders = LeaderManager.GetLeaders(Faction).Where(l => g.Applicable(Rule.BrownAuditor) || l.HeroType != HeroType.Auditor).ToList();

                var factionsInPlay = Players.Select(p => p.Faction);
                result.AddRange(LeaderManager.Leaders.Where(l => 
                    factionsInPlay.Contains(l.Faction) && 
                    (Version <= 140 || l.HeroType != HeroType.Auditor || Applicable(Rule.BrownAuditor))
                    ));

                if (Applicable(Rule.CheapHeroTraitor))
                {
                    result.Add(TreacheryCardManager.GetCardsInPlay(this).First(c => c.Type == TreacheryCardType.Mercenary));
                }

                return result;
            }
        }

        private Deck<IHero> CreateAndShuffleTraitorDeck(Random random)
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
                foreach (var p in Players.Where(p => p.Faction != Faction.Black && p.Faction != Faction.Purple))
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

            CurrentReport.Express(e);
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
            CurrentReport.Express(e);

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

            Enter(TreacheryCardsBeforeTraitors, SetupSpiceAndForces, AssignLeaderSkills);
        }

        private void AssignLeaderSkills()
        {
            if (Applicable(Rule.LeaderSkills))
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
                Enter(TreacheryCardsBeforeTraitors, DealTraitors, SetupSpiceAndForces);
            }
        }

        private Phase PhaseBeforeSkillAssignment;
        public void HandleEvent(SkillAssigned e)
        {

            CurrentReport.Express(e);
            SetSkill(e.Leader, e.Skill);
            e.Player.SkillsToChooseFrom.Remove(e.Skill);
            SetInFrontOfShield(e.Leader, true);
            SkillDeck.PutOnTop(e.Player.SkillsToChooseFrom);
            e.Player.SkillsToChooseFrom.Clear();

            if (!Players.Any(p => p.SkillsToChooseFrom.Any()))
            {
                SkillDeck.Shuffle();
                Enter(CurrentPhase != Phase.AssigningInitialSkills, PhaseBeforeSkillAssignment, TreacheryCardsBeforeTraitors, DealTraitors, SetupSpiceAndForces);
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

                if (TreacheryCardsBeforeTraitors)
                {
                    Enter(
                        IsPlaying(Faction.Yellow), Phase.YellowSettingUp,
                        IsPlaying(Faction.Blue) && Applicable(Rule.BlueFirstForceInAnyTerritory), Phase.BlueSettingUp,
                        EnterStormPhase);
                }
                else
                {
                    Enter(
                        IsPlaying(Faction.Yellow), Phase.YellowSettingUp,
                        IsPlaying(Faction.Blue) && Applicable(Rule.BlueFirstForceInAnyTerritory), Phase.BlueSettingUp,
                        DealStartingTreacheryCards);
                }
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

            CurrentReport.Express(faction, " initial positions set, starting with ", Payment(e.Resources));
            HasActedOrPassed.Add(faction);

            if (Players.Count == HasActedOrPassed.Count)
            {
                Enter(TreacheryCardsBeforeTraitors, EnterStormPhase, DealStartingTreacheryCards);
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

            CurrentReport.Express(e);
            Enter(IsPlaying(Faction.Blue) && Applicable(Rule.BlueFirstForceInAnyTerritory), Phase.BlueSettingUp, TreacheryCardsBeforeTraitors, EnterStormPhase, DealStartingTreacheryCards);
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

            CurrentReport.Express(e);
            Enter(TreacheryCardsBeforeTraitors, EnterStormPhase, DealStartingTreacheryCards);
        }

        public Deck<TreacheryCard> StartingTreacheryCards;
        private TreacheryCard ExtraStartingCardForBlack = null;

        private void DealStartingTreacheryCards()
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
            CurrentReport.Express(e);
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
                CurrentReport.ExpressTo(p.Faction, "Your starting treachery card is: ", card);

                if (p.Is(Faction.Black))
                {
                    p.TreacheryCards.Add(ExtraStartingCardForBlack);
                    CurrentReport.ExpressTo(Faction.Black, "Your extra card is: ", ExtraStartingCardForBlack);
                }
            }

            Enter(TreacheryCardsBeforeTraitors, AssignLeaderSkills, EnterStormPhase);
        }

        #endregion SettingUp
    }
}
