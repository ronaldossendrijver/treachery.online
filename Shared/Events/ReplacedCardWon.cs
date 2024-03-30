/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class ReplacedCardWon : PassableGameEvent
{
    #region Construction

    public ReplacedCardWon(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public ReplacedCardWon()
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
        if (!Passed)
        {
            Game.Discard(Game.CardJustWon);
            var initiator = GetPlayer(Initiator);
            var newCard = Game.DrawTreacheryCard();
            initiator.TreacheryCards.Add(newCard);
            Game.Stone(Milestone.CardWonSwapped);

            if (Game.ReplacingBoughtCardUsingNexus)
            {
                Game.PlayNexusCard(Player, "Secret Ally", "to replace the card they just bought");
                Game.ReplacingBoughtCardUsingNexus = false;
            }
            else
            {
                Log();
            }

            LogTo(initiator.Faction, "You replaced your ", Game.CardJustWon, " with a ", newCard);
        }

        if (Game.CardJustWon == Game.CardSoldOnBlackMarket)
            Game.EnterWhiteBidding();
        else
            Game.DetermineNextStepAfterCardWasSold();
    }

    public override Message GetMessage()
    {
        if (Passed)
            return Message.Express(Initiator, " keep the card they just won");
        return Message.Express(Initiator, " discard the card they just won and draw a new card");
    }

    #endregion Execution
}