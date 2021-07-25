/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


namespace Treachery.Shared
{
    public class BlackMarketBid : GameEvent, IBid
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


        public BlackMarketBid(Game game) : base(game)
        {
        }

        public BlackMarketBid()
        {
        }

        public override string Validate()
        {
            if (Passed) return "";

            var p = Game.GetPlayer(Initiator);

            if (Game.CurrentAuctionType != AuctionType.BlackMarketSilent && TotalAmount < 1) return "Bid must be higher than 0.";
            if (Game.CurrentAuctionType != AuctionType.BlackMarketSilent && Game.CurrentBid != null && TotalAmount <= Game.CurrentBid.TotalAmount) return "Bid not high enough.";

            var ally = Game.GetPlayer(p.Ally);
            if (AllyContributionAmount > 0 && AllyContributionAmount > ally.Resources) return "Your ally can't pay that much.";

            var red = Game.GetPlayer(Faction.Red);
            if (RedContributionAmount > 0 && RedContributionAmount > red.Resources) return Skin.Current.Format("{0} can't pay that much.", Faction.Red);

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
                return new Message(Initiator, "{0} bid.", Initiator);
            }
            else
            {
                return new Message(Initiator, "{0} pass.", Initiator);
            }
        }

        public static int ValidMaxAmount(Player p)
        {
            return p.Resources;
        }

        public static int ValidMaxAllyAmount(Game g, Player p)
        {
            return g.SpiceYourAllyCanPay(p);
        }

        public static IEnumerable<SequenceElement> PlayersToBid(Game g)
        {
            switch (g.CurrentAuctionType)
            {
                case AuctionType.BlackMarketNormal:
                case AuctionType.BlackMarketOnceAround:
                    return g.BidSequence.GetPlayersInSequence(g);

                case AuctionType.BlackMarketSilent:
                    return g.Players.Select(p => new SequenceElement() { Player = p, HasTurn = p.HasRoomForCards && !g.Bids.Keys.Contains(p.Faction) });

                default: return new SequenceElement[] { };
            }
        }

        public static bool MayBePlayed(Game game, Player player)
        {
            return game.CurrentAuctionType == AuctionType.BlackMarketSilent && !game.Bids.ContainsKey(player.Faction) && player.HasRoomForCards ||
                   game.CurrentAuctionType != AuctionType.BlackMarketSilent && player == game.BidSequence.CurrentPlayer;
        }
    }

}
