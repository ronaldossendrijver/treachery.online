/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class WhiteAnnouncesBlackMarket : GameEvent
    {
        public int _cardId;
        public bool Passed;
        public AuctionType AuctionType;
        public int Direction;

        public WhiteAnnouncesBlackMarket(Game game) : base(game)
        {
        }

        public WhiteAnnouncesBlackMarket()
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
            if (Passed) return "";

            if (AuctionType != AuctionType.BlackMarketNormal && AuctionType != AuctionType.BlackMarketOnceAround && AuctionType != AuctionType.BlackMarketSilent) return "Invalid auction type";
            if (!ValidCards(Player).Contains(Card)) return "Invalid card";

            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
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

        public static IEnumerable<TreacheryCard> ValidCards(Player p)
        {
            return p.TreacheryCards;
        }
    }
}
