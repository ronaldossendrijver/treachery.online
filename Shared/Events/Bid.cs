/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Treachery.Shared
{
    public class Bid : GameEvent, IBid
    {
        public int Amount { get; set; }

        public int AllyContributionAmount { get; set; }

        public int RedContributionAmount { get; set; }

        [JsonIgnore]
        public int TotalAmount
        {
            get
            {
                return Amount + AllyContributionAmount + RedContributionAmount;
            }
        }

        public bool Passed { get; set; }

        public int _karmaCardId = -1;

        public Bid(Game game) : base(game)
        {
        }

        public Bid()
        {
        }

        [JsonIgnore]
        public TreacheryCard KarmaCard
        {
            private get
            {
                return TreacheryCardManager.Lookup.Find(_karmaCardId);
            }

            set
            {
                if (value == null)
                {
                    _karmaCardId = -1;
                }
                else
                {
                    _karmaCardId = value.Id;
                }
            }
        }

        public TreacheryCard GetKarmaCard()
        {
            return KarmaCard;
        }

        /// <summary>
        /// This indicates Karma was used to remove the bid amount limit
        /// </summary>
        /// <param name="g"></param>
        [JsonIgnore]
        public bool UsingKarmaToRemoveBidLimit
        {
            get
            {
                return KarmaCard != null && !KarmaBid;
            }
        }

        /// <summary>
        /// This indicates the card is won immediately
        /// </summary>
        public bool KarmaBid { get; set; } = false;

        public override string Validate()
        {
            if ((Game.CurrentAuctionType == AuctionType.BlackMarketSilent || Game.CurrentAuctionType == AuctionType.WhiteSilent) && Passed) return "You cannot pass a silent bid";

            if (Passed) return "";

            bool isSpecialAuction = Game.CurrentAuctionType == AuctionType.WhiteOnceAround || Game.CurrentAuctionType == AuctionType.WhiteSilent;
            if (KarmaBid && isSpecialAuction) return Skin.Current.Format("You can't use {0} in Once Around or Silent bidding", TreacheryCardType.Karma);

            if (KarmaBid && !CanKarma(Game, Player)) return Skin.Current.Format("You can't use {0} for this bid", TreacheryCardType.Karma);

            if (KarmaBid) return "";

            var p = Game.GetPlayer(Initiator);
            if (TotalAmount < 1 && Game.CurrentAuctionType != AuctionType.WhiteSilent) return "Bid must be higher than 0.";
            if (Game.CurrentBid != null && TotalAmount <= Game.CurrentBid.TotalAmount && Game.CurrentAuctionType != AuctionType.WhiteSilent) return "Bid not high enough.";

            var ally = Game.GetPlayer(p.Ally);
            if (AllyContributionAmount > 0 && AllyContributionAmount > ally.Resources) return "Your ally can't pay that much.";

            var red = Game.GetPlayer(Faction.Red);
            if (RedContributionAmount > 0 && RedContributionAmount > red.Resources) return Skin.Current.Format("{0} can't pay that much.", Faction.Red);

            if (!UsingKarmaToRemoveBidLimit && Amount > Player.Resources) return string.Format("You can't pay {0}.", Amount);
            if (KarmaCard != null && !Karma.ValidKarmaCards(Game, p).Contains(KarmaCard)) return Skin.Current.Format("Invalid {0} card.", TreacheryCardType.Karma);

            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
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
            return g.SpiceYourAllyCanPay(p);
        }

        public static IEnumerable<SequenceElement> PlayersToBid(Game g)
        {
            switch (g.CurrentAuctionType)
            {
                case AuctionType.Normal:
                case AuctionType.WhiteOnceAround:
                    return g.BidSequence.GetPlayersInSequence();

                case AuctionType.WhiteSilent:
                    return g.Players.Select(p => new SequenceElement() { Player = p, HasTurn = p.HasRoomForCards && !g.Bids.Keys.Contains(p.Faction) });

                default:
                    return new SequenceElement[] { };
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

        public static bool CanKarma(Game g, Player p)
        {
            return ValidKarmaCards(g, p).Any();
        }

        public static bool MayBePlayed(Game game, Player player)
        {
            return game.CurrentAuctionType == AuctionType.WhiteSilent && !game.Bids.ContainsKey(player.Faction) && player.HasRoomForCards ||
                   game.CurrentAuctionType != AuctionType.WhiteSilent && player == game.BidSequence.CurrentPlayer;
        }
    }
}
