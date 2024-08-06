/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class PlayerReplaced : GameEvent
{
    #region Construction

    public PlayerReplaced(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public PlayerReplaced()
    {
    }

    #endregion Construction

    #region Properties

    public Faction ToReplace { get; set; }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        return null;
    }

    public static IEnumerable<Faction> ValidTargets(Game g)
    {
        return g.Players.Select(p => p.Faction);
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        var player = GetPlayer(ToReplace);
        Log(ToReplace, " will now be played by a ", Game.IsBot(player) ? "Bot" : "Human");
    }

    public override Message GetMessage()
    {
        return Message.Express(ToReplace, " player has been replaced");
    }

    #endregion Execution
}