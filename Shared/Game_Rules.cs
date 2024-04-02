/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;

namespace Treachery.Shared;

public partial class Game
{
    public IEnumerable<Rule> GetCustomRules()
    {
        return Rules.Where(rule =>
            GetRuleGroup(rule) == RuleGroup.House ||
            GetRuleGroup(rule) == RuleGroup.CoreBasicExceptions ||
            GetRuleGroup(rule) == RuleGroup.CoreAdvancedExceptions ||
            GetRuleGroup(rule) == RuleGroup.ExpansionIxAndBtBasicExceptions ||
            GetRuleGroup(rule) == RuleGroup.ExpansionIxAndBtAdvancedExceptions ||
            GetRuleGroup(rule) == RuleGroup.ExpansionBrownAndWhiteBasicExceptions ||
            GetRuleGroup(rule) == RuleGroup.ExpansionPinkAndCyanAdvancedExceptions ||
            GetRuleGroup(rule) == RuleGroup.ExpansionPinkAndCyanBasicExceptions ||
            GetRuleGroup(rule) == RuleGroup.ExpansionPinkAndCyanAdvancedExceptions ||
            GetRuleGroup(rule) == RuleGroup.CoreBasicExceptions);
    }

    public static Ruleset DetermineApproximateRuleset(IEnumerable<Faction> factions, IEnumerable<Rule> rules, int expansionLevel)
    {
        var hasExpansion1 = factions.Contains(Faction.Purple) || factions.Contains(Faction.Grey) ||
                            rules.Contains(Rule.ExpansionTreacheryCardsPBandSS) ||
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
            if (expansionLevel == 3 && hasExpansion1 && hasExpansion2 && hasExpansion3) return Ruleset.AllExpansionsAdvancedGame;
            if (expansionLevel == 2 && hasExpansion1 && hasExpansion2) return Ruleset.AllExpansionsAdvancedGame;
            if (expansionLevel == 1 && hasExpansion1) return Ruleset.AllExpansionsAdvancedGame;
            if (hasExpansion3) return Ruleset.Expansion3AdvancedGame;
            if (hasExpansion2) return Ruleset.Expansion2AdvancedGame;
            if (hasExpansion1) return Ruleset.ExpansionAdvancedGame;
            return Ruleset.AdvancedGame;
        }

