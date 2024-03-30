/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Treachery.Shared;

public class WhiteSpecifiesAuction : GameEvent
{
    #region Construction

    public WhiteSpecifiesAuction(Game game, Faction initiator) : base(game, initiator)
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
        return (occupierOfWhiteHomeworld == null && p.Faction == Faction.White) ||
               (occupierOfWhiteHomeworld != null && !g.WhiteOccupierSpecifiedCard && occupierOfWhiteHomeworld.Is(p.Faction));
    }

    public static bool MaySpecifyTypeOfAuction(Game g, Player p)
    {
        var occupierOfWhiteHomeworld = g.OccupierOf(World.White);
        return (occupierOfWhiteHomeworld == null && p.Faction == Faction.White) ||
               (occupierOfWhiteHomeworld != null && g.WhiteOccupierSpecifiedCard && p.Faction == Faction.White);
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
            var directionText = "";
            if (AuctionType == AuctionType.WhiteOnceAround)
            {
                if (Direction == 1)
                    directionText = " (counter-clockwise)";
                else
                    directionText = " (clockwise)";
            }

            Log(Initiator, " put select a ", AuctionType, " auction", directionText);

            Game.StartBidSequenceAndAuctionType(AuctionType, Player, Direction);
            Game.StartBiddingRound();
        }
    }

    public override Message GetMessage()
    {
        var directionText = "";
        if (AuctionType == AuctionType.WhiteOnceAround)
        {
            if (Direction == 1)
                directionText = " (counter-clockwise)";
            else
                directionText = " (clockwise)";
        }

        return Message.Express(Initiator, " put a ", Faction.White, " card on ", AuctionType, " auction", directionText);
    }

    #endregion Execution
}