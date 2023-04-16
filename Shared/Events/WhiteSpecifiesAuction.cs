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
        #region Construction

        public WhiteSpecifiesAuction(Game game) : base(game)
        {
        }

        public WhiteSpecifiesAuction()
        {
        }

        #endregion Construction

        #region Properties

        public AuctionType AuctionType { get; set; }

        public int Direction { get; set; }

        public int _cardId;

        [JsonIgnore]
        public TreacheryCard Card
        {
            get => TreacheryCardManager.Get(_cardId);
            set => _cardId = TreacheryCardManager.GetId(value);
        }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (!Game.WhiteOccupierSpecifiedCard && !ValidCards(Game).Contains(Card)) return Message.Express("Invalid card");

            if (AuctionType != AuctionType.WhiteOnceAround && AuctionType != AuctionType.WhiteSilent) return Message.Express("Invalid auction type");

            return null;
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

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            if (!Game.WhiteOccupierSpecifiedCard)
            {
                Game.WhiteAuctionShouldStillHappen = false;
                Game.CardsOnAuction.PutOnTop(Card);
                Game.WhiteCache.Remove(Card);
                Game.RegisterKnown(Card);

                var occupierOfWhiteHomeworld = Game.OccupierOf(World.White);
                if (occupierOfWhiteHomeworld == null)
                {
                    Log();
                    Game.StartBidSequenceAndAuctionType(AuctionType, Player, Direction);
                    Game.StartBiddingRound();
                }
                else
                {
                    Log(Initiator, " put ", Card, " on auction");
                    Game.WhiteOccupierSpecifiedCard = true;
                }
            }
            else
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

                Log(Initiator, " put select a ", AuctionType, " auction", directionText);

                Game.StartBidSequenceAndAuctionType(AuctionType, Player, Direction);
                Game.StartBiddingRound();
            }
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

        #endregion Execution
    }
}
