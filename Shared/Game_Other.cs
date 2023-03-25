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
        public List<Faction> ResourceAuditedFactions { get; private set; } = new();

        public void HandleEvent(ResourcesAudited e)
        {
            ResourceAuditedFactions.Add(e.Target);
            var target = GetPlayer(e.Target);

            Log(e);
            LogTo(e.Initiator, e.Target, " own ", Payment.Of(target.Resources), ", ", target.TreacheryCards.Count(tc => tc.IsWeapon), " weapons and ", target.TreacheryCards.Count(tc => tc.IsDefense), " defenses");
        }

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
                Stone(Milestone.Bribe);
            }
            else
            {
                if (e.Resources < 0)
                {
                    int resourcesToTake = Math.Min(Math.Abs(e.Resources), target.Resources);
                    Log("Host puts ", Payment.Of(resourcesToTake), " from ", e.Target, " into the ", Concept.Resource, " Bank");
                    target.Resources -= resourcesToTake;
                }
                else
                {
                    Log("Host gives ", e.Target, Payment.Of(e.Resources), " from the ", Concept.Resource, " Bank");
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
            Stone(Milestone.CardWonSwapped);
        }

        private Phase PhaseBeforeSearchingDiscarded { get; set; }
        public void HandleEvent(DiscardedSearchedAnnounced e)
        {
            Log(e);
            PhaseBeforeSearchingDiscarded = CurrentPhase;
            e.Player.Resources -= 2;
            Enter(Phase.SearchingDiscarded);
            Stone(Milestone.CardWonSwapped);
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
            Stone(Milestone.Shuffled);
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
            var card = ClairVoyancePlayed.Card(this, e.Player);

            if (card != null)
            {
                Discard(card);
                Log(e);
                Stone(Milestone.Clairvoyance);
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

            Stone(Milestone.ResourcesReceived);
        }


        public int KarmaHmsMovesLeft { get; private set; } = 2;
        public void HandleEvent(KarmaHmsMovement e)
        {
            var initiator = GetPlayer(e.Initiator);
            int collectionRate = initiator.AnyForcesIn(Map.HiddenMobileStronghold) * 2;
            Log(e);

            if (!initiator.SpecialKarmaPowerUsed)
            {
                Discard(initiator, Karma.ValidKarmaCards(this, e.Player).FirstOrDefault());
                initiator.SpecialKarmaPowerUsed = true;
            }

            var currentLocation = Map.HiddenMobileStronghold.AttachedToLocation;
            CollectSpiceFrom(e.Initiator, currentLocation, collectionRate);

            if (!e.Passed)
            {
                Map.HiddenMobileStronghold.PointAt(e.Target);
                CollectSpiceFrom(e.Initiator, e.Target, collectionRate);
                KarmaHmsMovesLeft--;
                Stone(Milestone.HmsMovement);
            }

            if (e.Passed)
            {
                KarmaHmsMovesLeft = 0;
            }
        }

        public void HandleEvent(KarmaBrownDiscard e)
        {
            Discard(e.Player, Karma.ValidKarmaCards(this, e.Player).FirstOrDefault());
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
            Discard(e.Player, Karma.ValidKarmaCards(this, e.Player).FirstOrDefault());
            Log(e);
            e.Player.TreacheryCards.Add(e.Card);
            WhiteCache.Remove(e.Card);
            e.Player.Resources -= 3;
            e.Player.SpecialKarmaPowerUsed = true;
        }

        public void HandleEvent(KarmaFreeRevival e)
        {
            Stone(Milestone.Revival);
            Log(e);
            var initiator = GetPlayer(e.Initiator);

            Discard(initiator, Karma.ValidKarmaCards(this, e.Player).FirstOrDefault());
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
            Discard(initiator, Karma.ValidKarmaCards(this, e.Player).FirstOrDefault());
            initiator.SpecialKarmaPowerUsed = true;
            Log(e);
            Stone(Milestone.Karma);
            NumberOfMonsters++;
            LetMonsterAppear(e.Territory, false);

            if (CurrentPhase == Phase.BlowReport)
            {
                Enter(Phase.AllianceB);
            }
        }

        public bool GreenKarma { get; private set; } = false;
        public void HandleEvent(KarmaPrescience e)
        {
            var initiator = GetPlayer(e.Initiator);
            Discard(initiator, Karma.ValidKarmaCards(this, e.Player).FirstOrDefault());
            initiator.SpecialKarmaPowerUsed = true;
            Log(e);
            Stone(Milestone.Karma);
            GreenKarma = true;
        }

        public int PinkKarmaBonus { get; private set; } = 0;
        public void HandleEvent(KarmaPinkDial e)
        {
            var initiator = GetPlayer(e.Initiator);
            Discard(initiator, Karma.ValidKarmaCards(this, e.Player).FirstOrDefault());
            initiator.SpecialKarmaPowerUsed = true;
            Stone(Milestone.Karma);
            var myLeader = CurrentBattle.PlanOf(e.Initiator).Hero;
            var opponentLeader = CurrentBattle.PlanOfOpponent(initiator).Hero;

            if (myLeader != null && opponentLeader != null)
            {
                PinkKarmaBonus = Math.Abs(myLeader.Value - opponentLeader.ValueInCombatAgainst(myLeader));
            }

            Log("Using ", TreacheryCardType.Karma, ", ", e.Initiator, " add ", PinkKarmaBonus, " to their dial");
        }

        public void HandleEvent(Karma e)
        {
            Discard(e.Card);
            Log(e);
            Stone(Milestone.Karma);

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
                Log(advantage, " is no longer prevented");
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

            if (NexusPlayed.CanUseCunning(e.Player))
            {
                DiscardNexusCard(e.Player);
                Stone(Milestone.NexusPlayed);
                LetPlayerDiscardTreacheryCardOfChoice(e.Initiator);
            }
            else
            {
                Discard(e.CardUsed());
            }

            CurrentKarmaPrevention = e;
            Stone(Milestone.SpecialUselessPlayed);
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

        private Faction WasVictimOfBureaucracy { get; set; }
        public void HandleEvent(Bureaucracy e)
        {
            Log(e.GetDynamicMessage());
            if (!e.Passed)
            {
                Stone(Milestone.Bureaucracy);
                BureaucratWasUsedThisPhase = true;
                GetPlayer(TargetOfBureaucracy).Resources -= 2;
                WasVictimOfBureaucracy = TargetOfBureaucracy;
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
                    Log(banker.Faction, " will receive ", Payment.Of(1), " from ", LeaderSkill.Banker, " at ", MainPhase.Collection);
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
            if (e.IsCunning)
            {
                HandleCunning(e);
            }
            else if (e.IsSecretAlly)
            {
                HandleSecretAlly(e);
            }
            else
            {
                HandleBetrayal(e);
            }
        }


        public bool BlackTraitorWasCancelled { get; private set; } = false;
        public bool FacedancerWasCancelled { get; private set; } = false;
        private void HandleBetrayal(NexusPlayed e)
        {
            switch (e.Faction)
            {
                case Faction.Green:
                    PlayNexusCard(e.Player);
                    Prevent(e.Initiator, FactionAdvantage.GreenBattlePlanPrescience);
                    CurrentPrescience = null;
                    break;

                case Faction.Black:
                    var traitor = CurrentBattle.PlanOfOpponent(Faction.Black).Hero;
                    GetPlayer(Faction.Black).Traitors.Remove(traitor);
                    TraitorDeck.Items.Add(traitor);
                    TraitorDeck.Shuffle();
                    Stone(Milestone.Shuffled);
                    BlackTraitorWasCancelled = true;
                    PlayNexusCard(e.Player, "cancel the ", Faction.Black, " traitor call");
                    Enter(Phase.CallTraitorOrPass);
                    HandleRevealedBattlePlans();
                    break;

                case Faction.Yellow:
                    PlayNexusCard(e.Player);
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
                    PlayNexusCard(e.Player);
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
                    PlayNexusCard(e.Player);
                    foreach (var p in StoredRecentlyPaid)
                    {
                        object from = p.To == Faction.None ? "the Bank" : p.To;
                        Log(e.Player, "get ", p.Amount, " from ", from);
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

                    if (TargetOfBureaucracy == Faction.Orange)
                    {
                        TargetOfBureaucracy = e.Initiator;
                    }

                    break;

                case Faction.Blue:
                    PlayNexusCard(e.Player);
                    Prevent(e.Initiator, FactionAdvantage.BlueUsingVoice);
                    CurrentVoice = null;
                    break;

                case Faction.Grey:
                    PlayNexusCard(e.Player);
                    if (CurrentPhase < Phase.GreySelectingCard)
                    {
                        Prevent(e.Initiator, FactionAdvantage.GreySelectingCardsOnAuction);
                    }
                    else if (CurrentPhase > Phase.GreySelectingCard && CurrentPhase < Phase.BiddingReport)
                    {
                        Prevent(e.Initiator, FactionAdvantage.GreySwappingCard);
                    }
                    break;

                case Faction.Purple:
                    FacedancerWasCancelled = true;
                    PlayNexusCard(e.Player, "cancel the ", Faction.Purple, " face dancer reveal");
                    FinishBattle();
                    break;

                case Faction.Brown:
                    PlayNexusCard(e.Player, "force ", Faction.Brown, " to discard one of their treachery cards at random");
                    var victimPlayer = GetPlayer(Faction.Brown);
                    if (victimPlayer.TreacheryCards.Any())
                    {
                        Discard(victimPlayer.TreacheryCards.RandomOrDefault(Random));
                    }
                    else
                    {
                        Log(victimPlayer.Faction, " have no treachery cards to discard");
                    }
                    break;

                case Faction.White:
                    var paymentToWhite = StoredRecentlyPaid.FirstOrDefault(p => p.To == Faction.White);
                    var white = GetPlayer(Faction.White);

                    if (paymentToWhite != null)
                    {
                        var amountReceived = paymentToWhite.Amount - (WasVictimOfBureaucracy == Faction.White ? 2 : 0);
                        PlayNexusCard(e.Player, "let ", Faction.White, " lose the payment of ", Payment.Of(amountReceived), " they just received");
                        white.Resources -= amountReceived;
                    }
                    else if (white.TreacheryCards.Contains(CardJustWon))
                    {
                        PlayNexusCard(e.Player, "force ", Faction.White, " to discard the card they just won");
                        Discard(white, CardJustWon);
                    }
                    break;

                case Faction.Pink:
                    var pinksAlly = GetAlliedPlayer(Faction.Pink);
                    pinksAlly.ForcesToReserves(e.PinkTerritory);
                    PlayNexusCard(e.Player, "return all ", pinksAlly.Faction, " forces in ", e.PinkTerritory, " to reserves");
                    FlipBeneGesseritWhenAloneOrWithPinkAlly();
                    break;

                case Faction.Cyan:
                    var terrorToRemove = TerrorIn(e.CyanTerritory).RandomOrDefault(Random);
                    TerrorOnPlanet.Remove(terrorToRemove);
                    PlayNexusCard(e.Player, "remove a terror token from ", e.CyanTerritory);
                    break;

            }
        }

        private void PlayNexusCard(Player initiator, params object[] messageElements)
        {
            Stone(Milestone.NexusPlayed);

            string typeOfNexus;
            if (NexusPlayed.CanUseCunning(initiator))
            {
                typeOfNexus = "Cunning";
            }
            else if (NexusPlayed.CanUseBetrayal(this, initiator))
            {
                typeOfNexus = "Betrayal";
            }
            else
            {
                typeOfNexus = "Secret Ally";
            }

            if (messageElements != null && messageElements.Length > 0)
            {
                Log(initiator.Faction, " play ", initiator.Nexus, $" Nexus {typeOfNexus} to ", MessagePart.Express(messageElements));
            }
            else
            {
                Log(initiator.Faction, " play ", initiator.Nexus, $" Nexus {typeOfNexus}");
            }

            DiscardNexusCard(initiator);
        }

        public NexusPlayed CurrentGreenNexus { get; private set; }
        public NexusPlayed CurrentYellowNexus { get; private set; }
        public NexusPlayed CurrentRedNexus { get; private set; }
        public NexusPlayed CurrentOrangeNexus { get; private set; }
        public NexusPlayed CurrentBlueNexus { get; private set; }
        public NexusPlayed CurrentGreyNexus { get; private set; }
        private void HandleCunning(NexusPlayed e)
        {
            switch (e.Faction)
            {
                case Faction.Green:
                    CurrentGreenNexus = e;
                    PlayNexusCard(e.Player, "see their opponent's ", e.GreenPrescienceAspect);
                    break;

                case Faction.Black:
                    PhaseBeforeDiscardingTraitor = CurrentPhase;
                    FactionThatMustDiscardTraitor = e.Initiator;
                    NumberOfTraitorsToDiscard = 1;
                    PlayNexusCard(e.Player, "draw a new traitor");
                    e.Player.Traitors.Add(TraitorDeck.Draw());
                    Enter(Phase.DiscardingTraitor);
                    break;

                case Faction.Red:
                    CurrentRedNexus = e;
                    PlayNexusCard(e.Player, "let 5 ", FactionForce.Red, " count as ", FactionSpecialForce.Red, " during this battle");
                    break;

                case Faction.Orange:
                    CurrentOrangeNexus = e;
                    PlayNexusCard(e.Player, "perform an extra shipment after their move");
                    break;

                case Faction.Blue:
                    CurrentBlueNexus = e;
                    PlayNexusCard(e.Player, "be able to flip advisor to fighters during ", MainPhase.ShipmentAndMove);
                    break;

                case Faction.Grey:
                    CurrentGreyNexus = e;
                    PlayNexusCard(e.Player, "let ", FactionForce.Grey, " be full strength during this battle");
                    break;

                case Faction.Purple:
                    var purple = GetPlayer(Faction.Purple);
                    PlayNexusCard(e.Player, "replace their ", purple.RevealedDancers.Count, " revealed face dancers");
                    if (purple.RevealedDancers.Count > 0)
                    {
                        for (int i = 0; i < purple.RevealedDancers.Count; i++)
                        {
                            purple.FaceDancers.Add(TraitorDeck.Draw());
                        }

                        TraitorDeck.Items.AddRange(purple.RevealedDancers);
                        TraitorDeck.Shuffle();

                        foreach (var dancer in purple.RevealedDancers)
                        {
                            purple.FaceDancers.Remove(dancer);
                        }
                        purple.RevealedDancers.Clear();

                        Stone(Milestone.Shuffled);
                    }
                    break;

                case Faction.Pink:
                    if (!IsAlive(Vidal))
                    {
                        ReviveHero(Vidal);

                        if (e.PurpleAssignSkill)
                        {
                            PrepareSkillAssignmentToRevivedLeader(e.Player, Vidal);
                        }
                    }
                    TakeVidal(e.Player, VidalMoment.EndOfTurn);
                    PlayNexusCard(e.Player, "take ", Vidal, " this turn");
                    break;
            }
        }

        private void HandleSecretAlly(NexusPlayed e)
        {
            switch (e.Faction)
            {
                case Faction.Green:
                    CurrentGreenNexus = e;
                    PlayNexusCard(e.Player, "see their opponent's ", e.GreenPrescienceAspect);
                    break;

                case Faction.Black:
                    PhaseBeforeDiscardingTraitor = CurrentPhase;
                    FactionThatMustDiscardTraitor = e.Initiator;
                    NumberOfTraitorsToDiscard = 2;
                    PlayNexusCard(e.Player, "draw two new traitors");
                    e.Player.Traitors.Add(TraitorDeck.Draw());
                    e.Player.Traitors.Add(TraitorDeck.Draw());
                    Enter(Phase.DiscardingTraitor);
                    break;

                case Faction.Yellow:
                    CurrentYellowNexus = e;
                    if (CurrentMainPhase == MainPhase.Blow)
                    {
                        PlayNexusCard(e.Player, "prevent their forces from being devoured by ", Concept.Monster);
                    }
                    else if (CurrentMainPhase == MainPhase.Resurrection)
                    {
                        PlayNexusCard(e.Player, "increase their free revival to 3");
                    }
                    break;

                case Faction.Orange:
                    CurrentOrangeNexus = e;
                    PlayNexusCard(e.Player, "be able to ship as ", Faction.Orange);
                    break;

                case Faction.Grey:
                    CurrentGreyNexus = e;
                    PlayNexusCard(e.Player, "discard a card you buy and draw a new card from the treachery deck");
                    break;

                case Faction.Purple:
                    Stone(Milestone.RaiseDead);
                    var player = GetPlayer(e.Initiator);

                    player.ReviveForces(e.PurpleForces);
                    player.ReviveSpecialForces(e.PurpleSpecialForces);

                    if (e.PurpleSpecialForces > 0)
                    {
                        FactionsThatRevivedSpecialForcesThisTurn.Add(e.Initiator);
                    }

                    if (e.PurpleHero != null)
                    {
                        ReviveHero(e.PurpleHero);

                        if (e.PurpleAssignSkill)
                        {
                            PrepareSkillAssignmentToRevivedLeader(player, e.PurpleHero as Leader);
                        }

                    }

                    PlayNexusCard(e.Player, "revive ",
                        MessagePart.ExpressIf(e.PurpleHero != null, e.PurpleHero),
                        MessagePart.ExpressIf(e.PurpleHero != null && e.PurpleForces + e.PurpleSpecialForces > 0, " and "),
                        MessagePart.ExpressIf(e.PurpleForces > 0, e.PurpleForces, " ", player.Force),
                        MessagePart.ExpressIf(e.PurpleForces > 0 && e.PurpleSpecialForces > 0, " and "),
                        MessagePart.ExpressIf(e.PurpleSpecialForces > 0, e.PurpleSpecialForces, " ", player.SpecialForce));

                    if (e.PurpleLocation != null && e.Initiator == Faction.Yellow)
                    {
                        player.ShipSpecialForces(e.PurpleLocation, 1);
                        Log(e.Initiator, " place ", FactionSpecialForce.Yellow, " in ", e.PurpleLocation);
                    }

                    break;

                case Faction.Brown:
                    if (CurrentMainPhase == MainPhase.Collection)
                    {
                        PlayNexusCard(e.Player, "discard a ", TreacheryCardType.Useless, " card to get ", Payment.Of(2));
                        Discard(e.Player, e.BrownCard);
                        e.Player.Resources += 2;
                    }
                    else if (CurrentPhase == Phase.BattleConclusion)
                    {
                        var auditee = CurrentBattle.OpponentOf(e.Initiator);
                        var recentBattlePlan = CurrentBattle.PlanOf(auditee);
                        var auditableCards = auditee.TreacheryCards.Where(c => c != recentBattlePlan.Weapon && c != recentBattlePlan.Defense && c != recentBattlePlan.Hero);

                        PlayNexusCard(e.Player, "see a random treachery card in the ", auditee.Faction, " hand");

                        if (auditableCards.Any())
                        {
                            var auditedCard = auditableCards.RandomOrDefault(Random);
                            RegisterKnown(e.Player, auditedCard);
                            LogTo(e.Initiator, "You see: ", auditedCard);
                            LogTo(auditee.Faction, "You showed them: ", auditedCard);
                        }
                        else
                        {
                            Log(Auditee.Faction, " don't have cards to audit");
                        }
                    }
                    break;

                case Faction.Pink:
                    PlayNexusCard(e.Player, "force ", e.PinkFaction, " to reveal if they have an ", Faction.Pink, " traitor");
                    Log(e.PinkFaction, " reveal to ", e.Initiator, " if they have a ", e.Initiator, " traitor");
                    var hasTraitor = GetPlayer(e.PinkFaction).Traitors.Any(t => t.Faction == e.Initiator);
                    LogTo(e.Initiator, e.PinkFaction, " reveal that they ", hasTraitor ? "DO" : "DON'T", " have a ", e.Initiator, " traitor ");
                    LogTo(e.PinkFaction, " you revealed to them that you ", hasTraitor ? "DO" : "DON'T", " have a ", e.Initiator, " traitor ");
                    break;

            }
        }

        public void HandleEvent(ResourcesTransferred e)
        {
            Log(e);
            e.Player.Resources -= e.Resources;
            e.Player.TransferrableResources -= e.Resources;
            e.Player.AlliedPlayer.Resources += e.Resources;
        }

        private void LogPreventionByKarma(FactionAdvantage prevented)
        {
            Log(TreacheryCardType.Karma, " prevents ", prevented);
        }

        private void LogPreventionByLowThreshold(FactionAdvantage prevented)
        {
            Log("Low Threshold prevents ", prevented);
        }

        private void LogPreventionByHighThreshold(FactionAdvantage prevented)
        {
            Log("Low Threshold prevents ", prevented);
        }

        public bool CharityIsCancelled => EconomicsStatus == BrownEconomicsStatus.Cancel || EconomicsStatus == BrownEconomicsStatus.CancelFlipped;
    }

    class TriggeredBureaucracy
    {
        internal Faction PaymentFrom;
        internal Faction PaymentTo;
    }
}
