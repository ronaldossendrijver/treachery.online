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
        public void HandleEvent(WhiteRevealedNoField e)
        {
            RevealCurrentNoField(e.Player);
        }

        private void RevealCurrentNoField(Player player, Location inLocation = null)
        {
            if (player != null && player.Faction == Faction.White)
            {
                var noFieldLocation = player.ForcesInLocations.FirstOrDefault(kvp => kvp.Value.AmountOfSpecialForces > 0).Key;

                if (noFieldLocation != null)
                {
                    if (inLocation == null || inLocation == noFieldLocation)
                    {
                        LatestRevealedNoFieldValue = CurrentNoFieldValue;
                        player.SpecialForcesToReserves(noFieldLocation, 1);
                        int nrOfForces = Math.Min(player.ForcesInReserve, CurrentNoFieldValue);
                        player.ShipForces(noFieldLocation, nrOfForces);
                        Log(player.Faction, " reveal ", nrOfForces, FactionForce.White, " under ", FactionSpecialForce.White, " in ", noFieldLocation);

                        if (CurrentNoFieldValue == 0)
                        {
                            FlipBeneGesseritWhenAloneOrWithPinkAlly();
                        }

                        CurrentNoFieldValue = -1;
                    }
                }
            }
        }

        private void RevealCurrentNoField(Player player, Territory inTerritory)
        {
            if (player != null && player.Faction == Faction.White)
            {
                foreach (var l in inTerritory.Locations)
                {
                    RevealCurrentNoField(player, l);
                }
            }
        }

        public void HandleEvent(BlueBattleAnnouncement e)
        {
            var initiator = GetPlayer(e.Initiator);
            initiator.FlipForces(e.Territory, false);
            Log(e);
        }

        public void HandleEvent(Donated e)
        {
            var target = GetPlayer(e.Target);

            if (!e.FromBank)
            {
                var initiator = GetPlayer(e.Initiator);

                ExchangeResourcesInBribe(initiator, target, e.Resources);

                if (e.Card != null)
                {
                    initiator.TreacheryCards.Remove(e.Card);
                    RegisterKnown(initiator, e.Card);
                    target.TreacheryCards.Add(e.Card);

                    foreach (var p in Players.Where(p => p != initiator && p != target))
                    {
                        UnregisterKnown(p, initiator.TreacheryCards);
                        UnregisterKnown(p, target.TreacheryCards);
                    }
                }

                Log(e);
                RecentMilestones.Add(Milestone.Bribe);
            }
            else
            {
                if (e.Resources < 0)
                {
                    int resourcesToTake = Math.Min(Math.Abs(e.Resources), target.Resources);
                    Log("Host puts ", Payment(resourcesToTake), " from ", e.Target, " into the ", Concept.Resource, " Bank");
                    target.Resources -= resourcesToTake;
                }
                else
                {
                    Log("Host gives ", e.Target, Payment(e.Resources), " from the ", Concept.Resource, " Bank");
                    target.Resources += e.Resources;
                }
            }
        }

        public void HandleEvent(DistransUsed e)
        {
            var initiator = GetPlayer(e.Initiator);
            var target = GetPlayer(e.Target);

            bool targetHadRoomForCards = target.HasRoomForCards;

            Discard(initiator, TreacheryCardType.Distrans);

            initiator.TreacheryCards.Remove(e.Card);
            RegisterKnown(initiator, e.Card);
            target.TreacheryCards.Add(e.Card);

            if (initiator.TreacheryCards.Any())
            {
                foreach (var p in Players.Where(p => p != initiator && p != target))
                {
                    UnregisterKnown(p, initiator.TreacheryCards);
                    UnregisterKnown(p, target.TreacheryCards);
                }
            }

            Log(e);

            CheckIfBiddingForPlayerShouldBeSkipped(target, targetHadRoomForCards);
        }

        private void CheckIfBiddingForPlayerShouldBeSkipped(Player player, bool hadRoomForCards)
        {
            if (CurrentPhase == Phase.BlackMarketBidding && hadRoomForCards && !player.HasRoomForCards && BlackMarketBid.MayBePlayed(this, player))
            {
                HandleEvent(new Bid(this) { Initiator = player.Faction, Passed = true });
            }
            else if (CurrentPhase == Phase.Bidding && hadRoomForCards && !player.HasRoomForCards && Bid.MayBePlayed(this, player))
            {
                HandleEvent(new Bid(this) { Initiator = player.Faction, Passed = true });
            }
        }

        public void HandleEvent(DiscardedTaken e)
        {
            Log(e);
            RecentlyDiscarded.Remove(e.Card);
            TreacheryDiscardPile.Items.Remove(e.Card);
            e.Player.TreacheryCards.Add(e.Card);
            Discard(e.Player, TreacheryCardType.TakeDiscarded);
            RecentMilestones.Add(Milestone.CardWonSwapped);
        }

        private Phase PhaseBeforeSearchingDiscarded { get; set; }
        public void HandleEvent(DiscardedSearchedAnnounced e)
        {
            Log(e);
            PhaseBeforeSearchingDiscarded = CurrentPhase;
            e.Player.Resources -= 2;
            Enter(Phase.SearchingDiscarded);
            RecentMilestones.Add(Milestone.CardWonSwapped);
        }

        public void HandleEvent(DiscardedSearched e)
        {
            Log(e);
            foreach (var p in Players)
            {
                UnregisterKnown(p, TreacheryDiscardPile.Items);
            }
            TreacheryDiscardPile.Items.Remove(e.Card);
            e.Player.TreacheryCards.Add(e.Card);
            TreacheryDiscardPile.Shuffle();
            Discard(e.Player, TreacheryCardType.SearchDiscarded);
            Enter(PhaseBeforeSearchingDiscarded);
            RecentMilestones.Add(Milestone.Shuffled);
        }

        private void ExchangeResourcesInBribe(Player from, Player target, int amount)
        {
            from.Resources -= amount;

            if (BribesDuringMentat)
            {
                if (from.Faction == Faction.Red && target.Faction == from.Ally)
                {
                    target.Resources += amount;
                }
                else
                {
                    target.Bribes += amount;
                }
            }
            else
            {
                target.Resources += amount;
            }
        }

        private Phase phasePausedByClairvoyance;
        public ClairVoyancePlayed LatestClairvoyance { get; private set; }
        public ClairVoyanceQandA LatestClairvoyanceQandA { get; private set; }
        public BattleInitiated LatestClairvoyanceBattle { get; private set; }

        public void HandleEvent(ClairVoyancePlayed e)
        {
            var initiator = GetPlayer(e.Initiator);
            var card = initiator.Card(TreacheryCardType.Clairvoyance);

            if (card != null)
            {
                Discard(card);
                Log(e);
                RecentMilestones.Add(Milestone.Clairvoyance);
            }

            if (e.Target != Faction.None)
            {
                LatestClairvoyance = e;
                LatestClairvoyanceQandA = null;
                LatestClairvoyanceBattle = CurrentBattle;
                phasePausedByClairvoyance = CurrentPhase;
                Enter(Phase.Clairvoyance);
            }
        }

        public void HandleEvent(ClairVoyanceAnswered e)
        {
            LatestClairvoyanceQandA = new ClairVoyanceQandA(LatestClairvoyance, e);
            Log(e);

            if (LatestClairvoyance.Question == ClairvoyanceQuestion.WillAttackX && e.Answer == ClairVoyanceAnswer.No)
            {
                var deal = new Deal() { Type = DealType.DontShipOrMoveTo, BoundFaction = e.Initiator, ConsumingFaction = LatestClairvoyance.Initiator, DealParameter1 = LatestClairvoyance.QuestionParameter1, End = Phase.ShipmentAndMoveConcluded };
                StartDeal(deal);
            }

            if (LatestClairvoyance.Question == ClairvoyanceQuestion.LeaderAsTraitor)
            {
                var hero = LatestClairvoyance.Parameter1 as IHero;

                if (e.Answer == ClairVoyanceAnswer.Yes)
                {
                    if (!e.Player.ToldTraitors.Contains(hero))
                    {
                        e.Player.ToldTraitors.Add(hero);
                    }
                }
                else if (e.Answer == ClairVoyanceAnswer.No)
                {
                    if (!e.Player.ToldNonTraitors.Contains(hero))
                    {
                        e.Player.ToldNonTraitors.Add(hero);
                    }
                }
            }

            if (LatestClairvoyance.Question == ClairvoyanceQuestion.LeaderAsFacedancer)
            {
                var hero = LatestClairvoyance.Parameter1 as IHero;

                if (e.Answer == ClairVoyanceAnswer.Yes)
                {
                    if (!e.Player.ToldFacedancers.Contains(hero))
                    {
                        e.Player.ToldFacedancers.Add(hero);
                    }
                }
                else if (e.Answer == ClairVoyanceAnswer.No)
                {
                    if (!e.Player.ToldNonFacedancers.Contains(hero))
                    {
                        e.Player.ToldNonFacedancers.Add(hero);
                    }
                }
            }

            Enter(phasePausedByClairvoyance);
        }

        public void HandleEvent(AmalPlayed e)
        {
            Discard(GetPlayer(e.Initiator), TreacheryCardType.Amal);
            Log(e);
            foreach (var p in Players)
            {
                int resourcesPaid = (int)Math.Ceiling(0.5 * p.Resources);
                p.Resources -= resourcesPaid;
                Log(p.Faction, " lose ", Payment(resourcesPaid));
            }
            RecentMilestones.Add(Milestone.Amal);
        }

        public void HandleEvent(BrownDiscarded e)
        {
            Discard(e.Card);
            Log(e);
            if (e.Card.Type == TreacheryCardType.Useless)
            {
                e.Player.Resources += 2;
            }
            else
            {
                e.Player.Resources += 3;
            }

            RecentMilestones.Add(Milestone.ResourcesReceived);
        }


        public int KarmaHmsMovesLeft { get; private set; } = 2;
        public void HandleEvent(KarmaHmsMovement e)
        {
            var initiator = GetPlayer(e.Initiator);
            int collectionRate = initiator.AnyForcesIn(Map.HiddenMobileStronghold) * 2;
            Log(e);

            if (!initiator.SpecialKarmaPowerUsed)
            {
                Discard(initiator, TreacheryCardType.Karma);
                initiator.SpecialKarmaPowerUsed = true;
            }

            var currentLocation = Map.HiddenMobileStronghold.AttachedToLocation;
            CollectSpiceFrom(e.Initiator, currentLocation, collectionRate);

            if (!e.Passed)
            {
                Map.HiddenMobileStronghold.PointAt(e.Target);
                CollectSpiceFrom(e.Initiator, e.Target, collectionRate);
                KarmaHmsMovesLeft--;
                RecentMilestones.Add(Milestone.HmsMovement);
            }

            if (e.Passed)
            {
                KarmaHmsMovesLeft = 0;
            }
        }

        public void HandleEvent(KarmaBrownDiscard e)
        {
            RecentMilestones.Add(Milestone.Discard);
            Discard(e.Player, TreacheryCardType.Karma);
            Log(e);

            foreach (var card in e.Cards)
            {
                Discard(e.Player, card);
            }

            e.Player.Resources += e.Cards.Count() * 3;
            e.Player.SpecialKarmaPowerUsed = true;
        }

        public void HandleEvent(KarmaWhiteBuy e)
        {
            Discard(e.Player, TreacheryCardType.Karma);
            Log(e);
            e.Player.TreacheryCards.Add(e.Card);
            WhiteCache.Remove(e.Card);
            e.Player.Resources -= 3;
            e.Player.SpecialKarmaPowerUsed = true;
        }

        public void HandleEvent(KarmaFreeRevival e)
        {
            RecentMilestones.Add(Milestone.Revival);
            Log(e);
            var initiator = GetPlayer(e.Initiator);

            Discard(initiator, TreacheryCardType.Karma);
            initiator.SpecialKarmaPowerUsed = true;

            if (e.Hero != null)
            {
                ReviveHero(e.Hero);

                if (e.AssignSkill)
                {
                    PrepareSkillAssignmentToRevivedLeader(e.Player, e.Hero as Leader);
                }

            }
            else
            {
                initiator.ReviveForces(e.AmountOfForces);
                initiator.ReviveSpecialForces(e.AmountOfSpecialForces);

                if (e.AmountOfSpecialForces > 0)
                {
                    FactionsThatRevivedSpecialForcesThisTurn.Add(e.Initiator);
                }
            }
        }

        public void HandleEvent(KarmaMonster e)
        {
            var initiator = GetPlayer(e.Initiator);
            Discard(initiator, TreacheryCardType.Karma);
            initiator.SpecialKarmaPowerUsed = true;
            Log(e);
            RecentMilestones.Add(Milestone.Karma);
            NumberOfMonsters++;
            LetMonsterAppear(e.Territory);

            if (CurrentPhase == Phase.BlowReport)
            {
                Enter(Phase.AllianceB);
            }
        }

        public bool GreenKarma { get; private set; } = false;
        public void HandleEvent(KarmaPrescience e)
        {
            var initiator = GetPlayer(e.Initiator);
            Discard(initiator, TreacheryCardType.Karma);
            initiator.SpecialKarmaPowerUsed = true;
            Log(e);
            RecentMilestones.Add(Milestone.Karma);
            GreenKarma = true;
        }

        public int PinkKarmaBonus { get; private set; } = 0;
        public void HandleEvent(KarmaPinkDial e)
        {
            var initiator = GetPlayer(e.Initiator);
            Discard(initiator, TreacheryCardType.Karma);
            initiator.SpecialKarmaPowerUsed = true;
            RecentMilestones.Add(Milestone.Karma);
            var myLeader = CurrentBattle.PlanOf(e.Initiator).Hero;
            var opponentLeader = CurrentBattle.PlanOfOpponent(initiator).Hero;

            if (myLeader != null && opponentLeader != null)
            {
                PinkKarmaBonus = Math.Abs(myLeader.Value - opponentLeader.ValueInCombatAgainst(myLeader));
            }

            Log("Using ", TreacheryCardType.Karma, ", ", e.Initiator, " add ", PinkKarmaBonus, " to their dial" );
        }

        public void HandleEvent(Karma e)
        {
            Discard(e.Card);
            Log(e);
            RecentMilestones.Add(Milestone.Karma);

            if (e.Prevented != FactionAdvantage.None)
            {
                Prevent(e.Initiator, e.Prevented);
            }

            RevokeBattlePlansIfNeeded(e);

            if (e.Prevented == FactionAdvantage.BlueUsingVoice)
            {
                CurrentVoice = null;
            }

            if (e.Prevented == FactionAdvantage.GreenBattlePlanPrescience)
            {
                CurrentPrescience = null;
            }
        }

        public IList<Faction> SecretsRemainHidden { get; private set; } = new List<Faction>();

        public void HandleEvent(HideSecrets e)
        {
            SecretsRemainHidden.Add(e.Initiator);
        }

        public bool YieldsSecrets(Player p)
        {
            return !SecretsRemainHidden.Contains(p.Faction);
        }
        public bool YieldsSecrets(Faction f)
        {
            return !SecretsRemainHidden.Contains(f);
        }

        private void RevokeBattlePlansIfNeeded(Karma e)
        {
            if (CurrentMainPhase == MainPhase.Battle)
            {
                if (e.Prevented == FactionAdvantage.YellowNotPayingForBattles ||
                    e.Prevented == FactionAdvantage.YellowSpecialForceBonus)
                {
                    RevokePlanIfNeeded(Faction.Yellow);
                }

                if (e.Prevented == FactionAdvantage.RedSpecialForceBonus)
                {
                    RevokePlanIfNeeded(Faction.Red);
                }

                if (e.Prevented == FactionAdvantage.GreySpecialForceBonus)
                {
                    RevokePlanIfNeeded(Faction.Grey);
                }

                if (e.Prevented == FactionAdvantage.GreenUseMessiah)
                {
                    RevokePlanIfNeeded(Faction.Green);
                }
            }
        }

        private void RevokePlanIfNeeded(Faction f)
        {
            RevokePlan(CurrentBattle?.PlanOf(f));
        }

        private void RevokePlan(Battle plan)
        {
            if (plan == AggressorBattleAction)
            {
                AggressorBattleAction = null;
            }
            else if (plan == DefenderBattleAction)
            {
                DefenderBattleAction = null;
            }
        }

        private List<FactionAdvantage> PreventedAdvantages = new();

        private void Prevent(Faction initiator, FactionAdvantage advantage)
        {
            Log(initiator, " prevent ", advantage);

            if (!PreventedAdvantages.Contains(advantage))
            {
                PreventedAdvantages.Add(advantage);
            }
        }

        private void Allow(FactionAdvantage advantage)
        {
            if (PreventedAdvantages.Contains(advantage))
            {
                PreventedAdvantages.Remove(advantage);
                Log(TreacheryCardType.Karma, " no longer prevents ", advantage);
            }
        }

        public bool Prevented(FactionAdvantage advantage) => PreventedAdvantages.Contains(advantage);

        public bool BribesDuringMentat => !Applicable(Rule.BribesAreImmediate);

        public void HandleEvent(PlayerReplaced e)
        {
            GetPlayer(e.ToReplace).IsBot = !GetPlayer(e.ToReplace).IsBot;
            Log(e.ToReplace, " will now be played by a ", GetPlayer(e.ToReplace).IsBot ? "Bot" : "Human");
        }

        public bool KarmaPrevented(Faction f)
        {
            return CurrentKarmaPrevention != null && CurrentKarmaPrevention.Target == f;
        }

        public BrownKarmaPrevention CurrentKarmaPrevention { get; set; } = null;
        public void HandleEvent(BrownKarmaPrevention e)
        {
            Log(e);
            Discard(e.CardUsed());
            CurrentKarmaPrevention = e;
            RecentMilestones.Add(Milestone.SpecialUselessPlayed);
        }

        public bool JuiceForcesFirstPlayer => CurrentJuice != null && CurrentJuice.Type == JuiceType.GoFirst;

        public bool JuiceForcesLastPlayer => CurrentJuice != null && CurrentJuice.Type == JuiceType.GoLast;


        public JuicePlayed CurrentJuice { get; set; }
        public void HandleEvent(JuicePlayed e)
        {
            Log(e);

            var aggressorBeforeJuiceIsPlayed = CurrentBattle?.AggressivePlayer;

            CurrentJuice = e;
            Discard(e.Player, TreacheryCardType.Juice);

            if ((e.Type == JuiceType.GoFirst || e.Type == JuiceType.GoLast) && Version <= 117)
            {
                switch (CurrentMainPhase)
                {
                    case MainPhase.Bidding: BidSequence.CheckCurrentPlayer(); break;
                    case MainPhase.ShipmentAndMove: ShipmentAndMoveSequence.CheckCurrentPlayer(); break;
                    case MainPhase.Battle: BattleSequence.CheckCurrentPlayer(); break;
                }
            }
            else if (CurrentBattle != null && e.Type == JuiceType.Aggressor && CurrentBattle.AggressivePlayer != aggressorBeforeJuiceIsPlayed)
            {
                var currentAggressorBattleAction = AggressorBattleAction;
                var currentAggressorTraitorAction = AggressorTraitorAction;
                AggressorBattleAction = DefenderBattleAction;
                AggressorTraitorAction = DefenderTraitorAction;
                DefenderBattleAction = currentAggressorBattleAction;
                DefenderTraitorAction = currentAggressorTraitorAction;
            }
        }

        private bool BureaucratWasUsedThisPhase { get; set; } = false;
        private Phase _phaseBeforeBureaucratWasActivated;
        public Faction TargetOfBureaucracy { get; private set; }

        private void ApplyBureaucracy(Faction payer, Faction receiver)
        {
            if (!BureaucratWasUsedThisPhase)
            {
                var bureaucrat = PlayerSkilledAs(LeaderSkill.Bureaucrat);
                if (bureaucrat != null && bureaucrat.Faction != payer && bureaucrat.Faction != receiver)
                {
                    if (Version < 133) BureaucratWasUsedThisPhase = true;
                    _phaseBeforeBureaucratWasActivated = CurrentPhase;
                    TargetOfBureaucracy = receiver;
                    Enter(Phase.Bureaucracy);
                }
            }
        }

        public void HandleEvent(Bureaucracy e)
        {
            Log(e.GetDynamicMessage());
            if (!e.Passed)
            {
                BureaucratWasUsedThisPhase = true;
                GetPlayer(TargetOfBureaucracy).Resources -= 2;
            }
            Enter(_phaseBeforeBureaucratWasActivated);
            TargetOfBureaucracy = Faction.None;
        }

        private bool BankerWasUsedThisPhase { get; set; } = false;

        private void ActivateBanker(Player playerWhoPaid)
        {
            if (!BankerWasUsedThisPhase)
            {
                var banker = PlayerSkilledAs(LeaderSkill.Banker);
                if (banker != null && banker != playerWhoPaid)
                {
                    BankerWasUsedThisPhase = true;
                    Log(banker.Faction, " will receive ", Payment(1), " from ", LeaderSkill.Banker, " at ", MainPhase.Collection);
                    banker.BankedResources += 1;
                }
            }
        }

        public Planetology CurrentPlanetology { get; private set; }
        public void HandleEvent(Planetology e)
        {
            Log(e);
            CurrentPlanetology = e;
        }

        public void HandleEvent(NexusPlayed e)
        {
            if (e.Cunning)
            {
                HandleCunning(e);
            }
            else if (e.SecretAlly)
            {
                HandleSecretAlly(e);
            }
            else
            {
                HandleBetrayal(e);
            }

            DiscardNexusCard(e.Player);
        }


        private bool BlackMayDrawNewTraitor { get; set; } = false;
        private void HandleBetrayal(NexusPlayed e)
        {
            MessagePart action = null;

            switch (e.Faction)
            {
                case Faction.Green:
                    Prevent(e.Initiator, FactionAdvantage.GreenBattlePlanPrescience);
                    break;

                case Faction.Black:
                    var treachery = CurrentBattle.TreacheryOf(Faction.Black);
                    treachery.TraitorCalled = false;
                    var traitor = CurrentBattle.PlanOfOpponent(Faction.Black).Hero;
                    GetPlayer(Faction.Black).Traitors.Remove(traitor);
                    TraitorDeck.Items.Add(traitor);
                    TraitorDeck.Shuffle();
                    RecentMilestones.Add(Milestone.Shuffled);
                    BlackMayDrawNewTraitor = true;
                    action = MessagePart.Express(" cancel the ", Faction.Black, " traitor call");
                    Enter(Phase.CallTraitorOrPass);
                    HandleRevealedBattlePlans();
                    break;

                case Faction.Yellow:
                    if (CurrentMainPhase == MainPhase.Blow)
                    {
                        Prevent(e.Initiator, FactionAdvantage.YellowRidesMonster);
                    }
                    else if (CurrentMainPhase == MainPhase.ShipmentAndMove)
                    {
                        Prevent(e.Initiator, FactionAdvantage.YellowExtraMove);
                    }
                    break;

                case Faction.Red:
                    if (CurrentMainPhase == MainPhase.Bidding)
                    {
                        Prevent(e.Initiator, FactionAdvantage.RedReceiveBid);
                    }
                    else if (CurrentMainPhase == MainPhase.Battle && Applicable(Rule.RedSpecialForces))
                    {
                        Prevent(e.Initiator, FactionAdvantage.RedSpecialForceBonus);
                    }
                    break;

                case Faction.Orange:
                    foreach (var p in StoredRecentlyPaid)
                    {
                        object from = p.To == Faction.None ? "the Bank" : p.To;
                        Log(e.Initiator, " play ", e.Faction, " Nexus Betrayal to get ", p.Amount, " from ", from);
                        if (p.To != Faction.None)
                        {
                            var getFrom = GetPlayer(p.To);
                            if (getFrom != null)
                            {
                                getFrom.Resources -= p.Amount;
                            }
                        }

                        e.Player.Resources += p.Amount;
                    }
                    RecentlyPaid.Clear();
                    break;
                    
            }    

            if (action != null)
            {
                Log(e.Initiator, " play ", e.Faction, " Nexus Betrayal to ", action);
            }
        }

        public NexusPlayed CurrentGreenNexus { get; private set; }
        public NexusPlayed CurrentYellowNexus { get; private set; }
        public NexusPlayed CurrentRedNexus { get; private set; }
        public NexusPlayed CurrentOrangeNexus { get; private set; }
        private void HandleCunning(NexusPlayed e)
        {
            var action = MessagePart.Express();

            switch (e.Faction)
            {
                case Faction.Green: 
                    CurrentGreenNexus = e;
                    action = MessagePart.Express("see their opponent's ", e.GreenPrescienceAspect);
                    break;

                case Faction.Black:
                    PhaseBeforeDiscardingTraitor = CurrentPhase;
                    FactionThatMustDiscardTraitor = e.Initiator;
                    NumberOfTraitorsToDiscard = 1;
                    action = MessagePart.Express("draw a new traitor");
                    e.Player.Traitors.Add(TraitorDeck.Draw());
                    Enter(Phase.DiscardingTraitor);
                    break;

                case Faction.Yellow:
                    CurrentYellowNexus = e;
                    action = MessagePart.Express("let forces ride ", Concept.Monster, " from from another territory than where it appeared");
                    break;

                case Faction.Red:
                    CurrentRedNexus = e;
                    action = MessagePart.Express("let 5 ", FactionForce.Red, " count as " , FactionSpecialForce.Red, " during this battle");
                    break;

                case Faction.Orange:
                    CurrentOrangeNexus = e;
                    action = MessagePart.Express("perform an extra shipment after their move");
                    break;

            }

            Log(e.Initiator, " play ", e.Faction, " Nexus Cunning to ", action);
        }

        private void HandleSecretAlly(NexusPlayed e)
        {
            var action = MessagePart.Express();
            
            switch (e.Faction)
            {
                case Faction.Green:
                    CurrentGreenNexus = e;
                    action = MessagePart.Express("see their opponent's ", e.GreenPrescienceAspect);
                    break;

                case Faction.Black:
                    PhaseBeforeDiscardingTraitor = CurrentPhase;
                    FactionThatMustDiscardTraitor = e.Initiator;
                    NumberOfTraitorsToDiscard = 2;
                    action = MessagePart.Express("draw two new traitors");
                    e.Player.Traitors.Add(TraitorDeck.Draw());
                    e.Player.Traitors.Add(TraitorDeck.Draw());
                    Enter(Phase.DiscardingTraitor);
                    break;

                case Faction.Yellow:
                    CurrentYellowNexus = e;
                    if (CurrentMainPhase == MainPhase.Blow)
                    {
                        action = MessagePart.Express("prevent their forces from being devoured by ", Concept.Monster);
                    }
                    else if (CurrentMainPhase == MainPhase.Resurrection)
                    {
                        action = MessagePart.Express("increase their free revival to 3");
                    }
                    break;

                case Faction.Orange:
                    CurrentOrangeNexus = e;
                    action = MessagePart.Express("be able to ship as ", Faction.Orange);
                    break;

            }

            Log(e.Initiator, " play ", e.Faction, " Nexus Secret Ally to ", action);
        }

        private void LogPrevention(FactionAdvantage prevented)
        {
            Log(TreacheryCardType.Karma, " prevents ", prevented);
        }

        public bool CharityIsCancelled => EconomicsStatus == BrownEconomicsStatus.Cancel || EconomicsStatus == BrownEconomicsStatus.CancelFlipped;
    }

    class TriggeredBureaucracy
    {
        internal Faction PaymentFrom;
        internal Faction PaymentTo;
    }
}
