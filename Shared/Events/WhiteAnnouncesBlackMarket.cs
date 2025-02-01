/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */


namespace Treachery.Shared;

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
        var directionText = "";
        if (AuctionType == AuctionType.BlackMarketOnceAround)
        {
            if (Direction == 1)
                directionText = " (counter-clockwise)";
            else
                directionText = " (clockwise)";
        }

        if (!Passed)
            return Message.Express(Initiator, " put a card on the black market by ", AuctionType, " auction", directionText);
        return Message.Express(Initiator, " don't put a card on the black market");
    }

    #endregion Execution
}