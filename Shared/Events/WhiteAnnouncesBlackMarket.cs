/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class WhiteAnnouncesBlackMarket : PassableGameEvent
    {
        #region Construction

        public WhiteAnnouncesBlackMarket(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public WhiteAnnouncesBlackMarket()
        {
        }

        #endregion Construction

        #region Properties

        public int _cardId;

        [JsonIgnore]
        public TreacheryCard Card
        {
            get => TreacheryCardManager.Get(_cardId);
            set => _cardId = TreacheryCardManager.GetId(value);
        }

        public AuctionType AuctionType { get; set; }

        public int Direction { get; set; }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (Passed) return null;

            if (AuctionType != AuctionType.BlackMarketNormal && AuctionType != AuctionType.BlackMarketOnceAround && AuctionType != AuctionType.BlackMarketSilent) return Message.Express("Invalid auction type");
            if (!ValidCards(Player).Contains(Card)) return Message.Express("Invalid card");

            return null;
        }

        public static IEnumerable<TreacheryCard> ValidCards(Player p)
        {
            return p.TreacheryCards;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log();

            if (!Passed)
            {
                Game.Enter(Phase.BlackMarketBidding);
                Player.TreacheryCards.Remove(Card);
                Game.CardsOnAuction.PutOnTop(Card);
                Game.RegisterKnown(Initiator, Card);
                Game.Bids.Clear();
                Game.CurrentBid = null;
                Game.StartBidSequenceAndAuctionType(AuctionType, Player, Direction);
            }
            else
            {
                Game.EnterWhiteBidding();
            }
        }

        public override Message GetMessage()
        {
            string directionText = "";
            if (AuctionType == AuctionType.BlackMarketOnceAround)
            {
                if (Direction == 1)
                {
                    directionText = " (counter-clockwise)";
                }
                else
                {
                    directionText = " (clockwise)";
                }
            }

            if (!Passed)
            {
                return Message.Express(Initiator, " put a card on the black market by ", AuctionType, " auction", directionText);
            }
            else
            {
                return Message.Express(Initiator, " don't put a card on the black market");
            }
        }

        #endregion Execution
    }
}
