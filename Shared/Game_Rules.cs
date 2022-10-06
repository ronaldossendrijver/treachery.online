/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        public Ruleset Ruleset { get; private set; }

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
                else if (expansionLevel == 2 && hasExpansion1 && hasExpansion2) return Ruleset.AllExpansionsAdvancedGame;
                else if (expansionLevel == 1 && hasExpansion1) return Ruleset.AllExpansionsAdvancedGame;
                else if (hasExpansion3) return Ruleset.Expansion3AdvancedGame;
                else if (hasExpansion2) return Ruleset.Expansion2AdvancedGame;
                else if (hasExpansion1) return Ruleset.ExpansionAdvancedGame;
                else return Ruleset.AdvancedGame;
            }
            else
            {
                if (expansionLevel == 3 && hasExpansion1 && hasExpansion2 && hasExpansion3) return Ruleset.AllExpansionsBasicGame;
                else if (expansionLevel == 2 && hasExpansion1 && hasExpansion2) return Ruleset.AllExpansionsBasicGame;
                else if (expansionLevel == 1 && hasExpansion1) return Ruleset.AllExpansionsBasicGame;
                else if (hasExpansion3) return Ruleset.Expansion3BasicGame;
                else if (hasExpansion2) return Ruleset.Expansion2BasicGame;
                else if (hasExpansion1) return Ruleset.ExpansionBasicGame;
                else return Ruleset.BasicGame;
            }
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

        public static Dictionary<Ruleset, Rule[]> RulesetDefinition = new Dictionary<Ruleset, Rule[]>
        {

            [Ruleset.BasicGame] = new Rule[] {
                Rule.BasicTreacheryCards,
                Rule.HasCharityPhase
            },

            [Ruleset.AdvancedGame] = new Rule[] {
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
                Rule.GreySwappingCardOnBid,
                Rule.PurpleGholas,
                Rule.BrownAuditor,
                Rule.WhiteBlackMarket,
                Rule.CyanAssassinate,
                Rule.PinkLoyalty,
            },

            [Ruleset.AllExpansionsBasicGame] = new Rule[] {
                Rule.TechTokens,
                Rule.CheapHeroTraitor,
                Rule.ExpansionTreacheryCards,
                Rule.SandTrout,
                Rule.LeaderSkills,
                Rule.Expansion2TreacheryCards,
                Rule.Expansion3TreacheryCards,
                Rule.DiscoveryTokens,
                Rule.Homeworlds,
                Rule.NexusCards,
                Rule.GreatMaker
            },

            [Ruleset.ExpansionBasicGame] = new Rule[] {
                Rule.TechTokens,
                Rule.CheapHeroTraitor,
                Rule.ExpansionTreacheryCards,
                Rule.SandTrout
            },

            [Ruleset.Expansion2BasicGame] = new Rule[] {
                Rule.LeaderSkills,
                Rule.Expansion2TreacheryCards
            },

            [Ruleset.Expansion3BasicGame] = new Rule[] {
                Rule.Expansion3TreacheryCards,
                Rule.DiscoveryTokens,
                Rule.Homeworlds,
                Rule.NexusCards,
                Rule.GreatMaker
            },

            [Ruleset.AllExpansionsAdvancedGame] = new Rule[] {
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
                Rule.TechTokens,
                Rule.CheapHeroTraitor,
                Rule.ExpansionTreacheryCards,
                Rule.SandTrout,
                Rule.GreySwappingCardOnBid,
                Rule.PurpleGholas,
                Rule.LeaderSkills,
                Rule.Expansion2TreacheryCards,
                Rule.StrongholdBonus,
                Rule.BrownAuditor,
                Rule.WhiteBlackMarket,
                Rule.Expansion3TreacheryCards,
                Rule.DiscoveryTokens,
                Rule.Homeworlds,
                Rule.NexusCards,
                Rule.GreatMaker,
                Rule.CyanAssassinate,
                Rule.PinkLoyalty,
            },

            [Ruleset.ExpansionAdvancedGame] = new Rule[] {
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
                Rule.TechTokens,
                Rule.CheapHeroTraitor,
                Rule.ExpansionTreacheryCards,
                Rule.SandTrout,
                Rule.GreySwappingCardOnBid,
                Rule.PurpleGholas,
                Rule.BrownAuditor,
                Rule.WhiteBlackMarket,
                Rule.CyanAssassinate,
                Rule.PinkLoyalty,
            },

            [Ruleset.Expansion2AdvancedGame] = new Rule[] {
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
                Rule.LeaderSkills,
                Rule.Expansion2TreacheryCards,
                Rule.StrongholdBonus,
                Rule.GreySwappingCardOnBid,
                Rule.PurpleGholas,
                Rule.BrownAuditor,
                Rule.WhiteBlackMarket,
                Rule.StrongholdBonus,
                Rule.CyanAssassinate,
                Rule.PinkLoyalty,
            },

            [Ruleset.ServerClassic] = new Rule[] {
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
            },
        };

        public static IEnumerable<Rule> GetRulesInGroup(RuleGroup group, int expansionLevel)
        {
            return Enumerations.GetValues<Rule>(typeof(Rule)).Where(r => GetRuleGroup(r) == group && GetRuleExpansion(r) <= expansionLevel);
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
}
