/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        public Ruleset Ruleset
        {
            get
            {
                return DetermineRuleset(Rules);
            }
        }

        public IEnumerable<Rule> GetCustomRules()
        {
            return Rules.Where(rule =>
                GetRuleGroup(rule) == RuleGroup.House ||
                GetRuleGroup(rule) == RuleGroup.CoreBasicExceptions ||
                GetRuleGroup(rule) == RuleGroup.CoreAdvancedExceptions ||
                GetRuleGroup(rule) == RuleGroup.ExpansionIxAndBtBasicExceptions ||
                GetRuleGroup(rule) == RuleGroup.ExpansionIxAndBtAdvancedExceptions ||
                GetRuleGroup(rule) == RuleGroup.ExpansionBrownAndWhiteBasicExceptions ||
                GetRuleGroup(rule) == RuleGroup.ExpansionBrownAndWhiteAdvancedExceptions ||
                GetRuleGroup(rule) == RuleGroup.CoreBasicExceptions);
        }

        public static Ruleset DetermineRuleset(IEnumerable<Rule> applicableRules)
        {
            foreach (var rulesetWithRules in RulesetDefinition)
            {
                if (rulesetWithRules.Value.OrderBy(r => r).SequenceEqual(applicableRules.OrderBy(r => r)))
                {
                    return rulesetWithRules.Key;
                }
            }

            return Ruleset.Custom;
        }

        public Ruleset DetermineApproximateRuleset()
        {
            var hasExpansion1 = Players.Any(p => p.Faction == Faction.Purple || p.Faction == Faction.Grey) ||
                Applicable(Rule.GreyAndPurpleExpansionTreacheryCardsPBandSS) ||
                Applicable(Rule.GreyAndPurpleExpansionTreacheryCardsAmal) ||
                Applicable(Rule.GreyAndPurpleExpansionTreacheryCardsExceptPBandSSandAmal) ||
                Applicable(Rule.GreyAndPurpleExpansionTechTokens);

            var hasExpansion2 = Players.Any(p => p.Faction == Faction.Brown || p.Faction == Faction.White) ||
                Applicable(Rule.BrownAndWhiteLeaderSkills) ||
                Applicable(Rule.BrownAndWhiteStrongholdBonus);

            if (AdvancedRulesApply)
            {
                if (hasExpansion1 && hasExpansion2) return Ruleset.AllExpansionsAdvancedGame;
                else if (hasExpansion1 && hasExpansion2) return Ruleset.ExpansionAdvancedGame;
                else if (hasExpansion1 && hasExpansion2) return Ruleset.Expansion2AdvancedGame;
                else return Ruleset.AdvancedGame;
            }
            else
            {
                if (hasExpansion1 && hasExpansion2) return Ruleset.AllExpansionsBasicGame;
                else if (hasExpansion1 && hasExpansion2) return Ruleset.ExpansionBasicGame;
                else if (hasExpansion1 && hasExpansion2) return Ruleset.Expansion2BasicGame;
                else return Ruleset.BasicGame;
            }
        }

        private bool AdvancedRulesApply
        {
            get
            {
                return
                    Applicable(Rule.AdvancedCombat) ||
                    Applicable(Rule.AdvancedKarama) ||
                    Applicable(Rule.GreenMessiah) ||
                    Applicable(Rule.BlackCapturesOrKillsLeaders) ||
                    Applicable(Rule.YellowSpecialForces) ||
                    Applicable(Rule.RedSpecialForces) ||
                    Applicable(Rule.OrangeDetermineShipment) ||
                    Applicable(Rule.BlueAdvisors) ||
                    Applicable(Rule.GreyAndPurpleExpansionGreySwappingCardOnBid) ||
                    Applicable(Rule.GreyAndPurpleExpansionPurpleGholas) ||
                    Applicable(Rule.BrownAuditor) ||
                    Applicable(Rule.WhiteBlackMarket);
            }
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
                Rule.BlueFirstForceInAnyTerritory,
                Rule.BlueAutoCharity,
                Rule.BlueWorthlessAsKarma,
                Rule.BlueAdvisors,
                Rule.BlueAccompaniesToShipmentLocation,
                Rule.OrangeDetermineShipment,
                Rule.RedSpecialForces,
            },

            [Ruleset.AllExpansionsBasicGame] = new Rule[] {
                Rule.GreyAndPurpleExpansionTechTokens,
                Rule.GreyAndPurpleExpansionCheapHeroTraitor,
                Rule.GreyAndPurpleExpansionTreacheryCards,
                Rule.GreyAndPurpleExpansionSandTrout,
                Rule.BrownAndWhiteLeaderSkills
            },

            [Ruleset.ExpansionBasicGame] = new Rule[] {
                Rule.GreyAndPurpleExpansionTechTokens,
                Rule.GreyAndPurpleExpansionCheapHeroTraitor,
                Rule.GreyAndPurpleExpansionTreacheryCards,
                Rule.GreyAndPurpleExpansionSandTrout
            },

            [Ruleset.Expansion2BasicGame] = new Rule[] {
                Rule.BrownAndWhiteLeaderSkills
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
                Rule.BlueFirstForceInAnyTerritory,
                Rule.BlueAutoCharity,
                Rule.BlueWorthlessAsKarma,
                Rule.BlueAdvisors,
                Rule.BlueAccompaniesToShipmentLocation,
                Rule.OrangeDetermineShipment,
                Rule.RedSpecialForces,
                Rule.GreyAndPurpleExpansionTechTokens,
                Rule.GreyAndPurpleExpansionCheapHeroTraitor,
                Rule.GreyAndPurpleExpansionTreacheryCards,
                Rule.GreyAndPurpleExpansionSandTrout,
                Rule.GreyAndPurpleExpansionGreySwappingCardOnBid,
                Rule.GreyAndPurpleExpansionPurpleGholas,
                Rule.BrownAndWhiteLeaderSkills,
                Rule.BrownAndWhiteStrongholdBonus,
                Rule.BrownAuditor,
                Rule.WhiteBlackMarket
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
                Rule.BlueFirstForceInAnyTerritory,
                Rule.BlueAutoCharity,
                Rule.BlueWorthlessAsKarma,
                Rule.BlueAdvisors,
                Rule.BlueAccompaniesToShipmentLocation,
                Rule.OrangeDetermineShipment,
                Rule.RedSpecialForces,
                Rule.GreyAndPurpleExpansionTechTokens,
                Rule.GreyAndPurpleExpansionCheapHeroTraitor,
                Rule.GreyAndPurpleExpansionTreacheryCards,
                Rule.GreyAndPurpleExpansionSandTrout,
                Rule.GreyAndPurpleExpansionGreySwappingCardOnBid,
                Rule.GreyAndPurpleExpansionPurpleGholas
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
                Rule.BlueFirstForceInAnyTerritory,
                Rule.BlueAutoCharity,
                Rule.BlueWorthlessAsKarma,
                Rule.BlueAdvisors,
                Rule.BlueAccompaniesToShipmentLocation,
                Rule.OrangeDetermineShipment,
                Rule.RedSpecialForces,
                Rule.BrownAndWhiteLeaderSkills,
                Rule.BrownAndWhiteStrongholdBonus,
                Rule.BrownAuditor,
                Rule.WhiteBlackMarket,
                Rule.BrownAndWhiteStrongholdBonus
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
                Rule.BlueFirstForceInAnyTerritory,
                Rule.BlueAutoCharity,
                Rule.BlueWorthlessAsKarma,
                Rule.BlueAdvisors,
                Rule.BlueAccompaniesToShipmentLocation,
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

        public static IEnumerable<Rule> GetRulesInGroup(RuleGroup group)
        {
            return Enumerations.GetValues<Rule>(typeof(Rule)).Where(r => GetRuleGroup(r) == group);
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
                case Rule.BlueFirstForceInAnyTerritory:
                case Rule.BlueAutoCharity:
                case Rule.BlueWorthlessAsKarma:
                case Rule.BlueAdvisors:
                case Rule.BlueAccompaniesToShipmentLocation:
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
                case Rule.SSW:
                case Rule.BlackMulligan:
                case Rule.ExtraKaramaCards:
                case Rule.CardsCanBeTraded:
                case Rule.PlayersChooseFactions:
                case Rule.RedSupportingNonAllyBids:
                case Rule.AssistedNotekeeping:
                case Rule.ResourceBonusForStrongholds:
                case Rule.BattleWithoutLeader:
                case Rule.CapturedLeadersAreTraitorsToOwnFaction:
                case Rule.DisableEndOfGameReport:
                case Rule.DisableOrangeSpecialVictory:
                case Rule.DisableResourceTransfers:
                    return RuleGroup.House;

                case Rule.FillWithBots:
                case Rule.BotsCannotAlly:
                    return RuleGroup.Bots;

                case Rule.GreyAndPurpleExpansionTechTokens:
                case Rule.GreyAndPurpleExpansionCheapHeroTraitor:
                case Rule.GreyAndPurpleExpansionTreacheryCards:
                case Rule.GreyAndPurpleExpansionSandTrout:
                    return RuleGroup.ExpansionIxAndBtBasic;

                case Rule.GreyAndPurpleExpansionGreySwappingCardOnBid:
                case Rule.GreyAndPurpleExpansionPurpleGholas:
                    return RuleGroup.ExpansionIxAndBtAdvanced;

                case Rule.BrownAndWhiteLeaderSkills:
                    return RuleGroup.ExpansionBrownAndWhiteBasic;

                case Rule.BrownAuditor:
                case Rule.WhiteBlackMarket:
                case Rule.BrownAndWhiteStrongholdBonus:
                    return RuleGroup.ExpansionBrownAndWhiteAdvanced;
            }

            return RuleGroup.None;
        }
    }
}
