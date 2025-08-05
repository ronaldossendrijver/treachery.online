/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class SwitchedSkilledLeader : GameEvent
{
    #region Construction

    public SwitchedSkilledLeader(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public SwitchedSkilledLeader()
    {
    }

    #endregion Construction

    #region Validation

    public override Message Validate()
    {
        return null;
    }

    public static Leader SwitchableLeader(Game game, Player player)
    {
        return player.Leaders.FirstOrDefault(l =>
            game.IsSkilled(l) && !game.CapturedLeaders.ContainsKey(l) &&
            (player.Faction == Faction.Pink || l.HeroType != HeroType.Vidal));
    }

    public static bool CanBePlayed(Game game, Player player)
    {
        return game.CurrentPhase == Phase.BattlePhase 
               && game.CurrentBattle != null 
               && game.CurrentBattle.IsAggressorOrDefender(player) 
               && game.CurrentBattle.PlanOf(player) == null 
               && SwitchableLeader(game, player) != null;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        var leader = SwitchableLeader(Game, Player);
        Game.SetInFrontOfShield(leader, !Game.IsInFrontOfShield(leader));
        Log(Initiator, " place ", Game.Skill(leader), " ", leader, Game.IsInFrontOfShield(leader) ? " in front of" : " behind", " their shield");
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " switch their skilled leader");
    }

    #endregion Execution
}