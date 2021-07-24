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
        public PlayerSequence BidSequence { get; set; }
        public Deck<TreacheryCard> CardsOnAuction { get; private set; }
        public AuctionType CurrentAuctionType { get; private set; }
        public int CardNumber { get; private set; }
        public IBid CurrentBid { get; private set; } = null;
        public Dictionary<Faction, IBid> Bids { get; private set; } = new Dictionary<Faction, IBid>();
        private Faction FirstFactionToBid { get; set; }
        private bool GreySwappedCardOnBid { get; set; }
        private bool CardWasSoldOnBlackMarket { get; set; }
        private bool DrawingCardsForRegularBiddingHasHappened { get; set; }
        private bool BiddingRoundWasStarted { get; set; }
        private bool WhiteAuctionShouldStillHappen { get; set; }
        private int NumberOfCardsOnAuction { get; set; }

        private void EnterBiddingPhase()
        {
            MainPhaseStart(MainPhase.Bidding);
            Allow(FactionAdvantage.BrownControllingCharity);
            Allow(FactionAdvantage.BlueCharity);
            ReceiveResourceTechIncome();
            CardsOnAuction = new Deck<TreacheryCard>(Random);
            GreySwappedCardOnBid = false;
            CardWasSoldOnBlackMarket = false;
            DrawingCardsForRegularBiddingHasHappened = false;
            BiddingRoundWasStarted = false;
            WhiteAuctionShouldStillHappen = false;
            CurrentAuctionType = AuctionType.None;

            var white = GetPlayer(Faction.White);
            Enter(white != null && !Prevented(FactionAdvantage.WhiteBlackMarket) && white.TreacheryCards.Count > 0 && Players.Count > 1, Phase.BlackMarketAnnouncement, EnterWhiteBidding);
        }

        public void HandleEvent(WhiteAnnouncesBlackMarket e)
        {
            CurrentReport.Add(e);

            if (!e.Passed)
            {
                Enter(Phase.BlackMarketBidding);
                CardsOnAuction.PutOnTop(e.Card);
                e.Player.TreacheryCards.Remove(e.Card);
                Bids.Clear();
                CurrentBid = null;
                StartBidSequenceAndAuctionType(e.AuctionType, e.Player, e.Direction);
            }
            else
            {
                EnterWhiteBidding();
            }
        }

        public void HandleEvent(BlackMarketBid bid)
        {
            BidSequence.NextPlayer(this, true);
            SkipPlayersThatCantBid(BidSequence);
            RecentMilestones.Add(Milestone.Bid);

            if (Bids.ContainsKey(bid.Initiator))
            {
                Bids.Remove(bid.Initiator);
            }

            Bids.Add(bid.Initiator, bid);

            if (!bid.Passed)
            {
                CurrentBid = bid;
            }

            switch (CurrentAuctionType)
            {
                case AuctionType.BlackMarketNormal:

                    if (bid.Passed)
                    {
                        if (CurrentBid != null && BidSequence.CurrentFaction == CurrentBid.Initiator)
                        {
                            WinByHighestBid(
                                CurrentBid.Player, 
                                CurrentBid.Amount, 
                                CurrentBid.AllyContributionAmount, 
                                CurrentBid.RedContributionAmount,
                                CurrentBid.Initiator != Faction.White ? Faction.White : Faction.Red, 
                                CardsOnAuction);

                            CardWasSoldOnBlackMarket = true;
                            EnterWhiteBidding();
                        }
                        else if (CurrentBid == null && FirstFactionToBid == BidSequence.CurrentFaction)
                        {
                            CurrentReport.Add("Card not sold as no faction bid on it.");
                            EnterWhiteBidding();
                        }
                    }

                    break;

                case AuctionType.BlackMarketOnceAround:
                case AuctionType.BlackMarketSilent:

                    if (Players.Count(p => p.HasRoomForCards) == Bids.Count)
                    {
                        var highestBid = DetermineHighestBid(Bids);
                        if (highestBid != null && highestBid.TotalAmount > 0)
                        {
                            if (CurrentAuctionType == AuctionType.BlackMarketSilent)
                            {
                                CurrentReport.Add(string.Join(", ", Bids.Select(b => Skin.Current.Format("{0} bid {1}", b.Key, b.Value.TotalAmount))));
                            }
                                                        
                            WinByHighestBid(
                                highestBid.Player,
                                highestBid.Amount, 
                                highestBid.AllyContributionAmount, 
                                highestBid.RedContributionAmount, 
                                highestBid.Initiator != Faction.White ? Faction.White : Faction.Red, 
                                CardsOnAuction);

                            CardWasSoldOnBlackMarket = true;
                            EnterWhiteBidding();
                        }
                        else
                        {
                            CurrentReport.Add("Card not sold as no faction bid on it.");
                            EnterWhiteBidding();
                        }
                    }

                    break;
            }
        }

        private IBid DetermineHighestBid(Dictionary<Faction, IBid> bids)
        {
            int highestBidValue = bids.Values.Max(b => b.TotalAmount);
            var determineBidWinnerSequence = new PlayerSequence(Players);
            determineBidWinnerSequence.Start(this, false);
            for (int i = 0; i < MaximumNumberOfPlayers; i++)
            {
                var f = determineBidWinnerSequence.CurrentFaction;
                if (bids.ContainsKey(f) && bids[f].TotalAmount == highestBidValue)
                {
                    return bids[f];
                }
                determineBidWinnerSequence.NextPlayer(this, false);
            }
            
            return null;
        }

        private void EnterWhiteBidding()
        {
            NumberOfCardsOnAuction = PlayersThatCanBid.Count();
            CardNumber = 1;

            if (CardWasSoldOnBlackMarket)
            {
                NumberOfCardsOnAuction--;
            }

            if (NumberOfCardsOnAuction == 0)
            {
                CurrentReport.Add("Bidding is skipped because no faction is able to buy cards");
                EndBiddingPhase();
            }
            else
            {
                Enter(IsPlaying(Faction.White) && WhiteCache.Count > 0, Phase.WhiteAnnouncingAuction, DrawCardsForRegularBidding);
            }
        }

        public void HandleEvent(WhiteAnnouncesAuction e)
        {
            CurrentReport.Add(e);
            if (!e.First && NumberOfCardsOnAuction > 1) WhiteAuctionShouldStillHappen = true;
            if (NumberOfCardsOnAuction == 1) DrawingCardsForRegularBiddingHasHappened = true;
            Enter(e.First || NumberOfCardsOnAuction == 1, Phase.WhiteSpecifyingAuction, DrawCardsForRegularBidding);
        }

        private void DrawCardsForRegularBidding()
        {
            DrawingCardsForRegularBiddingHasHappened = true;
            int numberOfCardsToDraw = NumberOfCardsOnAuction;

            if (IsPlaying(Faction.White))
            {
                numberOfCardsToDraw--;
            }

            if (numberOfCardsToDraw > 0 && IsPlaying(Faction.Grey))
            {
                numberOfCardsToDraw++;
            }

            for (int i = 0; i < numberOfCardsToDraw; i++)
            {
                var card = DrawTreacheryCard();
                if (card != null)
                {
                    if (Version <= 86)
                    {
                        CardsOnAuction.PutOnTop(card);
                    }
                    else
                    {
                        CardsOnAuction.PutOnBottom(card);
                    }
                }
            }

            StartBidSequenceAndAuctionType(AuctionType.Normal);
            Enter(IsPlaying(Faction.Grey) && !Prevented(FactionAdvantage.GreySelectingCardsOnAuction), Phase.GreyRemovingCardFromBid, StartBiddingRound);
        }

        //This could happen before starting normal bidding or right after normal bidding
        public void HandleEvent(WhiteSpecifiesAuction e)
        {
            WhiteAuctionShouldStillHappen = false;
            CurrentReport.Add(e);
            CardsOnAuction.PutOnTop(e.Card);
            WhiteCache.Remove(e.Card);
            StartBidSequenceAndAuctionType(e.AuctionType, e.Player, e.Direction);
            StartBiddingRound();
        }

        private void StartBidSequenceAndAuctionType(AuctionType auctionType, Player whitePlayer = null, int direction = 1)
        {
            if (
                (auctionType == AuctionType.Normal || auctionType == AuctionType.WhiteNormal) && 
                (CurrentAuctionType != AuctionType.BlackMarketNormal || CurrentAuctionType != AuctionType.Normal || CurrentAuctionType != AuctionType.WhiteNormal))
            {
                //We want to start normal bidding and the previous auction type was not normal bidding
                BidSequence.Start(this, true);
                SkipPlayersThatCantBid(BidSequence);
            }
            else if (auctionType == AuctionType.BlackMarketNormal)
            {
                BidSequence.Start(this, true);
                SkipPlayersThatCantBid(BidSequence);
            }
            else if (auctionType == AuctionType.BlackMarketOnceAround || auctionType == AuctionType.WhiteOnceAround)
            {
                BidSequence.Start(whitePlayer, direction);
                BidSequence.NextPlayer(this, true);
            }

            FirstFactionToBid = BidSequence.CurrentFaction;
            CurrentAuctionType = auctionType;
        }

        public void HandleEvent(GreyRemovedCardFromAuction e)
        {
            CardsOnAuction.Items.Remove(e.Card);

            if (e.PutOnTop)
            {
                TreacheryDeck.PutOnTop(e.Card);
            }
            else
            {
                TreacheryDeck.PutOnBottom(e.Card);
            }

            RegisterKnown(Faction.Grey, e.Card);
            CardsOnAuction.Shuffle();
            RecentMilestones.Add(Milestone.Shuffled);
            CurrentReport.Add(e);
            Enter(GreyMaySwapCardOnBid, Phase.GreySwappingCard, StartBiddingRound);
        }

        private bool GreyMaySwapCardOnBid
        {
            get
            {
                var grey = GetPlayer(Faction.Grey);
                return grey != null && grey.TreacheryCards.Count > 0 && Applicable(Rule.GreyAndPurpleExpansionGreySwappingCardOnBid) && !GreySwappedCardOnBid;
            }
        }

        public void HandleEvent(GreySwappedCardOnBid e)
        {
            if (!e.Passed)
            {
                GreySwappedCardOnBid = true;
                var initiator = GetPlayer(e.Initiator);
                initiator.TreacheryCards.Remove(e.Card);
                initiator.TreacheryCards.Add(CardsOnAuction.Draw());

                foreach (var p in Players.Where(p => !HasBiddingPrescience(p))) {

                    UnregisterKnown(p, initiator.TreacheryCards);
                }

                CardsOnAuction.PutOnTop(e.Card);
                RegisterKnown(Faction.Grey, e.Card);
                RecentMilestones.Add(Milestone.CardOnBidSwapped);
            }

            CurrentReport.Add(e);

            if (!BiddingRoundWasStarted)
            {
                StartBiddingRound();
            }
            else
            {
                Enter(IsPlaying(Faction.Green), Phase.WaitingForNextBiddingRound, PutNextCardOnAuction);
            }
        }

        private void StartBiddingRound()
        {
            BiddingRoundWasStarted = true;
            Enter(Phase.Bidding);

            if (Players.Count == 1 && CardsOnAuction.Items.Count == 1)
            {
                CurrentReport.Add(Players[0].Faction, "{0} are playing alone and therefore win the auction.", Players[0].Faction);
                var drawnCard = CardsOnAuction.Draw();
                Players[0].TreacheryCards.Add(drawnCard);
                GiveHarkonnenExtraCard(Players[0]);
                EndBiddingPhase();
            }
            else
            {
                CurrentBid = null;
                Bids.Clear();
            }
        }

        private void ReceiveResourceTechIncome()
        {
            if (ResourceTechTokenIncome)
            {
                var techTokenOwner = Players.FirstOrDefault(p => p.TechTokens.Contains(TechToken.Resources));
                if (techTokenOwner != null)
                {
                    var amount = techTokenOwner.TechTokens.Count;
                    techTokenOwner.Resources += amount;
                    CurrentReport.Add(techTokenOwner.Faction, "{0} receive {1} from {2}.", techTokenOwner.Faction, amount, TechToken.Resources);
                }
            }
        }

        public TreacheryCard GetCardSetAsideForBid(Player p)
        {
            if (CardUsedForKarmaBid != null && CardUsedForKarmaBid.Item1 == p)
            {
                return CardUsedForKarmaBid.Item2;
            }
            else
            {
                return null;
            }
        }

        private Tuple<Player, TreacheryCard> CardUsedForKarmaBid = null;
        public void HandleEvent(Bid bid)
        {
            if (!bid.Passed)
            {
                ReturnKarmaCardUsedForBid();

                if (bid.UsingKarmaToRemoveBidLimit)
                {
                    SetAsideKarmaCardUsedForBid(bid);
                }

                CurrentBid = bid;
                RecentMilestones.Add(Milestone.Bid);
            }

            BidSequence.NextPlayer(this, true);
            SkipPlayersThatCantBid(BidSequence);

            if (Bids.ContainsKey(bid.Initiator))
            {
                Bids.Remove(bid.Initiator);
            }

            Bids.Add(bid.Initiator, bid);

            switch (CurrentAuctionType)
            {
                case AuctionType.Normal:
                case AuctionType.WhiteNormal:

                    if (bid.Passed || bid.KarmaBid)
                    {
                        if (bid.KarmaBid)
                        {
                            //Immediate Karma
                            var card = WinWithKarma(bid, CardsOnAuction);
                            FinishBid(bid.Player, card);
                        }
                        else if (CurrentBid != null && BidSequence.CurrentFaction == CurrentBid.Initiator)
                        {
                            if (((Bid)CurrentBid).UsingKarmaToRemoveBidLimit)
                            {
                                //Karma was used to bid any amount
                                ReturnKarmaCardUsedForBid();
                                var card = WinWithKarma((Bid)CurrentBid, CardsOnAuction);
                                FinishBid(CurrentBid.Player, card);
                            }
                            else
                            {
                                var card = WinByHighestBid(CurrentBid.Player, CurrentBid.Amount, CurrentBid.AllyContributionAmount, CurrentBid.RedContributionAmount, Faction.Red, CardsOnAuction);
                                FinishBid(CurrentBid.Player, card);
                            }
                        }
                        else if (CurrentBid == null && FirstFactionToBid == BidSequence.CurrentFaction)
                        {
                            EveryonePassed();
                        }
                    }
                    else if (BidSequence.CurrentFaction == bid.Initiator)
                    {
                        var card = BidByOnlyPlayer(bid, Faction.Red, CardsOnAuction);
                        FinishBid(CurrentBid.Player, card);
                    }

                    break;

                case AuctionType.WhiteOnceAround:
                case AuctionType.WhiteSilent:

                    if (Players.Count(p => p.HasRoomForCards) == Bids.Count)
                    {
                        var highestBid = DetermineHighestBid(Bids);
                        if (highestBid != null && highestBid.TotalAmount > 0)
                        {
                            if (CurrentAuctionType == AuctionType.WhiteSilent)
                            {
                                CurrentReport.Add(string.Join(", ", Bids.Select(b => Skin.Current.Format("{0} bid {1}", b.Key, b.Value.TotalAmount))));
                            }

                            var card = WinByHighestBid(
                                highestBid.Player, 
                                highestBid.Amount, 
                                highestBid.AllyContributionAmount, 
                                highestBid.RedContributionAmount, 
                                highestBid.Initiator != Faction.White ? Faction.White : Faction.Red, 
                                CardsOnAuction);

                            FinishBid(highestBid.Player, card);
                        }
                        else
                        {
                            CurrentReport.Add("Card not sold as no faction bid on it.");
                            var white = GetPlayer(Faction.White);
                            if (white.HasRoomForCards)
                            {
                                Enter(Phase.WhiteKeepingUnsoldCard);
                            }
                            else
                            {
                                var card = CardsOnAuction.Draw();
                                RegisterWonCardAsKnown(card);
                                CurrentReport.Add(Faction.None, "{0} was removed from the game.", card);
                                FinishBid(null, card);
                            }
                        }
                    }

                    break;

            }
        }

        public void HandleEvent(WhiteKeepsUnsoldCard e)
        {
            CurrentReport.Add(e);
            var card = CardsOnAuction.Draw();
            RegisterWonCardAsKnown(card);

            if (!e.Passed)
            {
                e.Player.TreacheryCards.Add(card);
                CurrentReport.Add(Faction.None, e.Initiator, "You get: {0}.", card);
                FinishBid(e.Player, card);
            }
            else
            {
                FinishBid(null, card);
            }
        }

        private void SetAsideKarmaCardUsedForBid(Bid bid)
        {
            //If this bid uses Karma, set aside the card until it is not the highest bid anymore
            var initiator = GetPlayer(bid.Initiator);
            var karmaCard = bid.GetKarmaCard();
            CardUsedForKarmaBid = new Tuple<Player, TreacheryCard>(initiator, karmaCard);
            initiator.TreacheryCards.Remove(karmaCard);
        }

        private void ReturnKarmaCardUsedForBid()
        {
            if (CardUsedForKarmaBid != null)
            {
                CardUsedForKarmaBid.Item1.TreacheryCards.Add(CardUsedForKarmaBid.Item2);
                CardUsedForKarmaBid = null;
            }
        }

        private TreacheryCard BidByOnlyPlayer(Bid bid, Faction paymentReceiver, Deck<TreacheryCard> toDrawFrom)
        {
            CurrentBid = bid;
            var winner = GetPlayer(CurrentBid.Initiator);
            var receiverIncomeMessage = new MessagePart("");
            PayForCard(winner, CurrentBid.Amount, CurrentBid.AllyContributionAmount, CurrentBid.RedContributionAmount, paymentReceiver, ref receiverIncomeMessage);
            LogBid(winner, CurrentBid.Amount, CurrentBid.AllyContributionAmount, CurrentBid.RedContributionAmount, receiverIncomeMessage);
            RecentMilestones.Add(Milestone.AuctionWon);
            var card = toDrawFrom.Draw();
            RegisterWonCardAsKnown(card);
            winner.TreacheryCards.Add(card);
            CurrentReport.Add(Faction.None, winner.Faction, "You won: {0}.", card);
            GiveHarkonnenExtraCard(winner);
            return card;
        }

        public void HandleEvent(RedBidSupport e)
        {
            PermittedUseOfRedSpice = e.Amounts;
            CurrentReport.Add(e);
        }

        private void LogBid(Player initiator, int bidAmount, int bidAllyContributionAmount, int bidRedContributionAmount, MessagePart redIncome)
        {
            int bidTotalAmount = bidAmount + bidAllyContributionAmount + bidRedContributionAmount;
            string cardNumberText = CurrentAuctionType == AuctionType.Normal ? CardNumber + " " : "";

            if (bidAllyContributionAmount > 0)
            {
                if (bidRedContributionAmount > 0)
                {
                    CurrentReport.Add(initiator.Faction, "Card {0} won by {1} for {2}, of which {3} paid {4} and {5} paid {6}.{7}", cardNumberText, initiator.Faction, bidTotalAmount, initiator.Ally, bidAllyContributionAmount, Faction.Red, bidRedContributionAmount, redIncome);
                }
                else
                {
                    CurrentReport.Add(initiator.Faction, "Card {0} won by {1} for {2}, of which {3} paid {4}.{5}", cardNumberText, initiator.Faction, bidTotalAmount, initiator.Ally, bidAllyContributionAmount, redIncome);
                }
            }
            else
            {
                if (bidRedContributionAmount > 0)
                {
                    CurrentReport.Add(initiator.Faction, "Card {0} won by {1} for {2}, of which {3} paid {4}.{5}", cardNumberText, initiator.Faction, bidTotalAmount, Faction.Red, bidRedContributionAmount, redIncome);
                }
                else
                {
                    CurrentReport.Add(initiator.Faction, "Card {0} won by {1} for {2}.{3}", cardNumberText, initiator.Faction, bidAmount, redIncome);
                }
            }
        }

        private void EveryonePassed()
        {
            CurrentReport.Add("Card is passed on by everyone; bidding ends and remaining cards are returned to the Treachery Deck.");
            RecentMilestones.Add(Milestone.AuctionWon);

            while (!CardsOnAuction.IsEmpty)
            {
                TreacheryDeck.PutOnTop(CardsOnAuction.Draw());
            }

            EndBiddingPhase();
        }

        private TreacheryCard WinByHighestBid(Player winner, int bidAmount, int bidAllyContributionAmount, int bidRedContributionAmount, Faction paymentReceiver, Deck<TreacheryCard> toDrawFrom)
        {
            var receiverIncomeMessage = new MessagePart("");
            PayForCard(winner, bidAmount, bidAllyContributionAmount, bidRedContributionAmount, paymentReceiver, ref receiverIncomeMessage);
            LogBid(winner, bidAmount, bidAllyContributionAmount, bidRedContributionAmount, receiverIncomeMessage);
            RecentMilestones.Add(Milestone.AuctionWon);
            var card = toDrawFrom.Draw();
            RegisterWonCardAsKnown(card);
            winner.TreacheryCards.Add(card);
            CurrentReport.Add(Faction.None, winner.Faction, "You won: {0}.", card);
            GiveHarkonnenExtraCard(winner);
            return card;
        }

        private void RegisterWonCardAsKnown(TreacheryCard card)
        {
            foreach (var p in Players.Where(p => HasBiddingPrescience(p)))
            {
                RegisterKnown(p, card);
            }
        }

        private TreacheryCard WinWithKarma(Bid bid, Deck<TreacheryCard> toDrawFrom)
        {
            var winner = GetPlayer(bid.Initiator);
            var karmaCard = bid.GetKarmaCard();

            Discard(karmaCard);

            if (karmaCard.Type == TreacheryCardType.Karma)
            {
                CurrentReport.Add(bid.Initiator, "Card {2} won by {0} using {1}.", bid.Initiator, TreacheryCardType.Karma, CardNumber);
            }
            else
            {
                CurrentReport.Add(bid.Initiator, "Card {2} won by {0} using {1} for {3}.", bid.Initiator, karmaCard, CardNumber, TreacheryCardType.Karma);
            }

            RecentMilestones.Add(Milestone.AuctionWon);
            RecentMilestones.Add(Milestone.Karma);

            var card = toDrawFrom.Draw();
            winner.TreacheryCards.Add(card);
            RegisterWonCardAsKnown(card);
            CurrentReport.Add(Faction.None, winner.Faction, "You won: {0}.", card);
            GiveHarkonnenExtraCard(winner);
            return card;
        }

        private void SkipPlayersThatCantBid(PlayerSequence sequence)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (sequence.CurrentPlayer.HasRoomForCards) break;
                sequence.NextPlayer(this, Version >= 50);
            }
        }

        private void PayForCard(Player initiator, int bidAmount, int bidAllyContributionAmount, int bidRedContributionAmount, Faction paymentReceiver, ref MessagePart message)
        {
            initiator.Resources -= bidAmount;

            if (bidAllyContributionAmount > 0)
            {
                GetPlayer(initiator.Ally).Resources -= bidAllyContributionAmount;
                if (Version >= 76) DecreasePermittedUseOfAllySpice(initiator.Faction, bidAllyContributionAmount);
            }

            if (bidRedContributionAmount > 0)
            {
                GetPlayer(Faction.Red).Resources -= bidRedContributionAmount;
            }

            var receiver = GetPlayer(paymentReceiver);
            
            if (receiver != null && initiator.Faction != paymentReceiver)
            {
                if (paymentReceiver != Faction.Red || !Prevented(FactionAdvantage.RedReceiveBid))
                {
                    if (bidRedContributionAmount > 0)
                    {
                        var receiverProfit = bidAmount + bidAllyContributionAmount;
                        var receiverProfitAfterBidding = bidRedContributionAmount;
                        message = new MessagePart(" {0} receive {1} immediately and {2} at the end of the bidding phase.", receiver, receiverProfit, receiverProfitAfterBidding);
                        receiver.Resources += receiverProfit;
                        receiver.ResourcesAfterBidding += receiverProfitAfterBidding;
                    }
                    else
                    {
                        var receiverProfit = bidAmount + bidAllyContributionAmount + bidRedContributionAmount;
                        message = new MessagePart(" {0} receive {1}.", paymentReceiver, receiverProfit);
                        receiver.Resources += receiverProfit;
                    }
                }
                else
                {
                    message = new MessagePart(" {0} prevents {1} from receiving {2} for this card.", TreacheryCardType.Karma, paymentReceiver, Concept.Resource);
                    if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.RedReceiveBid);
                }
            }
        }

        private void GiveHarkonnenExtraCard(Player initiator)
        {
            if (initiator.Is(Faction.Black) && initiator.TreacheryCards.Count < 8)
            {
                if (!Prevented(FactionAdvantage.BlackFreeCard))
                {
                    var extraCard = DrawTreacheryCard();

                    if (extraCard != null)
                    {
                        initiator.TreacheryCards.Add(extraCard);
                        CurrentReport.Add(initiator.Faction, "{0} receive an extra treachery card.", initiator.Faction);
                        CurrentReport.Add(Faction.None, initiator.Faction, "Your extra card is: {0}.", extraCard);
                    }
                }
                else
                {
                    CurrentReport.Add(initiator.Faction, "{0} prevented {1} from receiving an extra treachery card.", TreacheryCardType.Karma, initiator.Faction);

                    if (Version >= 38)
                    {
                        if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.BlackFreeCard);
                    }
                }
            }
        }

        private TreacheryCard DrawTreacheryCard()
        {
            if (TreacheryDeck.IsEmpty)
            {
                foreach (var i in TreacheryDiscardPile.Items)
                {
                    TreacheryDeck.Items.Add(i);
                    UnregisterKnown(i);
                }

                TreacheryDiscardPile.Clear();
                TreacheryDeck.Shuffle();
                RecentMilestones.Add(Milestone.Shuffled);
                CurrentReport.Add("Reshuffled {0} cards from the treachery discard pile.", TreacheryDiscardPile.Items.Count);
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

        public TreacheryCard CardJustWon = null;

        private void FinishBid(Player winner, TreacheryCard card)
        {
            CardJustWon = card;
            CurrentBid = null;
            Bids.Clear();

            if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.GreenBiddingPrescience);

            if (winner != null && winner.Ally == Faction.Grey && GreyAllyMayReplaceCards)
            {
                if (Version <= 93)
                {
                    Enter(Phase.ReplacingCardJustWon);
                }
                else
                {
                    if (Prevented(FactionAdvantage.GreyAllyDiscardingCard))
                    {
                        if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.GreyAllyDiscardingCard);
                        DetermineNextCardToBePutOnAuction();
                    }
                    else
                    {
                        Enter(Phase.ReplacingCardJustWon);
                    }
                }
            }
            else
            {
                DetermineNextCardToBePutOnAuction();
            }
        }

        public void HandleEvent(ReplacedCardWon e)
        {
            if (!e.Passed)
            {
                Discard(CardJustWon);
                var initiator = GetPlayer(e.Initiator);
                var newCard = DrawTreacheryCard();
                initiator.TreacheryCards.Add(newCard);
                CurrentReport.Add(Faction.None, initiator.Faction, "You replaced your {0} with {1}.", CardJustWon, newCard);
                RecentMilestones.Add(Milestone.CardWonSwapped);
            }

            CurrentReport.Add(e);
            DetermineNextCardToBePutOnAuction();
        }

        private void DetermineNextCardToBePutOnAuction()
        {
            if (!CardsOnAuction.IsEmpty)
            {
                CardNumber++;
                Enter(GreyMaySwapCardOnBid, Phase.GreySwappingCard, IsPlaying(Faction.Green), Phase.WaitingForNextBiddingRound, PutNextCardOnAuction);
            }
            else
            {
                //there are no more cards on auction

                if (DrawingCardsForRegularBiddingHasHappened)
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
                    DrawCardsForRegularBidding();
                }
            }
        }

        private void PutNextCardOnAuction()
        {
            Enter(Phase.Bidding);
            BidSequence.NextRound(this, true);
            SkipPlayersThatCantBid(BidSequence);
            FirstFactionToBid = BidSequence.CurrentFaction;
        }

        public IEnumerable<Player> PlayersThatCanBid => Players.Where(p => p.HasRoomForCards);

        public int KarmaHandSwapNumberOfCards;
        public Faction KarmaHandSwapTarget;
        public Phase KarmaHandSwapPausedPhase;

        public void HandleEvent(KarmaHandSwapInitiated e)
        {
            KarmaHandSwapPausedPhase = CurrentPhase;
            Enter(Phase.PerformingKarmaHandSwap);

            var initiator = GetPlayer(e.Initiator);
            var victim = GetPlayer(e.Target);

            initiator.SpecialKarmaPowerUsed = true;
            Discard(initiator, TreacheryCardType.Karma);

            KarmaHandSwapNumberOfCards = victim.TreacheryCards.Count;
            KarmaHandSwapTarget = e.Target;

            var cardsToDrawFrom = new Deck<TreacheryCard>(victim.TreacheryCards, Random);
            RecentMilestones.Add(Milestone.Shuffled);
            cardsToDrawFrom.Shuffle();
            for (int i = 0; i < KarmaHandSwapNumberOfCards; i++)
            {
                var card = cardsToDrawFrom.Draw();
                RegisterKnown(victim, card);
                victim.TreacheryCards.Remove(card);
                initiator.TreacheryCards.Add(card);
            }

            CurrentReport.Add(e);
            RecentMilestones.Add(Milestone.Karma);
        }

        public void HandleEvent(KarmaHandSwap e)
        {
            var initiator = GetPlayer(e.Initiator);
            var victim = GetPlayer(KarmaHandSwapTarget);

            foreach (var p in Players.Where(p => p != initiator && p != victim))
            {
                UnregisterKnown(p, initiator.TreacheryCards);
                UnregisterKnown(p, victim.TreacheryCards);
            }

            foreach (var returned in e.ReturnedCards)
            {
                victim.TreacheryCards.Add(returned);
                initiator.TreacheryCards.Remove(returned);
            }

            foreach (var returned in e.ReturnedCards)
            {
                RegisterKnown(initiator, returned);
            }

            CurrentReport.Add(e);
            Enter(KarmaHandSwapPausedPhase);
        }

        private void EndBiddingPhase()
        {
            var red = GetPlayer(Faction.Red);
            if (red != null)
            {
                red.Resources += red.ResourcesAfterBidding;
                red.ResourcesAfterBidding = 0;
            }

            MainPhaseEnd();
            Enter(Phase.BiddingReport);
        }

        public int NumberOfCardsOnRegularAuction => CardNumber + CardsOnAuction.Items.Count - 1;
    }
}
