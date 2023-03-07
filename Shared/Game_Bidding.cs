/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Treachery.Shared
{
    public partial class Game
    {
        #region State

        public PlayerSequence BidSequence { get; private set; }
        public Deck<TreacheryCard> CardsOnAuction { get; private set; }
        public AuctionType CurrentAuctionType { get; private set; }
        public int CardNumber { get; private set; }
        public IBid CurrentBid { get; private set; }
        public Dictionary<Faction, IBid> Bids { get; private set; } = new Dictionary<Faction, IBid>();

        private bool GreySwappedCardOnBid { get; set; }
        private bool RegularBiddingIsDone { get; set; }
        private bool BiddingRoundWasStarted { get; set; }
        private bool WhiteAuctionShouldStillHappen { get; set; }
        private int NumberOfCardsOnAuction { get; set; }
        private TriggeredBureaucracy BiddingTriggeredBureaucracy { get; set; }
        private TreacheryCard CardSoldOnBlackMarket { get; set; }
        private IEnumerable<Player> PlayersThatCanBid => Players.Where(p => p.HasRoomForCards);

        #endregion

        #region BeginningOfBidding

        private void EnterBiddingPhase()
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

        private void StartBiddingPhase()
        {
            var white = GetPlayer(Faction.White);
            Enter(white != null && Applicable(Rule.WhiteBlackMarket) && !Prevented(FactionAdvantage.WhiteBlackMarket) && white.TreacheryCards.Count > 0,
                Phase.BlackMarketAnnouncement,
                EnterWhiteBidding);
        }

        #endregion

        #region BlackMarket

        public void HandleEvent(WhiteAnnouncesBlackMarket e)
        {
            Log(e);

            if (!e.Passed)
            {
                Enter(Phase.BlackMarketBidding);
                e.Player.TreacheryCards.Remove(e.Card);
                CardsOnAuction.PutOnTop(e.Card);
                RegisterKnown(e.Initiator, e.Card);
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
            BidSequence.NextPlayer();
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
                    HandleBlackMarketNormalBid(bid);
                    break;

                case AuctionType.BlackMarketOnceAround:
                case AuctionType.BlackMarketSilent:
                    HandleBlackMarketSpecialBid(bid);
                    break;
            }
        }

        private void HandleBlackMarketSpecialBid(BlackMarketBid bid)
        {
            var isLastBid = Version < 140 ? Players.Count(p => p.HasRoomForCards) == Bids.Count :
                CurrentAuctionType == AuctionType.BlackMarketSilent && Players.Count(p => p.HasRoomForCards) == Bids.Count ||
                CurrentAuctionType == AuctionType.BlackMarketOnceAround && bid.Initiator == Faction.White || !GetPlayer(Faction.White).HasRoomForCards && BidSequence.HasPassedWhite;

            if (isLastBid)
            {
                if (CurrentAuctionType == AuctionType.BlackMarketSilent)
                {
                    Log(Bids.Select(b => MessagePart.Express(b.Key, Payment(b.Value.TotalAmount), " ")).ToList());
                }

                var highestBid = DetermineHighestBid(Bids);
                if (highestBid != null && highestBid.TotalAmount > 0)
                {
                    var card = WinByHighestBid(
                        highestBid.Player,
                        highestBid,
                        highestBid.Amount,
                        highestBid.AllyContributionAmount,
                        highestBid.RedContributionAmount,
                        highestBid.Initiator != Faction.White ? Faction.White : Faction.Red,
                        CardsOnAuction, false);

                    FinishBlackMarketBid(highestBid.Player, card);
                }
                else
                {
                    GetPlayer(Faction.White).TreacheryCards.Add(CardsOnAuction.Draw());
                    Log(Faction.White, " keep their card as no faction bid on it");
                    EnterWhiteBidding();
                }
            }
        }

        private void HandleBlackMarketNormalBid(BlackMarketBid bid)
        {
            if (bid.Passed)
            {
                if (CurrentBid != null && BidSequence.CurrentFaction == CurrentBid.Initiator)
                {
                    var card = WinByHighestBid(
                        CurrentBid.Player,
                        CurrentBid,
                        CurrentBid.Amount,
                        CurrentBid.AllyContributionAmount,
                        CurrentBid.RedContributionAmount,
                        CurrentBid.Initiator != Faction.White ? Faction.White : Faction.Red,
                        CardsOnAuction, false);

                    FinishBlackMarketBid(CurrentBid.Player, card);
                }
                else if (CurrentBid == null && Bids.Count >= PlayersThatCanBid.Count())
                {
                    GetPlayer(Faction.White).TreacheryCards.Add(CardsOnAuction.Draw());
                    Log(Faction.White, " keep their card as no faction bid on it");
                    EnterWhiteBidding();
                }
            }
        }

        public Faction FactionThatMayReplaceBoughtCard { get; private set; }
        public bool ReplacingBoughtCardUsingNexus { get; private set; } = false;

        private void FinishBlackMarketBid(Player winner, TreacheryCard card)
        {
            CardJustWon = card;
            WinningBid = CurrentBid;
            CardSoldOnBlackMarket = card;
            FactionThatMayReplaceBoughtCard = Faction.None;
            bool enterReplacingCardJustWon = Version > 150 && Players.Any(p => p.Nexus != Faction.None);

            if (winner != null)
            {
                if (winner.Ally == Faction.Grey && AllyMayReplaceCards)
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
                            ReplacingBoughtCardUsingNexus = true;
                            enterReplacingCardJustWon = true;
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

            Enter(enterReplacingCardJustWon, Phase.ReplacingCardJustWon, EnterWhiteBidding);

            if (BiddingTriggeredBureaucracy != null)
            {
                ApplyBureaucracy(BiddingTriggeredBureaucracy.PaymentFrom, BiddingTriggeredBureaucracy.PaymentTo);
                BiddingTriggeredBureaucracy = null;
            }
        }

        private bool NexusAllowsReplacingBoughtCards(Player p) => (p.Nexus == Faction.Grey || p.Nexus == Faction.White) && NexusPlayed.CanUseSecretAlly(this, p);

        #endregion

        #region WhiteBidding

        private void EnterWhiteBidding()
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

        public bool WhiteOccupierSpecifiedCard { get; private set; }

        public void HandleEvent(WhiteAnnouncesAuction e)
        {
            WhiteOccupierSpecifiedCard = false;
            Log(e);

            int threshold = Version > 150 ? 0 : 1;
            if (Version > 150)
            {
                NumberOfCardsOnAuction--;
            }

            if (!e.First && NumberOfCardsOnAuction > threshold)
            {
                WhiteAuctionShouldStillHappen = true;
            }

            if (NumberOfCardsOnAuction == threshold)
            {
                RegularBiddingIsDone = true;
            }

            Enter(e.First || NumberOfCardsOnAuction == threshold, Phase.WhiteSpecifyingAuction, StartRegularBidding);
        }

        public void HandleEvent(WhiteSpecifiesAuction e)
        {
            if (!WhiteOccupierSpecifiedCard)
            {
                WhiteAuctionShouldStillHappen = false;
                CardsOnAuction.PutOnTop(e.Card);
                WhiteCache.Remove(e.Card);
                RegisterKnown(e.Card);

                var occupierOfWhiteHomeworld = OccupierOf(World.White);
                if (occupierOfWhiteHomeworld == null)
                {
                    Log(e);
                    StartBidSequenceAndAuctionType(e.AuctionType, e.Player, e.Direction);
                    StartBiddingRound();
                }
                else
                {
                    Log(e.Initiator, " put ", e.Card, " on auction");
                    WhiteOccupierSpecifiedCard = true;
                }
            }
            else
            {
                string directionText = "";
                if (e.AuctionType == AuctionType.WhiteOnceAround)
                {
                    if (e.Direction == 1)
                    {
                        directionText = " (counter-clockwise)";
                    }
                    else
                    {
                        directionText = " (clockwise)";
                    }
                }

                Log(e.Initiator, " put select a ", e.AuctionType, " auction", directionText);

                StartBidSequenceAndAuctionType(e.AuctionType, e.Player, e.Direction);
                StartBiddingRound();
            }
        }

        public void HandleEvent(WhiteKeepsUnsoldCard e)
        {
            Log(e);
            var card = CardsOnAuction.Draw();
            RegisterWonCardAsKnown(card);

            if (!e.Passed)
            {
                e.Player.TreacheryCards.Add(card);
                LogTo(e.Initiator, "You get: ", card);
                FinishBid(e.Player, card);
            }
            else
            {
                RemovedTreacheryCards.Add(card);
                FinishBid(null, card);
            }
        }

        private void HandleWhiteBid(Bid bid)
        {
            var isLastBid = Version < 140 ? Players.Count(p => p.HasRoomForCards) == Bids.Count :
                CurrentAuctionType == AuctionType.WhiteSilent && Players.Count(p => p.HasRoomForCards) == Bids.Count ||
                CurrentAuctionType == AuctionType.WhiteOnceAround && bid.Initiator == Faction.White || !GetPlayer(Faction.White).HasRoomForCards && BidSequence.HasPassedWhite;

            if (isLastBid)
            {
                if (CurrentAuctionType == AuctionType.WhiteSilent)
                {
                    Log("Bids: ", Bids.Select(b => MessagePart.Express(b.Key, Payment(b.Value.TotalAmount), " ")).ToList());
                }

                var highestBid = DetermineHighestBid(Bids);
                if (highestBid != null && highestBid.TotalAmount > 0)
                {
                    var card = WinByHighestBid(
                        highestBid.Player,
                        highestBid,
                        highestBid.Amount,
                        highestBid.AllyContributionAmount,
                        highestBid.RedContributionAmount,
                        highestBid.Initiator != Faction.White ? Faction.White : Faction.Red,
                        CardsOnAuction, false);

                    FinishBid(highestBid.Player, card);
                }
                else
                {
                    Log("Card not sold as no faction bid on it");
                    var white = GetPlayer(Faction.White);
                    if (white.HasRoomForCards)
                    {
                        Enter(Phase.WhiteKeepingUnsoldCard);
                    }
                    else
                    {
                        var card = CardsOnAuction.Draw();
                        RemovedTreacheryCards.Add(card);
                        RegisterWonCardAsKnown(card);
                        Log(card, " was removed from the game");
                        FinishBid(null, card);
                    }
                }
            }
        }

        #endregion

        #region Generic

        private void StartRegularBidding()
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
            Log(e);

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
                    StartBiddingRound();
                }
            }
            else
            {
                StartBiddingRound();
            }
        }

        private bool GreyMaySwapCardOnBid
        {
            get
            {
                var grey = GetPlayer(Faction.Grey);
                return grey != null && grey.TreacheryCards.Count > 0 && Applicable(Rule.GreySwappingCardOnBid) && !GreySwappedCardOnBid;
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

                foreach (var p in Players.Where(p => !HasBiddingPrescience(p)))
                {
                    UnregisterKnown(p, initiator.TreacheryCards);
                }

                CardsOnAuction.PutOnTop(e.Card);
                RegisterKnown(Faction.Grey, e.Card);
                RecentMilestones.Add(Milestone.CardOnBidSwapped);
                Log(e);
            }

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
            SkipPlayersThatCantBid(BidSequence);
            Enter(Phase.Bidding);
            CurrentBid = null;
            Bids.Clear();
            CardNumber = 1;
        }

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

        private Tuple<Player, TreacheryCard> CardUsedForKarmaBid { get; set; } = null;

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

            BidSequence.NextPlayer();
            SkipPlayersThatCantBid(BidSequence);

            if (Bids.ContainsKey(bid.Initiator))
            {
                Bids.Remove(bid.Initiator);
            }

            Bids.Add(bid.Initiator, bid);

            switch (CurrentAuctionType)
            {
                case AuctionType.Normal:

                    HandleNormalBid(bid);
                    break;

                case AuctionType.WhiteOnceAround:
                case AuctionType.WhiteSilent:

                    HandleWhiteBid(bid);
                    break;
            }
        }

        private void HandleNormalBid(Bid bid)
        {
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
                        var receiver = Faction.Red;
                        var card = WinByHighestBid(
                            CurrentBid.Player,
                            CurrentBid,
                            CurrentBid.Amount,
                            CurrentBid.AllyContributionAmount,
                            CurrentBid.RedContributionAmount,
                            receiver,
                            CardsOnAuction,
                            CurrentBid.UsesRedSecretAlly);

                        FinishBid(CurrentBid.Player, card);
                    }
                }
                else if (CurrentBid == null && Bids.Count >= PlayersThatCanBid.Count())
                {
                    EveryonePassed();
                }
            }
            else if (BidSequence.CurrentFaction == bid.Initiator)
            {
                var card = BidByOnlyPlayer(bid, Faction.Red, CardsOnAuction);
                FinishBid(CurrentBid.Player, card);
            }
        }

        private void SetAsideKarmaCardUsedForBid(Bid bid)
        {
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
            var receiverIncomeMessage = MessagePart.Express();

            if (!bid.UsesRedSecretAlly)
            {
                PayForCard(winner, bid, CurrentBid.Amount, CurrentBid.AllyContributionAmount, CurrentBid.RedContributionAmount, paymentReceiver, ref receiverIncomeMessage);
                LogBid(winner, CurrentBid.Amount, CurrentBid.AllyContributionAmount, CurrentBid.RedContributionAmount, receiverIncomeMessage);
            }
            else
            {
                LogNexusPlayed(winner.Faction, Faction.Red, "Secret Ally", "get this card for free");
                DiscardNexusCard(winner);
                LogBid(winner, 0, 0, 0, receiverIncomeMessage);
            }
            
            RecentMilestones.Add(Milestone.AuctionWon);
            var card = toDrawFrom.Draw();
            RegisterWonCardAsKnown(card);
            winner.TreacheryCards.Add(card);
            LogTo(winner.Faction, "You won: ", card);
            GivePlayerExtraCardIfApplicable(winner);
            return card;
        }

        private void LogBid(Player initiator, int bidAmount, int bidAllyContributionAmount, int bidRedContributionAmount, MessagePart receiverIncome)
        {
            int bidTotalAmount = bidAmount + bidAllyContributionAmount + bidRedContributionAmount;
            var cardNumber = MessagePart.ExpressIf(CurrentAuctionType == AuctionType.Normal, CardNumber);

            Log(
                initiator.Faction,
                " get card ",
                cardNumber,
                " for ",
                Payment(bidTotalAmount),
                MessagePart.ExpressIf(
                    bidAllyContributionAmount > 0 || bidRedContributionAmount > 0,
                    "(", Payment(bidAllyContributionAmount, initiator.Ally), MessagePart.ExpressIf(bidRedContributionAmount > 0, " and ", Payment(bidRedContributionAmount, Faction.Red)), ") "),
                receiverIncome);
        }

        private void EveryonePassed()
        {
            Log("Bid is passed by everyone; bidding ends and remaining cards are returned to the Treachery Deck");
            RecentMilestones.Add(Milestone.AuctionWon);

            while (!CardsOnAuction.IsEmpty)
            {
                if (Version >= 131)
                {
                    CardsOnAuction.Shuffle();
                }

                TreacheryDeck.PutOnTop(CardsOnAuction.Draw());
            }

            EndBiddingPhase();
        }

        private TreacheryCard WinByHighestBid(Player winner, IBid bid, int bidAmount, int bidAllyContributionAmount, int bidRedContributionAmount, Faction paymentReceiver, Deck<TreacheryCard> toDrawFrom, bool usesRedSecretAlly)
        {
            var receiverIncomeMessage = MessagePart.Express();

            if (!usesRedSecretAlly)
            {
                PayForCard(winner, bid, bidAmount, bidAllyContributionAmount, bidRedContributionAmount, paymentReceiver, ref receiverIncomeMessage);
                LogBid(winner, bidAmount, bidAllyContributionAmount, bidRedContributionAmount, receiverIncomeMessage);
            }
            else
            {
                LogNexusPlayed(winner.Faction, Faction.Red, "Secret Ally", "get this card for free");
                DiscardNexusCard(winner);
                LogBid(winner, 0, 0, 0, receiverIncomeMessage);
            }
                        
            RecentMilestones.Add(Milestone.AuctionWon);
            var card = toDrawFrom.Draw();
            RegisterWonCardAsKnown(card);
            winner.TreacheryCards.Add(card);
            LogTo(winner.Faction, "You won: ", card);
            GivePlayerExtraCardIfApplicable(winner);
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
                Log(bid.Initiator, " get card ", CardNumber, " using ", TreacheryCardType.Karma);
            }
            else
            {
                Log(bid.Initiator, " get card ", CardNumber, " using ", karmaCard, " for ", TreacheryCardType.Karma);
            }

            RecentMilestones.Add(Milestone.AuctionWon);
            RecentMilestones.Add(Milestone.Karma);

            var card = toDrawFrom.Draw();
            winner.TreacheryCards.Add(card);
            RegisterWonCardAsKnown(card);
            LogTo(winner.Faction, "You won: ", card);
            GivePlayerExtraCardIfApplicable(winner);
            return card;
        }

        private void SkipPlayersThatCantBid(PlayerSequence sequence)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (sequence.CurrentPlayer.HasRoomForCards) break;
                sequence.NextPlayer();
            }
        }

        private void PayForCard(Player initiator, IBid bid, int bidAmount, int bidAllyContributionAmount, int bidRedContributionAmount, Faction paymentReceiver, ref MessagePart message)
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
                    message = MessagePart.Express(" → ", receiver.Faction, " get ", Payment(receiverProfit), 
                        MessagePart.ExpressIf(bidRedContributionAmount > 0, " immediately and ", Payment(bidRedContributionAmount), " at the end of the bidding phase"));

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

        public TreacheryCard CardThatMustBeKeptOrGivenToAlly { get; private set; }
        private void GivePlayerExtraCardIfApplicable(Player winner)
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

        public void HandleEvent(CardGiven e)
        {
            Log(e);

            if (!e.Passed)
            {
                e.Player.TreacheryCards.Remove(CardThatMustBeKeptOrGivenToAlly);
                e.Player.AlliedPlayer.TreacheryCards.Add(CardThatMustBeKeptOrGivenToAlly);
            }

            CardThatMustBeKeptOrGivenToAlly = null;
        }

        private TreacheryCard DrawTreacheryCard()
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
                RecentMilestones.Add(Milestone.Shuffled);
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

        public TreacheryCard CardJustWon { get; private set; }

        public IBid WinningBid { get; private set; }

        private void FinishBid(Player winner, TreacheryCard card)
        {
            CardJustWon = card;
            WinningBid = CurrentBid;
            CurrentBid = null;
            FactionThatMayReplaceBoughtCard = Faction.None;

            Bids.Clear();

            if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.GreenBiddingPrescience);

            bool enterReplacingCardJustWon = Version > 150 && Players.Any(p => p.Nexus != Faction.None);

            if (winner != null)
            {
                if (winner.Ally == Faction.Grey && AllyMayReplaceCards)
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

        public void HandleEvent(ReplacedCardWon e)
        {
            if (!e.Passed)
            {
                Discard(CardJustWon);
                var initiator = GetPlayer(e.Initiator);
                var newCard = DrawTreacheryCard();
                initiator.TreacheryCards.Add(newCard);
                RecentMilestones.Add(Milestone.CardWonSwapped);

                if (ReplacingBoughtCardUsingNexus)
                {
                    DiscardNexusCard(e.Player);
                    ReplacingBoughtCardUsingNexus = false;
                }

                LogTo(initiator.Faction, "You replaced your ", CardJustWon, " with a ", newCard);
                Log(e);
            }

            if (CardJustWon == CardSoldOnBlackMarket)
            {
                EnterWhiteBidding();
            }
            else
            {
                DetermineNextStepAfterCardWasSold();
            }
        }

        public bool WhiteBiddingJustFinished { get; private set; }

        private void DetermineNextStepAfterCardWasSold()
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

        private void PutNextCardOnAuction()
        {
            if (!WhiteBiddingJustFinished)
            {
                BidSequence.NextRound();
            }

            SkipPlayersThatCantBid(BidSequence);
            WinningBid = null;
            Enter(Phase.Bidding);
        }

        public int NumberOfCardsOnRegularAuction => CardNumber + CardsOnAuction.Items.Count - 1;

        private AuctionType BlackMarketAuctionType { get; set; }

        private void StartBidSequenceAndAuctionType(AuctionType auctionType, Player whitePlayer = null, int direction = 1)
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

        private IBid DetermineHighestBid(Dictionary<Faction, IBid> bids)
        {
            int highestBidValue = bids.Values.Max(b => b.TotalAmount);
            var determineBidWinnerSequence = new PlayerSequence(this, false, 1);

            for (int i = 0; i < MaximumNumberOfPlayers; i++)
            {
                var f = determineBidWinnerSequence.CurrentFaction;
                if (bids.ContainsKey(f) && bids[f].TotalAmount == highestBidValue)
                {
                    return bids[f];
                }

                determineBidWinnerSequence.NextPlayer();
            }

            return null;
        }

        #endregion

        #region OtherBiddingEvents

        public int KarmaHandSwapNumberOfCards { get; private set; }
        public Faction KarmaHandSwapTarget { get; private set; }
        private Phase KarmaHandSwapPausedPhase { get; set; }

        public void HandleEvent(KarmaHandSwapInitiated e)
        {
            KarmaHandSwapPausedPhase = CurrentPhase;
            Enter(Phase.PerformingKarmaHandSwap);

            var initiator = GetPlayer(e.Initiator);
            var victim = GetPlayer(e.Target);

            initiator.SpecialKarmaPowerUsed = true;
            Discard(initiator, Karma.ValidKarmaCards(this, e.Player).FirstOrDefault());

            KarmaHandSwapNumberOfCards = victim.TreacheryCards.Count;
            KarmaHandSwapTarget = e.Target;

            var cardsToDrawFrom = new Deck<TreacheryCard>(victim.TreacheryCards, Random);
            RecentMilestones.Add(Milestone.Shuffled);
            cardsToDrawFrom.Shuffle();
            for (int i = 0; i < KarmaHandSwapNumberOfCards; i++)
            {
                var card = cardsToDrawFrom.Draw();
                RegisterKnown(initiator, card);
                victim.TreacheryCards.Remove(card);
                initiator.TreacheryCards.Add(card);
            }

            Log(e);
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

            Log(e);
            Enter(KarmaHandSwapPausedPhase);
        }

        public bool HasBidToPay(Player p) => CurrentPhase == Phase.Bidding && CurrentBid != null &&
            (CurrentBid.Initiator == p.Faction || CurrentBid.Initiator == p.Ally && CurrentBid.AllyContributionAmount > 0);



        public void HandleEvent(RedBidSupport e)
        {
            PermittedUseOfRedSpice = e.Amounts;
            Log(e);
        }

        #endregion

        #region EndOfBidding

        private void EndBiddingPhase()
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
    }
}