        if (expansionLevel == 3 && hasExpansion1 && hasExpansion2 && hasExpansion3) return Ruleset.AllExpansionsBasicGame;
        if (expansionLevel == 2 && hasExpansion1 && hasExpansion2) return Ruleset.AllExpansionsBasicGame;
        if (expansionLevel == 1 && hasExpansion1) return Ruleset.AllExpansionsBasicGame;
        if (hasExpansion3) return Ruleset.Expansion3BasicGame;
        if (hasExpansion2) return Ruleset.Expansion2BasicGame;
        if (hasExpansion1) return Ruleset.ExpansionBasicGame;
        return Ruleset.BasicGame;
    }

    private static bool AdvancedRulesApply(IEnumerable<Rule> rules)
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
        [Ruleset.BasicGame] = new[] {
            Rule.BasicTreacheryCards,
            Rule.HasCharityPhase
        },

        [Ruleset.AdvancedGame] = new[] {
            Rule.AdvancedCombat,
            Rule.IncreasedResourceFlow,
            Rule.AdvancedKarama,
            Rule.YellowSeesStorm,
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
        }.Concat(ExpansionLevel < 1 ? Array.Empty<Rule>() : new[] {
            Rule.GreySwappingCardOnBid,
            Rule.PurpleGholas
        }).Concat(ExpansionLevel < 2 ? Array.Empty<Rule>() : new[] {
            Rule.BrownAuditor,
            Rule.WhiteBlackMarket
        }).Concat(ExpansionLevel < 3 ? Array.Empty<Rule>() : new[] {
            Rule.CyanAssassinate,
            Rule.PinkLoyalty
        }).ToArray(),

        [Ruleset.AllExpansionsBasicGame] = Array.Empty<Rule>()
            .Concat(ExpansionLevel < 1 ? Array.Empty<Rule>() : new[] {
                Rule.TechTokens,
                Rule.CheapHeroTraitor,
                Rule.ExpansionTreacheryCards,
                Rule.SandTrout
            }).Concat(ExpansionLevel < 2 ? Array.Empty<Rule>() : new[] {
                Rule.LeaderSkills,
                Rule.Expansion2TreacheryCards
            }).Concat(ExpansionLevel < 3 ? Array.Empty<Rule>() : new[] {
                Rule.Expansion3TreacheryCards,
                Rule.DiscoveryTokens,
                Rule.Homeworlds,
                Rule.NexusCards,
                Rule.GreatMaker
            }).ToArray(),

        [Ruleset.ExpansionBasicGame] = new[] {
            Rule.TechTokens,
            Rule.CheapHeroTraitor,
            Rule.ExpansionTreacheryCards,
            Rule.SandTrout
        },

        [Ruleset.Expansion2BasicGame] = new[] {
            Rule.LeaderSkills,
            Rule.Expansion2TreacheryCards
        },

        [Ruleset.Expansion3BasicGame] = new[] {
            Rule.Expansion3TreacheryCards,
            Rule.DiscoveryTokens,
            Rule.Homeworlds,
            Rule.NexusCards,
            Rule.GreatMaker
        },

        [Ruleset.AllExpansionsAdvancedGame] = new[] {
            Rule.AdvancedCombat,
            Rule.IncreasedResourceFlow,
            Rule.AdvancedKarama,
            Rule.YellowSeesStorm,
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
        }.Concat(ExpansionLevel < 1 ? Array.Empty<Rule>() : new[] {
            Rule.TechTokens,
            Rule.CheapHeroTraitor,
            Rule.ExpansionTreacheryCards,
            Rule.SandTrout,
            Rule.GreySwappingCardOnBid,
            Rule.PurpleGholas
        }).Concat(ExpansionLevel < 2 ? Array.Empty<Rule>() : new[] {
            Rule.LeaderSkills,
            Rule.Expansion2TreacheryCards,
            Rule.StrongholdBonus,
            Rule.BrownAuditor,
            Rule.WhiteBlackMarket
        }).Concat(ExpansionLevel < 3 ? Array.Empty<Rule>() : new[] {
            Rule.Expansion3TreacheryCards,
            Rule.DiscoveryTokens,
            Rule.Homeworlds,
            Rule.NexusCards,
            Rule.GreatMaker,
            Rule.CyanAssassinate,
            Rule.PinkLoyalty
        }).ToArray(),

        [Ruleset.ExpansionAdvancedGame] = new[] {
            Rule.AdvancedCombat,
            Rule.IncreasedResourceFlow,
            Rule.AdvancedKarama,
            Rule.YellowSeesStorm,
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
        }.Concat(ExpansionLevel < 1 ? Array.Empty<Rule>() : new[] {
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
        }).ToArray(),

        [Ruleset.Expansion2AdvancedGame] = new[] {
            Rule.AdvancedCombat,
            Rule.IncreasedResourceFlow,
            Rule.AdvancedKarama,
            Rule.YellowSeesStorm,
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
        }.Concat(ExpansionLevel < 2 ? Array.Empty<Rule>() : new[] {
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

        [Ruleset.Expansion3AdvancedGame] = new[] {
            Rule.AdvancedCombat,
            Rule.IncreasedResourceFlow,
            Rule.AdvancedKarama,
            Rule.YellowSeesStorm,
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
        }.Concat(ExpansionLevel < 3 ? Array.Empty<Rule>() : new[] {
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
        }).ToArray(),

        [Ruleset.ServerClassic] = new[] {
            Rule.AdvancedCombat,
            Rule.IncreasedResourceFlow,
            Rule.AdvancedKarama,
            Rule.YellowSeesStorm,
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
            Rule.SSW,
            Rule.BlackMulligan
        }
    };

    public static IEnumerable<Rule> GetRulesInGroup(RuleGroup group, int expansionLevel)
    {
        return Enumerations.GetValues<Rule>(typeof(Rule)).Where(r => GetRuleGroup(r) == group && GetRuleExpansion(r) <= expansionLevel);
    }

    public static IEnumerable<Ruleset> GetAvailableRulesets()
    {
        return Enumerations.GetValuesExceptDefault(typeof(Ruleset), Ruleset.None).Where(rs => GetRulesetExpansion(rs) <= ExpansionLevel);
    }

    public bool Applicable(Rule rule)
    {
        return Rules.Contains(rule) || RulesForBots.Contains(rule);
    }

    public static RuleGroup GetRuleGroup(Rule rule)
    {
        switch (rule)
        {
            case Rule.HasCharityPhase:
            case Rule.BasicTreacheryCards:
                return RuleGroup.CoreBasic;

            case Rule.AdvancedCombat:
            case Rule.IncreasedResourceFlow:
            case Rule.AdvancedKarama:
            case Rule.YellowSeesStorm:
            case Rule.YellowStormLosses:
            case Rule.YellowSendingMonster:
            case Rule.YellowSpecialForces:
            case Rule.GreenMessiah:
            case Rule.BlackCapturesOrKillsLeaders:
            case Rule.BlueAutoCharity:
            case Rule.BlueWorthlessAsKarma:
            case Rule.BlueAdvisors:
            case Rule.OrangeDetermineShipment:
            case Rule.RedSpecialForces:
                return RuleGroup.CoreAdvanced;

            case Rule.BribesAreImmediate:
            case Rule.ContestedStongholdsCountAsOccupied:
            case Rule.OrangeShipmentContributionsFlowBack:
            case Rule.FullPhaseKarma:
            case Rule.BlueVoiceMustNameSpecialCards:
            case Rule.BattlesUnderStorm:
            case Rule.MovementBonusRequiresOccupationBeforeMovement:
                return RuleGroup.CoreBasicExceptions;

            case Rule.AdvisorsDontConflictWithAlly:
            case Rule.YellowMayMoveIntoStorm:
                return RuleGroup.CoreAdvancedExceptions;

            case Rule.CustomInitialForcesAndResources:
            case Rule.CustomDecks:
            case Rule.HMSwithoutGrey:
            case Rule.StormDeckWithoutYellow:
            case Rule.SSW:
            case Rule.BlackMulligan:
            case Rule.ExtraKaramaCards:
            case Rule.CardsCanBeTraded:
            case Rule.PlayersChooseFactions:
            case Rule.RedSupportingNonAllyBids:
            case Rule.AssistedNotekeeping:
            case Rule.AssistedNotekeepingForGreen:
            case Rule.ResourceBonusForStrongholds:
            case Rule.BattleWithoutLeader:
            case Rule.CapturedLeadersAreTraitorsToOwnFaction:
            case Rule.DisableEndOfGameReport:
            case Rule.DisableOrangeSpecialVictory:
            case Rule.DisableResourceTransfers:
            case Rule.YellowAllyGetsDialedResourcesRefunded:
                return RuleGroup.House;

            case Rule.FillWithBots:
            case Rule.BotsCannotAlly:
                return RuleGroup.Bots;

            case Rule.TechTokens:
            case Rule.CheapHeroTraitor:
            case Rule.ExpansionTreacheryCards:
            case Rule.SandTrout:
                return RuleGroup.ExpansionIxAndBtBasic;

            case Rule.GreySwappingCardOnBid:
            case Rule.PurpleGholas:
                return RuleGroup.ExpansionIxAndBtAdvanced;

            case Rule.LeaderSkills:
            case Rule.Expansion2TreacheryCards:
                return RuleGroup.ExpansionBrownAndWhiteBasic;

            case Rule.BrownAuditor:
            case Rule.WhiteBlackMarket:
            case Rule.StrongholdBonus:
                return RuleGroup.ExpansionBrownAndWhiteAdvanced;

            case Rule.Expansion3TreacheryCards:
            case Rule.DiscoveryTokens:
            case Rule.Homeworlds:
            case Rule.NexusCards:
            case Rule.GreatMaker:
                return RuleGroup.ExpansionPinkAndCyanBasic;

            case Rule.CyanAssassinate:
            case Rule.PinkLoyalty:
                return RuleGroup.ExpansionPinkAndCyanAdvanced;
        }

        return RuleGroup.None;
    }

    public static int GetRulesetExpansion(Ruleset ruleset)
    {
        switch (ruleset)
        {
            case Ruleset.None:
            case Ruleset.BasicGame:
            case Ruleset.AdvancedGame:
            case Ruleset.ServerClassic:
            case Ruleset.Custom:
            case Ruleset.AllExpansionsBasicGame:
            case Ruleset.AllExpansionsAdvancedGame:
                return 0;

            case Ruleset.ExpansionBasicGame:
            case Ruleset.ExpansionAdvancedGame:
                return 1;

            case Ruleset.Expansion2BasicGame:
            case Ruleset.Expansion2AdvancedGame:
                return 2;

            case Ruleset.Expansion3BasicGame:
            case Ruleset.Expansion3AdvancedGame:
                return 3;

        }

        return int.MaxValue;
    }

    public static int GetRuleExpansion(Rule rule)
    {
        switch (rule)
        {
            case Rule.HasCharityPhase:
            case Rule.BasicTreacheryCards:
                return 0;

            case Rule.AdvancedCombat:
            case Rule.IncreasedResourceFlow:
            case Rule.AdvancedKarama:
            case Rule.YellowSeesStorm:
            case Rule.YellowStormLosses:
            case Rule.YellowSendingMonster:
            case Rule.YellowSpecialForces:
            case Rule.GreenMessiah:
            case Rule.BlackCapturesOrKillsLeaders:
            case Rule.BlueAutoCharity:
            case Rule.BlueWorthlessAsKarma:
            case Rule.BlueAdvisors:
            case Rule.OrangeDetermineShipment:
            case Rule.RedSpecialForces:
                return 0;

            case Rule.BribesAreImmediate:
            case Rule.ContestedStongholdsCountAsOccupied:
            case Rule.OrangeShipmentContributionsFlowBack:
            case Rule.FullPhaseKarma:
            case Rule.BlueVoiceMustNameSpecialCards:
            case Rule.BattlesUnderStorm:
            case Rule.MovementBonusRequiresOccupationBeforeMovement:
                return 0;

            case Rule.AdvisorsDontConflictWithAlly:
            case Rule.YellowMayMoveIntoStorm:
                return 0;

            case Rule.CustomInitialForcesAndResources:
            case Rule.CustomDecks:
            case Rule.HMSwithoutGrey:
            case Rule.StormDeckWithoutYellow:
            case Rule.SSW:
            case Rule.BlackMulligan:
            case Rule.ExtraKaramaCards:
            case Rule.CardsCanBeTraded:
            case Rule.PlayersChooseFactions:
            case Rule.RedSupportingNonAllyBids:
            case Rule.AssistedNotekeeping:
            case Rule.AssistedNotekeepingForGreen:
            case Rule.ResourceBonusForStrongholds:
            case Rule.BattleWithoutLeader:
            case Rule.CapturedLeadersAreTraitorsToOwnFaction:
            case Rule.DisableEndOfGameReport:
            case Rule.DisableOrangeSpecialVictory:
            case Rule.DisableResourceTransfers:
            case Rule.YellowAllyGetsDialedResourcesRefunded:
                return 0;

            case Rule.FillWithBots:
            case Rule.BotsCannotAlly:
                return 0;

            case Rule.TechTokens:
            case Rule.CheapHeroTraitor:
            case Rule.ExpansionTreacheryCards:
            case Rule.SandTrout:
                return 1;

            case Rule.GreySwappingCardOnBid:
            case Rule.PurpleGholas:
                return 1;

            case Rule.LeaderSkills:
            case Rule.Expansion2TreacheryCards:
                return 2;

            case Rule.BrownAuditor:
            case Rule.WhiteBlackMarket:
            case Rule.StrongholdBonus:
                return 2;

            case Rule.GreatMaker:
            case Rule.DiscoveryTokens:
            case Rule.Homeworlds:
            case Rule.NexusCards:
            case Rule.Expansion3TreacheryCards:
            case Rule.CyanAssassinate:
            case Rule.PinkLoyalty:
                return 3;
        }

        return int.MaxValue;
    }
}