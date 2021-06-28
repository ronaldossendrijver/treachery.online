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
        public Deck<TreacheryCard> CardsOnAuction { get; private set; }
        public int CardNumber { get; private set; }
        public Bid CurrentBid { get; private set; } = null;
        public Dictionary<Faction, Bid> Bids { get; private set; } = new Dictionary<Faction, Bid>();
        private Faction FirstFactionToBid { get; set; }
        private bool GreySwappedCardOnBid { get; set; }

        private void EnterBiddingPhase()
        {
            BidSequence.Start(this, Version >= 50);
            CurrentMainPhase = MainPhase.Bidding;
            CurrentReport = new Report(MainPhase.Bidding);
            ReceiveResourceTechIncome();
            GreySwappedCardOnBid = false;
            DrawCardsOnAuction();
        }

        private void DrawCardsOnAuction()
        {
            CardsOnAuction = new Deck<TreacheryCard>(Random);
            CardNumber = 1;
            int numberOfCardsToDraw = PlayersThatCanBid.Count();

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

            if (!CardsOnAuction.IsEmpty)
            {
                Enter(IsPlaying(Faction.Grey) && !Prevented(FactionAdvantage.GreySelectingCardsOnAuction), Phase.GreyRemovingCardFromBid, StartAuction);
            }
            else
            {
                Enter(Version >= 38, Phase.BiddingReport, EnterRevivalPhase);
            }
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
            CurrentReport.Add(e.GetMessage());
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

            CurrentReport.Add(e.GetMessage());

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
                Enter(Version >= 38, Phase.BiddingReport, EnterRevivalPhase);
            }
            else
            {
                CurrentBid = null;
                Bids.Clear();
                BidSequence.Start(this, Version >= 50);
                SkipPlayersThatCantBid();
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

        public TreacheryCard CardSetAsideForBid(Player p)
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
            SkipPlayersThatCantBid();

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
                    WinWithKarma(bid);
                }
                else if (CurrentBid != null && BidSequence.CurrentFaction == CurrentBid.Initiator)
                {
                    if (CurrentBid.UsingKarmaToRemoveBidLimit)
                    {
                        //Karma was used to bid any amount
                        ReturnKarmaCardUsedForBid();
                        WinWithKarma(CurrentBid);
                    }
                    else
                    {
                        WinByHighestBid();
                    }
                }
                else if (CurrentBid == null && FirstFactionToBid == BidSequence.CurrentFaction)
                {
                    EveryonePassed();
                }
            }
            else if (BidSequence.CurrentFaction == bid.Initiator)
            {
                BidByOnlyPlayer(bid);
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

        private void BidByOnlyPlayer(Bid bid)
        {
            CurrentBid = bid;
            var initiator = GetPlayer(CurrentBid.Initiator);
            var redIncomeMessage = new MessagePart("");
            PayForCard(initiator, ref redIncomeMessage);
            LogBid(initiator, redIncomeMessage);
            RecentMilestones.Add(Milestone.AuctionWon);
            var card = CardsOnAuction.Draw();
            RegisterWonCardAsKnown(card);
            initiator.TreacheryCards.Add(card);
            CurrentReport.Add(Faction.None, initiator.Faction, "You won: {0}.", card);
            GiveHarkonnenExtraCard(initiator);
            FinishBid(initiator, card);
        }

        public void HandleEvent(RedBidSupport e)
        {
            PermittedUseOfRedSpice = e.Amounts;
            CurrentReport.Add(e.GetMessage());
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

            Enter(Version >= 38, Phase.BiddingReport, EnterRevivalPhase);
        }

        private void WinByHighestBid()
        {
            //The current bid is from this player, everyone passed after that, so the auction for this card is finished
            var initiator = GetPlayer(CurrentBid.Initiator);
            var redIncomeMessage = new MessagePart("");
            PayForCard(initiator, ref redIncomeMessage);
            LogBid(initiator, redIncomeMessage);
            RecentMilestones.Add(Milestone.AuctionWon);
            var card = CardsOnAuction.Draw();
            RegisterWonCardAsKnown(card);
            initiator.TreacheryCards.Add(card);
            CurrentReport.Add(Faction.None, initiator.Faction, "You won: {0}.", card);
            GiveHarkonnenExtraCard(initiator);
            FinishBid(initiator, card);
        }

        private void RegisterWonCardAsKnown(TreacheryCard card)
        {
            foreach (var p in Players.Where(p => HasBiddingPrescience(p)))
            {
                RegisterKnown(p, card);
            }
        }

        private void WinWithKarma(Bid bid)
        {
            var initiator = GetPlayer(bid.Initiator);
            var card = bid.GetKarmaCard();
            Discard(card);
            if (card.Type == TreacheryCardType.Karma)
            {
                CurrentReport.Add(bid.Initiator, "Card {2} won by {0} using {1}.", bid.Initiator, TreacheryCardType.Karma, CardNumber);
            }
            else
            {
                CurrentReport.Add(bid.Initiator, "Card {2} won by {0} using {1} for {3}.", bid.Initiator, card, CardNumber, TreacheryCardType.Karma);
            }
            RecentMilestones.Add(Milestone.AuctionWon);
            RecentMilestones.Add(Milestone.Karma);

            var newCard = CardsOnAuction.Draw();
            initiator.TreacheryCards.Add(newCard);
            RegisterWonCardAsKnown(newCard);
            CurrentReport.Add(Faction.None, initiator.Faction, "You won: {0}.", newCard);
            GiveHarkonnenExtraCard(initiator);
            FinishBid(initiator, newCard);
        }

        private void SkipPlayersThatCantBid()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (BidSequence.CurrentPlayer.MayBidOnCards) break;
                BidSequence.NextPlayer(this, Version >= 50);
            }
        }

        private void PayForCard(Player initiator, ref MessagePart message)
        {
            initiator.Resources -= CurrentBid.Amount;

            if (CurrentBid.AllyContributionAmount > 0)
            {
                GetPlayer(initiator.Ally).Resources -= CurrentBid.AllyContributionAmount;
                if (Version >= 76) DecreasePermittedUseOfAllySpice(initiator.Faction, CurrentBid.AllyContributionAmount);
            }

            if (CurrentBid.RedContributionAmount > 0)
            {
                GetPlayer(Faction.Red).Resources -= CurrentBid.RedContributionAmount;
            }

            var emperor = GetPlayer(Faction.Red);
            if (emperor != null && CurrentBid.Initiator != Faction.Red)
            {
                if (!Prevented(FactionAdvantage.RedReceiveBid))
                {
                    if (CurrentBid.RedContributionAmount > 0)
                    {
                        var redProfit = CurrentBid.Amount + CurrentBid.AllyContributionAmount;
                        var redProfitAfterBidding = CurrentBid.RedContributionAmount;
                        message = new MessagePart(" {0} receives {1} immediately and {2} at the end of the bidding phase.", Faction.Red, redProfit, redProfitAfterBidding);
                        emperor.Resources += redProfit;
                        emperor.ResourcesAfterBidding += redProfitAfterBidding;
                    }
                    else
                    {
                        var redProfit = CurrentBid.TotalAmount;
                        message = new MessagePart(" {0} receives {1}.", Faction.Red, redProfit);
                        emperor.Resources += redProfit;
                    }
                }
                else
                {
                    message = new MessagePart(" {0} prevents {1} from receiving {2} for this card.", TreacheryCardType.Karma, Faction.Red, Concept.Resource);
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

            CurrentReport.Add(e.GetMessage());
            WaitForNextCardToBePutOnAuction();
        }

        private void WaitForNextCardToBePutOnAuction()
        {
            CardNumber++;

            if (!CardsOnAuction.IsEmpty)
            {
                if (Version >= 46)
                {
                    Enter(GreyMaySwapCardOnBid, Phase.GreySwappingCard, IsPlaying(Faction.Green) && Version > 14, Phase.WaitingForNextBiddingRound, PutNextCardOnAuction);
                }
                else
                {
                    Enter(IsPlaying(Faction.Green) && Version > 14 || GreyMaySwapCardOnBid, Phase.WaitingForNextBiddingRound, PutNextCardOnAuction);
                }
            }
            else
            {
                var red = GetPlayer(Faction.Red);
                if (red != null)
                {
                    red.Resources += red.ResourcesAfterBidding;
                    red.ResourcesAfterBidding = 0;
                }
                Enter(Phase.BiddingReport);
            }
        }

        private void PutNextCardOnAuction()
        {
            Enter(Phase.Bidding);
            BidSequence.NextRound(this, Version >= 50);
            SkipPlayersThatCantBid();
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

            CurrentReport.Add(e.GetMessage());
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

            CurrentReport.Add(e.GetMessage());
            Enter(KarmaHandSwapPausedPhase);
        }
    }
}
