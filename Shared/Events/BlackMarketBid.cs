/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class BlackMarketBid : PassableGameEvent, IBid
    {
        #region Construction

        public BlackMarketBid(Game game) : base(game)
        {
        }

        public BlackMarketBid()
        {
        }

        #endregion Construction

        #region Properties

        public int Amount { get; set; }

        public int AllyContributionAmount { get; set; }

        public int RedContributionAmount { get; set; }

        [JsonIgnore]
        public bool UsesRedSecretAlly => false;

        [JsonIgnore]
        public int TotalAmount => Amount + AllyContributionAmount + RedContributionAmount;

        #endregion Properties

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.BidSequence.NextPlayer();
            Game.SkipPlayersThatCantBid(Game.BidSequence);
            Game.Stone(Milestone.Bid);
            Game.Bids.Remove(Initiator);
            Game.Bids.Add(Initiator, this);

            if (!Passed)
            {
                Game.CurrentBid = this;
            }

            switch (Game.CurrentAuctionType)
            {
                case AuctionType.BlackMarketNormal:
                    HandleBlackMarketNormalBid();
                    break;

                case AuctionType.BlackMarketOnceAround:
                case AuctionType.BlackMarketSilent:
                    HandleBlackMarketSpecialBid();
                    break;
            }
        }

        private void HandleBlackMarketNormalBid()
        {
            if (Passed)
            {
                var winningBid = Game.CurrentBid;

                if (winningBid != null && Game.BidSequence.CurrentFaction == winningBid.Initiator)
                {
                    var card = Game.WinByHighestBid(
                        winningBid.Player,
                        winningBid,
                        winningBid.Amount,
                        winningBid.AllyContributionAmount,
                        winningBid.RedContributionAmount,
                        winningBid.Initiator != Faction.White ? Faction.White : Faction.Red,
                        Game.CardsOnAuction, false);

                    FinishBlackMarketBid(winningBid.Player, card);
                }
                else if (winningBid == null && Game.Bids.Count >= Game.PlayersThatCanBid.Count())
                {
                    GetPlayer(Faction.White).TreacheryCards.Add(Game.CardsOnAuction.Draw());
                    Log(Faction.White, " keep their card as no faction bid on it");
                    Game.EnterWhiteBidding();
                }
            }
        }

        private void HandleBlackMarketSpecialBid()
        {
            var isLastBid = Game.Version < 140 ? Game.Players.Count(p => p.HasRoomForCards) == Game.Bids.Count :
                Game.CurrentAuctionType == AuctionType.BlackMarketSilent && Game.Players.Count(p => p.HasRoomForCards) == Game.Bids.Count ||
                Game.CurrentAuctionType == AuctionType.BlackMarketOnceAround && Initiator == Faction.White || !GetPlayer(Faction.White).HasRoomForCards && Game.BidSequence.HasPassedWhite;

            if (isLastBid)
            {
                if (Game.CurrentAuctionType == AuctionType.BlackMarketSilent)
                {
                    Log(Game.Bids.Select(b => MessagePart.Express(b.Key, Payment.Of(b.Value.TotalAmount), " ")).ToList());
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

                    FinishBlackMarketBid(highestBid.Player, card);
                }
                else
                {
                    GetPlayer(Faction.White).TreacheryCards.Add(Game.CardsOnAuction.Draw());
                    Log(Faction.White, " keep their card as no faction bid on it");
                    Game.EnterWhiteBidding();
                }
            }
        }

        private void FinishBlackMarketBid(Player winner, TreacheryCard card)
        {
            Game.CardJustWon = card;
            Game.WinningBid = Game.CurrentBid;
            Game.CardSoldOnBlackMarket = card;
            Game.FactionThatMayReplaceBoughtCard = Faction.None;

            bool enterReplacingCardJustWon = winner != null && Game.Version > 150 && Game.Players.Any(p => p.Nexus != Faction.None);

            if (winner != null)
            {
                if (winner.Ally == Faction.Grey && Game.GreyAllowsReplacingCards)
                {
                    if (!Game.Prevented(FactionAdvantage.GreyAllyDiscardingCard))
                    {
                        Game.FactionThatMayReplaceBoughtCard = winner.Faction;
                        enterReplacingCardJustWon = true;
                    }
                    else
                    {
                        if (!Game.Applicable(Rule.FullPhaseKarma)) Game.Allow(FactionAdvantage.GreyAllyDiscardingCard);

                        if (Game.NexusAllowsReplacingBoughtCards(winner))
                        {
                            Game.FactionThatMayReplaceBoughtCard = winner.Faction;
                            Game.ReplacingBoughtCardUsingNexus = true;
                            enterReplacingCardJustWon = true;
                        }
                    }
                }
                else if (Game.NexusAllowsReplacingBoughtCards(winner))
                {
                    Game.FactionThatMayReplaceBoughtCard = winner.Faction;
                    enterReplacingCardJustWon = true;
                    Game.ReplacingBoughtCardUsingNexus = true;
                }
            }

            Game.Enter(enterReplacingCardJustWon, Phase.ReplacingCardJustWon, Game.EnterWhiteBidding);

            if (Game.BiddingTriggeredBureaucracy != null)
            {
                Game.ApplyBureaucracy(Game.BiddingTriggeredBureaucracy.PaymentFrom, Game.BiddingTriggeredBureaucracy.PaymentTo);
                Game.BiddingTriggeredBureaucracy = null;
            }
        }

        public override Message GetMessage()
        {
            if (!Passed)
            {
                return Message.Express(Initiator, " bid");
            }
            else
            {
                return Message.Express(Initiator, " pass");
            }
        }

        #endregion Execution

        #region Validation

        public override Message Validate()
        {
            if (Passed) return null;

            var p = Game.GetPlayer(Initiator);

            if (Game.CurrentAuctionType != AuctionType.BlackMarketSilent && TotalAmount < 1) return Message.Express("Bid must be higher than 0");
            if (Game.CurrentAuctionType != AuctionType.BlackMarketSilent && Game.CurrentBid != null && TotalAmount <= Game.CurrentBid.TotalAmount) return Message.Express("Bid not high enough");

            var ally = Game.GetPlayer(p.Ally);
            if (AllyContributionAmount > 0 && AllyContributionAmount > ally.Resources) return Message.Express("Your ally won't pay that much");

            var red = Game.GetPlayer(Faction.Red);
            if (RedContributionAmount > 0 && RedContributionAmount > red.Resources) return Message.Express(Faction.Red, " won't pay that much");

            return null;
        }

        public static int ValidMaxAmount(Player p) => p.Resources;

        public static int ValidMaxAllyAmount(Game g, Player p) => g.ResourcesYourAllyCanPay(p);

        public static IEnumerable<SequenceElement> PlayersToBid(Game g)
        {
            return g.CurrentAuctionType switch
            {
                AuctionType.BlackMarketNormal or AuctionType.BlackMarketOnceAround => g.BidSequence.GetPlayersInSequence(),
                AuctionType.BlackMarketSilent => g.Players.Select(p => new SequenceElement() { Player = p, HasTurn = p.HasRoomForCards && !g.Bids.ContainsKey(p.Faction) }),
                _ => Array.Empty<SequenceElement>(),
            };
        }

        public static bool MayBePlayed(Game game, Player player)
        {
            return game.CurrentAuctionType == AuctionType.BlackMarketSilent && !game.Bids.ContainsKey(player.Faction) && player.HasRoomForCards ||
                   game.CurrentAuctionType != AuctionType.BlackMarketSilent && player == game.BidSequence.CurrentPlayer;
        }

        #endregion Validation
    }
}
