/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class CharityClaimed : GameEvent
{
    #region Construction

    public CharityClaimed(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public CharityClaimed()
    {
    }

    #endregion Construction

    #region Validation

    public override Message Validate()
    {
        var p = Player;
        if (p.Resources > 1) return Message.Express("You are not eligable for charity");

        return null;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.HasActedOrPassed.Add(Initiator);

        Game.GiveCharity(Player, (2 - Player.Resources) * Game.CurrentCharityMultiplier);

        if (!By(Faction.Blue)) Game.ResourceTechTokenIncome = true;

        Game.Stone(Milestone.CharityClaimed);
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " claim charity");
    }

    #endregion Execution
}