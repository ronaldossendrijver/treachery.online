/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class RockWasMelted : GameEvent
{
    #region Construction

    public RockWasMelted(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public RockWasMelted()
    {
    }

    #endregion Construction

    #region Properties

    public bool Kill { get; set; }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        return null;
    }

    public static bool CanBePlayed(Game g, Player p)
    {
        var plan = g.CurrentBattle.PlanOf(p);
        return plan != null && plan.Weapon != null && plan.Weapon.IsRockMelter;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Log();
        if (Game.Version < 146) Game.Discard(Player, TreacheryCardType.Rockmelter);
        Game.CurrentRockWasMelted = this;
        Game.Enter(Phase.CallTraitorOrPass);
    }

    public override Message GetMessage()
    {
        if (Kill)
            return Message.Express(Initiator, " use their ", TreacheryCardType.Rockmelter, " to kill both leaders");
        return Message.Express(Initiator, " use their ", TreacheryCardType.Rockmelter, " to reduce both leaders to 0 strength");
    }

    #endregion Execution
}