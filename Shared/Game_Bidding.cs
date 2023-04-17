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

        public PlayerSequence BidSequence { get; private set; }
        public Deck<TreacheryCard> CardsOnAuction { get; private set; }
        public AuctionType CurrentAuctionType { get; private set; }
        public int CardNumber { get; private set; }
        public IBid CurrentBid { get; internal set; }
        public Dictionary<Faction, IBid> Bids { get; } = new();
        public TreacheryCard CardJustWon { get; internal set; }
        public IBid WinningBid { get; internal set; }
        internal bool BiddingRoundWasStarted { get; set; }
        internal bool RegularBiddingIsDone { get; set; }
        public TreacheryCard CardThatMustBeKeptOrGivenToAlly { get; internal set; }
        internal int NumberOfCardsOnAuction { get; set; }
        internal TriggeredBureaucracy BiddingTriggeredBureaucracy { get; set; }
        internal bool GreySwappedCardOnBid { get; set; }

        public Faction FactionThatMayReplaceBoughtCard { get; internal set; }
        public bool ReplacingBoughtCardUsingNexus { get; internal set; }

        public int KarmaHandSwapNumberOfCards { get; internal set; }
        public Faction KarmaHandSwapTarget { get; internal set; }
        internal Phase KarmaHandSwapPausedPhase { get; set; }

        private AuctionType BlackMarketAuctionType { get; set; }
        internal TreacheryCard CardSoldOnBlackMarket { get; set; }
        internal bool WhiteAuctionShouldStillHappen { get; set; }
        public bool WhiteBiddingJustFinished { get; private set; }
        public bool WhiteOccupierSpecifiedCard { get; internal set; }

        #endregion

        #region BeginningOfBidding

        internal void EnterBiddingPhase()
        {
            MainPhaseStart(MainPhase.Bidding);
            Allow(FactionAdvantage.BrownControllingCharity);
            Allow(FactionAdvantage.BlueCharity);
            CardsOnAuction = new Deck<TreacheryCard>(Random);
            GreySwappedCardOnBid = false;
            CardSoldOnBlackMarket = null;
            BlackMarketAuctionType = AuctionType.None;
            RegularBiddingIsDone = false;
            BiddingRoundWasStarted = false;
            WhiteAuctionShouldStillHappen = false;
            CurrentAuctionType = AuctionType.None;
            BiddingTriggeredBureaucracy = null;
            BidSequence = new PlayerSequence(this);
            WinningBid = null;

            Enter(Version > 150, Phase.BeginningOfBidding, StartBiddingPhase);
        }

        internal void StartBiddingPhase()
        {
            var white = GetPlayer(Faction.White);
            Enter(white != null && Applicable(Rule.WhiteBlackMarket) && !Prevented(FactionAdvantage.WhiteBlackMarket) && white.TreacheryCards.Count > 0,
                Phase.BlackMarketAnnouncement,
                EnterWhiteBidding);
        }

        internal void EnterWhiteBidding()
        {
            NumberOfCardsOnAuction = PlayersThatCanBid.Count();

            if (CardSoldOnBlackMarket != null)
            {
                NumberOfCardsOnAuction--;
            }

            CardSoldOnBlackMarket = null;

            if (NumberOfCardsOnAuction == 0)
            {
                Log("Bidding is skipped because no faction is able to buy cards");
                EndBiddingPhase();
            }
            else
            {
                Enter(IsPlaying(Faction.White) && WhiteCache.Count > 0 && !Prevented(FactionAdvantage.WhiteAuction), Phase.WhiteAnnouncingAuction, StartRegularBidding);
            }
        }

        #endregion BeginningOfBidding

        #region Bidding

        internal void StartRegularBidding()
        {
            RegularBiddingIsDone = true;
            int numberOfCardsToDraw = NumberOfCardsOnAuction;

            if (Version <= 150)
            {
                if (IsPlaying(Faction.White) && (Version < 150 || !WhiteAuctionShouldStillHappen || WhiteAuctionShouldStillHappen && WhiteCache.Count > 0))
                {
                    numberOfCardsToDraw--;
                }
            }

            if (numberOfCardsToDraw > 0 && IsPlaying(Faction.Grey) && !Prevented(FactionAdvantage.GreySelectingCardsOnAuction))
            {
                numberOfCardsToDraw++;
            }

            Log(numberOfCardsToDraw, " cards were drawn for bidding");

            for (int i = 0; i < numberOfCardsToDraw; i++)
            {
                var card = DrawTreacheryCard();
                if (card != null)
                {
                    CardsOnAuction.PutOnBottom(card);
                }
            }

            StartBidSequenceAndAuctionType(AuctionType.Normal);

            Enter(IsPlaying(Faction.Grey) && !Prevented(FactionAdvantage.GreySelectingCardsOnAuction), Phase.GreyRemovingCardFromBid, StartBiddingRound);
        }

        internal void StartBiddingRound()
        {
            BiddingRoundWasStarted = true;
            SkipPlayersThatCantBid(BidSequence);
            Enter(Phase.Bidding);
            CurrentBid = null;
            Bids.Clear();
            CardNumber = 1;
        }
                
        internal void LogBid(Player initiator, int bidAmount, int bidAllyContributionAmount, int bidRedContributionAmount, MessagePart receiverIncome)
        {
            int bidTotalAmount = bidAmount + bidAllyContributionAmount + bidRedContributionAmount;
            var cardNumber = MessagePart.ExpressIf(CurrentAuctionType == AuctionType.Normal, CardNumber);

            Log(
                initiator.Faction,
                " get card ",
                cardNumber,
                " for ",
                Payment.Of(bidTotalAmount),
                MessagePart.ExpressIf(
                    bidAllyContributionAmount > 0 || bidRedContributionAmount > 0,
                    "(", Payment.Of(bidAllyContributionAmount, initiator.Ally), MessagePart.ExpressIf(bidRedContributionAmount > 0, " and ", Payment.Of(bidRedContributionAmount, Faction.Red)), ") "),
                receiverIncome);
        }

        internal TreacheryCard WinByHighestBid(Player winner, IBid bid, int bidAmount, int bidAllyContributionAmount, int bidRedContributionAmount, Faction paymentReceiver, Deck<TreacheryCard> toDrawFrom, bool usesRedSecretAlly)
        {
            var receiverIncomeMessage = MessagePart.Express();

            if (!usesRedSecretAlly)
            {
                PayForCard(winner, bid, bidAmount, bidAllyContributionAmount, bidRedContributionAmount, paymentReceiver, ref receiverIncomeMessage);
                LogBid(winner, bidAmount, bidAllyContributionAmount, bidRedContributionAmount, receiverIncomeMessage);
            }
            else
            {
                PlayNexusCard(winner, "Secret Ally", "get this card for free");
                LogBid(winner, 0, 0, 0, receiverIncomeMessage);
            }

            Stone(Milestone.AuctionWon);
            var card = toDrawFrom.Draw();
            RegisterWonCardAsKnown(card);
            winner.TreacheryCards.Add(card);
            LogTo(winner.Faction, "You won: ", card);
            GivePlayerExtraCardIfApplicable(winner);
            return card;
        }

        internal void RegisterWonCardAsKnown(TreacheryCard card)
        {
            foreach (var p in Players.Where(p => HasBiddingPrescience(p)))
            {
                RegisterKnown(p, card);
            }
        }

        internal void SkipPlayersThatCantBid(PlayerSequence sequence)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (sequence.CurrentPlayer.HasRoomForCards) break;
                sequence.NextPlayer();
            }
        }

        internal void PayForCard(Player initiator, IBid bid, int bidAmount, int bidAllyContributionAmount, int bidRedContributionAmount, Faction paymentReceiver, ref MessagePart message)
        {
            initiator.Resources -= bidAmount;

            int receiverProfit = 0;

            if (bidAllyContributionAmount > 0)
            {
                GetPlayer(initiator.Ally).Resources -= bidAllyContributionAmount;
                DecreasePermittedUseOfAllySpice(initiator.Faction, bidAllyContributionAmount);
            }

            if (bidRedContributionAmount > 0)
            {
                GetPlayer(Faction.Red).Resources -= bidRedContributionAmount;
            }

            var receiver = GetPlayer(paymentReceiver);
            var receiverAndAllyAreWhite = initiator.Ally == Faction.White && paymentReceiver == Faction.White;
            var totalAmount = bidAmount + (Version >= 139 && receiverAndAllyAreWhite ? 0 : bidAllyContributionAmount);

            if (receiver != null && initiator.Faction != paymentReceiver)
            {
                if (paymentReceiver == Faction.Red && Prevented(FactionAdvantage.RedReceiveBid))
                {
                    message = MessagePart.Express(" → ", paymentReceiver, " do not receive ", Concept.Resource, " for this card");
                    if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.RedReceiveBid);
                }
                else
                {
                    receiverProfit = totalAmount;
                    ModifyIncomeBasedOnThresholdOrOccupation(receiver, ref receiverProfit);
                    receiver.Resources += receiverProfit;
                    receiver.ResourcesAfterBidding += bidRedContributionAmount;
                    message = MessagePart.Express(" → ", receiver.Faction, " get ", Payment.Of(receiverProfit),
                        MessagePart.ExpressIf(bidRedContributionAmount > 0, " immediately and ", Payment.Of(bidRedContributionAmount), " at the end of the bidding phase"));

                    if (receiverProfit >= 5)
                    {
                        //This should also take into account payments received by an occupier
                        BiddingTriggeredBureaucracy = new TriggeredBureaucracy() { PaymentFrom = initiator.Faction, PaymentTo = paymentReceiver };
                    }

                    SetRecentPayment(receiverProfit, initiator.Faction, receiver.Faction, (GameEvent)bid);
                }
            }

            if (totalAmount - receiverProfit >= 4)
            {
                ActivateBanker(initiator);
            }
        }

        internal void GivePlayerExtraCardIfApplicable(Player winner)
        {
            if (winner.Is(Faction.Black))
            {
                bool extraCardMustBeDecidedAbout = false;
                Player receiver = null;

                var occupierOfBlackHomeworld = OccupierOf(World.Black);
                if (occupierOfBlackHomeworld != null)
                {
                    if (occupierOfBlackHomeworld.HasRoomForCards)
                    {
                        receiver = occupierOfBlackHomeworld;
                        extraCardMustBeDecidedAbout = occupierOfBlackHomeworld.HasAlly && occupierOfBlackHomeworld.AlliedPlayer.HasRoomForCards;

                    }
                    else if (occupierOfBlackHomeworld.HasAlly && occupierOfBlackHomeworld.AlliedPlayer.HasRoomForCards)
                    {
                        receiver = occupierOfBlackHomeworld.AlliedPlayer;
                    }
                }
                else if (winner.HasRoomForCards)
                {
                    if (!Prevented(FactionAdvantage.BlackFreeCard))
                    {
                        receiver = winner;
                    }
                    else
                    {
                        LogPreventionByKarma(FactionAdvantage.BlackFreeCard);
                        if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.BlackFreeCard);
                    }
                }

                if (receiver != null)
                {
                    var extraCard = DrawTreacheryCard();

                    if (extraCard != null)
                    {
                        receiver.TreacheryCards.Add(extraCard);
                        Log(receiver.Faction, " receive an extra treachery card");
                        LogTo(receiver.Faction, "Your extra card is: ", extraCard);

                        if (extraCardMustBeDecidedAbout)
                        {
                            CardThatMustBeKeptOrGivenToAlly = extraCard;
                        }
                    }
                }
            }
        }

        internal TreacheryCard DrawTreacheryCard()
        {
            if (TreacheryDeck.IsEmpty)
            {
                Log("Shuffled ", TreacheryDiscardPile.Items.Count, " cards from the treachery discard pile into a new deck");

                foreach (var i in TreacheryDiscardPile.Items)
                {
                    TreacheryDeck.Items.Add(i);
                    UnregisterKnown(i);
                }

                TreacheryDiscardPile.Clear();
                TreacheryDeck.Shuffle();
                Stone(Milestone.Shuffled);
            }

            if (TreacheryDeck.Items.Count > 0)
            {
                return TreacheryDeck.Draw();
            }
            else
            {
                return null;
            }
        }

        internal void FinishBid(Player winner, TreacheryCard card, bool mightReplace)
        {
            CardJustWon = card;
            WinningBid = CurrentBid;
            CurrentBid = null;
            FactionThatMayReplaceBoughtCard = Faction.None;

            Bids.Clear();

            if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.GreenBiddingPrescience);

            bool enterReplacingCardJustWon = mightReplace && Version > 150 && Players.Any(p => p.Nexus != Faction.None);

            if (mightReplace && winner != null)
            {
                if (winner.Ally == Faction.Grey && GreyAllowsReplacingCards)
                {
                    if (!Prevented(FactionAdvantage.GreyAllyDiscardingCard))
                    {
                        FactionThatMayReplaceBoughtCard = winner.Faction;
                        enterReplacingCardJustWon = true;
                    }
                    else
                    {
                        if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.GreyAllyDiscardingCard);

                        if (NexusAllowsReplacingBoughtCards(winner))
                        {
                            FactionThatMayReplaceBoughtCard = winner.Faction;
                            enterReplacingCardJustWon = true;
                            ReplacingBoughtCardUsingNexus = true;
                        }
                    }
                }
                else if (NexusAllowsReplacingBoughtCards(winner))
                {
                    FactionThatMayReplaceBoughtCard = winner.Faction;
                    enterReplacingCardJustWon = true;
                    ReplacingBoughtCardUsingNexus = true;
                }
            }

            Enter(enterReplacingCardJustWon, Phase.ReplacingCardJustWon, DetermineNextStepAfterCardWasSold);

            if (BiddingTriggeredBureaucracy != null)
            {
                ApplyBureaucracy(BiddingTriggeredBureaucracy.PaymentFrom, BiddingTriggeredBureaucracy.PaymentTo);
                BiddingTriggeredBureaucracy = null;
            }
        }

        internal void DetermineNextStepAfterCardWasSold()
        {
            WhiteBiddingJustFinished = CurrentAuctionType == AuctionType.WhiteOnceAround || CurrentAuctionType == AuctionType.WhiteSilent;

            if (!CardsOnAuction.IsEmpty)
            {
                MoveToNextCardOnAuction();
            }
            else
            {
                if (RegularBiddingIsDone)
                {
                    if (WhiteAuctionShouldStillHappen)
                    {
                        Enter(Phase.WhiteSpecifyingAuction);
                    }
                    else
                    {
                        EndBiddingPhase();
                    }
                }
                else
                {
                    StartRegularBidding();
                }
            }
        }

        private void MoveToNextCardOnAuction()
        {
            CardNumber++;
            CardThatMustBeKeptOrGivenToAlly = null;

            if (GreyMaySwapCardOnBid)
            {
                if (Version < 113 || !Prevented(FactionAdvantage.GreySwappingCard))
                {
                    Enter(Phase.GreySwappingCard);
                }
                else
                {
                    LogPreventionByKarma(FactionAdvantage.GreySwappingCard);
                    if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.GreySwappingCard);
                    Enter(Version >= 107 || IsPlaying(Faction.Green), Phase.WaitingForNextBiddingRound, PutNextCardOnAuction);
                }
            }
            else
            {
                Enter(Version >= 107 || IsPlaying(Faction.Green), Phase.WaitingForNextBiddingRound, PutNextCardOnAuction);
            }
        }

        internal void PutNextCardOnAuction()
        {
            if (!WhiteBiddingJustFinished)
            {
                BidSequence.NextRound();
            }

            SkipPlayersThatCantBid(BidSequence);
            WinningBid = null;
            Enter(Phase.Bidding);
        }

        internal void StartBidSequenceAndAuctionType(AuctionType auctionType, Player whitePlayer = null, int direction = 1)
        {
            switch (auctionType)
            {
                case AuctionType.BlackMarketNormal:
                    BlackMarketAuctionType = AuctionType.BlackMarketNormal;
                    BidSequence = new PlayerSequence(this, true, 1);
                    break;

                case AuctionType.BlackMarketOnceAround:
                    BlackMarketAuctionType = AuctionType.BlackMarketOnceAround;
                    BidSequence = new PlayerSequence(this, true, direction, whitePlayer, true);
                    break;

                case AuctionType.BlackMarketSilent:
                    BidSequence = new PlayerSequence(this, true, 1);
                    BlackMarketAuctionType = AuctionType.BlackMarketSilent;
                    break;

                case AuctionType.WhiteOnceAround:
                    BidSequence = new PlayerSequence(this, true, direction, whitePlayer, true);
                    break;

                case AuctionType.WhiteSilent:
                    BidSequence = new PlayerSequence(this, true, 1);
                    break;

                case AuctionType.Normal:
                    BidSequence = new PlayerSequence(this, true, 1);
                    if (BlackMarketAuctionType == AuctionType.BlackMarketNormal)
                    {
                        //Continue where black market bidding left off
                        BidSequence.NextRound();
                        WinningBid = null;
                    }
                    break;
            }

            CurrentAuctionType = auctionType;
        }

        internal IBid DetermineHighestBid(Dictionary<Faction, IBid> bids)
        {
            int highestBidValue = bids.Values.Max(b => b.TotalAmount);
            var determineBidWinnerSequence = new PlayerSequence(this, false, 1);

            for (int i = 0; i < MaximumNumberOfPlayers; i++)
            {
                var f = determineBidWinnerSequence.CurrentFaction;
                if (bids.TryGetValue(f, out IBid value) && value.TotalAmount == highestBidValue)
                {
                    return bids[f];
                }

                determineBidWinnerSequence.NextPlayer();
            }

            return null;
        }

        #endregion Bidding

        #region EndOfBidding

        internal void EndBiddingPhase()
        {
            CardThatMustBeKeptOrGivenToAlly = null;
            CurrentGreyNexus = null;
            var red = GetPlayer(Faction.Red);
            if (red != null)
            {
                red.Resources += red.ResourcesAfterBidding;
                red.ResourcesAfterBidding = 0;
            }

            MainPhaseEnd();
            Enter(Phase.BiddingReport);
        }

        #endregion

        #region Information

        public int NumberOfCardsOnRegularAuction => CardNumber + CardsOnAuction.Items.Count - 1;

        internal IEnumerable<Player> PlayersThatCanBid => Players.Where(p => p.HasRoomForCards);

        internal bool NexusAllowsReplacingBoughtCards(Player p) => (p.Nexus == Faction.Grey || p.Nexus == Faction.White) && NexusPlayed.CanUseSecretAlly(this, p);
        
        public bool HasBidToPay(Player p) => CurrentPhase == Phase.Bidding && CurrentBid != null &&
            (CurrentBid.Initiator == p.Faction || CurrentBid.Initiator == p.Ally && CurrentBid.AllyContributionAmount > 0);

        internal bool GreyMaySwapCardOnBid => GetPlayer(Faction.Grey)?.TreacheryCards.Count > 0 && Applicable(Rule.GreySwappingCardOnBid) && !GreySwappedCardOnBid;

        public IEnumerable<TreacheryCard> CardsOwnedBy(Player p)
        {
            var cardSetAside = GetCardSetAsideForBid(p);
            if (cardSetAside == null)
            {
                return p.TreacheryCards;
            }
            else
            {
                var result = p.TreacheryCards.ToList();
                result.Add(cardSetAside);
                return result;
            }
        }

        public TreacheryCard GetCardSetAsideForBid(Player p) => CurrentBid?.Player == p && CurrentBid.UsingKarmaToRemoveBidLimit ? CurrentBid.KarmaCard : null;

        #endregion Information
    }
}
