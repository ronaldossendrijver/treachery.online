/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class Prescience : GameEvent
{
    #region Construction

    public Prescience(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public Prescience()
    {
    }

    #endregion Construction

    #region Properties

    public PrescienceAspect Aspect { get; set; }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        return null;
    }

    public static IEnumerable<PrescienceAspect> ValidAspects(Game g, Player p)
    {
        var opponent = g.CurrentBattle.OpponentOf(p);
        if (opponent.HasNoFieldIn(g.CurrentBattle.Territory))
            return new[] { PrescienceAspect.Leader, PrescienceAspect.Weapon, PrescienceAspect.Defense };
        return new[] { PrescienceAspect.Dial, PrescienceAspect.Leader, PrescienceAspect.Weapon, PrescienceAspect.Defense };
    }

    public static bool MayUsePrescience(Game g, Player p)
    {
        if (g.CurrentBattle != null && g.CurrentPrescience == null && !g.Prevented(FactionAdvantage.GreenBattlePlanPrescience))
        {
            if (p.Faction == Faction.Green)
                return g.CurrentBattle.IsInvolved(p);
            if (p.Ally == Faction.Green && g.GreenSharesPrescience) return g.CurrentBattle.IsAggressorOrDefender(p);
        }

        return false;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.CurrentPrescience = this;
        Log();
        Game.Stone(Milestone.Prescience);
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " use Prescience to see the enemy ", Aspect);
    }

    #endregion Execution
}