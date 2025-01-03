/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class ResourcesAudited : GameEvent
{
    #region Construction

    public ResourcesAudited(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public ResourcesAudited()
    {
    }

    #endregion Construction

    #region Properties

    public Faction Target { get; set; }

    #endregion Properties

    #region Validation
    public override Message Validate()
    {
        if (!ValidFactions(Game, Player).Contains(Target)) return Message.Express("Invalid faction");

        return null;
    }

    public static IEnumerable<Faction> ValidFactions(Game game, Player player)
    {
        return game.Players.Where(opp => opp != player &&
                                         !game.ResourceAuditedFactions.Contains(opp.Faction) &&
                                         (player.HomeWorlds.Any(hw => opp.AnyForcesIn(hw) > 0) ||
                                          opp.HomeWorlds.Any(hw => player.AnyForcesIn(hw) > 0))
        ).Select(p => p.Faction);
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.ResourceAuditedFactions.Add(Target);
        var target = GetPlayer(Target);

        Log();
        LogTo(Initiator, Target, " own ", Payment.Of(target.Resources), ", ", target.TreacheryCards.Count(tc => tc.IsWeapon), " weapons and ", target.TreacheryCards.Count(tc => tc.IsDefense), " defenses");
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " force ", Target, " to reveal amount of weapons, defenses and ", Concept.Resource);
    }

    #endregion Execution
}