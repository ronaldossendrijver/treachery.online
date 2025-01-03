/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class PortableAntidoteUsed : GameEvent
{
    #region Construction

    public PortableAntidoteUsed(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public PortableAntidoteUsed()
    {
    }

    #endregion Construction

    #region Validation

    public override Message Validate()
    {
        return null;
    }

    public static bool CanBePlayed(Game g, Player p)
    {
        var card = p.Card(TreacheryCardType.PortableAntidote);
        if (card != null)
        {
            var plan = g.CurrentBattle?.PlanOf(p);
            if (plan != null && plan.Defense == null && g.CurrentPortableAntidoteUsed == null && Battle.ValidDefenses(g, p, plan.Weapon, g.CurrentBattle?.Territory).Contains(card)) return true;
        }

        return false;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Log();
        Game.CurrentPortableAntidoteUsed = this;
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " equip a ", TreacheryCardType.PortableAntidote);
    }

    #endregion Execution
}