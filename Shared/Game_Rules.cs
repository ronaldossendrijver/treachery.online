/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public partial class Game
{
    public const int ExpansionLevel = 3;

    public IEnumerable<Rule> GetCustomRules()
    {
        return Rules.Where(rule => GetRuleGroup(rule) is
            RuleGroup.House or RuleGroup.CoreBasicExceptions or RuleGroup.CoreAdvancedExceptions or
            RuleGroup.ExpansionIxAndBtBasicExceptions or RuleGroup.ExpansionIxAndBtAdvancedExceptions or
            RuleGroup.ExpansionBrownAndWhiteBasicExceptions or RuleGroup.ExpansionBrownAndWhiteAdvancedExceptions or
            RuleGroup.ExpansionPinkAndCyanAdvancedExceptions or RuleGroup.ExpansionPinkAndCyanBasicExceptions);
    }

    public static Ruleset DetermineApproximateRuleset(List<Faction> factions, List<Rule> rules, int expansionLevel)
    {
        var hasExpansion1 = factions.Contains(Faction.Purple) || factions.Contains(Faction.Grey) ||
                            rules.Contains(Rule.ExpansionTreacheryCardsPBandSs) ||
                            rules.Contains(Rule.ExpansionTreacheryCardsAmal) ||
                            rules.Contains(Rule.ExpansionTreacheryCardsExceptPBandSSandAmal) ||
                            rules.Contains(Rule.TechTokens);

        var hasExpansion2 = factions.Contains(Faction.Brown) || factions.Contains(Faction.White) ||
                            rules.Contains(Rule.LeaderSkills) ||
                            rules.Contains(Rule.Expansion2TreacheryCards) ||
                            rules.Contains(Rule.StrongholdBonus);

        var hasExpansion3 = factions.Contains(Faction.Pink) || factions.Contains(Faction.Cyan) ||
                            rules.Contains(Rule.Expansion3TreacheryCards) ||
                            rules.Contains(Rule.GreatMaker) ||
                            rules.Contains(Rule.DiscoveryTokens) ||
                            rules.Contains(Rule.Homeworlds) ||
                            rules.Contains(Rule.NexusCards);

        if (AdvancedRulesApply(rules))
        {
            if (expansionLevel == 3 && hasExpansion1 && hasExpansion2 && hasExpansion3)
                return Ruleset.AllExpansionsAdvancedGame;
            if (expansionLevel == 2 && hasExpansion1 && hasExpansion2) return Ruleset.AllExpansionsAdvancedGame;
            if (expansionLevel == 1 && hasExpansion1) return Ruleset.AllExpansionsAdvancedGame;
            if (hasExpansion3) return Ruleset.Expansion3AdvancedGame;
            if (hasExpansion2) return Ruleset.Expansion2AdvancedGame;
            if (hasExpansion1) return Ruleset.ExpansionAdvancedGame;
            return Ruleset.AdvancedGame;
        }

        if (expansionLevel == 3 && hasExpansion1 && hasExpansion2 && hasExpansion3)
            return Ruleset.AllExpansionsBasicGame;
        if (expansionLevel == 2 && hasExpansion1 && hasExpansion2) return Ruleset.AllExpansionsBasicGame;
        if (expansionLevel == 1 && hasExpansion1) return Ruleset.AllExpansionsBasicGame;
        if (hasExpansion3) return Ruleset.Expansion3BasicGame;
        if (hasExpansion2) return Ruleset.Expansion2BasicGame;
        if (hasExpansion1) return Ruleset.ExpansionBasicGame;
        return Ruleset.BasicGame;
    }

    private static bool AdvancedRulesApply(List<Rule> rules)
    {
        return
            rules.Contains(Rule.AdvancedCombat) ||
            rules.Contains(Rule.IncreasedResourceFlow) ||
            rules.Contains(Rule.AdvancedKarama) ||
            rules.Contains(Rule.GreenMessiah) ||
            rules.Contains(Rule.BlackCapturesOrKillsLeaders) ||
            rules.Contains(Rule.YellowSpecialForces) ||
            rules.Contains(Rule.RedSpecialForces) ||
            rules.Contains(Rule.OrangeDetermineShipment) ||
            rules.Contains(Rule.BlueAdvisors) ||
            rules.Contains(Rule.GreySwappingCardOnBid) ||
            rules.Contains(Rule.PurpleGholas) ||
            rules.Contains(Rule.BrownAuditor) ||
            rules.Contains(Rule.WhiteBlackMarket) ||
            rules.Contains(Rule.CyanAssassinate) ||
            rules.Contains(Rule.PinkLoyalty) ||
            rules.Contains(Rule.StrongholdBonus);
    }

    public static Dictionary<Ruleset, Rule[]> RulesetDefinition { get; private set; } = new()
    {
        [Ruleset.BasicGame] =
        [
            Rule.BasicTreacheryCards,
            Rule.HasCharityPhase
        ],

        [Ruleset.AdvancedGame] = new[]
        {
            Rule.AdvancedCombat,
            Rule.IncreasedResourceFlow,
            Rule.AdvancedKarama,
            Rule.YellowDeterminesStorm,
            Rule.YellowStormLosses,
            Rule.YellowSendingMonster,
            Rule.YellowSpecialForces,
            Rule.GreenMessiah,
            Rule.BlackCapturesOrKillsLeaders,
            Rule.BlueAutoCharity,
            Rule.BlueWorthlessAsKarma,
            Rule.BlueAdvisors,
            Rule.OrangeDetermineShipment,
            Rule.RedSpecialForces
        }.Concat(ExpansionLevel < 1
            ? []
            : [ Rule.GreySwappingCardOnBid, Rule.PurpleGholas]
        ).Concat(ExpansionLevel < 2
            ? []
            : [ Rule.BrownAuditor, Rule.WhiteBlackMarket ]
        ).Concat(ExpansionLevel < 3
            ? []
            : [ Rule.CyanAssassinate, Rule.PinkLoyalty]).ToArray(),

        [Ruleset.AllExpansionsBasicGame] = Array.Empty<Rule>()
            .Concat(ExpansionLevel < 1
                ? []
                : [ Rule.TechTokens, Rule.CheapHeroTraitor, Rule.ExpansionTreacheryCards, Rule.SandTrout]
            ).Concat(ExpansionLevel < 2
                ? []
                : [ Rule.LeaderSkills, Rule.Expansion2TreacheryCards])
            .Concat(ExpansionLevel < 3
                ? []
                : [ Rule.Expansion3TreacheryCards, Rule.DiscoveryTokens, Rule.Homeworlds, Rule.NexusCards, Rule.GreatMaker]).ToArray(),

        [Ruleset.ExpansionBasicGame] =
        [
            Rule.TechTokens,
            Rule.CheapHeroTraitor,
            Rule.ExpansionTreacheryCards,
            Rule.SandTrout
        ],

        [Ruleset.Expansion2BasicGame] =
        [
            Rule.LeaderSkills,
            Rule.Expansion2TreacheryCards
        ],

        [Ruleset.Expansion3BasicGame] =
        [
            Rule.Expansion3TreacheryCards,
            Rule.DiscoveryTokens,
            Rule.Homeworlds,
            Rule.NexusCards,
            Rule.GreatMaker
        ],

        [Ruleset.AllExpansionsAdvancedGame] = new[]
        {
            Rule.AdvancedCombat,
            Rule.IncreasedResourceFlow,
            Rule.AdvancedKarama,
            Rule.YellowDeterminesStorm,
            Rule.YellowStormLosses,
            Rule.YellowSendingMonster,
            Rule.YellowSpecialForces,
            Rule.GreenMessiah,
            Rule.BlackCapturesOrKillsLeaders,
            Rule.BlueAutoCharity,
            Rule.BlueWorthlessAsKarma,
            Rule.BlueAdvisors,
            Rule.OrangeDetermineShipment,
            Rule.RedSpecialForces
        }.Concat(ExpansionLevel < 1
            ? []
            : [ Rule.TechTokens, Rule.CheapHeroTraitor, Rule.ExpansionTreacheryCards, Rule.SandTrout, Rule.GreySwappingCardOnBid, Rule.PurpleGholas ]
        ).Concat(ExpansionLevel < 2
            ? []
            : [Rule.LeaderSkills, Rule.Expansion2TreacheryCards, Rule.StrongholdBonus, Rule.BrownAuditor, Rule.WhiteBlackMarket ]
        ).Concat(ExpansionLevel < 3
            ? []
            : [ Rule.Expansion3TreacheryCards, Rule.DiscoveryTokens, Rule.Homeworlds, Rule.NexusCards, Rule.GreatMaker, Rule.CyanAssassinate, Rule.PinkLoyalty]).ToArray(),

        [Ruleset.ExpansionAdvancedGame] = new[]
        {
            Rule.AdvancedCombat,
            Rule.IncreasedResourceFlow,
            Rule.AdvancedKarama,
            Rule.YellowDeterminesStorm,
            Rule.YellowStormLosses,
            Rule.YellowSendingMonster,
            Rule.YellowSpecialForces,
            Rule.GreenMessiah,
            Rule.BlackCapturesOrKillsLeaders,
            Rule.BlueAutoCharity,
            Rule.BlueWorthlessAsKarma,
            Rule.BlueAdvisors,
            Rule.OrangeDetermineShipment,
            Rule.RedSpecialForces
        }.Concat(ExpansionLevel < 1
            ? []
            : [
            
                Rule.TechTokens,
                Rule.CheapHeroTraitor,
                Rule.ExpansionTreacheryCards,
                Rule.SandTrout,
                Rule.GreySwappingCardOnBid,
                Rule.PurpleGholas,
                Rule.BrownAuditor,
                Rule.WhiteBlackMarket,
                Rule.CyanAssassinate,
                Rule.PinkLoyalty
            ]).ToArray(),

        [Ruleset.Expansion2AdvancedGame] = new[]
        {
            Rule.AdvancedCombat,
            Rule.IncreasedResourceFlow,
            Rule.AdvancedKarama,
            Rule.YellowDeterminesStorm,
            Rule.YellowStormLosses,
            Rule.YellowSendingMonster,
            Rule.YellowSpecialForces,
            Rule.GreenMessiah,
            Rule.BlackCapturesOrKillsLeaders,
            Rule.BlueAutoCharity,
            Rule.BlueWorthlessAsKarma,
            Rule.BlueAdvisors,
            Rule.OrangeDetermineShipment,
            Rule.RedSpecialForces
        }.Concat(ExpansionLevel < 2
            ? []
            : new[]
            {
                Rule.LeaderSkills,
                Rule.Expansion2TreacheryCards,
                Rule.StrongholdBonus,
                Rule.GreySwappingCardOnBid,
                Rule.PurpleGholas,
                Rule.BrownAuditor,
                Rule.WhiteBlackMarket,
                Rule.StrongholdBonus,
                Rule.CyanAssassinate,
                Rule.PinkLoyalty
            }).ToArray(),

        [Ruleset.Expansion3AdvancedGame] = new[]
        {
            Rule.AdvancedCombat,
            Rule.IncreasedResourceFlow,
            Rule.AdvancedKarama,
            Rule.YellowDeterminesStorm,
            Rule.YellowStormLosses,
            Rule.YellowSendingMonster,
            Rule.YellowSpecialForces,
            Rule.GreenMessiah,
            Rule.BlackCapturesOrKillsLeaders,
            Rule.BlueAutoCharity,
            Rule.BlueWorthlessAsKarma,
            Rule.BlueAdvisors,
            Rule.OrangeDetermineShipment,
            Rule.RedSpecialForces
        }.Concat(ExpansionLevel < 3
            ? []
            : [
                Rule.DiscoveryTokens,
                Rule.GreatMaker,
                Rule.NexusCards,
                Rule.Expansion3TreacheryCards,
                Rule.Homeworlds,
                Rule.GreySwappingCardOnBid,
                Rule.PurpleGholas,
                Rule.BrownAuditor,
                Rule.WhiteBlackMarket,
                Rule.CyanAssassinate,
                Rule.PinkLoyalty
            ]).ToArray(),

        [Ruleset.ServerClassic] =
        [
            Rule.AdvancedCombat,
            Rule.IncreasedResourceFlow,
            Rule.AdvancedKarama,
            Rule.YellowDeterminesStorm,
            Rule.YellowStormLosses,
            Rule.YellowSendingMonster,
            Rule.YellowSpecialForces,
            Rule.GreenMessiah,
            Rule.BlackCapturesOrKillsLeaders,
            Rule.BlueAutoCharity,
            Rule.BlueWorthlessAsKarma,
            Rule.BlueAdvisors,
            Rule.OrangeDetermineShipment,
            Rule.RedSpecialForces,
            Rule.OrangeShipmentContributionsFlowBack,
            Rule.FullPhaseKarma,
            Rule.BlueVoiceMustNameSpecialCards,
            Rule.BattlesUnderStorm,
            Rule.AdvisorsDontConflictWithAlly,
            Rule.YellowMayMoveIntoStorm,
            Rule.Ssw,
            Rule.BlackMulligan
        ]
    };

    public static IEnumerable<Rule> GetRulesInGroup(RuleGroup group, int expansionLevel)
    {
        return Enumerations.GetValues<Rule>()
            .Where(r => GetRuleGroup(r) == group && GetRuleExpansion(r) <= expansionLevel);
    }

    public static IEnumerable<Ruleset> GetAvailableRulesets()
    {
        return Enumerations.GetValuesExceptDefault(Ruleset.None).Where(rs => GetRulesetExpansion(rs) <= ExpansionLevel);
    }

    public bool Applicable(Rule rule)
    {
        return Rules.Contains(rule) || RulesForBots.Contains(rule);
    }

    public static RuleGroup GetRuleGroup(Rule rule) => rule switch
    {
        Rule.HasCharityPhase or Rule.BasicTreacheryCards => RuleGroup.CoreBasic,
        Rule.AdvancedCombat or Rule.IncreasedResourceFlow or Rule.AdvancedKarama or Rule.YellowDeterminesStorm
            or Rule.YellowStormLosses or Rule.YellowSendingMonster or Rule.YellowSpecialForces or Rule.GreenMessiah
            or Rule.BlackCapturesOrKillsLeaders or Rule.BlueAutoCharity or Rule.BlueWorthlessAsKarma
            or Rule.BlueAdvisors or Rule.OrangeDetermineShipment or Rule.RedSpecialForces => RuleGroup.CoreAdvanced,
        Rule.BribesAreImmediate or Rule.ContestedStongholdsCountAsOccupied
            or Rule.OrangeShipmentContributionsFlowBack or Rule.FullPhaseKarma or Rule.BlueVoiceMustNameSpecialCards
            or Rule.BattlesUnderStorm
            or Rule.MovementBonusRequiresOccupationBeforeMovement => RuleGroup.CoreBasicExceptions,
        Rule.AdvisorsDontConflictWithAlly or Rule.YellowMayMoveIntoStorm => RuleGroup.CoreAdvancedExceptions,
        Rule.CustomInitialForcesAndResources or Rule.CustomDecks or Rule.HmSwithoutGrey
            or Rule.StormDeckWithoutYellow or Rule.Ssw or Rule.BlackMulligan or Rule.ExtraKaramaCards
            or Rule.CardsCanBeTraded or Rule.PlayersChooseFactions or Rule.RedSupportingNonAllyBids
            or Rule.AssistedNotekeeping or Rule.AssistedNotekeepingForGreen or Rule.ResourceBonusForStrongholds
            or Rule.BattleWithoutLeader or Rule.CapturedLeadersAreTraitorsToOwnFaction
            or Rule.DisableEndOfGameReport or Rule.DisableOrangeSpecialVictory or Rule.DisableResourceTransfers
            or Rule.DisableNovaFlipping or Rule.YellowAllyGetsDialedResourcesRefunded => RuleGroup.House,
        Rule.BotsCannotAlly => RuleGroup.Bots,
        Rule.TechTokens or Rule.CheapHeroTraitor or Rule.ExpansionTreacheryCards or Rule.SandTrout => RuleGroup
            .ExpansionIxAndBtBasic,
        Rule.GreySwappingCardOnBid or Rule.PurpleGholas => RuleGroup.ExpansionIxAndBtAdvanced,
        Rule.LeaderSkills or Rule.Expansion2TreacheryCards => RuleGroup.ExpansionBrownAndWhiteBasic,
        Rule.BrownAuditor or Rule.WhiteBlackMarket or Rule.StrongholdBonus => RuleGroup
            .ExpansionBrownAndWhiteAdvanced,
        Rule.Expansion3TreacheryCards or Rule.DiscoveryTokens or Rule.Homeworlds or Rule.NexusCards
            or Rule.GreatMaker => RuleGroup.ExpansionPinkAndCyanBasic,
        Rule.CyanAssassinate or Rule.PinkLoyalty => RuleGroup.ExpansionPinkAndCyanAdvanced,
        _ => RuleGroup.None
    };

    private static int GetRulesetExpansion(Ruleset ruleset)
        => ruleset switch
        {
            Ruleset.None or Ruleset.BasicGame or Ruleset.AdvancedGame or Ruleset.ServerClassic or Ruleset.Custom
                or Ruleset.AllExpansionsBasicGame or Ruleset.AllExpansionsAdvancedGame => 0,
            Ruleset.ExpansionBasicGame or Ruleset.ExpansionAdvancedGame => 1,
            Ruleset.Expansion2BasicGame or Ruleset.Expansion2AdvancedGame => 2,
            Ruleset.Expansion3BasicGame or Ruleset.Expansion3AdvancedGame => 3,
            _ => int.MaxValue
        };

    private static int GetRuleExpansion(Rule rule)
        => rule switch
        {
            Rule.HasCharityPhase or Rule.BasicTreacheryCards => 0,
            Rule.AdvancedCombat or Rule.IncreasedResourceFlow or Rule.AdvancedKarama or Rule.YellowDeterminesStorm
                or Rule.YellowStormLosses or Rule.YellowSendingMonster or Rule.YellowSpecialForces or Rule.GreenMessiah
                or Rule.BlackCapturesOrKillsLeaders or Rule.BlueAutoCharity or Rule.BlueWorthlessAsKarma
                or Rule.BlueAdvisors or Rule.OrangeDetermineShipment or Rule.RedSpecialForces => 0,
            Rule.BribesAreImmediate or Rule.ContestedStongholdsCountAsOccupied
                or Rule.OrangeShipmentContributionsFlowBack or Rule.FullPhaseKarma or Rule.BlueVoiceMustNameSpecialCards
                or Rule.BattlesUnderStorm or Rule.MovementBonusRequiresOccupationBeforeMovement => 0,
            Rule.AdvisorsDontConflictWithAlly or Rule.YellowMayMoveIntoStorm => 0,
            Rule.CustomInitialForcesAndResources or Rule.CustomDecks or Rule.HmSwithoutGrey
                or Rule.StormDeckWithoutYellow or Rule.Ssw or Rule.BlackMulligan or Rule.ExtraKaramaCards
                or Rule.CardsCanBeTraded or Rule.PlayersChooseFactions or Rule.RedSupportingNonAllyBids
                or Rule.AssistedNotekeeping or Rule.AssistedNotekeepingForGreen or Rule.ResourceBonusForStrongholds
                or Rule.BattleWithoutLeader or Rule.CapturedLeadersAreTraitorsToOwnFaction
                or Rule.DisableEndOfGameReport or Rule.DisableOrangeSpecialVictory or Rule.DisableResourceTransfers
                or Rule.YellowAllyGetsDialedResourcesRefunded or Rule.DisableNovaFlipping => 0,
            Rule.FillWithBots or Rule.BotsCannotAlly => 0,
            Rule.TechTokens or Rule.CheapHeroTraitor or Rule.ExpansionTreacheryCards or Rule.SandTrout => 1,
            Rule.GreySwappingCardOnBid or Rule.PurpleGholas => 1,
            Rule.LeaderSkills or Rule.Expansion2TreacheryCards => 2,
            Rule.BrownAuditor or Rule.WhiteBlackMarket or Rule.StrongholdBonus => 2,
            Rule.GreatMaker or Rule.DiscoveryTokens or Rule.Homeworlds or Rule.NexusCards
                or Rule.Expansion3TreacheryCards or Rule.CyanAssassinate or Rule.PinkLoyalty => 3,
            _ => int.MaxValue
        };

    public bool IsAutomated(AutomationRuleType rule, Player p)
        => AutomationRules.Any(x => x.Initiator == p.Faction && x.RuleType == rule);
    
    public bool IsAutoPassedBid(Faction f)
    {
        foreach (var rule in AutomationRules.Where(x => x.Initiator == f))
        {
            switch (rule.RuleType)
            {
                case AutomationRuleType.BiddingPassAboveAmount:
                    if (CurrentBid != null && CurrentBid.TotalAmount >= rule.BiddingAboveAmount) return true;
                    break;
                
                case AutomationRuleType.BiddingPassWhenGreenOrGreenAllyPassed:
                    if (LatestBidByGreenOrGreenAllyWasPassed && f is not Faction.Green) return true;
                    break;
                
                case AutomationRuleType.BiddingPassWhenHighestBidByFaction:
                    if (CurrentBid != null && CurrentBid.Initiator == rule.BiddingWinningFaction) return true;
                    break;    
            }
        } 
        
        return false;
    }
}