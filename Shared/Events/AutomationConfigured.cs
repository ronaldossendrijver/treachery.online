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

    // CharityAutoClaim = 201, *
    
    // BiddingPassWhenGreenOrGreenAllyPassed = 301,
    
    // BiddingPassAboveAmount = 302,
    public int BiddingAboveAmount { get; set; }
    
    // BiddingPassWhenHighestBidByFaction = 303,
    public Faction BiddingWinningFaction { get; set; }
    
    // RevivalAutoClaimFreeRevival = 401,
    
    // ShipmentAutoPass = 501,
    
    // ShipmentOrangeAutoDelay = 502,
    
    // MovementAutoPass = 503,
    
    // MovementAutoPassIfNoForcesOnPlanet = 504,
    
    // BattleAutoSkipTraitorCallIfNotPossible = 601,
    
    // FlipToAdvisorsWhenFactionEntersTerritory = 901,
    

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
            AutomationRuleId = Game.AutomationRules.Count == 0 
                ? 0 
                : Game.AutomationRules.Max(x => x.AutomationRuleId) + 1;
            
            Game.AutomationRules.Add(this);
            
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