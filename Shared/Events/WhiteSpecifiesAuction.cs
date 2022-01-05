/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class WhiteSpecifiesAuction : GameEvent
    {
        public int _cardId;
        public AuctionType AuctionType;
        public int Direction;

        public WhiteSpecifiesAuction(Game game) : base(game)
        {
        }

        public WhiteSpecifiesAuction()
        {
        }

        [JsonIgnore]
        public TreacheryCard Card
        {
            get
            {
                return TreacheryCardManager.Get(_cardId);
            }
            set
            {
                _cardId = TreacheryCardManager.GetId(value);
            }
        }

        public override string Validate()
        {
            if (!ValidCards(Game).Contains(Card)) return "Invalid card";

            if (AuctionType != AuctionType.WhiteOnceAround && AuctionType != AuctionType.WhiteSilent) return "Invalid auction type";

            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            string directionText = "";
            if (AuctionType == AuctionType.WhiteOnceAround)
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

            return new Message(Initiator, "{0} put {1} on {2} auction{3}", Initiator, Card, AuctionType, directionText);
        }

        public static IEnumerable<TreacheryCard> ValidCards(Game g)
        {
            return g.WhiteCache;
        }
    }
}
