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
        #region AwaitingPlayers

        private void EnterPhaseAwaitingPlayers()
        {
            Players = new List<Player>();
            CurrentTurn = 0;
            Enter(Phase.AwaitingPlayers);
        }

        public List<Faction> FactionsInPlay { get; internal set; }

        

        public List<TerrorType> UnplacedTerrorTokens { get; internal set; } = new();
        

        

        

        


        internal void AssignFactionsAndEnterFactionTrade()
        {
            var inPlay = new Deck<Faction>(FactionsInPlay, Random);
            Stone(Milestone.Shuffled);
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

        internal void EnterPhaseTradingFactions()
        {
            CurrentTradeOffers.Clear();
            Enter(Phase.TradingFactions);
        }

        public void HandleEvent(FactionTradeOffered thisOffer)
        {
            if (!IsPlaying(thisOffer.Target))
            {
                Log(thisOffer.Initiator, " switch to ", thisOffer.Target);
                FactionsInPlay.Add(thisOffer.Initiator);
                thisOffer.Player.Faction = thisOffer.Target;

            }
            else
            {
                var match = CurrentTradeOffers.SingleOrDefault(matchingOffer => matchingOffer.Initiator == thisOffer.Target && matchingOffer.Target == thisOffer.Initiator);
                if (match != null)
                {
                    Log(thisOffer.Initiator, " and ", match.Initiator, " traded factions");
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
                    Log(thisOffer.GetMessage());
                    if (!CurrentTradeOffers.Any(o => o.Initiator == thisOffer.Initiator && o.Target == thisOffer.Target))
                    {
                        CurrentTradeOffers.Add(thisOffer);
                    }
                }
            }
        }

        #endregion TradingFactions

        #region SettingUp

        

        internal void EnterSetupPhase()
        {
            Enter(Applicable(Rule.CustomDecks) && Version >= 134, Phase.CustomizingDecks, EnterPhaseTradingFactions);

            CurrentTradeOffers.Clear();

            HasBattleWheel.Add(Players.OrderBy(p => p.PositionAtTable).First().Faction);
            HasBattleWheel.Add(Players.OrderBy(p => p.PositionAtTable).Last().Faction);

            foreach (var p in Players)
            {
                p.AssignLeaders(this);
            }

            var pink = GetPlayer(Faction.Pink);
            if (pink != null)
            {
                AssignInitialAmbassadors(pink);
                Log(Faction.Pink, " get ", Ambassador.Pink, " and draw 4 random ambassadors");

                if (Applicable(Rule.PinkLoyalty))
                {
                    PinkLoyalLeader = pink.Leaders.RandomOrDefault(Random);
                    Log(PinkLoyalLeader, " is forever loyal to ", Faction.Pink);
                }
            }

            Enter(IsPlaying(Faction.Blue), Phase.BluePredicting, TreacheryCardsBeforeTraitors, DealStartingTreacheryCards, DealTraitors);
        }

        internal bool TreacheryCardsBeforeTraitors => Version >= 121 && Applicable(Rule.LeaderSkills);

        internal Deck<IHero> TraitorDeck { get; set; }

        internal void DealTraitors()
        {
            Stone(Milestone.Shuffled);
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

                var factionsInPlay = Players.Select(p => p.Faction);
                result.AddRange(LeaderManager.Leaders.Where(l =>
                    factionsInPlay.Contains(l.Faction) &&
                    l.HeroType != HeroType.Vidal &&
                    (Version <= 140 || l.HeroType != HeroType.Auditor || Applicable(Rule.BrownAuditor))
                    ));

                if (Applicable(Rule.CheapHeroTraitor))
                {
                    result.Add(TreacheryCardManager.GetCardsInPlay(this).First(c => c.Type == TreacheryCardType.Mercenary));
                }

                return result;
            }
        }

        public Leader PinkLoyalLeader { get; private set; }
        private Deck<IHero> CreateAndShuffleTraitorDeck(Random random)
        {
            var result = new Deck<IHero>(TraitorsInPlay, random);

            if (PinkLoyalLeader != null)
            {
                result.Items.Remove(PinkLoyalLeader);
                foreach (var p in Players)
                {
                    p.KnownNonTraitors.Add(PinkLoyalLeader);
                }
            }

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
                Stone(Milestone.Shuffled);
                DealBlackTraitorCards();
                Enter(Phase.BlackMulligan);
            }
            else
            {
                DealNonBlackTraitorCards();
                EnterSelectTraitors();
            }

            Log(e);
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
            Log(e);

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
                Stone(Milestone.Shuffled);

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

        private Phase PhaseBeforeSkillAssignment { get; set; }
        public void HandleEvent(SkillAssigned e)
        {

            Log(e);
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
            foreach (var p in Players)
            {
                SetupPlayerHomeworld(p);
            }

            if (Applicable(Rule.CustomInitialForcesAndResources))
            {
                HasActedOrPassed.Clear();
                Enter(Phase.PerformCustomSetup);
            }
            else
            {
                foreach (var p in Players)
                {
                    SetupPlayerSpiceAndForcesOnPlanet(p);
                }

                Action methodAfterSettingUp;
                if (TreacheryCardsBeforeTraitors)
                {
                    methodAfterSettingUp = EnterStormPhase;
                }
                else
                {
                    methodAfterSettingUp = DealStartingTreacheryCards;
                }

                Enter(
                    IsPlaying(Faction.Yellow), Phase.YellowSettingUp,
                    IsPlaying(Faction.Blue) && PerformBluePlacement.BlueMayPlaceFirstForceInAnyTerritory(this), Phase.BlueSettingUp,
                    IsPlaying(Faction.Cyan), Phase.CyanSettingUp,
                    methodAfterSettingUp);
            }
        }

        private void SetupPlayerHomeworld(Player p)
        {
            var normalForceWorld = Map.Homeworlds.First(w => w.Faction == p.Faction && w.IsHomeOfNormalForces);

            switch (p.Faction)
            {
                case Faction.Yellow:
                    p.InitializeHomeworld(normalForceWorld, Applicable(Rule.YellowSpecialForces) ? 17 : 20, Applicable(Rule.YellowSpecialForces) ? 3 : 0);
                    break;

                case Faction.Red:
                    p.InitializeHomeworld(normalForceWorld, Applicable(Rule.RedSpecialForces) ? 15 : 20, 0);
                    if (Applicable(Rule.RedSpecialForces)) p.InitializeHomeworld(Map.Homeworlds.First(w => w.Faction == p.Faction && w.IsHomeOfSpecialForces), 0, 5);
                    break;

                case Faction.Grey:
                    p.InitializeHomeworld(normalForceWorld, 13, 7);
                    break;

                case Faction.Green:
                case Faction.Black:
                case Faction.Orange:
                case Faction.Blue:
                case Faction.Purple:
                case Faction.Brown:
                case Faction.White:
                case Faction.Pink:
                case Faction.Cyan:
                    p.InitializeHomeworld(normalForceWorld, 20, 0);
                    break;
            }
        }

        private void SetupPlayerSpiceAndForcesOnPlanet(Player p)
        {
            switch (p.Faction)
            {
                case Faction.Yellow:
                    p.Resources = 3;
                    break;

                case Faction.Green:
                    p.Resources = 10;
                    p.AddForces(Map.Arrakeen, 10, true);
                    break;

                case Faction.Black:
                    p.Resources = 10;
                    p.AddForces(Map.Carthag, 10, true);
                    break;

                case Faction.Red:
                    p.Resources = 10;
                    break;

                case Faction.Orange:
                    p.Resources = 5;
                    p.AddForces(Map.TueksSietch, 5, true);
                    break;

                case Faction.Blue:
                    p.Resources = 5;
                    if (!PerformBluePlacement.BlueMayPlaceFirstForceInAnyTerritory(this))
                    {
                        p.AddForces(Map.PolarSink, 1, true);
                    }
                    break;

                case Faction.Grey:
                    p.Resources = 10;
                    p.AddForces(Map.HiddenMobileStronghold, 3, true);
                    p.AddSpecialForces(Map.HiddenMobileStronghold, 3, true);
                    break;

                case Faction.Purple:
                    p.Resources = 5;
                    break;

                case Faction.Brown:
                    p.Resources = 2;
                    break;

                case Faction.White:
                    p.Resources = 5;
                    break;

                case Faction.Pink:
                    p.Resources = 12;
                    p.AddForces(Map.ImperialBasin.MiddleLocation, 6, true);
                    break;

                case Faction.Cyan:
                    p.Resources = 12;
                    break;
            }
        }

        private void AssignInitialAmbassadors(Player p)
        {
            p.Ambassadors.Add(Ambassador.Pink);
            UnassignedAmbassadors.Items.Remove(Ambassador.Pink);
            Log(p.Faction, " receive the ", Faction.Pink, " ambassador");
            AssignRandomAmbassadors(p);
        }

        internal void AssignRandomAmbassadors(Player p)
        {
            foreach (var item in AmbassadorsSetAside)
            {
                UnassignedAmbassadors.Items.Add(item);
            }
            AmbassadorsSetAside.Clear();

            UnassignedAmbassadors.Shuffle();
            Stone(Milestone.Shuffled);
            Log(p.Faction, " draw 5 random Ambassadors");
            for (int i = 0; i < 5; i++)
            {
                p.Ambassadors.Add(UnassignedAmbassadors.Draw());
            }
        }

        public Faction NextFactionToPerformCustomSetup => Players.Select(p => p.Faction).Where(f => !HasActedOrPassed.Contains(f)).FirstOrDefault();

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

            Log(faction, " initial positions set, starting with ", Payment.Of(e.Resources));
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

            Log(e);
            Enter(
                IsPlaying(Faction.Blue) && PerformBluePlacement.BlueMayPlaceFirstForceInAnyTerritory(this), Phase.BlueSettingUp,
                IsPlaying(Faction.Cyan), Phase.CyanSettingUp,
                TreacheryCardsBeforeTraitors, EnterStormPhase, DealStartingTreacheryCards);
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

            Log(e);
            Enter(IsPlaying(Faction.Cyan), Phase.CyanSettingUp, TreacheryCardsBeforeTraitors, EnterStormPhase, DealStartingTreacheryCards);
        }

        public void HandleEvent(PerformCyanSetup e)
        {
            e.Player.ShipForces(e.Target, 6);
            Log(e);
            Enter(TreacheryCardsBeforeTraitors, EnterStormPhase, DealStartingTreacheryCards);
        }

        public Deck<TreacheryCard> StartingTreacheryCards { get; private set; }

        private TreacheryCard ExtraStartingCardForBlack = null;

        internal void DealStartingTreacheryCards()
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
            Log(e);
            StartingTreacheryCards.Shuffle();
            Stone(Milestone.Shuffled);
            DealRemainingStartingTreacheryCardsToNonGrey();
        }

        private void DealRemainingStartingTreacheryCardsToNonGrey()
        {
            foreach (var p in Players.Where(p => p.Faction != Faction.Grey))
            {
                var card = StartingTreacheryCards.Draw();
                p.TreacheryCards.Add(card);
                LogTo(p.Faction, "Your starting treachery card is: ", card);

                if (p.Is(Faction.Black))
                {
                    p.TreacheryCards.Add(ExtraStartingCardForBlack);
                    LogTo(Faction.Black, "Your extra card is: ", ExtraStartingCardForBlack);
                }
            }

            Enter(TreacheryCardsBeforeTraitors, AssignLeaderSkills, EnterStormPhase);
        }

        #endregion SettingUp
    }
}
