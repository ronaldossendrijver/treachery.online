/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class WhiteAnnouncesAuction : GameEvent
{
    #region Construction

    public WhiteAnnouncesAuction(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public WhiteAnnouncesAuction()
    {
    }

    #endregion Construction

    #region Properties

    public bool First { get; set; }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        return null;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.WhiteOccupierSpecifiedCard = false;
        Log();

        var threshold = Game.Version > 150 ? 0 : 1;
        if (Game.Version > 150) Game.NumberOfCardsOnAuction--;

        if (!First && Game.NumberOfCardsOnAuction > threshold) Game.WhiteAuctionShouldStillHappen = true;

        if (Game.NumberOfCardsOnAuction == threshold) Game.RegularBiddingIsDone = true;

        Game.Enter(First || Game.NumberOfCardsOnAuction == threshold, Phase.WhiteSpecifyingAuction, Game.StartRegularBidding);
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " will auction a card from their cache ", First ? "first" : "last");
    }

    #endregion Execution
}