/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class AutomationConfigured : GameEvent
{
    #region Construction

    public AutomationConfigured(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public AutomationConfigured()
    {
    }

    #endregion Construction

    #region Properties

    public int AutomationRuleId { get; set; } = -1;
    
    public bool Delete { get; set; }
    
    public AutomationRuleType RuleType { get; set; }
    

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        return null;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        if (AutomationRuleId == -1)
        {
            // New rule
            IAutomationRule newRule = RuleType switch
            {
                AutomationRuleType.AutoPassBidding => new AutomationPassBidding(),
                AutomationRuleType.AutoFlipAdvisors => new AutomationFlipAdvisors(),
                AutomationRuleType.AutoKarma => new AutomationKarma(),
                _ => null
            };
            
            if (newRule != null)
                Game.AutomationRules.Add(newRule);
            
            return;
        }

        if (Game.AutomationRules.Count <= AutomationRuleId)
            return;

        if (Delete)
        {
            // Delete rule
            Game.AutomationRules.RemoveAt(AutomationRuleId);
        }
        else
        {
            var existingRule = Game.AutomationRules[AutomationRuleId];
            // Update rule
            //existingRule
        }
        
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " change ally permissions");
    }

    #endregion Execution
}

public interface IAutomationRule
{
    
}

public class AutomationPassBidding : IAutomationRule
{
    
}

public class AutomationFlipAdvisors : IAutomationRule
{
    
}

public class AutomationKarma : IAutomationRule
{
    
}