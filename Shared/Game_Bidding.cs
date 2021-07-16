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
        public PlayerSequence BlackMarketBidSequence { get; set; }
        public PlayerSequence WhiteBidSequence { get; set; }
        public Deck<TreacheryCard> CardsOnAuction { get; private set; }
        public int CardNumber { get; private set; }
        public Bid CurrentBid { get; private set; } = null;
        public BlackMarketBid BlackMarketCurrentBid { get; private set; } = null;
        public Dictionary<Faction, Bid> Bids { get; private set; } = new Dictionary<Faction, Bid>();
        public Dictionary<Faction, BlackMarketBid> BlackMarketBids { get; private set; } = new Dictionary<Faction, BlackMarketBid>();
        private Faction FirstFactionToBid { get; set; }
        private bool GreySwappedCardOnBid { get; set; }
        private bool CardWasSoldOnBlackMarket { get; set; }
        public Deck<TreacheryCard> BlackMarketCardsOnAuction;
        public AuctionType BlackMarketAuctionType;
        public AuctionType AuctionType;

        private void EnterBiddingPhase()
        {
            MainPhaseStart(MainPhase.Bidding);
            Allow(FactionAdvantage.BrownControllingCharity);
            Allow(FactionAdvantage.BlueCharity);
            ReceiveResourceTechIncome();
            GreySwappedCardOnBid = false;
            CardWasSoldOnBlackMarket = false;
            AuctionType = AuctionType.Normal;

            var white = GetPlayer(Faction.White);
            Enter(white != null && !Prevented(FactionAdvantage.WhiteBlackMarket) && white.TreacheryCards.Count > 0 && Players.Count > 1, Phase.BlackMarketAnnouncement, EnterWhiteBidding);
        }

        public void HandleEvent(WhiteAnnouncesBlackMarket e)
        {
            CurrentReport.Add(e);

            if (!e.Passed)
            {
                Enter(Phase.BlackMarketBidding);
                BlackMarketCardsOnAuction = new Deck<TreacheryCard>(Random);
                BlackMarketCardsOnAuction.PutOnTop(e.Card);
                e.Player.TreacheryCards.Remove(e.Card);
                BlackMarketAuctionType = e.AuctionType;

                if (e.AuctionType == AuctionType.Normal)
                {
                    BlackMarketBidSequence = new PlayerSequence(Players);
                    BlackMarketBidSequence.Start(this, true);
                }
                else if (e.AuctionType == AuctionType.OnceAround)
                {
                    BlackMarketBidSequence = new PlayerSequence(Players, e.Direction);
                    BlackMarketBidSequence.Start(e.Player);
                    BlackMarketBidSequence.NextPlayer(this, true);
                }

                SkipPlayersThatCantBid(BlackMarketBidSequence);
                BlackMarketBids.Clear();
                BlackMarketCurrentBid = null;
                FirstFactionToBid = BlackMarketBidSequence.CurrentFaction;
            }
            else
            {
                EnterWhiteBidding();
            }
        }

        public void HandleEvent(BlackMarketBid bid)
        {
            BlackMarketBidSequence.NextPlayer(this, Version >= 50);
            SkipPlayersThatCantBid(BlackMarketBidSequence);
            RecentMilestones.Add(Milestone.Bid);

            if (Bids.ContainsKey(bid.Initiator))
            {
                Bids.Remove(bid.Initiator);
            }

            BlackMarketBids.Add(bid.Initiator, bid);

            if (!bid.Passed)
            {
                BlackMarketCurrentBid = bid;
            }

            switch (BlackMarketAuctionType)
            {
                case AuctionType.Normal:

                    if (bid.Passed)
                    {
                        if (BlackMarketCurrentBid != null && BlackMarketBidSequence.CurrentFaction == BlackMarketCurrentBid.Initiator)
                        {
                            WinByHighestBid(
                                BlackMarketCurrentBid.Player, 
                                BlackMarketCurrentBid.Amount, 
                                BlackMarketCurrentBid.AllyContributionAmount, 
                                BlackMarketCurrentBid.RedContributionAmount,
                                BlackMarketCurrentBid.Player.Faction != Faction.White ? Faction.White : Faction.Red, 
                                BlackMarketCardsOnAuction);

                            CardWasSoldOnBlackMarket = true;
                            EnterWhiteBidding();
                        }
                        else if (BlackMarketCurrentBid == null && FirstFactionToBid == BlackMarketBidSequence.CurrentFaction)
                        {
                            CurrentReport.Add("Card not sold as no faction bid on it.");
                            EnterWhiteBidding();
                        }
                    }

                    break;

                case AuctionType.OnceAround:
                case AuctionType.Silent:

                    if (Players.Count(p => p.MayBidOnCards) == BlackMarketBids.Count)
                    {
                        var highestBid = BlackMarketBids.Where(bid => !bid.Value.Passed).OrderByDescending(bid => bid.Value.TotalAmount).FirstOrDefault().Value;
                        if (highestBid != null && highestBid.TotalAmount > 0)
                        {
                            WinByHighestBid(highestBid.Player, BlackMarketCurrentBid.Amount, highestBid.AllyContributionAmount, highestBid.RedContributionAmount, Faction.White, BlackMarketCardsOnAuction);
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

        private WhiteAnnouncesAuction AnnouncedWhiteAuction;
        private void EnterWhiteBidding()
        {
            int numberOfCardsToSell = PlayersThatCanBid.Count();

            if (CardWasSoldOnBlackMarket)
            {
                numberOfCardsToSell--;
            }

            AnnouncedWhiteAuction = null;

            if (numberOfCardsToSell == 0)
            {
                CurrentReport.Add("Bidding is skipped because no faction is able to buy cards");
                EndBiddingPhase();
            }
            else
            {
                Enter(IsPlaying(Faction.White) && WhiteCache.Count > 0, Phase.WhiteAnnouncingAuction, EnterRegularBidding);
            }
        }

        public void HandleEvent(WhiteAnnouncesAuction e)
        {
            CurrentReport.Add(e);
            AnnouncedWhiteAuction = e;
            Enter(e.First, Phase.WhiteSpecifyingAuction, EnterRegularBidding);
        }

        private void EnterRegularBidding()
        {
            BidSequence.Start(this, Version >= 50);
            DrawCardsOnAuction();
        }

        private void DrawCardsOnAuction()
        {
            CardsOnAuction = new Deck<TreacheryCard>(Random);
            CardNumber = 1;
            int numberOfCardsToDraw = PlayersThatCanBid.Count();

            if (CardWasSoldOnBlackMarket)
            {
                numberOfCardsToDraw--;
            }

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

            if (AnnouncedWhiteAuction != null && AnnouncedWhiteAuction.First)
            {
                Enter(Phase.WhiteSpecifyingAuction);
            }
            else
            {
                Enter(IsPlaying(Faction.Grey) && !Prevented(FactionAdvantage.GreySelectingCardsOnAuction), Phase.GreyRemovingCardFromBid, StartAuction);
            }
        }

        //This could happen before starting normal bidding or right after normal bidding
        public void HandleEvent(WhiteSpecifiesAuction e)
        {
            CardsOnAuction.PutOnTop(e.Card);
            WhiteCache.Remove(e.Card);

            if (AnnouncedWhiteAuction.First)
            {
                Enter(IsPlaying(Faction.Grey) && !Prevented(FactionAdvantage.GreySelectingCardsOnAuction), Phase.GreyRemovingCardFromBid, StartAuction);
            }
            else
            {
                WaitForNextCardToBePutOnAuction();
            }

            AnnouncedWhiteAuction = null;
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
            Enter(GreyMaySwapCardOnBid, Phase.GreySwappingCard, StartAuction);
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

            if (CardNumber == 1)
            {
                StartAuction();
            }
            else if (Version >= 46)
            {
                Enter(IsPlaying(Faction.Green) && Version > 14, Phase.WaitingForNextBiddingRound, PutNextCardOnAuction);
            }
        }

        private void StartAuction()
        {
            Enter(Phase.Bidding);

            if (Players.Count == 1 && CardsOnAuction.Items.Count == 1)
            {
                CurrentReport.Add(Players[0].Faction, "{0} are playing alone and therefore win the auction.", Players[0].Faction);
                var drawnCard = CardsOnAuction.Draw();
                Players[0].TreacheryCards.Add(drawnCard);
                if (Version >= 41) GiveHarkonnenExtraCard(Players[0]);
                EndBiddingPhase();
            }
            else
            {
                CurrentBid = null;
                Bids.Clear();
                BidSequence.Start(this, Version >= 50);
                SkipPlayersThatCantBid(BidSequence);
                FirstFactionToBid = BidSequence.CurrentFaction;
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
            }

            BidSequence.NextPlayer(this, Version >= 50);
            SkipPlayersThatCantBid(BidSequence);

            if (Bids.ContainsKey(bid.Initiator))
            {
                Bids.Remove(bid.Initiator);
            }

            Bids.Add(bid.Initiator, bid);

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
                    if (CurrentBid.UsingKarmaToRemoveBidLimit)
                    {
                        //Karma was used to bid any amount
                        ReturnKarmaCardUsedForBid();
                        var card = WinWithKarma(CurrentBid, CardsOnAuction);
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
            else
            {
                CurrentBid = bid;
                RecentMilestones.Add(Milestone.Bid);
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
            LogBid(winner, receiverIncomeMessage);
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

        private void LogBid(Player initiator, MessagePart redIncome)
        {
            if (CurrentBid.AllyContributionAmount > 0)
            {
                if (CurrentBid.RedContributionAmount > 0)
                {
                    CurrentReport.Add(CurrentBid.Initiator, "Card {0} won by {1} for {2}, of which {3} paid {4} and {5} paid {6}.{7}", CardNumber, CurrentBid.Initiator, CurrentBid.TotalAmount, initiator.Ally, CurrentBid.AllyContributionAmount, Faction.Red, CurrentBid.RedContributionAmount, redIncome);
                }
                else
                {
                    CurrentReport.Add(CurrentBid.Initiator, "Card {0} won by {1} for {2}, of which {3} paid {4}.{5}", CardNumber, CurrentBid.Initiator, CurrentBid.TotalAmount, initiator.Ally, CurrentBid.AllyContributionAmount, redIncome);
                }
            }
            else
            {
                if (CurrentBid.RedContributionAmount > 0)
                {
                    CurrentReport.Add(CurrentBid.Initiator, "Card {0} won by {1} for {2}, of which {3} paid {4}.{5}", CardNumber, CurrentBid.Initiator, CurrentBid.TotalAmount, Faction.Red, CurrentBid.RedContributionAmount, redIncome);
                }
                else
                {
                    CurrentReport.Add(CurrentBid.Initiator, "Card {0} won by {1} for {2}.{3}", CardNumber, CurrentBid.Initiator, CurrentBid.Amount, redIncome);
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
            //The current bid is from this player, everyone passed after that, so the auction for this card is finished
            var receiverIncomeMessage = new MessagePart("");
            PayForCard(winner, bidAmount, bidAllyContributionAmount, bidRedContributionAmount, paymentReceiver, ref receiverIncomeMessage);
            LogBid(winner, receiverIncomeMessage);
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
                if (sequence.CurrentPlayer.MayBidOnCards) break;
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

            if (winner.Ally == Faction.Grey && GreyAllyMayReplaceCards)
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
                        WaitForNextCardToBePutOnAuction();
                    }
                    else
                    {
                        Enter(Phase.ReplacingCardJustWon);
                    }
                }
            }
            else
            {
                WaitForNextCardToBePutOnAuction();
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
            WaitForNextCardToBePutOnAuction();
        }

        private void WaitForNextCardToBePutOnAuction()
        {
            if (!CardsOnAuction.IsEmpty)
            {
                CardNumber++;
                Enter(GreyMaySwapCardOnBid, Phase.GreySwappingCard, IsPlaying(Faction.Green), Phase.WaitingForNextBiddingRound, PutNextCardOnAuction);
            }
            else
            {
                Enter(AnnouncedWhiteAuction != null && !AnnouncedWhiteAuction.First, Phase.WhiteSpecifyingAuction, EndBiddingPhase);
            }
        }

        private void PutNextCardOnAuction()
        {
            Enter(Phase.Bidding);
            BidSequence.NextRound(this, Version >= 50);
            SkipPlayersThatCantBid(BidSequence);
            FirstFactionToBid = BidSequence.CurrentFaction;
        }

        public IEnumerable<Player> PlayersThatCanBid => Players.Where(p => p.MayBidOnCards);

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
            DiscardTreacheryCard(initiator, TreacheryCardType.Karma);

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
    }
}
