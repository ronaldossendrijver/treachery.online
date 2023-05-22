/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class Bid : PassableGameEvent, IBid
    {
        #region Construction

        public Bid(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public Bid()
        {
        }

        #endregion

        #region Properties

        public int Amount { get; set; }

        public int AllyContributionAmount { get; set; }

        public int RedContributionAmount { get; set; }

        public bool UsesRedSecretAlly { get; set; }

        public int _karmaCardId = -1;

        [JsonIgnore]
        public TreacheryCard KarmaCard
        {
            get => TreacheryCardManager.Lookup.Find(_karmaCardId);
            set => _karmaCardId = TreacheryCardManager.GetId(value);
        }

        [JsonIgnore]
        public int TotalAmount => Amount + AllyContributionAmount + RedContributionAmount;

        /// <summary>
        /// This indicates Karma was used to remove the bid amount limit
        /// </summary>
        [JsonIgnore]
        public bool UsingKarmaToRemoveBidLimit => KarmaCard != null && !KarmaBid;

        /// <summary>
        /// This indicates the card is won immediately
        /// </summary>
        public bool KarmaBid { get; set; } = false;

        #endregion

        #region Validation

        public override Message Validate()
        {
            if ((Game.CurrentAuctionType == AuctionType.BlackMarketSilent || Game.CurrentAuctionType == AuctionType.WhiteSilent) && Passed) return Message.Express("You cannot pass a silent bid");

            if (Passed) return null;

            bool isSpecialAuction = Game.CurrentAuctionType == AuctionType.WhiteOnceAround || Game.CurrentAuctionType == AuctionType.WhiteSilent;
            if (KarmaBid && isSpecialAuction) return Message.Express("You can't use ", TreacheryCardType.Karma, " in Once Around or Silent bidding");

            if (KarmaBid && !CanKarma(Game, Player)) return Message.Express("You can't use ", TreacheryCardType.Karma, " for this bid");

            if (KarmaBid) return null;

            var p = Game.GetPlayer(Initiator);
            if (TotalAmount < 1 && Game.CurrentAuctionType != AuctionType.WhiteSilent) return Message.Express("Bid must be higher than 0");
            if (Game.CurrentBid != null && TotalAmount <= Game.CurrentBid.TotalAmount && Game.CurrentAuctionType != AuctionType.WhiteSilent) return Message.Express("Bid not high enough");

            if (AllyContributionAmount > ValidMaxAllyAmount(Game, Player)) return Message.Express("your ally won't pay that much");

            var red = Game.GetPlayer(Faction.Red);
            if (RedContributionAmount > 0 && RedContributionAmount > red.Resources) return Message.Express(Faction.Red, " won't pay that much");

            if (!UsingKarmaToRemoveBidLimit && Amount > Player.Resources) return Message.Express("You can't pay ", Payment.Of(Amount));
            if (KarmaCard != null && !Karma.ValidKarmaCards(Game, p).Contains(KarmaCard)) return Message.Express("Invalid ", TreacheryCardType.Karma, " card");

            if (UsesRedSecretAlly && !MayUseRedSecretAlly(Game, Player)) return Message.Express("You can't use ", Faction.Red, " cunning");

            if (Game.Version >= 155 && Game.CurrentAuctionType == AuctionType.WhiteSilent && TotalAmount > Player.Resources) return Message.Express("In a Silent auction, you can't bid more than you have");

            return null;
        }

        public static int ValidMaxAmount(Player p, bool usingKarma)
        {
            if (usingKarma)
            {
                return 100;
            }
            else
            {
                return p.Resources;
            }
        }

        public static int ValidMaxAllyAmount(Game g, Player p)
        {
            return g.ResourcesYourAllyCanPay(p);
        }

        public static IEnumerable<SequenceElement> PlayersToBid(Game g)
        {
            return g.CurrentAuctionType switch
            {
                AuctionType.Normal or AuctionType.WhiteOnceAround => g.BidSequence.GetPlayersInSequence(),
                AuctionType.WhiteSilent => g.Players.Select(p => new SequenceElement() { Player = p, HasTurn = p.HasRoomForCards && !g.Bids.Keys.Contains(p.Faction) }),
                _ => Array.Empty<SequenceElement>(),
            };
        }

        public static IEnumerable<TreacheryCard> ValidKarmaCards(Game g, Player p)
        {
            if (g.CurrentAuctionType == AuctionType.Normal)
            {
                return Karma.ValidKarmaCards(g, p);
            }
            else
            {
                return Array.Empty<TreacheryCard>();
            }
        }

        public static bool CanKarma(Game g, Player p) => ValidKarmaCards(g, p).Any();

        public static bool MayBePlayed(Game game, Player player)
        {
            return game.CurrentAuctionType == AuctionType.WhiteSilent && !game.Bids.ContainsKey(player.Faction) && player.HasRoomForCards ||
                   game.CurrentAuctionType != AuctionType.WhiteSilent && player == game.BidSequence.CurrentPlayer;
        }

        public static bool MayUseRedSecretAlly(Game game, Player player) => game.CurrentAuctionType == AuctionType.Normal && player.Nexus == Faction.Red && NexusPlayed.CanUseSecretAlly(game, player);

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            if (!Passed)
            {
                ReturnKarmaCardUsedForBid();
                SetAsideKarmaCardUsedForBid();
                Game.CurrentBid = this;
                Game.Stone(Milestone.Bid);
            }

            Game.BidSequence.NextPlayer();
            Game.SkipPlayersThatCantBid(Game.BidSequence);
            Game.Bids.Remove(Initiator);
            Game.Bids.Add(Initiator, this);

            switch (Game.CurrentAuctionType)
            {
                case AuctionType.Normal:

                    HandleNormalBid();
                    break;

                case AuctionType.WhiteOnceAround:
                case AuctionType.WhiteSilent:

                    HandleWhiteBid();
                    break;
            }
        }

        private void HandleNormalBid()
        {
            if (Passed || KarmaBid)
            {
                if (KarmaBid)
                {
                    //Immediate Karma
                    var card = WinWithKarma(this);
                    Game.FinishBid(Player, card, true);
                }
                else if (Game.CurrentBid != null && Game.BidSequence.CurrentFaction == Game.CurrentBid.Initiator)
                {
                    var winningBid = Game.CurrentBid as Bid;

                    if (winningBid.UsingKarmaToRemoveBidLimit)
                    {
                        //Karma was used to bid any amount
                        ReturnKarmaCardUsedForBid();
                        var card = WinWithKarma(winningBid);
                        Game.FinishBid(winningBid.Player, card, true);
                    }
                    else
                    {
                        var receiver = Faction.Red;
                        var card = Game.WinByHighestBid(
                            winningBid.Player,
                            winningBid,
                            winningBid.Amount,
                            winningBid.AllyContributionAmount,
                            winningBid.RedContributionAmount,
                            receiver,
                            Game.CardsOnAuction,
                            winningBid.UsesRedSecretAlly);

                        Game.FinishBid(winningBid.Player, card, true);
                    }
                }
                else if (Game.CurrentBid == null && Game.Bids.Count >= Game.PlayersThatCanBid.Count())
                {
                    EveryonePassedBid();
                }
            }
            else if (Game.BidSequence.CurrentFaction == Initiator)
            {
                var card = BidWonByOnlyPlayer(this, Faction.Red, Game.CardsOnAuction);
                Game.FinishBid(Game.CurrentBid.Player, card, true);
            }
        }

        private TreacheryCard WinWithKarma(Bid bid)
        {
            var winner = GetPlayer(bid.Initiator);
            var karmaCard = bid.KarmaCard;

            Game.Discard(karmaCard);

            if (karmaCard.Type == TreacheryCardType.Karma)
            {
                Log(bid.Initiator, " get card ", Game.CardNumber, " using ", TreacheryCardType.Karma);
            }
            else
            {
                Log(bid.Initiator, " get card ", Game.CardNumber, " using ", karmaCard, " for ", TreacheryCardType.Karma);
            }

            Game.Stone(Milestone.AuctionWon);
            Game.Stone(Milestone.Karma);

            var card = Game.CardsOnAuction.Draw();
            winner.TreacheryCards.Add(card);
            Game.RegisterWonCardAsKnown(card);
            LogTo(winner.Faction, "You won: ", card);
            Game.GivePlayerExtraCardIfApplicable(winner);
            return card;
        }

        private void ReturnKarmaCardUsedForBid()
        {
            if (Game.CurrentBid != null && Game.CurrentBid.UsingKarmaToRemoveBidLimit)
            {
                Game.CurrentBid.Player.TreacheryCards.Add(Game.CurrentBid.KarmaCard);
            }
        }

        private void SetAsideKarmaCardUsedForBid()
        {
            if (UsingKarmaToRemoveBidLimit)
            {
                Player.TreacheryCards.Remove(KarmaCard);
            }
        }

        private void EveryonePassedBid()
        {
            Log("Bid is passed by everyone; bidding ends and remaining cards are returned to the Treachery Deck");
            Game.Stone(Milestone.AuctionWon);

            while (!Game.CardsOnAuction.IsEmpty)
            {
                if (Game.Version >= 131)
                {
                    Game.CardsOnAuction.Shuffle();
                }

                Game.TreacheryDeck.PutOnTop(Game.CardsOnAuction.Draw());
            }

            Game.EndBiddingPhase();
        }

        private TreacheryCard BidWonByOnlyPlayer(Bid bid, Faction paymentReceiver, Deck<TreacheryCard> toDrawFrom)
        {
            Game.CurrentBid = bid;
            var winner = GetPlayer(Game.CurrentBid.Initiator);
            var receiverIncomeMessage = MessagePart.Express();

            if (!bid.UsesRedSecretAlly)
            {
                Game.PayForCard(winner, bid, Game.CurrentBid.Amount, Game.CurrentBid.AllyContributionAmount, Game.CurrentBid.RedContributionAmount, paymentReceiver, ref receiverIncomeMessage);
                Game.LogBid(winner, Game.CurrentBid.Amount, Game.CurrentBid.AllyContributionAmount, Game.CurrentBid.RedContributionAmount, receiverIncomeMessage);
            }
            else
            {
                Game.PlayNexusCard(winner, "Secret Ally", "get this card for free");
                Game.LogBid(winner, 0, 0, 0, receiverIncomeMessage);
            }

            Game.Stone(Milestone.AuctionWon);
            var card = toDrawFrom.Draw();
            Game.RegisterWonCardAsKnown(card);
            winner.TreacheryCards.Add(card);
            LogTo(winner.Faction, "You won: ", card);
            Game.GivePlayerExtraCardIfApplicable(winner);
            return card;
        }

        private void HandleWhiteBid()
        {
            var isLastBid = Game.Version < 140 ? Game.Players.Count(p => p.HasRoomForCards) == Game.Bids.Count :
                Game.CurrentAuctionType == AuctionType.WhiteSilent && Game.Players.Count(p => p.HasRoomForCards) == Game.Bids.Count ||
                Game.Version < 151 && (Game.CurrentAuctionType == AuctionType.WhiteOnceAround && Initiator == Faction.White || !GetPlayer(Faction.White).HasRoomForCards && Game.BidSequence.HasPassedWhite) ||
                Game.Version >= 151 && Game.CurrentAuctionType == AuctionType.WhiteOnceAround && (Initiator == Faction.White || !GetPlayer(Faction.White).HasRoomForCards && Game.BidSequence.HasPassedWhite);

            if (isLastBid)
            {
                if (Game.CurrentAuctionType == AuctionType.WhiteSilent)
                {
                    Log("Bids: ", Game.Bids.Select(b => MessagePart.Express(b.Key, Payment.Of(b.Value.TotalAmount), " ")).ToList());
                }

                var highestBid = Game.DetermineHighestBid(Game.Bids);
                if (highestBid != null && highestBid.TotalAmount > 0)
                {
                    var card = Game.WinByHighestBid(
                        highestBid.Player,
                        highestBid,
                        highestBid.Amount,
                        highestBid.AllyContributionAmount,
                        highestBid.RedContributionAmount,
                        highestBid.Initiator != Faction.White ? Faction.White : Faction.Red,
                        Game.CardsOnAuction, false);

                    Game.FinishBid(highestBid.Player, card, true);
                }
                else
                {
                    Log("Card not sold as no faction bid on it");
                    var white = GetPlayer(Faction.White);
                    if (white.HasRoomForCards)
                    {
                        Game.Enter(Phase.WhiteKeepingUnsoldCard);
                    }
                    else
                    {
                        var card = Game.CardsOnAuction.Draw();
                        Game.RemovedTreacheryCards.Add(card);
                        Game.RegisterWonCardAsKnown(card);
                        Log(card, " was removed from the game");
                        Game.FinishBid(null, card, Game.Version < 152);
                    }
                }
            }
        }

        public override Message GetMessage()
        {
            if (!Passed)
            {
                if (KarmaBid)
                {
                    return Message.Express(Initiator, " win the bid using ", TreacheryCardType.Karma);
                }
                else
                {
                    return Message.Express(Initiator, " bid");
                }
            }
            else
            {
                return Message.Express(Initiator, " pass");
            }
        }

        #endregion

    }
}
