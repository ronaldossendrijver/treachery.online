/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class RecruitsPlayed : GameEvent
{
    #region Construction

    public RecruitsPlayed(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public RecruitsPlayed()
    {
    }

    #endregion Construction

    #region Validation

    public override Message Validate()
    {
        return null;
    }

    public static bool IsApplicable(Game g, Player p)
    {
        return g.CurrentPhase == Phase.BeginningOfResurrection && p.Has(TreacheryCardType.Recruits);
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Log();
        Game.CurrentRecruitsPlayed = this;
        Game.Discard(Player, TreacheryCardType.Recruits);
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " use ", TreacheryCardType.Recruits, " to double free revivals and set revival limits to ", 7);
    }

    #endregion Execution
}