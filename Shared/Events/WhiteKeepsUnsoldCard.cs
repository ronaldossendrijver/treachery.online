/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class WhiteKeepsUnsoldCard : PassableGameEvent
{
    #region Construction

    public WhiteKeepsUnsoldCard(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public WhiteKeepsUnsoldCard()
    {
    }

    #endregion Construction

    #region Validation

    public override Message Validate()
    {
        return null;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Log();
        var card = Game.CardsOnAuction.Draw();
        Game.RegisterWonCardAsKnown(card);

        if (!Passed)
        {
            Player.TreacheryCards.Add(card);
            LogTo(Initiator, "You get: ", card);
            Game.FinishBid(Player, null, card, Game.Version < 152);
        }
        else
        {
            Game.RemovedTreacheryCards.Add(card);
            Game.FinishBid(null, null, card, Game.Version < 152);
        }
    }

    public override Message GetMessage()
    {
        if (!Passed)
            return Message.Express(Initiator, " keep the card no faction bid on");
        return Message.Express(Initiator, " remove the card no faction bid on from the game");
    }

    #endregion Execution
}