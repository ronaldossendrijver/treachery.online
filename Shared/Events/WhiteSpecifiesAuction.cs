/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
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

        public override Message Validate()
        {
            if (!Game.WhiteOccupierSpecifiedCard && !ValidCards(Game).Contains(Card)) return Message.Express("Invalid card");

            if (AuctionType != AuctionType.WhiteOnceAround && AuctionType != AuctionType.WhiteSilent) return Message.Express("Invalid auction type");

            return null;
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

            return Message.Express(Initiator, " put a ", Faction.White, " card on ", AuctionType, " auction", directionText);
        }

        public static IEnumerable<TreacheryCard> ValidCards(Game g)
        {
            return g.WhiteCache;
        }

        public static bool MaySpecifyCard(Game g, Player p)
        {
            var occupierOfWhiteHomeworld = g.OccupierOf(World.White);
            return occupierOfWhiteHomeworld == null && p.Faction == Faction.White ||
                   occupierOfWhiteHomeworld != null && !g.WhiteOccupierSpecifiedCard && occupierOfWhiteHomeworld.Is(p.Faction);
        }

        public static bool MaySpecifyTypeOfAuction(Game g, Player p)
        {
            var occupierOfWhiteHomeworld = g.OccupierOf(World.White);
            return occupierOfWhiteHomeworld == null && p.Faction == Faction.White ||
                   occupierOfWhiteHomeworld != null && g.WhiteOccupierSpecifiedCard && p.Faction == Faction.White;
        }
    }
}
