/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;

namespace Treachery.Shared;

public class KarmaPinkDial : GameEvent
{
    #region Construction

    public KarmaPinkDial(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public KarmaPinkDial()
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
        Game.Discard(Player, Karma.ValidKarmaCards(Game, Player).FirstOrDefault());
        Player.SpecialKarmaPowerUsed = true;
        Game.Stone(Milestone.Karma);
        var myLeader = Game.CurrentBattle.PlanOf(Initiator).Hero;
        var opponentLeader = Game.CurrentBattle.PlanOfOpponent(Player).Hero;

        if (myLeader != null && opponentLeader != null) Game.PinkKarmaBonus = Math.Abs(myLeader.Value - opponentLeader.ValueInCombatAgainst(myLeader));

        Log("Using ", TreacheryCardType.Karma, ", ", Initiator, " add ", Game.PinkKarmaBonus, " to their dial");
    }

    public override Message GetMessage()
    {
        return Message.Express("Using ", TreacheryCardType.Karma, ", ", Initiator, " add the difference between leader discs to their dial");
    }

    #endregion Execution
}