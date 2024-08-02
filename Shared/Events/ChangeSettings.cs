/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using Newtonsoft.Json;

namespace Treachery.Shared;

public class ChangeSettings : GameEvent
{
    #region Construction

    public ChangeSettings(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public ChangeSettings()
    {
    }

    #endregion Construction

    #region Properties

    public GameSettings Settings { get; set; }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (Settings.AllowedFactionsInPlay.Any(f => !AvailableFactions().Contains(f))) return Message.Express("Invalid faction");

        return null;
    }

    public static List<Faction> AvailableFactions()
    {
        var result = new List<Faction>();

        if (Game.ExpansionLevel >= 0)
        {
            result.Add(Faction.Green);
            result.Add(Faction.Black);
            result.Add(Faction.Yellow);
            result.Add(Faction.Red);
            result.Add(Faction.Orange);
            result.Add(Faction.Blue);
        }

        if (Game.ExpansionLevel >= 1)
        {
            result.Add(Faction.Grey);
            result.Add(Faction.Purple);
        }

        if (Game.ExpansionLevel >= 2)
        {
            result.Add(Faction.Brown);
            result.Add(Faction.White);
        }

        if (Game.ExpansionLevel >= 3)
        {
            result.Add(Faction.Pink);
            result.Add(Faction.Cyan);
        }

        return result;
    }

    public static List<RuleGroup> AvailableRuleGroups()
    {
        var result = new List<RuleGroup>();

        if (Game.ExpansionLevel >= 0)
        {
            result.Add(RuleGroup.CoreAdvanced);
            result.Add(RuleGroup.CoreBasicExceptions);
            result.Add(RuleGroup.CoreAdvancedExceptions);
        }

        if (Game.ExpansionLevel >= 1)
        {
            result.Add(RuleGroup.ExpansionIxAndBtBasic);
            result.Add(RuleGroup.ExpansionIxAndBtAdvanced);
        }

        if (Game.ExpansionLevel >= 2)
        {
            result.Add(RuleGroup.ExpansionBrownAndWhiteBasic);
            result.Add(RuleGroup.ExpansionBrownAndWhiteAdvanced);
        }

        if (Game.ExpansionLevel >= 3)
        {
            result.Add(RuleGroup.ExpansionPinkAndCyanBasic);
            result.Add(RuleGroup.ExpansionPinkAndCyanAdvanced);
        }

        if (Game.ExpansionLevel >= 0) result.Add(RuleGroup.House);

        return result;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.Settings = Settings;
    }

    #endregion Execution
}