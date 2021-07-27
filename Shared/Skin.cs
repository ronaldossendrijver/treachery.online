/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace Treachery.Shared
{
    public class Skin
    {
        #region Attributes

        public const int CurrentVersion = 2;

        public const string DEFAULT_ART_LOCATION = "https://treachery.online";
        //public const string DEFAULT_ART_LOCATION = ".";

        public string Description = null;
        public int Version;

        public bool DrawResourceIconsOnMap = false;
        public bool ShowVerboseToolipsOnMap = true;
        public bool ShowArrowsForRecentMoves = true;

        public string ReportBackground_ShipmentAndMove_URL = null;
        public string ReportBackground_StormAndResourceBlow_URL = null;
        public string ReportBackground_Resurrection_URL = null;
        public string ReportBackground_EndOfTurn_URL = null;
        public string PanelBackground_Bidding_URL = null;
        public string PanelBackground_EndOfGame_URL = null;
        public string PanelBackground_Battle_URL = null;

        public Dictionary<TreacheryCardType, string> TreacheryCardType_STR;
        public Dictionary<int, string> TreacheryCardName_STR;
        public Dictionary<int, string> TreacheryCardDescription_STR;
        public Dictionary<int, string> TreacheryCardImage_URL;
        public Dictionary<TechToken, string> TechTokenDescription_STR;
        public Dictionary<int, string> ResourceCardImage_URL;
        public Dictionary<LeaderSkill, string> LeaderSkillCardName_STR;
        public Dictionary<LeaderSkill, string> LeaderSkillCardImage_URL;
        public Dictionary<int, string> StrongholdCardName_STR;
        public Dictionary<int, string> StrongholdCardImage_URL;

        public string Map_URL = null;
        public string Eye_URL = null;
        public string EyeSlash_URL = null;
        public string Planet_URL = null;
        public string CardBack_ResourceCard_URL = null;
        public string CardBack_TreacheryCard_URL = null;
        public string BattleScreen_URL = null;
        public string Messiah_URL = null;
        public string Monster_URL = null;
        public string Harvester_URL = null;
        public string Resource_URL = null;
        public string HMS_URL = null;

        public Dictionary<Concept, string> Concept_STR;
        public Dictionary<MainPhase, string> MainPhase_STR;
        public Dictionary<int, string> PersonName_STR;
        public Dictionary<int, string> PersonImage_URL;
        public Dictionary<int, string> TerritoryName_STR;
        public Dictionary<Faction, string> FactionName_STR;
        public Dictionary<Faction, string> FactionImage_URL;
        public Dictionary<Faction, string> FactionTableImage_URL;
        public Dictionary<Faction, string> FactionFacedownImage_URL;
        public Dictionary<Faction, string> FactionForceImage_URL;
        public Dictionary<Faction, string> FactionSpecialForceImage_URL;
        public Dictionary<Faction, string> FactionColorTransparant;
        public Dictionary<Faction, string> FactionColor;
        public Dictionary<Faction, string> ForceName_STR;
        public Dictionary<Faction, string> SpecialForceName_STR;
        public Dictionary<TechToken, string> TechTokenName_STR;
        public Dictionary<TechToken, string> TechTokenImage_URL;
        public Dictionary<Milestone, string> Sound;

        public string MusicGeneral_URL = null;
        public string MusicResourceBlow_URL = null;
        public string MusicSetup_URL = null;
        public string MusicBidding_URL = null;
        public string MusicShipmentAndMove_URL = null;
        public string MusicBattle_URL = null;
        public string MusicBattleClimax_URL = null;
        public string MusicMentat_URL = null;

        public string Sound_YourTurn_URL = null;
        public string Sound_Chatmessage_URL = null;

        public Point MapDimensions = new Point(4145, 4601);
        public Point PlanetCenter = new Point(2070, 2287);
        public int PlanetRadius = 1790;
        public int MapRadius = 1940;
        public int PlayerTokenRadius = 95;

        public Point SpiceDeckLocation = new Point(3750, 3490);
        public Point TreacheryDeckLocation = new Point(3300, 4050);

        public int BattleScreenWidth = -1;
        public int BattleScreenHeight = -1;
        public int BattleScreenHeroX = -1;
        public int BattleScreenHeroY = -1;
        public int BattleWheelHeroWidth = -1;
        public int BattleWheelHeroHeight = -1;
        public int BattleWheelForcesX = -1;
        public int BattleWheelForcesY = -1;
        public int BattleWheelCardX = -1;
        public int BattleWheelCardY = -1;
        public int BattleWheelCardWidth = -1;
        public int BattleWheelCardHeight = -1;

        //Monster token
        public int MONSTERTOKEN_RADIUS = 100;

        //Force tokens
        public string FORCETOKEN_FONT = "normal normal bolder 85px Calibri, Tahoma, sans-serif";
        public string FORCETOKEN_FONTCOLOR = "white";
        public string FORCETOKEN_SPECIAL_FONTCOLOR = "gold";
        public string FORCETOKEN_FONT_BORDERCOLOR = "black";
        public int FORCETOKEN_FONT_BORDERWIDTH = 3;
        public string FORCETOKEN_SPECIAL_BORDERCOLOR = "gold";
        public string FORCETOKEN_BORDERCOLOR = "white";
        public int FORCETOKEN_BORDERWIDTH = 5;
        public int FORCETOKEN_SPECIAL_BORDERWIDTH = 10;
        public int FORCETOKEN_RADIUS = 60;

        //Spice tokens
        public string RESOURCETOKEN_FONT = "normal normal bolder 85px Calibri, Tahoma, sans-serif";
        public string RESOURCETOKEN_FONTCOLOR = "white";
        public string RESOURCETOKEN_FONT_BORDERCOLOR = "black";
        public int RESOURCETOKEN_FONT_BORDERWIDTH = 3;
        public string RESOURCETOKEN_COLOR = "rgba(255,140,60,0.9)";
        public string RESOURCETOKEN_BORDERCOLOR = "white";
        public int RESOURCETOKEN_RADIUS = 80;

        //Other highlights
        public string HIGHLIGHT_OVERLAY_COLOR = "rgba(255,255,255,0.5)";
        public string METHEOR_OVERLAY_COLOR = "rgba(209,247,137,0.5)";
        public string BLOWNSHIELDWALL_OVERLAY_COLOR = "rgba(137,238,247,0.5)";
        public string STORM_OVERLAY_COLOR = "rgba(255,100,100,0.5)";
        public string STORM_PRESCIENCE_OVERLAY_COLOR = "rgba(255,100,100,0.2)";

        //Card piles
        public string CARDPILE_FONT = "normal normal normal 140px Advokat, Calibri, Tahoma, sans-serif";
        public string CARDPILE_FONTCOLOR = "white";
        public string CARDPILE_FONT_BORDERCOLOR = "black";
        public int CARDPILE_FONT_BORDERWIDTH = 3;

        //Phases
        public string PHASE_FONT = "normal normal bold 90px Advokat, Calibri, Tahoma, sans-serif";
        public string PHASE_ACTIVE_FONT = "normal normal normal 130px Advokat, Calibri, Tahoma, sans-serif";
        public string PHASE_FONTCOLOR = "white";
        public string PHASE_ACTIVE_FONTCOLOR = "rgb(231,191,60)";
        public string PHASE_FONT_BORDERCOLOR = "black";
        public int PHASE_FONT_BORDERWIDTH = 3;
        public int PHASE_ACTIVE_FONT_BORDERWIDTH = 3;

        //Player names
        public string PLAYERNAME_FONT = "normal normal bold 95px Advokat, Calibri, Tahoma, sans-serif";
        public string PLAYERNAME_FONTCOLOR = "white";
        public string PLAYERNAME_FONT_BORDERCOLOR = "black";
        public int PLAYERNAME_FONT_BORDERWIDTH = 3;

        //Player positions
        public string TABLEPOSITION_BACKGROUNDCOLOR = "rgb(231,191,60)";

        //Turns
        public string TURN_FONT = "normal normal normal 130px Advokat, Calibri, Tahoma, sans-serif";
        public string TURN_FONT_COLOR = "white";
        public string TURN_FONT_BORDERCOLOR = "black";
        public int TURN_FONT_BORDERWIDTH = 3;

        //Wheel
        public string WHEEL_FONT;
        public string WHEEL_FONTCOLOR;
        public string WHEEL_FONT_AGGRESSOR_BORDERCOLOR;
        public string WHEEL_FONT_DEFENDER_BORDERCOLOR;
        public int WHEEL_FONT_BORDERWIDTH = 6;

        //Shadows
        public string SHADOW_DARK = "black";
        public string SHADOW_LIGHT = "#505050";

        //General
        public string GAMEVERSION_FONT = "normal normal normal 16px Advokat, Calibri, Tahoma, sans-serif;";
        public string PLAYEDCARD_MESSAGE_FONT = "normal normal normal 16px Calibri, Tahoma, sans-serif";
        public string FACTION_INFORMATIONCARDSTYLE = "font: normal normal normal 14px Calibri, Tahoma, sans-serif; color: white; padding: 5px 5px 5px 5px; overflow: auto; line-height: 95%; background-color: rgba(32,32,32,0.95); border-color: grey; border-style: solid; border-width: 1px; border-radius: 3px;";
        public string TRACKER_FONT = "normal normal normal 12px Calibri, Tahoma, sans-serif;";
        public string JSPANEL_DEFAULTSTYLE = "font-family: Calibri, Tahoma, sans-serif";

        #endregion Attributes

        #region Descriptions

        private string[] Describe(object[] objects)
        {
            var result = new string[objects.Length];
            for (int i = 0; i < objects.Length; i++)
            {
                result[i] = Describe(objects[i]);
            }
            return result;
        }

        public string Format(string m, params object[] list)
        {
            try
            {
                return string.Format(m, Describe(list));
            }
            catch (Exception)
            {
                return m;
            }
        }

        public string FormatCapitalized(string m, params object[] list)
        {
            try
            {
                return FirstCharToUpper(string.Format(m, Describe(list)));
            }
            catch (Exception)
            {
                return m;
            }
        }

        public string Join<T>(IEnumerable<T> items)
        {
            if (items == null) return "";

            var result = string.Join(", ", items.Select(i => Describe(i)));

            if (result.Length == 0)
            {
                return "-";
            }
            else
            {
                return result;
            }
        }

        public string Describe(object value, bool capitalize = false)
        {
            string result;

            if (value == null) result = "";
            else if (value is string str) result = str;
            else if (value is bool b) result = b ? "Yes" : "No";
            else if (value is MessagePart part) result = Describe(part);
            else if (value is Concept c) result = Describe(c);
            else if (value is Faction faction) result = Describe(faction);
            else if (value is FactionForce ff) result = Describe(ff);
            else if (value is FactionSpecialForce fsf) result = Describe(fsf);
            else if (value is FactionAdvantage factionadvantage) result = Describe(factionadvantage);
            else if (value is TreacheryCardType tct) result = Describe(tct);
            else if (value is Ruleset r) result = Describe(r);
            else if (value is RuleGroup rg) result = Describe(rg);
            else if (value is Rule rule) result = Describe(rule);
            else if (value is MainPhase m) result = Describe(m);
            else if (value is TechToken tt) result = Describe(tt);
            else if (value is ClairvoyanceQuestion q) result = Describe(q);
            else if (value is DealType d) result = Describe(d);
            else if (value is WinMethod w) result = Describe(w);
            else if (value is Phase p) result = Describe(p);
            else if (value is BrownEconomicsStatus bes) result = Describe(bes);
            else if (value is AuctionType at) result = Describe(at);
            else if (value is JuiceType jt) result = Describe(jt);
            else if (value is IEnumerable ienum) result = Join(Enumerable.Cast<object>(ienum));
            else result = value.ToString();

            if (capitalize)
            {
                return FirstCharToUpper(result);
            }
            else
            {
                return result;
            }
        }

        public static string FirstCharToUpper(string input)
        {
            if (input == null || input == "")
            {
                return input;
            }
            else
            {
                return input[0].ToString().ToUpper() + input[1..];
            }
        }

        public string Describe(Faction f)
        {
            return FactionName_STR[f];
        }

        public string Describe(Concept c)
        {
            return Concept_STR[c];
        }

        public string Describe(TreacheryCardType t)
        {
            return TreacheryCardType_STR[t];
        }

        public string Describe(MainPhase p)
        {
            return MainPhase_STR[p];
        }

        public string Describe(TechToken tt)
        {
            return TechTokenName_STR[tt];
        }

        public string Describe(FactionForce f)
        {
            return f switch
            {
                FactionForce.Green => ForceName_STR[Faction.Green],
                FactionForce.Black => ForceName_STR[Faction.Black],
                FactionForce.Yellow => ForceName_STR[Faction.Yellow],
                FactionForce.Red => ForceName_STR[Faction.Red],
                FactionForce.Orange => ForceName_STR[Faction.Orange],
                FactionForce.Blue => ForceName_STR[Faction.Blue],
                FactionForce.Grey => ForceName_STR[Faction.Grey],
                FactionForce.Purple => ForceName_STR[Faction.Purple],
                FactionForce.Brown => ForceName_STR[Faction.Brown],
                FactionForce.White => ForceName_STR[Faction.White],
                FactionForce.Pink => ForceName_STR[Faction.Pink],
                FactionForce.Cyan => ForceName_STR[Faction.Cyan],
                _ => "-",
            };
        }

        public string Describe(FactionSpecialForce f)
        {
            return f switch
            {
                FactionSpecialForce.Red => SpecialForceName_STR[Faction.Red],
                FactionSpecialForce.Yellow => SpecialForceName_STR[Faction.Yellow],
                FactionSpecialForce.Blue => SpecialForceName_STR[Faction.Blue],
                FactionSpecialForce.Grey => SpecialForceName_STR[Faction.Grey],
                FactionSpecialForce.White => SpecialForceName_STR[Faction.White],
                _ => "-",
            };
        }

        public string Describe(MessagePart part)
        {
            return part.ToString();
        }

        public string Describe(FactionAdvantage advantage)
        {
            return advantage switch
            {
                FactionAdvantage.None => "- Any stoppable advantage not listed below -",
                FactionAdvantage.GreenBiddingPrescience => Format("{0} seeing the next treachery card", Faction.Green),
                FactionAdvantage.GreenSpiceBlowPrescience => Format("{0} seeing the top {1} card", Faction.Green, Concept_STR[Concept.Resource]),
                FactionAdvantage.GreenBattlePlanPrescience => Format("{0} seeing part of a battle plan", Faction.Green),
                FactionAdvantage.GreenUseMessiah => Format("{0} using the {1}", Faction.Green, Concept_STR[Concept.Messiah]),
                FactionAdvantage.BlackFreeCard => Format("{0} taking a second free treachery card", Faction.Black),
                FactionAdvantage.BlackCaptureLeader => Format("{0} capturing a leader once", Faction.Black),
                FactionAdvantage.BlackCallTraitorForAlly => Format("{0} calling TREACHERY for their ally", Faction.Black),
                FactionAdvantage.BlueAccompanies => Format("{0} accompanying shipment*", Faction.Blue),
                FactionAdvantage.BlueAnnouncesBattle => Format("{0} flipping advisors to fighters*", Faction.Blue),
                FactionAdvantage.BlueNoFlipOnIntrusion => Format("{0} becoming advisors on intrusion*", Faction.Blue),
                FactionAdvantage.BlueUsingVoice => Format("{0} using Voice", Faction.Blue),
                FactionAdvantage.BlueWorthlessAsKarma => Format("{0} using a {1} as a {2} card*", Faction.Blue, TreacheryCardType.Useless, TreacheryCardType.Karma),
                FactionAdvantage.BlueCharity => Format("{0} receiving 2 {1} at {2}", Faction.Blue, Concept.Resource, MainPhase.Charity),
                FactionAdvantage.YellowControlsMonster => Format("{0} sending {1}", Faction.Yellow, Concept.Monster),
                FactionAdvantage.YellowNotPayingForBattles => Format("{0} not paying for battle", Faction.Yellow),
                FactionAdvantage.YellowSpecialForceBonus => Format("{0} counting {1} bonus in one battle", Faction.Yellow, FactionSpecialForce.Yellow),
                FactionAdvantage.YellowExtraMove => Format("{0} moving two territories as part of their move action", Faction.Yellow),
                FactionAdvantage.YellowProtectedFromStorm => Format("{0} not taking storm losses", Faction.Yellow),
                FactionAdvantage.YellowProtectedFromMonster => Format("{0} not being devoured by {1}", Faction.Yellow, Concept.Monster),
                FactionAdvantage.YellowProtectedFromMonsterAlly => Format("{0} ally not being devoured by {1}", Faction.Yellow, Concept.Monster),
                FactionAdvantage.YellowStormPrescience => Format("{0} seeing the next storm movement", Faction.Yellow),
                FactionAdvantage.RedSpecialForceBonus => Format("{0} counting {1} bonus in one battle", Faction.Red, FactionSpecialForce.Red),
                FactionAdvantage.RedReceiveBid => Format("{0} receiving {1} for a treachery card", Faction.Red, Concept.Resource),
                FactionAdvantage.RedGiveSpiceToAlly => Format("{0} giving {1} to their ally*", Faction.Red, Concept.Resource),
                FactionAdvantage.RedLetAllyReviveExtraForces => Format("{0} allowing their ally to revive 3 extra forces*", Faction.Red, Concept.Resource),
                FactionAdvantage.OrangeDetermineMoveMoment => Format("{0} shipping out of turn order*", Faction.Orange),
                FactionAdvantage.OrangeSpecialShipments => Format("{0} shipping site-to-site or back to reserves", Faction.Orange),
                FactionAdvantage.OrangeShipmentsDiscount => Format("{0} shipping at half price", Faction.Orange),
                FactionAdvantage.OrangeShipmentsDiscountAlly => Format("{0} ally shipping at half price", Faction.Orange),
                FactionAdvantage.OrangeReceiveShipment => Format("{0} receiving {1} for a shipment", Faction.Orange, Concept.Resource),
                FactionAdvantage.PurpleRevivalDiscount => Format("{0} reviving at half price", Faction.Purple),
                FactionAdvantage.PurpleRevivalDiscountAlly => Format("{0} ally reviving at half price", Faction.Purple),
                FactionAdvantage.PurpleReplacingFaceDancer => Format("{0} replacing a face dancer at end of turn*", Faction.Purple),
                FactionAdvantage.PurpleIncreasingRevivalLimits => Format("{0} increasing revival limits*", Faction.Purple),
                FactionAdvantage.PurpleReceiveRevive => Format("{0} receiving {1} for a revival", Faction.Purple, Concept.Resource),
                FactionAdvantage.PurpleEarlyLeaderRevive => Format("{0} allowing early revival of a leader*", Faction.Purple),
                FactionAdvantage.PurpleReviveGhola => Format("{0} reviving a leader as a Ghola*", Faction.Purple),
                FactionAdvantage.GreyMovingHMS => Format("{0} moving the Hidden Mobile Stronghold", Faction.Grey),
                FactionAdvantage.GreySpecialForceBonus => Format("{0} counting {1} bonus in one battle", Faction.Grey, FactionSpecialForce.Grey),
                FactionAdvantage.GreySelectingCardsOnAuction => Format("{0} selecting the cards on auction", Faction.Grey),
                FactionAdvantage.GreyCyborgExtraMove => Format("{0} moving {1} two territories", Faction.Grey, FactionSpecialForce.Grey),
                FactionAdvantage.GreyReplacingSpecialForces => Format("{0} replacing {1} lost in battle with {2}*", Faction.Grey, FactionSpecialForce.Grey, FactionForce.Grey),
                FactionAdvantage.GreyAllyDiscardingCard => Format("{0} allowing their ally to replace a won card*", Faction.Grey),
                FactionAdvantage.GreySwappingCard => Format("{0} replacing a treachery card during bidding*", Faction.Grey),

                FactionAdvantage.BrownControllingCharity => Format("{0} receiving and giving {1} during {2}", Faction.Brown, Concept.Resource, MainPhase.Charity),
                FactionAdvantage.BrownDiscarding => Format("{0} discarding a cards for {1} or discarding a {2} card for its special effect*", Faction.Brown, Concept.Resource, TreacheryCardType.Useless),
                FactionAdvantage.BrownRevival => Format("{0} having unlimited force revival and reduced revival cost", Faction.Brown),
                FactionAdvantage.BrownEconomics => Format("{0} playing their Economics token during {1}", Faction.Brown, MainPhase.Contemplate),
                FactionAdvantage.BrownReceiveForcePayment => Format("{0} collecting {1} payment for forces for one battle", Faction.Brown, Concept.Resource),
                FactionAdvantage.BrownAudit => Format(""),

                FactionAdvantage.WhiteAuction => Format("{0} auctioning a card from their card cache", Faction.White),
                FactionAdvantage.WhiteNofield => Format("{0} using a No-Field to ship", Faction.White),
                FactionAdvantage.WhiteBlackMarket => Format("{0} selling a card from their hand", Faction.White),

                _ => "Unknown",
            };
        }


        public string Describe(Ruleset s)
        {
            return s switch
            {
                Ruleset.BasicGame => "Standard Dune - Basic",
                Ruleset.AdvancedGame => "Standard Dune - Advanced",
                Ruleset.AdvancedGameWithoutPayingForBattles => "Standard Dune - Advanced without Advanced Combat",

                Ruleset.ExpansionBasicGame => "Expansion - Basic",
                Ruleset.ExpansionAdvancedGame => "Expansion - Advanced",
                Ruleset.ExpansionAdvancedGameWithoutPayingForBattles => "Expansion - Advanced without Advanced Combat",

                Ruleset.Expansion2BasicGame => "Expansion 2 - Basic",
                Ruleset.Expansion2AdvancedGame => "Expansion 2 - Advanced",
                Ruleset.Expansion2AdvancedGameWithoutPayingForBattles => "Expansion 2 - Advanced without Advanced Combat",

                Ruleset.AllExpansionsBasicGame => "Both Expansions - Basic",
                Ruleset.AllExpansionsAdvancedGame => "Both Expansions - Advanced",
                Ruleset.AllExpansionsAdvancedGameWithoutPayingForBattles => "Both Expansions - Advanced without Advanced Combat",

                Ruleset.ServerClassic => "Server Classic",
                Ruleset.Custom => "Custom",

                _ => "unknown rule set",
            };
        }

        public string Describe(RuleGroup s)
        {
            return s switch
            {
                RuleGroup.CoreAdvanced => "Core Game, Advanced Rules",
                RuleGroup.CoreBasicExceptions => "Core Game, Exceptions to Basic Rules",
                RuleGroup.CoreAdvancedExceptions => "Core Game, Exceptions to Advanced Rules",
                
                RuleGroup.ExpansionIxAndBtBasic => "Ixians & Tleilaxu Expansion",
                RuleGroup.ExpansionIxAndBtAdvanced => "Tleilaxu Expansion, Advanced Rules",
                
                RuleGroup.ExpansionBrownAndWhiteBasic => "CHOAM & Richese Expansion",
                RuleGroup.ExpansionBrownAndWhiteAdvanced => "CHOAM Richese, Advanced Rules",
                
                RuleGroup.House => "House Rules",

                _ => "unknown rule group",
            };
        }

        public string Describe(WinMethod m)
        {
            return m switch
            {
                WinMethod.Strongholds => "by number of occupied strongholds",
                WinMethod.Prediction => "by prediction",
                WinMethod.Timeout => "by running out of time",
                WinMethod.Forfeit => "by forfeit",
                WinMethod.YellowSpecial => Format("by {0} special victory", Faction.Yellow),
                WinMethod.OrangeSpecial => Format("by {0} special victory", Faction.Orange),
                _ => "None",
            };
        }

        public string Describe(Phase p)
        {
            return p switch
            {
                Phase.TurnConcluded => "End of turn",
                Phase.Bidding => "Next bidding round",
                Phase.BiddingReport => "End of bidding phase",
                Phase.ShipmentAndMoveConcluded => "End of movement phase",
                Phase.BattleReport => "End of battle phase",
                Phase.GameEnded=> "End of game",
                _ => "unknown"
            };
        }

        public string Describe(BrownEconomicsStatus p)
        {
            return p switch
            {
                BrownEconomicsStatus.Cancel => "Cancel",
                BrownEconomicsStatus.CancelFlipped => "Cancel (flipped)",
                BrownEconomicsStatus.Double => "Double",
                BrownEconomicsStatus.DoubleFlipped => "Double (flipped)",
                BrownEconomicsStatus.RemovedFromGame => "Removed from game",
                _ => "None"
            };
        }

        public string Describe(AuctionType t)
        {
            return t switch
            {
                AuctionType.Normal => "Normal",
                AuctionType.BlackMarketNormal => "Normal",
                AuctionType.BlackMarketSilent => "Silent",
                AuctionType.BlackMarketOnceAround => "Once Around",
                AuctionType.WhiteNormal => Format("Normal {0}", Faction.White),
                AuctionType.WhiteSilent => Format("Silent {0}", Faction.White),
                AuctionType.WhiteOnceAround => Format("Once Around {0}", Faction.White),
                _ => "None"
            };
        }

        public string Describe(JuiceType jt)
        {
            return jt switch
            {
                JuiceType.GoFirst => "be considered first in storm order",
                JuiceType.GoLast => "be considered last in storm order",
                JuiceType.Aggressor => "be considered aggressor in this battle",
                _ => "None"
            };
        }

        public string Describe(Rule r)
        {
            return r switch
            {
                Rule.AdvancedCombat => "Advanced Combat",
                Rule.IncreasedResourceFlow => "Increased Resource Flow",
                Rule.AdvancedKarama => "Advanced Karama Cards",
                Rule.YellowSeesStorm => Format("{0} can look at the storm dial", Faction.Yellow),
                Rule.YellowStormLosses => Format("{0} storm losses are halved", Faction.Yellow),
                Rule.YellowSendingMonster => Format("{0} place additional {1}s", Faction.Yellow, Concept.Monster),
                Rule.YellowSpecialForces => Format("{0} can use {1}", Faction.Yellow, FactionSpecialForce.Yellow),
                Rule.GreenMessiah => Format("Enable {0}", Concept.Messiah),
                Rule.BlackCapturesOrKillsLeaders => Format("{0} can kill or capture leaders", Faction.Black),
                Rule.BlueFirstForceInAnyTerritory => Format("{0} can put their initial force anywhere", Faction.Blue),
                Rule.BlueAutoCharity => Format("{0} get 2 charity each turn", Faction.Blue),
                Rule.BlueWorthlessAsKarma => Format("{0} can use {1} cards for {2}", Faction.Blue, TreacheryCardType.Useless, TreacheryCardType.Karma),
                Rule.BlueAdvisors => Format("{0} can use {1}", Faction.Blue, FactionSpecialForce.Blue),
                Rule.BlueAccompaniesToShipmentLocation => Format("{0} can accompany to the shipped to territory", Faction.Blue),
                Rule.OrangeDetermineShipment => Format("{0} can determine when they ship and move", Faction.Orange),
                Rule.RedSpecialForces => Format("{0} can use {1}", Faction.Red, FactionSpecialForce.Red),
                Rule.BribesAreImmediate => Format("{0} bribes are immediately added to reserves", Concept.Resource),
                Rule.ContestedStongholdsCountAsOccupied => "Contested strongholds count as occupied @ end of turn",
                Rule.AdvisorsDontConflictWithAlly => Format("{0} {1} can coexist with allied forces", Faction.Blue, FactionSpecialForce.Blue),
                Rule.OrangeShipmentContributionsFlowBack => Format("{0} contributions to shipments flow back to them", Faction.Orange),
                Rule.CustomInitialForcesAndResources => Format("Custom initial {0} and force positions (for Spice Harvest)", Concept.Resource),
                Rule.HMSwithoutGrey => Format("Use the {0} if {1} are not in play", "Hidden Mobile Stronghold", Faction.Grey),
                Rule.SSW => Format("SSW: {0} counts for victory after fourth {1}", "Shield Wall", Concept.Monster),
                Rule.BlackMulligan => Format("{0} mulligan traitors when they drew > 1 of their own", Faction.Black),
                
                Rule.GreyAndPurpleExpansionTechTokens => Format("Tech Tokens"),
                Rule.GreyAndPurpleExpansionTreacheryCardsExceptPBandSSandAmal => Format("Treachery Cards: all except {0}, {1} and {2}", TreacheryCardType.ProjectileAndPoison, TreacheryCardType.ShieldAndAntidote, TreacheryCardType.Amal),
                Rule.GreyAndPurpleExpansionTreacheryCardsPBandSS => Format("Treachery Cards: {0} and {1}", TreacheryCardType.ProjectileAndPoison, TreacheryCardType.ShieldAndAntidote),
                Rule.GreyAndPurpleExpansionTreacheryCardsAmal => Format("Treachery Card: {0}", TreacheryCardType.Amal),
                Rule.GreyAndPurpleExpansionCheapHeroTraitor => Format("Cheap Hero Traitor"),
                Rule.GreyAndPurpleExpansionSandTrout => Describe(Concept.BabyMonster),
                Rule.GreyAndPurpleExpansionPurpleGholas => Format("{0} may revive leaders as Gholas", Faction.Purple),
                Rule.GreyAndPurpleExpansionGreySwappingCardOnBid => Format("{0} may swap one card on bid with on card from their hand", Faction.Grey),

                Rule.BrownAndWhiteLeaderSkills => "Leader Skills",
                Rule.BrownAndWhiteStrongholdBonus => "Stronghold Bonus",
                Rule.BrownAuditor => Format("{0} gains the Auditor leader", Faction.Brown),
                Rule.WhiteBlackMarket => Format("{0} Black Market bidding", Faction.White),

                Rule.ExtraKaramaCards => Format("Add three extra {0} cards to the game", TreacheryCardType.Karma),
                Rule.FullPhaseKarma => Format("Full phase {0} (instead of single instance)", TreacheryCardType.Karma),
                Rule.YellowMayMoveIntoStorm => Format("{0} may move into storm", Faction.Yellow),
                Rule.BlueVoiceMustNameSpecialCards => "Voice must target special cards by name",
                Rule.BattlesUnderStorm => "Battles may happen under the storm",
                Rule.MovementBonusRequiresOccupationBeforeMovement => "Arrakeen/Carthag must be occupied before Ship&Move to grant ornithopters",
                Rule.AssistedNotekeeping => "Mentat: auto notekeeping of knowable info (spice owned, cards seen, ...)",
                Rule.FillWithBots => "Fill empty seats with random Bots",
                Rule.OrangeBot => Format("{0}Bot", Faction.Orange),
                Rule.RedBot => Format("{0}Bot", Faction.Red),
                Rule.BlackBot => Format("{0}Bot", Faction.Black),
                Rule.PurpleBot => Format("{0}Bot", Faction.Purple),
                Rule.GreenBot => Format("{0}Bot", Faction.Green),
                Rule.BlueBot => Format("{0}Bot", Faction.Blue),
                Rule.YellowBot => Format("{0}Bot", Faction.Yellow),
                Rule.GreyBot => Format("{0}Bot", Faction.Grey),
                Rule.BotsCannotAlly => "Bots may not initiate alliances",
                Rule.CardsCanBeTraded => Format("Allow players to give cards to each other"),
                Rule.PlayersChooseFactions => Format("Let players choose their factions at start"),
                Rule.RedSupportingNonAllyBids => Format("{0} may support bids of non-ally players", Faction.Red),
                _ => "unknown rule",
            };
        }

        public string Describe(ClairvoyanceQuestion q)
        {
            return q switch
            {
                ClairvoyanceQuestion.None => "Any question",
                ClairvoyanceQuestion.CardTypeInBattle => "In this battle, is one of your cards a {0}?",
                ClairvoyanceQuestion.CardTypeAsDefenseInBattle => "In this battle, will you use {0} as defense?",
                ClairvoyanceQuestion.CardTypeAsWeaponInBattle => "In this battle, will you use {0} as weapon?",
                ClairvoyanceQuestion.HasCardTypeInHand => "Do you own a {0}?",
                ClairvoyanceQuestion.LeaderInBattle => "In this battle, is your leader {0}?",
                ClairvoyanceQuestion.DialOfMoreThanXInBattle => "In this battle, is your dial higher than {0}?",
                ClairvoyanceQuestion.LeaderAsFacedancer => "Is {0} one of your face dancers?",
                ClairvoyanceQuestion.LeaderAsTraitor => "Is {0} one of your traitors?",
                ClairvoyanceQuestion.Prediction => "Did you predict a {0} win in turn {1}?",
                ClairvoyanceQuestion.WillAttackX => "Will you ship or move to {0} this turn?",
                _ => "unknown rule",
            };
        }

        public string Describe(DealType d)
        {
            return d switch
            {
                DealType.None => "Custom deal",
                DealType.DontShipOrMoveTo => "Don't ship or move to {0}",
                DealType.ShareBiddingPrescience => "Share treachery card prescience",
                DealType.ShareResourceDeckPrescience => Format("Share prescience of the top {0} card", Concept.Resource),
                DealType.ShareStormPrescience => "Share storm prescience",
                DealType.ForfeitBattle => "Forfeit this battle (no weapons and defenses, lowest leader, zero dial)",
                _ => "unknown deal type",
            };
        }

        #endregion Descriptions

        #region NamesAndImages

        public string GetReportBackground(Report report)
        {
            return report.About switch
            {
                MainPhase.ShipmentAndMove => ReportBackground_ShipmentAndMove_URL,
                MainPhase.Storm => ReportBackground_StormAndResourceBlow_URL,
                MainPhase.Resurrection => ReportBackground_Resurrection_URL,
                MainPhase.Contemplate => ReportBackground_EndOfTurn_URL,
                MainPhase.Bidding => PanelBackground_Bidding_URL,
                MainPhase.Ended => PanelBackground_EndOfGame_URL,
                MainPhase.Battle => PanelBackground_Battle_URL,
                _ => "?",
            };
        }

        public string GetTerritoryName(Territory t)
        {
            return GetLabel(TerritoryName_STR, t.SkinId);
        }

        public string GetImageURL(TreacheryCard c)
        {
            return GetURL(TreacheryCardImage_URL, c.SkinId);
        }

        public string GetImageURL(ResourceCard c)
        {
            return GetURL(ResourceCardImage_URL, c.SkinId);
        }

        public string GetTreacheryCardName(TreacheryCard c)
        {
            return GetLabel(TreacheryCardName_STR, c.SkinId);
        }

        public string GetTreacheryCardDescription(TreacheryCard c)
        {
            return GetLabel(TreacheryCardDescription_STR, c.SkinId);
        }

        public object GetTechTokenDescription(TechToken t)
        {
            return GetLabel(TechTokenDescription_STR, t);
        }

        public string GetImageURL(IHero h)
        {
            if (h is TreacheryCard)
            {
                return GetURL(TreacheryCardImage_URL, h.SkinId);
            }
            else if (h is Messiah)
            {
                return Messiah_URL;
            }
            else
            {
                return GetURL(PersonImage_URL, h.SkinId);
            }
        }
        public string GetImageURL(Faction faction)
        {
            return GetURL(FactionImage_URL, faction);
        }

        public string GetFactionTableImageURL(Faction faction)
        {
            return GetURL(FactionTableImage_URL, faction);
        }

        public string GetFactionFacedownImageURL(Faction faction)
        {
            return GetURL(FactionFacedownImage_URL, faction);
        }

        public string GetFactionForceImageURL(Faction faction)
        {
            return GetURL(FactionForceImage_URL, faction);
        }

        public string GetFactionSpecialForceImageURL(Faction faction)
        {
            return GetURL(FactionSpecialForceImage_URL, faction);
        }

        public string GetImageURL(TechToken tech)
        {
            return GetURL(TechTokenImage_URL, tech);
        }


        public string GetPersonName(Leader l)
        {
            return GetLabel(PersonName_STR, l.SkinId);
        }

        private string GetLabel<T>(Dictionary<T, string> labels, T key)
        {
            if (labels == null || !labels.ContainsKey(key))
            {
                return "?";
            }
            else
            {
                return labels[key];
            }
        }

        private string GetURL<T>(Dictionary<T, string> labels, T key)
        {
            if (labels.ContainsKey(key))
            {
                return labels[key];
            }
            else
            {
                return "";
            }
        }

        public string GetSound(Milestone m)
        {
            return GetLabel(Sound, m);
        }

        public string GetFactionColor(Faction faction)
        {
            return FactionColor[faction];
        }

        public string GetFactionColorTransparant(Faction faction)
        {
            return FactionColorTransparant[faction];
        }

        public string GetFactionColorTransparant(Faction faction, string transparancy)
        {
            return FactionColor[faction] + transparancy;
        }

        #endregion NamesAndImages

        #region FactionManual

        public string GetFactionInfo_HTML(Game g, Faction f)
        {
            object[] parameters = new object[]
            {
                Faction.Green, //0
                Faction.Black, //1
                Faction.Red, //2
                Faction.Yellow, //3
                Faction.Orange, //4
                Faction.Blue, //5
                g.Map.Arrakeen, //6 
                g.Map.Carthag, //7
                g.Map.TueksSietch, //8
                g.Map.SietchTabr, //9
                g.Map.FalseWallSouth, //10
                g.Map.FalseWallWest, //11
                g.Map.TheGreatFlat, //12
                TreacheryCardType.Mercenary, //13
                TreacheryCardType.Useless, //14
                Concept_STR[Concept.Monster], //15
                Concept_STR[Concept.Resource], //16
                TreacheryCardType.Shield, //17
                TreacheryCardType.Laser, //18
                TreacheryCardType.Karma, //19
                Concept_STR[Concept.Messiah], //20
                MainPhase_STR[MainPhase.ShipmentAndMove], //21
                g.Map.HabbanyaSietch, //22
                SpecialForceName_STR[Faction.Yellow], //23
                SpecialForceName_STR[Faction.Red], //24
                FACTION_INFORMATIONCARDSTYLE, //25
                g.Map.PolarSink, //26
                Faction.Purple, //27
                Faction.Grey, //28
                ForceName_STR[Faction.Grey], //29
                SpecialForceName_STR[Faction.Grey], //30
                Concept.Graveyard, //31
                FactionForce.Grey, //32
                FactionSpecialForce.Grey, //33
                g.Map.HiddenMobileStronghold // 34
            };

            return f switch
            {
                Faction.Green => Format(GetGreenTemplate(g), parameters),
                Faction.Black => Format(GetBlackTemplate(g), parameters),
                Faction.Yellow => Format(GetYellowTemplate(g), parameters),
                Faction.Red => Format(GetRedTemplate(g), parameters),
                Faction.Orange => Format(GetOrangeTemplate(g), parameters),
                Faction.Blue => Format(GetBlueTemplate(g), parameters),
                Faction.Purple => Format(GetPurpleTemplate(g), parameters),
                Faction.Grey => Format(GetGreyTemplate(g), parameters),
                _ => "",
            };
        }

        private string GetGreenTemplate(Game g)
        {
            return
              @"<div style='{25}'>
                <p><strong>At start:</strong> 10 tokens in {6} and 10 in reserve. Start with 10 {16}.</p>
                <p><strong>Free revival:</strong> 2.</p>
                <h5>Basic Advantages</h5>
                <p>You have limited prescience.</p>
                <p>During Bidding you see the treachery card on bid.</p>
                <p>During Shipment & Move you see the top card of the {16} deck.</p>
                <p>During Battle, you may force your opponent to tell you your choice of one of the four elements he will use in his battle plan against you; the leader, the weapon, the defense or the number dialed. If your opponent tells you that he is not playing a weapon or defense, you may not ask something else.</p>" +

              (g.Applicable(Rule.GreenMessiah) ?
              @"<h5>Advanced Advantages</h5>
                <p>After losing a at least 7 forces in battle(s), you may use the {20}. It cannot be used alone in battle but may add its +2 strength to any one leader or {13} per turn. If the leader or {13} is killed, the {20} has no effect in the battle. {20} can only be killed if blown up by a {18}/{17} explosion. A leader accompanied by {20} cannot turn traitor. If killed, the {20} must be revived like any other leader. The {20} has no effect on {0} leader revival.</p>" : "") +

              @"<p><strong>Special {19}:</strong> you may use a {19} card to ask one player's entire battle plan.</p>
                <h5>Alliance Power</h5>
                <p>You may assist your allies by forcing their opponents to tell them one element of their battle plan.</p>
                <h5>Strategy</h5>
                <p>You are handicapped by the fact that you must both purchase cards and ship onto the planet, and you have no source of income other than {16}. This will keep you in constant battles. Since you start from {6} you have the movement advantage of 3 from the outset, and it is wise to protect this. Your prescience allows you to avoid being devoured by {15} and helps you to get some slight head start on the {16} blow. In addition, you can gain some slight advantage over those who would do battle with you by your foreknowledge of one element of their battle plan.</p>
                </div>";
        }

        private string GetBlackTemplate(Game g)
        {

            return
              @"<div style='{25}'>
                <p><strong>At start:</strong> 10 tokens in {7} and 10 tokens in reserve. Start with 10 {16}.</p>
                <p><strong>Free revival:</strong> 2.</p>
                <h5>Basic Advantages</h5>
                <p>You excel in treachery.</p>
                <p>You keep all traitors you draw at the start of the game.</p>
                <p>You may hold up to 8 treachery cards. At start of game, you are dealt 2 cards instead of 1, and every time you buy a card you get an extra card free from the deck (if you have less than 8 total).</p>" +

             (g.Applicable(Rule.BlackMulligan) ? "<p>At start, when you draw 2 or more of your own leaders as traitors, you may shuffle them back and redraw four traitors.</p>" : "") +

             (g.Applicable(Rule.BlackCapturesOrKillsLeaders) ?
              @"<h5>Advanced Advantages</h5>
                <p>Every time you win a battle you can select randomly one leader from the loser (including the leader used in battle, if not killed, but excluding all leaders already used elsewhere that turn). You can kill that leader for 2 {16}; or use the leader once in a battle after which you must return him (her) to the original owner. If all your own leaders have been killed, all captured leaders are immediately returned to their original owners. Killed captured leaders are put in the 'tanks' from which the original owners can revive them (subject to the revival rules).</p>
                <p><strong>Special {19}:</strong> during the Bidding phase, you may use a {19} card to take at random up to all treachery cards of any one player of your choice, as long has your maximum hand size is not exceeded. Then, for each card you took you must give him one of your cards in return.</p>" : "") +

              @"<h5>Alliance Power</h5>
                <p>Leaders in your pay may betray your allies opponents, too.</p>
                <h5>Strategy</h5>
                <p>Your major handicap is your difficulty in obtaining {16}. You are at your greatest relative strength at the beginning of the game and should capitalize on this fact by quickly buying as many treachery cards as you can, and then surging into battle. Since you get 2 cards for every one you bid for, you can afford to bid a little higher than most, but if you spend too lavishly at first you will not have enough {16} to ship in tokens or buy more cards at a later date. The large number of cards you may hold will increase your chances of holding worthless cards. To counteract this you should pick your battles, both to unload cards and to flush out the traitors in your pay.</p>
                </div>";
        }

        private string GetYellowTemplate(Game g)
        {
            bool advancedApplies = g.Applicable(Rule.YellowSeesStorm) || g.Applicable(Rule.YellowSendingMonster) || g.Applicable(Rule.YellowStormLosses) || g.Applicable(Rule.YellowSpecialForces);

            return
              @"<div style='{25}'>
                <p><strong>At start:</strong> 10 tokens distributed as you like on {9}, {10}, and {11}; and 10 tokens in on-planet reserves. Start with 3 {16}.</p>
                <p><strong>Free revival:</strong> 3 (you cannot buy additional revivals).</p>
                <h5>Basic Advantages</h5>
                <p>You are native to this planet and know its ways.</p>
                <p>You may move two territories instead of one.</p>
                <p>Instead of shipping like other factions, you may bring any number of forces from your reserves onto any territory within two territories of and including {12} (subject to storm and occupancy rules).</p>
                <p>If {15} appears in a territory where you have tokens, they are not devoured but, immediately upon conclusion of the following nexus phase, may move from that territory to any one territory (subject to storm and occupancy rules).</p>
                <p>Special Victory Condition: If no player has won by the end of the last turn and if you (or no one) occupies {9} and {22} and neither {1}, {0} nor {2} occupies {8}, you have prevented interference with your plans to alter the planet and win the game.</p>
                <p>If no player has won by the end of the last turn and {4} is not playing, you win the game.</p>" +
              (advancedApplies ? "<h5>Advanced Advantages</h5>" : "") +
              (g.Applicable(Rule.YellowSeesStorm) ? "<p>You can see the number of sectors the next storm will move.</p>" : "") +
              (g.Applicable(Rule.YellowSendingMonster) ? "<p>During {16} blow, each time {15} appears after the first time, you choose in which unprotected territory it appears.</p>" : "") +
              (g.Applicable(Rule.YellowStormLosses) ? "<p>If caught in a storm, only half your forces are killed. You may rally forces into a storm at half loss.</p>" : "") +
              (g.Applicable(Rule.YellowSpecialForces) ? "<p>Your 3 {23} are worth two normal tokens in battle and in taking losses. They are treated as one token in revival. Only one {23} token can be revived per turn.</p>" : "") +

              @"<p><strong>Special {19}:</strong> during {16} blow, you may use a {19} card to cause {15} to appear in any unprotected territory that you wish.</p>
                <h5>Alliance Powers</h5>
                <p>You may protect your allies from being devoured by {15} and may let them revive 3 forces for free. They win with you if you win with your special victory condition.</p>
                <h5>Strategy</h5>
                <p>Your major handicap is poverty. You won't be available to buy cards early game. You must be patient and move your forces into any vacant strongholds, avoiding battles until you are prepared. In battles you can afford to dial high and sacrifice your troops since they have a high revival rate and you can bring them back into play at no cost. You have better mobility than those without a city, and good fighting leaders. Bide your time and wait for an accessible {16} blow that no one else wants in order to build up your resources.<p>
                </div>";
        }

        private string GetRedTemplate(Game g)
        {

            return
              @"<div style='{25}'>
                <p><strong>At start:</strong> 20 tokens in reserve. Start with 10 {16}.</p>
                <p><strong>Free revival:</strong> 1.</p>
                <h5>Basic Advantages</h5>
                <p>You have access to great wealth.</p>
                <p>Whenever any other player pays for {16} for a treachery card, he pays it to you instead of to the {16} bank.</p>" +

              (g.Applicable(Rule.RedSupportingNonAllyBids) ? "<p>You may support bids of non-allied players. Any {16} paid this way flows back to you at the end of the bidding phase.</p>" : "") +

              (g.Applicable(Rule.RedSpecialForces) ?
              @"<h5>Advanced Advantages</h5>
                <p>Your 5 {24} have a special fighting capability. They are worth two normal tokens in battle and in taking losses against all opponents except {3}. Your starred tokens are worth just one against {3}. They are treated as one token in revival. Only one starred token can be revived per turn.</p>" : "") +

              @"<p><strong>Special {19}:</strong> you may use a {19} card to revive up to three tokens or one leader for free.</p>
                <h5>Alliance Powers</h5>
                <p>Unlike other factions, you may give {16} to your allies which they receive immediately. Their payment for any treachery card even with your own {16} comes right back to you. In addition, you may pay (directly to the bank) for the revival of up to 3 extra of their forces (for a possible total of 6).</p>
                <h5>Strategy</h5>
                <p>Your major handicap is that you must ship in all of your tokens at the start of the game, and often this move requires a battle before you are prepared. Even though you do not need to forage for {16} on the surface of the planet often, you still are quite subject to attack since you are likely to concentrate on the cities for the mobility they give you. On the plus side you will never need {16} badly, since the bidding will keep you supplied.</p>
                </div>";
        }

        private string GetOrangeTemplate(Game g)
        {

            return
              @"<div style='{25}'>
                <p><strong>At start:</strong> 5 tokens in {8} and 15 tokens in reserve. Start with 5 {16}.</p>
                <p><strong>Free revival:</strong> 1.</p>
                <h5>Basic Advantages</h5>
                <p>You control all shipments onto the planet.</p>
                <p>You can make one of three possible types of shipments; you may ship normally from reserves onto the planet; or you may site-to-site ship any number of tokens from any one territory to any other territory on the board; or you may ship any number of tokens from any one territory back to your reserves.</p>
                <p>You pay only half the fee when shipping. The cost for shipping to your reserves is one {16} for every two tokens shipped.</p>
                <p>When any other player ships tokens onto the planet from reserves, he pays the {16} to you instead of to the {16} bank.</p>
                <p>If no player has been able to win the game by the end of play, you prevented control over the planet and you automatically win the game.</p>" +

              (g.Applicable(Rule.OrangeDetermineShipment) ?
              @"<h5>Advanced Advantages</h5>
                <p>During Shipment & Move, you decide when you take your turn. You do not have to reveal when you intend to take your turn until the moment you wish to take it.</p>" : "") +

              @"<p><strong>Special {19}:</strong> you may use a {19} card to stop one off-planet shipment of any one player. You may do this directly before or after the shipment occurs.</p>
                <h5>Alliance Powers</h5>
                <p>Allies may also perform site-to-site shipments and may ship at the same fee as you. They win with you if no one else wins. Ally payments for shipments, even with your own {16}, come right back to you.</p>
                <h5>Strategy</h5>
                <p>Your major handicap is your weak array of leaders and your inability to revive quickly. In addition, you usually cannot buy treachery cards at the beginning of the game. You are vulnerable at this point and should make your stronger moves after building up your resources. If players do not ship on at a steady rate you will have to fight for {16} on the planet or collect only the isolated blows. Your major advantage is that you can ship onto the planet inexpensively and can ship from any one territory to any other. This mobility allows you to make surprise moves and is particularly useful when you are the last player in the movement round. If the game is out of reach and well along, try suicide battles against the strongest players to weaken them and prevent a win until the last turn: the victory is then yours.</p>
                </div>";
        }

        private string GetBlueTemplate(Game g)
        {
            bool advancedApplies = g.Applicable(Rule.BlueFirstForceInAnyTerritory) || g.Applicable(Rule.BlueAutoCharity) || g.Applicable(Rule.BlueAdvisors) || g.Applicable(Rule.BlueAccompaniesToShipmentLocation) || g.Applicable(Rule.BlueWorthlessAsKarma);

            return
              @"<div style='{25}'>
                <p><strong>At start:</strong> 1 token in {26} and 19 tokens in reserve. Start with 5 {16}.</p>
                <p><strong>Free revival:</strong> 1.</p>
                <h5>Basic Advantages</h5>
                <p>You are adept in the ways of mind control.</p>
                <p>At start of game you secretly predict which faction will win in which turn (you can't predict the special victory condition of {4} or {3} at the end of play). If that factions wins (alone or as an ally, even your own) when you have predicted, you alone win instead. You can also win normally.</p>
                <p>Whenever any other player ships, you may ship for free one force to {26}.</p>
                <p>You may 'voice' your opponent in battle to play or not to play a projectile/poison weapon or defense, a worthless card or a <i>specific</i> special card. If he can't comply with your command, he may do as he wishes.</p>" +

              (advancedApplies ? @"<h5>Advanced Advantages</h5>" : "") +
              (g.Applicable(Rule.BlueFirstForceInAnyTerritory) ? @"<p>You start with one advisor in any territory of your choice.</p>" : "") +
              (g.Applicable(Rule.BlueAutoCharity) ? @"<p>You automatically receive 2 charity during the Charity Phase.</p>" : "") +
              (g.Applicable(Rule.BlueAdvisors) ? @"<p>You can peacefully coexist as spiritual advisors with all other players' forces in the same territory. Advisors have no effect on the play of the other players whatsoever as if they are not even on the board. They cannot collect {16}, cannot be involved in combat, cannot prevent opponent control of a stronghold and don't get three territory movement bonus. They are still susceptible to storms, {15} and {18}/{17} explosions.</p>" : "") +
              (g.Applicable(Rule.BlueAccompaniesToShipmentLocation) ? @"<p>Whenever any other player ships, instead of shipping a force to {26} you may ship an advisor to the same territory. You cannot ship advisors into a territory where you have fighters.</p>" : "") +
              (g.Applicable(Rule.BlueAdvisors) ? @"<p>You may announce before the {21} phase all territories in which you no longer wish to peacefully coexist. Advisors there are flipped to fighters.</p>" : "") +
              (g.Applicable(Rule.BlueAdvisors) ? @"<p>When you move forces into an occupied territory or when another player moves tokens into a territory where you have advisors, you must decide whether or not you will stay there as advisors or fighters.</p>" : "") +
              (g.Applicable(Rule.BlueWorthlessAsKarma) ? @"<p>You may use a worthless card as a {19} card.</p>" : "") +

              @"<h5>Alliance Power</h5>
                <p>You may 'voice' an ally's opponent.</p>
                <h5>Strategy</h5>
                <p>Your major handicap is your low revival rate. Don't allow large numbers of your tokens to be killed or you may find yourself without sufficient reserves to bring onto the planet. Your strengths are that you have the ability to win by correctly predicting another winner and the secretly working for that player. In addition, you can be quite effective in battles by voicing your opponent and leaving him weaponless or defenseless. You can afford to bide your time while casting subtle innuendoes about which player you have picked to win.<p>
                </div>";
        }

        private string GetPurpleTemplate(Game g)
        {
            return
              @"<div style='{25}'>
                <p><strong>At start:</strong> 20 tokens in reserve. Start with 5 {16}.</p>
                <p><strong>Free revival:</strong> 2.</p>
                <h5>Basic Advantages</h5>
                <p>You have superior genetic engineering technology.</p>
                <p>At the start of the game you are not dealt Traitor Cards. After traitors have been selected, unused traitor cards are shuffled and you get the top 3 cards. These are your Face Dancers. After another faction wins a battle you may reveal their leader to be a Face Dancer, and the following occurs:</p>
                <ol>
                <li>The battle still counts as a win for that player (they keep or discard treachery cards, killed forces and leaders are put in the {31}, {16} is collected for any leaders killed, and a Tech Token is claimed).</li>
                <li>The Face Dancer leader is sent to the {31} if it was not already killed, but you don't collect {16} for it.</li>
                <li>The remaining amount of forces in the territory go back to their reserves and are replaced by up to the same amount of your forces from your reserves and/or from anywhere on the planet.</li>
                </ol>
                <p>Once revealed you do not replace a Face Dancer (Traitor Card) until you have revealed all 3. When that happens, they are shuffled into the Traitor deck and you get 3 new Face Dancers.</p>
                <p>At end of turn, you may replace an unrevealed Face Dancer with one from the traitor deck (after putting it back and shuffling).</p>
                <p>You have no revival limits, and your revive at half price (rounded up). Other factions make revival payments to you.</p>
                <p>You may increase the 3 force revival limit for any other faction to 5. Also, for each faction using free revival or a ghola card, you take 1 {16} from the bank.</p>
                <p>Upon request by a faction for a specific killed leader, you can set a price for its early revival. This can only be done when this leader cannot be revived according to normal revival rules.</p>
                <p>Zoal’s value in battle matches the value of the opponent’s leader (0 against a Cheap Hero), and for collecting {16} for his death. His discounted price to revive is 3.</p>" +

              (g.Applicable(Rule.GreyAndPurpleExpansionPurpleGholas) ?
              @"<h5>Advanced Advantages</h5>
                <p>When you have fewer than five leaders available, you may revive dead leaders of other factions at your discounted rate and add them to your leader pool.</p>" : "") +

              @"<p><strong>Special {19}:</strong> you may prevent a player from performing a revival (forces and/or leader).</p>
                <h5>Alliance Power</h5>
                <p>Your ally may revive at half price (rounded up).</p>
                <h5>Strategy</h5>
                <p>You are handicapped by having no forces on the planet to start and only a small amount of {16} until you begin receiving {16} for revivals. You will have to bide your time as other factions battle, waiting until you start gaining {16} and giving your Face Dancers a chance to suddenly strike, or get into minor battles early to drive forces to the tanks, and possibly get a Face Dancer reveal. Use your ability to cycle through Face Dancers during the Mentat Pause to position yourself with a potentially more useful Face Dancer.</p>
                </div>";
        }

        private string GetGreyTemplate(Game g)
        {
            bool advancedApplies = g.Applicable(Rule.GreyAndPurpleExpansionGreySwappingCardOnBid) || g.Applicable(Rule.AdvancedCombat);

            return
              @"<div style='{25}'>
                <p><strong>At start:</strong> 3 {32} and 3 {33} in the {34}. 10 {32} and 4 {33} in reserve. Start with 10 {16}.</p>
                <p><strong>Free revival:</strong> 1.</p>
                <h5>Basic Advantages</h5>
                <p>You are skilled in technology and production.</p>
                <p>During Setup you see all initially dealt Treachery Cards and choose your starting card from them.</p>
                <p>Before Bidding, one extra card is drawn and you see them all and put one of those cards on top or on the bottom of the Treachery Card deck. The remaining cards are shuffled for the bidding round.</p>
                <p>Your 7 {33} forces are each worth 2 normal forces in battle, are able to move 2 territories instead of 1 and can collect 3 {16}. Your {33} forces ship normally, but each costs 3 to revive.</p>
                <p>Your 13 {32} forces ship normally but are worth ½ in battle. {32} can be used to absorb losses after a battle. After battle losses are calculated, any of your surviving {32} in that territory can be exchanged for {33} you lost in that battle. {32} can control strongholds and collect {16}. {32} move 2 if accompanied by a {33}, or 1 if they are not.</p>
                <p>After the first storm movement, place your {34} in any non-stronghold territory. This stronghold counts towards the game win and is protected from worms and storms.</p>
                <p>Subsequently, before text storms are revealed, as long as your forces occupy it, you may move your {34} up to 3 territories to any non-stronghold territory. You can't move it into or out of a storm. When you move into, from, or through a sector containing {16}, you may immediately collect 2 {16} per force in your stronghold.</p>
                <p>No other faction may ship forces directly into your {34}, or move it if they take control. Other factions must move or ship forces into the territory it is pointing at (including {26}), and then use one movement to enter.</p>" +

              (advancedApplies ? @"<h5>Advanced Advantages</h5>" : "") +
              (g.Applicable(Rule.GreyAndPurpleExpansionGreySwappingCardOnBid) ? @"<p>Once, during the bidding round, before bidding begins on a card and before {0} gets to look at the card, you may take the Treachery Card about to be bid on, replacing it with one from your hand.</p>" : "") +
              (g.Applicable(Rule.AdvancedCombat) ? @"<p>{32} are always considered half strength for dialing. You can’t increase the effectiveness of {32} in battle by spending {16}.</p>" : "") +

              @"<p><strong>Special {19}:</strong> during Shipment and Movement, you may move the {34} 2 territories on your turn as well as make your normal movement.</p>
                <h5>Alliance Power</h5>
                <p>After an ally purchases a Treachery Card during bidding, they may immediately discard it and draw the top card from the deck.</p>
                <h5>Strategy</h5>
                <p>You are handicapped by having weaker forces in the halfstrength {32}, which make up the bulk of your forces. You have no regular source of {16} income. However, tactical placement of your {34} can position you to acquire {16} left behind on the planet. You also have an advantage over other factions because you know what Treachery cards are in play and you can mix in or suppress certain cards during the bidding phase.</p>
                </div>";
        }

        #endregion FactionManual

        #region SkinValidationAndFixing

        public async Task<IEnumerable<string>> ValidateAndFix(Func<string, Task<bool>> UrlExists)
        {
            var errors = new List<string>();

            /*
            var tReportBackground_ShipmentAndMove_URL = FixIfMissing("ReportBackground_ShipmentAndMove_URL", true, ReportBackground_ShipmentAndMove_URL, Dune1979.ReportBackground_ShipmentAndMove_URL, errors, UrlExists);
            var tReportBackground_StormAndResourceBlow_URL = FixIfMissing("ReportBackground_StormAndResourceBlow_URL", true, ReportBackground_StormAndResourceBlow_URL, Dune1979.ReportBackground_StormAndResourceBlow_URL, errors, UrlExists);
            var tReportBackground_EndOfTurn_URL = FixIfMissing("ReportBackground_EndOfTurn_URL", true, ReportBackground_EndOfTurn_URL, Dune1979.ReportBackground_EndOfTurn_URL, errors, UrlExists);
            var tReportBackground_Resurrection_URL = FixIfMissing("ReportBackground_Resurrection_URL", true, ReportBackground_Resurrection_URL, Dune1979.ReportBackground_Resurrection_URL, errors, UrlExists);
            var tPanelBackground_Bidding_URL = FixIfMissing("PanelBackground_Bidding_URL", true, PanelBackground_Bidding_URL, Dune1979.PanelBackground_Bidding_URL, errors, UrlExists);
            var tPanelBackground_EndOfGame_URL = FixIfMissing("PanelBackground_EndOfGame_URL", true, PanelBackground_EndOfGame_URL, Dune1979.PanelBackground_EndOfGame_URL, errors, UrlExists);
            var tPanelBackground_Battle_URL = FixIfMissing("PanelBackground_Battle_URL", true, PanelBackground_Battle_URL, Dune1979.PanelBackground_Battle_URL, errors, UrlExists);
            */

            var tMap_URL = FixIfMissing("Map_URL", true, Map_URL, Dune1979.Map_URL, errors, UrlExists);
            var tEye_URL = FixIfMissing("Eye_URL", true, Eye_URL, Dune1979.Eye_URL, errors, UrlExists);
            var tEyeSlash_URL = FixIfMissing("EyeSlash_URL", true, EyeSlash_URL, Dune1979.EyeSlash_URL, errors, UrlExists);
            var tPlanet_URL = FixIfMissing("Planet_URL", true, Planet_URL, Dune1979.Planet_URL, errors, UrlExists);
            var tCardBack_ResourceCard_URL = FixIfMissing("CardBack_ResourceCard_URL", true, CardBack_ResourceCard_URL, Dune1979.CardBack_ResourceCard_URL, errors, UrlExists);
            var tCardBack_TreacheryCard_URL = FixIfMissing("CardBack_TreacheryCard_URL", true, CardBack_TreacheryCard_URL, Dune1979.CardBack_TreacheryCard_URL, errors, UrlExists);
            var tBattleScreen_URL = FixIfMissing("BattleScreen_URL", true, BattleScreen_URL, Dune1979.BattleScreen_URL, errors, UrlExists);
            var tMessiah_URL = FixIfMissing("Messiah_URL", true, Messiah_URL, Dune1979.Messiah_URL, errors, UrlExists);
            var tMonster_URL = FixIfMissing("Monster_URL", true, Monster_URL, Dune1979.Monster_URL, errors, UrlExists);
            var tHarvester_URL = FixIfMissing("Harvester_URL", true, Harvester_URL, Dune1979.Harvester_URL, errors, UrlExists);
            var tResource_URL = FixIfMissing("Resource_URL", true, Resource_URL, Dune1979.Resource_URL, errors, UrlExists);
            var tHMS_URL = FixIfMissing("HMS_URL", true, HMS_URL, Dune1979.HMS_URL, errors, UrlExists);
            
            /*
            var tMusicGeneral_URL = FixIfMissing("MusicGeneral_URL", false, MusicGeneral_URL, Dune1979.MusicGeneral_URL, errors, UrlExists);
            var tMusicSetup_URL = FixIfMissing("MusicSetup_URL", false, MusicSetup_URL, Dune1979.MusicSetup_URL, errors, UrlExists);
            var tMusicResourceBlow_URL = FixIfMissing("MusicResourceBlow_URL", false, MusicResourceBlow_URL, Dune1979.MusicResourceBlow_URL, errors, UrlExists);
            var tMusicBidding_URL = FixIfMissing("MusicBidding_URL", false, MusicBidding_URL, Dune1979.MusicBidding_URL, errors, UrlExists);
            var tMusicShipmentAndMove_URL = FixIfMissing("MusicShipmentAndMove_URL", false, MusicShipmentAndMove_URL, Dune1979.MusicShipmentAndMove_URL, errors, UrlExists);
            var tMusicBattle_URL = FixIfMissing("MusicBattle_URL", false, MusicBattle_URL, Dune1979.MusicBattle_URL, errors, UrlExists);
            var tMusicBattleClimax_URL = FixIfMissing("MusicBattleClimax_URL", false, MusicBattleClimax_URL, Dune1979.MusicBattleClimax_URL, errors, UrlExists);
            var tMusicMentat_URL = FixIfMissing("MusicMentat_URL", false, MusicMentat_URL, Dune1979.MusicMentat_URL, errors, UrlExists);
            */

            var tMapDimensions = FixIfMissing("MapDimensions", false, MapDimensions, Dune1979.MapDimensions, errors, UrlExists);
            var tPlanetCenter = FixIfMissing("PlanetCenter", false, PlanetCenter, Dune1979.PlanetCenter, errors, UrlExists);
            var tPlanetRadius = FixIfMissing("PlanetRadius", false, PlanetRadius, Dune1979.PlanetRadius, errors, UrlExists);
            var tMapRadius = FixIfMissing("MapRadius", false, MapRadius, Dune1979.MapRadius, errors, UrlExists);
            var tPlayerTokenRadius = FixIfMissing("PlayerTokenRadius", false, PlayerTokenRadius, Dune1979.PlayerTokenRadius, errors, UrlExists);
            var tSpiceDeckLocation = FixIfMissing("SpiceDeckLocation", false, SpiceDeckLocation, Dune1979.SpiceDeckLocation, errors, UrlExists);
            var tTreacheryDeckLocation = FixIfMissing("TreacheryDeckLocation", false, TreacheryDeckLocation, Dune1979.TreacheryDeckLocation, errors, UrlExists);
            var tBattleScreenWidth = FixIfMissing("BattleScreenWidth", false, BattleScreenWidth, Dune1979.BattleScreenWidth, errors, UrlExists);
            var tBattleScreenHeight = FixIfMissing("BattleScreenHeight", false, BattleScreenHeight, Dune1979.BattleScreenHeight, errors, UrlExists);
            var tBattleScreenHeroX = FixIfMissing("BattleScreenHeroX", false, BattleScreenHeroX, Dune1979.BattleScreenHeroX, errors, UrlExists);
            var tBattleScreenHeroY = FixIfMissing("BattleScreenHeroY", false, BattleScreenHeroY, Dune1979.BattleScreenHeroY, errors, UrlExists);
            var tBattleWheelHeroWidth = FixIfMissing("BattleWheelHeroWidth", false, BattleWheelHeroWidth, Dune1979.BattleWheelHeroWidth, errors, UrlExists);
            var tBattleWheelHeroHeight = FixIfMissing("BattleWheelHeroHeight", false, BattleWheelHeroHeight, Dune1979.BattleWheelHeroHeight, errors, UrlExists);
            var tBattleWheelForcesX = FixIfMissing("BattleWheelForcesX", false, BattleWheelForcesX, Dune1979.BattleWheelForcesX, errors, UrlExists);
            var tBattleWheelForcesY = FixIfMissing("BattleWheelForcesY", false, BattleWheelForcesY, Dune1979.BattleWheelForcesY, errors, UrlExists);
            var tBattleWheelCardX = FixIfMissing("BattleWheelCardX", false, BattleWheelCardX, Dune1979.BattleWheelCardX, errors, UrlExists);
            var tBattleWheelCardY = FixIfMissing("BattleWheelCardY", false, BattleWheelCardY, Dune1979.BattleWheelCardY, errors, UrlExists);
            var tBattleWheelCardWidth = FixIfMissing("BattleWheelCardWidth", false, BattleWheelCardWidth, Dune1979.BattleWheelCardWidth, errors, UrlExists);
            var tBattleWheelCardHeight = FixIfMissing("BattleWheelCardHeight", false, BattleWheelCardHeight, Dune1979.BattleWheelCardHeight, errors, UrlExists);

            /*
            var tSound_YourTurn_URL = FixIfMissing("Sound_YourTurn_URL", false, Sound_YourTurn_URL, Dune1979.Sound_YourTurn_URL, errors, UrlExists);
            var tSound_Chatmessage_URL = FixIfMissing("Sound_Chatmessage_URL", false, Sound_Chatmessage_URL, Dune1979.Sound_Chatmessage_URL, errors, UrlExists);
            var tSound = FixDictionaryIfMissing("Sound", false, Sound, Dune1979.Sound, errors, UrlExists);
            */

            var tTreacheryCardType_STR = FixDictionaryIfMissing("TreacheryCardType_STR", false, TreacheryCardType_STR, Dune1979.TreacheryCardType_STR, errors, UrlExists);
            var tTreacheryCardName_STR = FixDictionaryIfMissing("TreacheryCardName_STR", false, TreacheryCardName_STR, Dune1979.TreacheryCardName_STR, errors, UrlExists);
            var tTreacheryCardDescription_STR = FixDictionaryIfMissing("TreacheryCardDescription_STR", false, TreacheryCardDescription_STR, Dune1979.TreacheryCardDescription_STR, errors, UrlExists);
            var tTechTokenDescription_STR = FixDictionaryIfMissing("TechTokenDescription_STR", false, TechTokenDescription_STR, Dune1979.TechTokenDescription_STR, errors, UrlExists);
            var tTreacheryCardImage_URL = FixDictionaryIfMissing("TreacheryCardImage_URL", true, TreacheryCardImage_URL, Dune1979.TreacheryCardImage_URL, errors, UrlExists);
            var tResourceCardImage_URL = FixDictionaryIfMissing("ResourceCardImage_URL", true, ResourceCardImage_URL, Dune1979.ResourceCardImage_URL, errors, UrlExists);
            var tConcept_STR = FixDictionaryIfMissing("Concept_STR", false, Concept_STR, Dune1979.Concept_STR, errors, UrlExists);
            var tMainPhase_STR = FixDictionaryIfMissing("MainPhase_STR", false, MainPhase_STR, Dune1979.MainPhase_STR, errors, UrlExists);
            var tPersonName_STR = FixDictionaryIfMissing("PersonName_STR", false, PersonName_STR, Dune1979.PersonName_STR, errors, UrlExists);
            var tPersonImage_URL = FixDictionaryIfMissing("PersonImage_URL", true, PersonImage_URL, Dune1979.PersonImage_URL, errors, UrlExists);
            var tTerritoryName_STR = FixDictionaryIfMissing("TerritoryName_STR", false, TerritoryName_STR, Dune1979.TerritoryName_STR, errors, UrlExists);
            var tFactionName_STR = FixDictionaryIfMissing("FactionName_STR", false, FactionName_STR, Dune1979.FactionName_STR, errors, UrlExists);
            var tFactionImage_URL = FixDictionaryIfMissing("FactionImage_URL", true, FactionImage_URL, Dune1979.FactionImage_URL, errors, UrlExists);
            var tFactionForceImage_URL = FixDictionaryIfMissing("FactionForceImage_URL", true, FactionForceImage_URL, Dune1979.FactionForceImage_URL, errors, UrlExists);
            var tFactionSpecialForceImage_URL = FixDictionaryIfMissing("FactionSpecialForceImage_URL", true, FactionSpecialForceImage_URL, Dune1979.FactionSpecialForceImage_URL, errors, UrlExists);
            var tFactionTableImage_URL = FixDictionaryIfMissing("FactionTableImage_URL", true, FactionTableImage_URL, Dune1979.FactionTableImage_URL, errors, UrlExists);
            var tFactionFacedownImage_URL = FixDictionaryIfMissing("FactionFacedownImage_URL", true, FactionFacedownImage_URL, Dune1979.FactionFacedownImage_URL, errors, UrlExists);
            var tFactionColorTransparant = FixDictionaryIfMissing("FactionColorTransparant", false, FactionColorTransparant, Dune1979.FactionColorTransparant, errors, UrlExists);
            var tFactionColor = FixDictionaryIfMissing("FactionColor", false, FactionColor, Dune1979.FactionColor, errors, UrlExists);
            var tSpecialForceName_STR = FixDictionaryIfMissing("SpecialForceName_STR", false, SpecialForceName_STR, Dune1979.SpecialForceName_STR, errors, UrlExists);
            var tForceName_STR = FixDictionaryIfMissing("ForceName_STR", false, ForceName_STR, Dune1979.ForceName_STR, errors, UrlExists);
            var tTechTokenName_STR = FixDictionaryIfMissing("TechTokenName_STR", false, TechTokenName_STR, Dune1979.TechTokenName_STR, errors, UrlExists);
            var tTechTokenImage_URL = FixDictionaryIfMissing("TechTokenImage_URL", true, TechTokenImage_URL, Dune1979.TechTokenImage_URL, errors, UrlExists);
            
            /*
            ReportBackground_ShipmentAndMove_URL = await tReportBackground_ShipmentAndMove_URL;
            ReportBackground_StormAndResourceBlow_URL = await tReportBackground_StormAndResourceBlow_URL;
            ReportBackground_EndOfTurn_URL = await tReportBackground_EndOfTurn_URL;
            ReportBackground_Resurrection_URL = await tReportBackground_Resurrection_URL;
            PanelBackground_Bidding_URL = await tPanelBackground_Bidding_URL;
            PanelBackground_EndOfGame_URL = await tPanelBackground_EndOfGame_URL;
            PanelBackground_Battle_URL = await tPanelBackground_Battle_URL;
            */

            Map_URL = await tMap_URL;
            Eye_URL = await tEye_URL;
            EyeSlash_URL = await tEyeSlash_URL;
            Planet_URL = await tPlanet_URL;
            CardBack_ResourceCard_URL = await tCardBack_ResourceCard_URL;
            CardBack_TreacheryCard_URL = await tCardBack_TreacheryCard_URL;
            BattleScreen_URL = await tBattleScreen_URL;
            Messiah_URL = await tMessiah_URL;
            Monster_URL = await tMonster_URL;
            Harvester_URL = await tHarvester_URL;
            Resource_URL = await tResource_URL;
            HMS_URL = await tHMS_URL;

            /*
            MusicGeneral_URL = await tMusicGeneral_URL;
            MusicSetup_URL = await tMusicSetup_URL;
            MusicResourceBlow_URL = await tMusicResourceBlow_URL;
            MusicBidding_URL = await tMusicBidding_URL;
            MusicShipmentAndMove_URL = await tMusicShipmentAndMove_URL;
            MusicBattle_URL = await tMusicBattle_URL;
            MusicBattleClimax_URL = await tMusicBattleClimax_URL;
            MusicMentat_URL = await tMusicMentat_URL;
            */

            /*
            Sound_YourTurn_URL = await tSound_YourTurn_URL;
            Sound_Chatmessage_URL = await tSound_Chatmessage_URL;
            Sound = await tSound;
            */

            MapDimensions = await tMapDimensions;
            PlanetCenter = await tPlanetCenter;
            PlanetRadius = await tPlanetRadius;
            MapRadius = await tMapRadius;
            PlayerTokenRadius = await tPlayerTokenRadius;
            SpiceDeckLocation = await tSpiceDeckLocation;
            TreacheryDeckLocation = await tTreacheryDeckLocation;
            BattleScreenWidth = await tBattleScreenWidth;
            BattleScreenHeight = await tBattleScreenHeight;
            BattleScreenHeroX = await tBattleScreenHeroX;
            BattleScreenHeroY = await tBattleScreenHeroY;
            BattleWheelHeroWidth = await tBattleWheelHeroWidth;
            BattleWheelHeroHeight = await tBattleWheelHeroHeight;
            BattleWheelForcesX = await tBattleWheelForcesX;
            BattleWheelForcesY = await tBattleWheelForcesY;
            BattleWheelCardX = await tBattleWheelCardX;
            BattleWheelCardY = await tBattleWheelCardY;
            BattleWheelCardWidth = await tBattleWheelCardWidth;
            BattleWheelCardHeight = await tBattleWheelCardHeight;
            
            TreacheryCardType_STR = await tTreacheryCardType_STR;
            TreacheryCardName_STR = await tTreacheryCardName_STR;
            TreacheryCardDescription_STR = await tTreacheryCardDescription_STR;
            TechTokenDescription_STR = await tTechTokenDescription_STR;
            TreacheryCardImage_URL = await tTreacheryCardImage_URL;
            ResourceCardImage_URL = await tResourceCardImage_URL;
            Concept_STR = await tConcept_STR;
            MainPhase_STR = await tMainPhase_STR;
            PersonName_STR = await tPersonName_STR;
            PersonImage_URL = await tPersonImage_URL;
            TerritoryName_STR = await tTerritoryName_STR;
            FactionName_STR = await tFactionName_STR;
            FactionImage_URL = await tFactionImage_URL;
            FactionForceImage_URL = await tFactionForceImage_URL;
            FactionSpecialForceImage_URL = await tFactionSpecialForceImage_URL;
            FactionTableImage_URL = await tFactionTableImage_URL;
            FactionFacedownImage_URL = await tFactionFacedownImage_URL;
            FactionColorTransparant = await tFactionColorTransparant;
            FactionColor = await tFactionColor;
            SpecialForceName_STR = await tSpecialForceName_STR;
            ForceName_STR = await tForceName_STR;
            TechTokenName_STR = await tTechTokenName_STR;
            TechTokenImage_URL = await tTechTokenImage_URL;

            return errors;
        }

        private async Task<T> FixIfMissing<T>(string propertyName, bool checkIfUrlExists, T toCheck, T referenceValue, List<string> errors, Func<string, Task<bool>> UrlExists)
        {
            if (toCheck == null)
            {
                errors.Add(propertyName + " is missing.");
                return referenceValue;
            }
            else if (checkIfUrlExists && !await UrlExists(toCheck.ToString()))
            {
                errors.Add("Resource \"" + toCheck + "\" (" + propertyName + ") not found.");
                return referenceValue;
            }

            return toCheck;
        }

        private async Task<Dictionary<T, string>> FixDictionaryIfMissing<T>(string propertyName, bool checkIfUrlExists, Dictionary<T, string> toCheck, Dictionary<T, string> referenceValues, List<string> errors, Func<string, Task<bool>> UrlExists)
        {
            if (toCheck == null)
            {
                errors.Add(propertyName + " is missing.");
                return referenceValues;
            }

            foreach (var key in referenceValues.Keys)
            {
                if (!toCheck.ContainsKey(key))
                {
                    errors.Add(string.Format("{0} does not contain \"{1}\"", propertyName, key));
                    toCheck.Add(key, referenceValues[key]);
                }
                else if (checkIfUrlExists && !await UrlExists(toCheck[key]))
                {
                    errors.Add("Resource \"" + toCheck[key] + "\" (" + propertyName + "." + key + ") not found.");
                    toCheck.Remove(key);
                    toCheck.Add(key, referenceValues[key]);
                }
            }

            return toCheck;
        }

        #endregion SkinValidationAndFixing

        #region LoadingAndSaving
        public static Skin Load(string data)
        {
            var serializer = JsonSerializer.CreateDefault();
            serializer.Formatting = Formatting.Indented;
            var textReader = new StringReader(data);
            var jsonReader = new JsonTextReader(textReader);
            return serializer.Deserialize<Skin>(jsonReader);
        }

        public string GetSkinAsString()
        {
            return SkinToString(Current);
        }

        public static string SkinToString(Skin skin)
        {
            var serializer = JsonSerializer.CreateDefault();
            serializer.Formatting = Formatting.Indented;
            var writer = new StringWriter();
            serializer.Serialize(writer, skin);
            writer.Close();
            return writer.ToString();
        }
        #endregion LoadingAndSaving

        public static Skin Dune1979 = new Skin()
        {
            Description = "1979 Art",
            Version = CurrentVersion,

            Map_URL = DEFAULT_ART_LOCATION + "/art/map.svg",
            Eye_URL = DEFAULT_ART_LOCATION + "/art/eye.svg",
            EyeSlash_URL = DEFAULT_ART_LOCATION + "/art/eyeslash.svg",
            Planet_URL = DEFAULT_ART_LOCATION + "/art/planet.png",
            CardBack_ResourceCard_URL = DEFAULT_ART_LOCATION + "/art/SpiceBack.gif",
            CardBack_TreacheryCard_URL = DEFAULT_ART_LOCATION + "/art/TreacheryBack.gif",
            BattleScreen_URL = DEFAULT_ART_LOCATION + "/art/wheel.png",
            Messiah_URL = DEFAULT_ART_LOCATION + "/art/messiah.svg",
            Monster_URL = DEFAULT_ART_LOCATION + "/art/monster.svg",
            DrawResourceIconsOnMap = true,
            ShowVerboseToolipsOnMap = true,
            Resource_URL = DEFAULT_ART_LOCATION + "/art/PassiveSpice.svg",
            Harvester_URL = DEFAULT_ART_LOCATION + "/art/ActiveSpice.svg",
            HMS_URL = DEFAULT_ART_LOCATION + "/art/hms.png",

            ReportBackground_ShipmentAndMove_URL = DEFAULT_ART_LOCATION + "/art/vortex.gif",
            ReportBackground_StormAndResourceBlow_URL = DEFAULT_ART_LOCATION + "/art/storm.gif",
            ReportBackground_EndOfTurn_URL = DEFAULT_ART_LOCATION + "/art/collection.gif",
            ReportBackground_Resurrection_URL = DEFAULT_ART_LOCATION + "/art/storm.gif",

            PanelBackground_Bidding_URL = DEFAULT_ART_LOCATION + "/art/spacetravel.gif",
            PanelBackground_EndOfGame_URL = DEFAULT_ART_LOCATION + "/art/blueeyes.gif",
            PanelBackground_Battle_URL = DEFAULT_ART_LOCATION + "/art/battle.gif",

            Concept_STR = new Dictionary<Concept, string>()
            {

                [Concept.Messiah] = "Kwisatz Haderach",
                [Concept.Monster] = "Shai Hulud",
                [Concept.Resource] = "Spice",
                [Concept.Graveyard] = "Tleilaxu Tanks",
                [Concept.BabyMonster] = "Sandtrout"
            },

            MainPhase_STR = new Dictionary<MainPhase, string>
            {
                [MainPhase.None] = "None",
                [MainPhase.Started] = "Awaiting Players",
                [MainPhase.Setup] = "Setting Up",
                [MainPhase.Storm] = "Storm",
                [MainPhase.Charity] = "CHOAM Charity",
                [MainPhase.Blow] = "Spice Blow",
                [MainPhase.Bidding] = "Bidding",
                [MainPhase.Resurrection] = "Revival",
                [MainPhase.ShipmentAndMove] = "Ship & Move",
                [MainPhase.Battle] = "Battle",
                [MainPhase.Collection] = "Spice Collection",
                [MainPhase.Contemplate] = "Mentat",
                [MainPhase.Ended] = "Game Ended"
            },

            TreacheryCardType_STR = new Dictionary<TreacheryCardType, string>
            {
                [TreacheryCardType.None] = "None",
                [TreacheryCardType.Laser] = "Lasegun",

                [TreacheryCardType.ProjectileDefense] = "Projectile Defense",
                [TreacheryCardType.Projectile] = "Projectile Weapon",
                [TreacheryCardType.WeirdingWay] = "Weirding Way",
                [TreacheryCardType.ProjectileAndPoison] = "Poison Blade",

                [TreacheryCardType.PoisonDefense] = "Poison Defense",
                [TreacheryCardType.Poison] = "Poison Weapon",
                [TreacheryCardType.Chemistry] = "Chemistry",
                [TreacheryCardType.PoisonTooth] = "Poison Tooth",

                [TreacheryCardType.Shield] = "Shield",
                [TreacheryCardType.Antidote] = "Snooper",
                [TreacheryCardType.ShieldAndAntidote] = "Shield Snooper",

                [TreacheryCardType.Mercenary] = "Cheap Hero",
                [TreacheryCardType.Karma] = "Karama",
                [TreacheryCardType.Useless] = "Worthless",
                [TreacheryCardType.StormSpell] = "Weather Control",
                [TreacheryCardType.RaiseDead] = "Tleilaxu Ghola",
                [TreacheryCardType.Metheor] = "Family Atomics",
                [TreacheryCardType.Caravan] = "Hajr",
                [TreacheryCardType.Clairvoyance] = "Truthtrance",

                [TreacheryCardType.ArtilleryStrike] = "Artillery Strike",
                [TreacheryCardType.Harvester] = "Harvester",
                [TreacheryCardType.Thumper] = "Thumper",
                [TreacheryCardType.Amal] = "Amal",

                [TreacheryCardType.Distrans] = "Distrans",
                [TreacheryCardType.Juice] = "Juice Of Sapho",
                [TreacheryCardType.MirrorWeapon] = "Mirror Weapon",
                [TreacheryCardType.PortableAntidote] = "Portable Snooper",
                [TreacheryCardType.Flight] = "Ornithopter",
                [TreacheryCardType.SearchDiscarded] = "Nullentropy",
                [TreacheryCardType.TakeDiscarded] = "Semuta Drug",
                [TreacheryCardType.Residual] = "Residual Poison",
                [TreacheryCardType.Rockmelter] = "Stone Burner"
            },

            TreacheryCardName_STR = new Dictionary<int, string>
            {
                [0] = "Lasegun",
                [1] = "Crysknife",
                [2] = "Maula Pistol",
                [3] = "Slip Tip",
                [4] = "Stunner",
                [5] = "Chaumas",
                [6] = "Chaumurky",
                [7] = "Ellaca Drug",
                [8] = "Gom Jabbar",
                [9] = "Shield",
                [13] = "Snooper",
                [17] = "Cheap Hero",
                [18] = "Cheap Hero",
                [19] = "Cheap Hero",
                [20] = "Tleilaxu Ghola",
                [21] = "Family Atomics",
                [22] = "Hajr",
                [23] = "Karama",
                [25] = "Truthtrance",
                [27] = "Weather Control",
                [28] = "Baliset",
                [29] = "Jubba Cloak",
                [30] = "Kulon",
                [31] = "La La La",
                [32] = "Trip to Gamont",

                [33] = "Poison Blade",
                [34] = "Hunter-Seeker",
                [35] = "Basilia Weapon",
                [36] = "Weirding Way",
                [37] = "Poison Tooth",
                [38] = "Shield Snooper",
                [39] = "Chemistry",
                [40] = "Artillery Strike",
                [41] = "Harvester",
                [42] = "Thumper",
                [43] = "Amal",
                [44] = "Kull Wahad",

                [45] = "Distrans",
                [46] = "Juice Of Sapho",
                [47] = "Mirror Weapon",
                [48] = "Portable Snooper",
                [49] = "Ornithopter",
                [50] = "Nullentropy",
                [51] = "Semuta Drug",
                [52] = "Residual Poison",
                [53] = "Stone Burner",
            },

            TreacheryCardDescription_STR = new Dictionary<int, string>
            {
                [0] = "Play as part of your Battle Plan. Automatically kills your opponent's leader. Causes an explosion when a Shield is used in the same battle, killing both leaders and all forces in the territory, cause both factions to loose the battle.",
                [1] = "Weapon - Projectile - Play as part of your Battle Plan. Kills opponent's leader before battle is resolved. Opponent may protect leader with a Shield. You may keep this card if you win the battle.",
                [2] = "Weapon - Projectile - Play as part of your Battle Plan. Kills opponent's leader before battle is resolved. Opponent may protect leader with a Shield. You may keep this card if you win the battle.",
                [3] = "Weapon - Projectile - Play as part of your Battle Plan. Kills opponent's leader before battle is resolved. Opponent may protect leader with a Shield. You may keep this card if you win the battle.",
                [4] = "Weapon - Projectile - Play as part of your Battle Plan. Kills opponent's leader before battle is resolved. Opponent may protect leader with a Shield. You may keep this card if you win the battle.",
                [5] = "Weapon - Poison - Play as part of your Battle Plan. Kills opponent's leader before battle is resolved. Opponent may protect leader with a Snooper. You may keep this card if you win the battle.",
                [6] = "Weapon - Poison - Play as part of your Battle Plan. Kills opponent's leader before battle is resolved. Opponent may protect leader with a Snooper. You may keep this card if you win the battle.",
                [7] = "Weapon - Poison - Play as part of your Battle Plan. Kills opponent's leader before battle is resolved. Opponent may protect leader with a Snooper. You may keep this card if you win the battle.",
                [8] = "Weapon - Poison - Play as part of your Battle Plan. Kills opponent's leader before battle is resolved. Opponent may protect leader with a Snooper. You may keep this card if you win the battle.",
                [9] = "Defense - Projectile - Play as part of your Battle Plan. Protects your leader from a projectile weapon in this battle. You may keep this card if you win this battle.",
                [13] = "Defense - Poison - Play as part of your Battle Plan. Protects your leader from a poison weapon in this battle. You may keep this card if you win this battle.",
                [17] = "Play as a leader with zero strength on your Battle Plan and discard after the battle.",
                [18] = "Play as a leader with zero strength on your Battle Plan and discard after the battle.",
                [19] = "Play as a leader with zero strength on your Battle Plan and discard after the battle.",
                [20] = "Play at any time to revive up to 5 forces or 1 leader.",
                [21] = "Can be played after turn 1 just before the storm moves if you have forces on the Shield Wall or an adjacent territory. Destroys the Shield Wall and all forces on it, causing Arrakeen, Carthag and the Imperial Basin to be vulnerable to storms for the rest of the game. This card is then removed from play.",
                [22] = "Play during your turn at the Movement phase to perform an additional move.",
                [23] = "Allows you to bid any amount of spice on a card or immediately win a card on bid. Allows you to ship at half price. In the advanced game, allows use of your Special Karama Power once during the game and can be used to prevent a Faction Advantage. Discard after use.",
                [25] = "Publicly ask one player a yes or no question about the game. That question must be answered truthfully.",
                [27] = "Can be played after turn 1 just before the storm moves. Instead of normal storm moved, you can move the storm 0 to 10 sectors.",
                [28] = "Worthless Card - Play as part of your Battle Plan, in place of a weapon, defense, or both. This card has no value in play, and you can discard it only by playing it in your Battle plan.",
                [29] = "Worthless Card - Play as part of your Battle Plan, in place of a weapon, defense, or both. This card has no value in play, and you can discard it only by playing it in your Battle plan.",
                [30] = "Worthless Card - Play as part of your Battle Plan, in place of a weapon, defense, or both. This card has no value in play, and you can discard it only by playing it in your Battle plan.",
                [31] = "Worthless Card - Play as part of your Battle Plan, in place of a weapon, defense, or both. This card has no value in play, and you can discard it only by playing it in your Battle plan.",
                [32] = "Worthless Card - Play as part of your Battle Plan, in place of a weapon, defense, or both. This card has no value in play, and you can discard it only by playing it in your Battle plan.",

                [33] = "Play as part of your Battle Plan. This weapon counts as both projectile and poison. You may keep this card if you win this battle.",
                [34] = "Weapon - Projectile - Play as part of your Battle Plan. Kills opponent's leader before battle is resolved. Opponent may protect leader with a Shield. You may keep this card if you win the battle.",
                [35] = "Weapon - Poison - Play as part of your Battle Plan. Kills opponent's leader before battle is resolved. Opponent may protect leader with a Snooper. You may keep this card if you win the battle.",
                [36] = "Play as part of your Battle Plan. Counts as a projectile weapon but has the same effect as a projectile defense when played as a defense with another weapon. You may keep this card if you win this battle.",
                [37] = "Play as part of your Battle Plan. Kills both leaders, and is not stopped by a Snooper. After seeing the battle results, you may choose not to use this weapon in which case you don't need to discard it if you win the battle.",
                [38] = "Play as part of your Battle Plan. Counts as both a Shield (projectile defense) and Snooper (poison defense). You may keep this card if you win this battle.",
                [39] = "Play as part of your Battle Plan. Counts as a poison defense but has the same effect as a poison weapon when played as a weapon with another defense. You may keep this card if you win this battle.",
                [40] = "Play as part of your Battle Plan. Kills both leaders (no spice is paid for them). Both players may use Shields to protect their leader against the Artillery Strike. Surviving (shielded) leaders do not count towards the battle total, the side that dialed higher wins the battle. Discard after use.",
                [41] = "Play just after a spice blow comes up. Doubles the Spice blow. Place double the amount of spice in the territory.",
                [42] = "Play at beginning of Spice Blow Phase instead of revealing the Spice Blow card. Causes a Shai-Hulud to appear. Play proceeds as though Shai-Hulud has been revealed.",
                [43] = "At the beginning of any phase, cause all players to discard half of the spice behind their shields, rounded up, to the Spice Bank.",
                [44] = "Worthless Card - Play as part of your Battle Plan, in place of a weapon, defense, or both. This card has no value in play, and you can discard it only by playing it in your Battle plan.",

                [45] = "",
                [46] = "",
                [47] = "",
                [48] = "",
                [49] = "",
                [50] = "",
                [51] = "",
                [52] = "",
                [53] = "",
            },

            TechTokenDescription_STR = new Dictionary<TechToken, string>
            {
                [TechToken.Graveyard] = "At the end of the Revival phase, yields 1 spice per tech token you own if any player except Tleilaxu used free revival.",
                [TechToken.Resources] = "At the end of the Charity phase, yields 1 spice per tech token you own if any player except Bene Gesserit claimed charity.",
                [TechToken.Ships] = "At the end of the Shipment & Move phase, yields 1 spice per tech token you own if any player except Guild shipped forces from off-planet.",
            },

            TreacheryCardImage_URL = new Dictionary<int, string>()
            {
                [0] = DEFAULT_ART_LOCATION + "/art/Lasegun.gif",
                [1] = DEFAULT_ART_LOCATION + "/art/Chrysknife.gif",
                [2] = DEFAULT_ART_LOCATION + "/art/MaulaPistol.gif",
                [3] = DEFAULT_ART_LOCATION + "/art/Slip-Tip.gif",
                [4] = DEFAULT_ART_LOCATION + "/art/Stunner.gif",
                [5] = DEFAULT_ART_LOCATION + "/art/Chaumas.gif",
                [6] = DEFAULT_ART_LOCATION + "/art/Chaumurky.gif",
                [7] = DEFAULT_ART_LOCATION + "/art/EllacaDrug.gif",
                [8] = DEFAULT_ART_LOCATION + "/art/GomJabbar.gif",
                [9] = DEFAULT_ART_LOCATION + "/art/Shield.gif",
                [13] = DEFAULT_ART_LOCATION + "/art/Snooper.gif",
                [17] = DEFAULT_ART_LOCATION + "/art/CheapHero.gif",
                [19] = DEFAULT_ART_LOCATION + "/art/CheapHeroine.gif",
                [20] = DEFAULT_ART_LOCATION + "/art/TleilaxuGhola.gif",
                [21] = DEFAULT_ART_LOCATION + "/art/FamilyAtomics.gif",
                [22] = DEFAULT_ART_LOCATION + "/art/Hajr.gif",
                [23] = DEFAULT_ART_LOCATION + "/art/Karama.gif",
                [25] = DEFAULT_ART_LOCATION + "/art/Truthtrance.gif",
                [27] = DEFAULT_ART_LOCATION + "/art/WeatherControl.gif",
                [28] = DEFAULT_ART_LOCATION + "/art/Baliset.gif",
                [29] = DEFAULT_ART_LOCATION + "/art/JubbaCloak.gif",
                [30] = DEFAULT_ART_LOCATION + "/art/Kulon.gif",
                [31] = DEFAULT_ART_LOCATION + "/art/LaLaLa.gif",
                [32] = DEFAULT_ART_LOCATION + "/art/TripToGamont.gif",

                [33] = DEFAULT_ART_LOCATION + "/art/PoisonBlade.gif",
                [34] = DEFAULT_ART_LOCATION + "/art/Hunter-Seeker.gif",
                [35] = DEFAULT_ART_LOCATION + "/art/BasiliaWeapon.gif",
                [36] = DEFAULT_ART_LOCATION + "/art/WeirdingWay.gif",
                [37] = DEFAULT_ART_LOCATION + "/art/PoisonTooth.gif",
                [38] = DEFAULT_ART_LOCATION + "/art/ShieldSnooper.gif",
                [39] = DEFAULT_ART_LOCATION + "/art/Chemistry.gif",
                [40] = DEFAULT_ART_LOCATION + "/art/ArtilleryStrike.gif",
                [41] = DEFAULT_ART_LOCATION + "/art/Harvester.gif",
                [42] = DEFAULT_ART_LOCATION + "/art/Thumper.gif",
                [43] = DEFAULT_ART_LOCATION + "/art/Amal.gif",
                [44] = DEFAULT_ART_LOCATION + "/art/KullWahad.gif",

                [45] = DEFAULT_ART_LOCATION + "/art/Distrans.gif",
                [46] = DEFAULT_ART_LOCATION + "/art/JuiceOfSapho.gif",
                [47] = DEFAULT_ART_LOCATION + "/art/MirrorWeapon.gif",
                [48] = DEFAULT_ART_LOCATION + "/art/PortableSnooper.gif",
                [49] = DEFAULT_ART_LOCATION + "/art/Ornithopter.gif",
                [50] = DEFAULT_ART_LOCATION + "/art/Nullentropy.gif",
                [51] = DEFAULT_ART_LOCATION + "/art/SemutaDrug.gif",
                [52] = DEFAULT_ART_LOCATION + "/art/ResidualPoison.gif",
                [53] = DEFAULT_ART_LOCATION + "/art/StoneBurner.gif",
            },

            ResourceCardImage_URL = new Dictionary<int, string>()
            {
                [7] = DEFAULT_ART_LOCATION + "/art/CielagoNorth.gif",
                [10] = DEFAULT_ART_LOCATION + "/art/CielagoSouth.gif",
                [15] = DEFAULT_ART_LOCATION + "/art/TheMinorErg.gif",
                [17] = DEFAULT_ART_LOCATION + "/art/RedChasm.gif",
                [18] = DEFAULT_ART_LOCATION + "/art/SouthMesa.gif",
                [22] = DEFAULT_ART_LOCATION + "/art/SihayaRidge.gif",
                [25] = DEFAULT_ART_LOCATION + "/art/OHGap.gif",
                [26] = DEFAULT_ART_LOCATION + "/art/BrokenLand.gif",
                [29] = DEFAULT_ART_LOCATION + "/art/RockOutcroppings.gif",
                [31] = DEFAULT_ART_LOCATION + "/art/HaggaBasin.gif",
                [33] = DEFAULT_ART_LOCATION + "/art/FuneralPlain.gif",
                [34] = DEFAULT_ART_LOCATION + "/art/TheGreatFlat.gif",
                [37] = DEFAULT_ART_LOCATION + "/art/HabbanyaErg.gif",
                [39] = DEFAULT_ART_LOCATION + "/art/WindPassNorth.gif",
                [40] = DEFAULT_ART_LOCATION + "/art/HabbanyaRidgeFlat.gif",
                [98] = DEFAULT_ART_LOCATION + "/art/Shai-Hulud.gif",
                [99] = DEFAULT_ART_LOCATION + "/art/Sandtrout.gif"
            },

            PersonName_STR = new Dictionary<int, string>()
            {
                [1001] = "Thufir Hawat",
                [1002] = "Lady Jessica",
                [1003] = "Gurney Halleck",
                [1004] = "Duncan Idaho",
                [1005] = "Dr. Wellington Yueh",
                [1006] = "Alia",
                [1007] = "Margot Lady Fenring",
                [1008] = "Mother Ramallo",
                [1009] = "Princess Irulan",
                [1010] = "Wanna Marcus",
                [1011] = "Hasimir Fenring",
                [1012] = "Captain Aramsham",
                [1013] = "Caid",
                [1014] = "Burseg",
                [1015] = "Bashar",
                [1016] = "Stilgar",
                [1017] = "Chani",
                [1018] = "Otheym",
                [1019] = "Shadout Mapes",
                [1020] = "Jamis",
                [1021] = "Staban Tuek",
                [1022] = "Master Bewt",
                [1023] = "Esmar Tuek",
                [1024] = "Soo-Soo Sook",
                [1025] = "Guild Rep.",
                [1026] = "Feyd Rautha",
                [1027] = "Beast Rabban",
                [1028] = "Piter de Vries",
                [1029] = "Captain Iakin Nefud",
                [1030] = "Umman Kudu",
                [1031] = "Dominic Vernius",
                [1032] = "C'Tair Pilru",
                [1033] = "Tessia Vernius",
                [1034] = "Kailea Vernius",
                [1035] = "Cammar Pilru",
                [1036] = "Zoal",
                [1037] = "Hidar Fen Ajidica",
                [1038] = "Master Zaaf",
                [1039] = "Wykk",
                [1040] = "Blin",

                [1041] = "Viscount Tull",
                [1042] = "Duke Verdun",
                [1043] = "Rajiv Londine",
                [1044] = "Lady Jalma",
                [1045] = "Frankos Aru",
                [1046] = "Auditor",

                [1047] = "Talis Balt",
                [1048] = "Haloa Rund",
                [1049] = "Flinto Kinnis",
                [1050] = "Lady Helena",
                [1051] = "Premier Ein Calimar",

                [1052] = "Bindikk Narvi",
                [1053] = "Rivvy Dinari",
                [1054] = "Ilesa Ecaz",
                [1055] = "Whitmore Bludd",
                [1056] = "Sanya Ecaz",
                [1057] = "Duke Vidal",

                [1058] = "Vando Terboli",
                [1059] = "Trin Kronos",
                [1060] = "Hiir Resser",
                [1061] = "Grieu Kronos",
                [1062] = "Lupino Ord",
            },

            PersonImage_URL = new Dictionary<int, string>()
            {
                [1001] = DEFAULT_ART_LOCATION + "/art/person0.png",
                [1002] = DEFAULT_ART_LOCATION + "/art/person1.png",
                [1003] = DEFAULT_ART_LOCATION + "/art/person2.png",
                [1004] = DEFAULT_ART_LOCATION + "/art/person3.png",
                [1005] = DEFAULT_ART_LOCATION + "/art/person4.png",
                [1006] = DEFAULT_ART_LOCATION + "/art/person20.png",
                [1007] = DEFAULT_ART_LOCATION + "/art/person21.png",
                [1008] = DEFAULT_ART_LOCATION + "/art/person22.png",
                [1009] = DEFAULT_ART_LOCATION + "/art/person23.png",
                [1010] = DEFAULT_ART_LOCATION + "/art/person24.png",
                [1011] = DEFAULT_ART_LOCATION + "/art/person15.png",
                [1012] = DEFAULT_ART_LOCATION + "/art/person16.png",
                [1013] = DEFAULT_ART_LOCATION + "/art/person17.png",
                [1014] = DEFAULT_ART_LOCATION + "/art/person18.png",
                [1015] = DEFAULT_ART_LOCATION + "/art/person19.png",
                [1016] = DEFAULT_ART_LOCATION + "/art/person10.png",
                [1017] = DEFAULT_ART_LOCATION + "/art/person11.png",
                [1018] = DEFAULT_ART_LOCATION + "/art/person12.png",
                [1019] = DEFAULT_ART_LOCATION + "/art/person13.png",
                [1020] = DEFAULT_ART_LOCATION + "/art/person14.png",
                [1021] = DEFAULT_ART_LOCATION + "/art/person25.png",
                [1022] = DEFAULT_ART_LOCATION + "/art/person26.png",
                [1023] = DEFAULT_ART_LOCATION + "/art/person27.png",
                [1024] = DEFAULT_ART_LOCATION + "/art/person28.png",
                [1025] = DEFAULT_ART_LOCATION + "/art/person29.png",
                [1026] = DEFAULT_ART_LOCATION + "/art/person5.png",
                [1027] = DEFAULT_ART_LOCATION + "/art/person6.png",
                [1028] = DEFAULT_ART_LOCATION + "/art/person7.png",
                [1029] = DEFAULT_ART_LOCATION + "/art/person8.png",
                [1030] = DEFAULT_ART_LOCATION + "/art/person9.png",
                [1031] = DEFAULT_ART_LOCATION + "/art/person30.png",
                [1032] = DEFAULT_ART_LOCATION + "/art/person31.png",
                [1033] = DEFAULT_ART_LOCATION + "/art/person32.png",
                [1034] = DEFAULT_ART_LOCATION + "/art/person33.png",
                [1035] = DEFAULT_ART_LOCATION + "/art/person34.png",
                [1036] = DEFAULT_ART_LOCATION + "/art/person35.png",
                [1037] = DEFAULT_ART_LOCATION + "/art/person36.png",
                [1038] = DEFAULT_ART_LOCATION + "/art/person37.png",
                [1039] = DEFAULT_ART_LOCATION + "/art/person38.png",
                [1040] = DEFAULT_ART_LOCATION + "/art/person39.png",

                [1041] = DEFAULT_ART_LOCATION + "/art/person1041.png",
                [1042] = DEFAULT_ART_LOCATION + "/art/person1042.png",
                [1043] = DEFAULT_ART_LOCATION + "/art/person1043.png",
                [1044] = DEFAULT_ART_LOCATION + "/art/person1044.png",
                [1045] = DEFAULT_ART_LOCATION + "/art/person1045.png",
                [1046] = DEFAULT_ART_LOCATION + "/art/person1046.png",
                [1047] = DEFAULT_ART_LOCATION + "/art/person1047.png",
                [1048] = DEFAULT_ART_LOCATION + "/art/person1048.png",
                [1049] = DEFAULT_ART_LOCATION + "/art/person1049.png",
                [1050] = DEFAULT_ART_LOCATION + "/art/person1050.png",
                [1051] = DEFAULT_ART_LOCATION + "/art/person1051.png",
                [1052] = DEFAULT_ART_LOCATION + "/art/person1052.png",
                [1053] = DEFAULT_ART_LOCATION + "/art/person1053.png",
                [1054] = DEFAULT_ART_LOCATION + "/art/person1054.png",
                [1055] = DEFAULT_ART_LOCATION + "/art/person1055.png",
                [1056] = DEFAULT_ART_LOCATION + "/art/person1056.png",
                [1057] = DEFAULT_ART_LOCATION + "/art/person1057.png",
                [1058] = DEFAULT_ART_LOCATION + "/art/person1058.png",
                [1059] = DEFAULT_ART_LOCATION + "/art/person1059.png",
                [1060] = DEFAULT_ART_LOCATION + "/art/person1060.png",
                [1061] = DEFAULT_ART_LOCATION + "/art/person1061.png",
                [1062] = DEFAULT_ART_LOCATION + "/art/person1062.png",
            },

            TerritoryName_STR = new Dictionary<int, string>()
            {
                [0] = "Polar Sink",
                [1] = "Imperial Basin",
                [2] = "Carthag",
                [3] = "Arrakeen",
                [4] = "Tuek's Sietch",
                [5] = "Sietch Tabr",
                [6] = "Habbanya Sietch",
                [7] = "Cielago North",
                [8] = "Cielago Depression",
                [9] = "Meridian",
                [10] = "Cielago South",
                [11] = "Cielago East",
                [12] = "Harg Pass",
                [13] = "False Wall South",
                [14] = "False Wall East",
                [15] = "The Minor Erg",
                [16] = "Pasty Mesa",
                [17] = "Red Chasm",
                [18] = "South Mesa",
                [19] = "Basin",
                [20] = "Rim Wall West",
                [21] = "Hole In The Rock",
                [22] = "Sihaya Ridge",
                [23] = "Shield Wall",
                [24] = "Gara Kulon",
                [25] = "OH Gap",
                [26] = "Broken Land",
                [27] = "Tsimpo",
                [28] = "Arsunt",
                [29] = "Rock Outcroppings",
                [30] = "Plastic Basin",
                [31] = "Hagga Basin",
                [32] = "Bight Of The Cliff",
                [33] = "Funeral Plain",
                [34] = "The Great Flat",
                [35] = "Wind Pass",
                [36] = "The Greater Flat",
                [37] = "Habbanya Erg",
                [38] = "False Wall West",
                [39] = "Wind Pass North",
                [40] = "Habbanya Ridge Flat",
                [41] = "Cielago West",
                [42] = "Hidden Mobile Stronghold"
            },

            FactionName_STR = new Dictionary<Faction, string>()
            {
                [Faction.None] = "None",
                [Faction.Green] = "Atreides",
                [Faction.Black] = "Harkonnen",
                [Faction.Yellow] = "Fremen",
                [Faction.Red] = "Emperor",
                [Faction.Orange] = "Guild",
                [Faction.Blue] = "Bene Gesserit",
                [Faction.Grey] = "Ixian",
                [Faction.Purple] = "Tleilaxu",

                [Faction.Brown] = "CHOAM",
                [Faction.White] = "Richese",
                [Faction.Pink] = "Ecaz",
                [Faction.Cyan] = "Moritani"
            },

            FactionImage_URL = new Dictionary<Faction, string>()
            {
                [Faction.Green] = DEFAULT_ART_LOCATION + "/art/faction1.svg",
                [Faction.Black] = DEFAULT_ART_LOCATION + "/art/faction2.svg",
                [Faction.Yellow] = DEFAULT_ART_LOCATION + "/art/faction3.svg",
                [Faction.Red] = DEFAULT_ART_LOCATION + "/art/faction4.svg",
                [Faction.Orange] = DEFAULT_ART_LOCATION + "/art/faction5.svg",
                [Faction.Blue] = DEFAULT_ART_LOCATION + "/art/faction6.svg",
                [Faction.Grey] = DEFAULT_ART_LOCATION + "/art/faction7.svg",
                [Faction.Purple] = DEFAULT_ART_LOCATION + "/art/faction8.svg",

                [Faction.Brown] = DEFAULT_ART_LOCATION + "/art/faction9.svg",
                [Faction.White] = DEFAULT_ART_LOCATION + "/art/faction10.svg",
                [Faction.Pink] = DEFAULT_ART_LOCATION + "/art/faction11.svg",
                [Faction.Cyan] = DEFAULT_ART_LOCATION + "/art/faction12.svg"
            },

            FactionTableImage_URL = new Dictionary<Faction, string>()
            {
                [Faction.Green] = DEFAULT_ART_LOCATION + "/art/faction1.svg",
                [Faction.Black] = DEFAULT_ART_LOCATION + "/art/faction2.svg",
                [Faction.Yellow] = DEFAULT_ART_LOCATION + "/art/faction3.svg",
                [Faction.Red] = DEFAULT_ART_LOCATION + "/art/faction4.svg",
                [Faction.Orange] = DEFAULT_ART_LOCATION + "/art/faction5.svg",
                [Faction.Blue] = DEFAULT_ART_LOCATION + "/art/faction6.svg",
                [Faction.Grey] = DEFAULT_ART_LOCATION + "/art/faction7.svg",
                [Faction.Purple] = DEFAULT_ART_LOCATION + "/art/faction8.svg",

                [Faction.Brown] = DEFAULT_ART_LOCATION + "/art/faction9.svg",
                [Faction.White] = DEFAULT_ART_LOCATION + "/art/faction10.svg",
                [Faction.Pink] = DEFAULT_ART_LOCATION + "/art/faction11.svg",
                [Faction.Cyan] = DEFAULT_ART_LOCATION + "/art/faction12.svg"
            },

            FactionFacedownImage_URL = new Dictionary<Faction, string>()
            {
                [Faction.Green] = DEFAULT_ART_LOCATION + "/art/faction1.svg",
                [Faction.Black] = DEFAULT_ART_LOCATION + "/art/faction2.svg",
                [Faction.Yellow] = DEFAULT_ART_LOCATION + "/art/faction3.svg",
                [Faction.Red] = DEFAULT_ART_LOCATION + "/art/faction4.svg",
                [Faction.Orange] = DEFAULT_ART_LOCATION + "/art/faction5.svg",
                [Faction.Blue] = DEFAULT_ART_LOCATION + "/art/faction6.svg",
                [Faction.Grey] = DEFAULT_ART_LOCATION + "/art/faction7.svg",
                [Faction.Purple] = DEFAULT_ART_LOCATION + "/art/faction8.svg",

                [Faction.Brown] = DEFAULT_ART_LOCATION + "/art/faction9.svg",
                [Faction.White] = DEFAULT_ART_LOCATION + "/art/faction10.svg",
                [Faction.Pink] = DEFAULT_ART_LOCATION + "/art/faction11.svg",
                [Faction.Cyan] = DEFAULT_ART_LOCATION + "/art/faction12.svg"
            },

            FactionForceImage_URL = new Dictionary<Faction, string>()
            {
                { Faction.Green, DEFAULT_ART_LOCATION + "/art/faction1force.svg" },
                { Faction.Black, DEFAULT_ART_LOCATION + "/art/faction2force.svg" },
                { Faction.Yellow, DEFAULT_ART_LOCATION + "/art/faction3force.svg" },
                { Faction.Red, DEFAULT_ART_LOCATION + "/art/faction4force.svg" },
                { Faction.Orange, DEFAULT_ART_LOCATION + "/art/faction5force.svg" },
                { Faction.Blue, DEFAULT_ART_LOCATION + "/art/faction6force.svg" },
                { Faction.Grey, DEFAULT_ART_LOCATION + "/art/faction7force.svg" },
                { Faction.Purple, DEFAULT_ART_LOCATION + "/art/faction8force.svg" },

                { Faction.Brown, DEFAULT_ART_LOCATION + "/art/faction9force.png" },
                { Faction.White, DEFAULT_ART_LOCATION + "/art/faction10force.svg" },
                { Faction.Pink, DEFAULT_ART_LOCATION + "/art/faction11force.svg" },
                { Faction.Cyan, DEFAULT_ART_LOCATION + "/art/faction12force.svg" }
            },

            FactionSpecialForceImage_URL = new Dictionary<Faction, string>()
            {
                { Faction.Yellow, DEFAULT_ART_LOCATION + "/art/faction3specialforce.svg" },
                { Faction.Red, DEFAULT_ART_LOCATION + "/art/faction4specialforce.svg" },
                { Faction.Blue, DEFAULT_ART_LOCATION + "/art/faction6specialforce.svg" },
                { Faction.Grey, DEFAULT_ART_LOCATION + "/art/faction7specialforce.svg" },
                { Faction.White, DEFAULT_ART_LOCATION + "/art/faction10specialforce.svg" }
            },

            TechTokenName_STR = new Dictionary<TechToken, string>()
            {
                { TechToken.Graveyard, "Axlotl Tanks" },
                { TechToken.Ships, "Heighliners" },
                { TechToken.Resources, "Spice Production" }
            },

            TechTokenImage_URL = new Dictionary<TechToken, string>()
            {
                { TechToken.Graveyard, DEFAULT_ART_LOCATION + "/art/techtoken0.svg" },
                { TechToken.Ships, DEFAULT_ART_LOCATION + "/art/techtoken1.svg" },
                { TechToken.Resources, DEFAULT_ART_LOCATION + "/art/techtoken2.svg" }
            },

            FactionColorTransparant = new Dictionary<Faction, string>()
            {
                [Faction.None] = "#646464bb",
                [Faction.Green] = "#63842ebb",
                [Faction.Black] = "#2c2c2cbb",
                [Faction.Yellow] = "#d29422bb",
                [Faction.Red] = "#b33715bb",
                [Faction.Orange] = "#c85b20bb",
                [Faction.Blue] = "#385884bb",
                [Faction.Grey] = "#b0b079bb",
                [Faction.Purple] = "#602d8bbb",

                [Faction.Brown] = "#582d1bbb",
                [Faction.White] = "fffdd0bb",
                [Faction.Pink] = "#ac65a9bb",
                [Faction.Cyan] = "#28a4bcbb",

            },

            FactionColor = new Dictionary<Faction, string>()
            {
                [Faction.None] = "#646464",
                [Faction.Green] = "#63842e",
                [Faction.Black] = "#2c2c2c",
                [Faction.Yellow] = "#d29422",
                [Faction.Red] = "#b33715",
                [Faction.Orange] = "#c85b20",
                [Faction.Blue] = "#385884",
                [Faction.Grey] = "#b0b079",
                [Faction.Purple] = "#602d8b",

                [Faction.Brown] = "#582d1b",
                [Faction.White] = "fffdd0",
                [Faction.Pink] = "#ac65a9",
                [Faction.Cyan] = "#28a4bc",
            },

            ForceName_STR = new Dictionary<Faction, string>()
            {
                [Faction.None] = "-",
                [Faction.Green] = "forces",
                [Faction.Black] = "forces",
                [Faction.Yellow] = "forces",
                [Faction.Red] = "forces",
                [Faction.Orange] = "forces",
                [Faction.Blue] = "fighters",
                [Faction.Grey] = "suboids",
                [Faction.Purple] = "forces",

                [Faction.Brown] = "forces",
                [Faction.White] = "forces",
                [Faction.Pink] = "forces",
                [Faction.Cyan] = "forces"
            },

            SpecialForceName_STR = new Dictionary<Faction, string>()
            {
                [Faction.None] = "-",
                [Faction.Green] = "-",
                [Faction.Black] = "-",
                [Faction.Yellow] = "Fedaykin",
                [Faction.Red] = "Sardaukar",
                [Faction.Orange] = "-",
                [Faction.Blue] = "advisors",
                [Faction.Grey] = "cyborgs",
                [Faction.Purple] = "-",

                [Faction.Brown] = "-",
                [Faction.White] = "No-Field",
                [Faction.Pink] = "-",
                [Faction.Cyan] = "-"
            },

            LeaderSkillCardName_STR = new Dictionary<LeaderSkill, string>()
            {
                [LeaderSkill.None] = "",
                [LeaderSkill.Bureaucrat] = "Bureaucrat",
                [LeaderSkill.Diplomat] = "Diplomat",
                [LeaderSkill.Decipherer] = "Rihani Decipherer",
                [LeaderSkill.Smuggler] = "Smuggler",
                [LeaderSkill.Graduate] = "Suk Graduate",
                [LeaderSkill.Planetologist] = "Planetologist",
                [LeaderSkill.Warmaster] = "Warmaster",
                [LeaderSkill.Adept] = "Prana Bindu Adept",
                [LeaderSkill.Swordmaster] = "Swordmaster of Ginaz",
                [LeaderSkill.KillerMedic] = "Killer Medic",
                [LeaderSkill.MasterOfAssassins] = "Master of Assassins",
                [LeaderSkill.Sandmaster] = "Sandmaster",
                [LeaderSkill.Thinker] = "Mentat",
                [LeaderSkill.Banker] = "Spice Banker"
            },

            LeaderSkillCardImage_URL = new Dictionary<LeaderSkill, string>()
            {
                [LeaderSkill.None] = "",
                [LeaderSkill.Bureaucrat] = "Bureaucrat.gif",
                [LeaderSkill.Diplomat] = "Diplomat.gif",
                [LeaderSkill.Decipherer] = "Decipherer.gif",
                [LeaderSkill.Smuggler] = "Smuggler.gif",
                [LeaderSkill.Graduate] = "Graduate.gif",
                [LeaderSkill.Planetologist] = "Planetologist.gif",
                [LeaderSkill.Warmaster] = "Warmaster.gif",
                [LeaderSkill.Adept] = "Adept.gif",
                [LeaderSkill.Swordmaster] = "Swordmaster.gif",
                [LeaderSkill.KillerMedic] = "KillerMedic.gif",
                [LeaderSkill.MasterOfAssassins] = "MasterOfAssassins.gif",
                [LeaderSkill.Sandmaster] = "Sandmaster.gif",
                [LeaderSkill.Thinker] = "Thinker.gif",
                [LeaderSkill.Banker] = "Banker.gif"
            },

            StrongholdCardName_STR = new Dictionary<int, string>()
            {
                [2] = "Carthag",
                [3] = "Arrakeen",
                [4] = "Tuek's Sietch",
                [5] = "Sietch Tabr",
                [6] = "Habbanya Sietch",
                [42] = "Habbanya Sietch"
            },

            StrongholdCardImage_URL = new Dictionary<int, string>()
            {
                [2] = "Carthag.gif",
                [3] = "Arrakeen.gif",
                [4] = "TueksSietch.gif",
                [5] = "SietchTabr.gif",
                [6] = "HabbanyaSietch.gif",
                [42] = "HabbanyaSietch.gif"
            },

            MusicGeneral_URL = DEFAULT_ART_LOCATION + "/art/101_-_Dune_-_DOS_-_Arrakis.mp3",
            MusicResourceBlow_URL = DEFAULT_ART_LOCATION + "/art/104_-_Dune_-_DOS_-_Sequence.mp3",
            MusicSetup_URL = DEFAULT_ART_LOCATION + "/art/109_-_Dune_-_DOS_-_Water.mp3",
            MusicBidding_URL = DEFAULT_ART_LOCATION + "/art/103_-_Dune_-_DOS_-_Baghdad.mp3",
            MusicShipmentAndMove_URL = DEFAULT_ART_LOCATION + "/art/106_-_Dune_-_DOS_-_Wormsuit.mp3",
            MusicBattle_URL = DEFAULT_ART_LOCATION + "/art/105_-_Dune_-_DOS_-_Worm_Intro.mp3",
            MusicBattleClimax_URL = DEFAULT_ART_LOCATION + "/art/108_-_Dune_-_DOS_-_War_Song.mp3",
            MusicMentat_URL = DEFAULT_ART_LOCATION + "/art/102_-_Dune_-_DOS_-_Morning.mp3",

            Sound_YourTurn_URL = DEFAULT_ART_LOCATION + "/art/yourturn.mp3",
            Sound_Chatmessage_URL = DEFAULT_ART_LOCATION + "/art/whisper.mp3",

            Sound = new Dictionary<Milestone, string>()
            {
                [Milestone.GameStarted] = DEFAULT_ART_LOCATION + "/art/intro.mp3",
                [Milestone.Shuffled] = DEFAULT_ART_LOCATION + "/art/shuffleanddeal.mp3",
                [Milestone.BabyMonster] = DEFAULT_ART_LOCATION + "/art/monster.mp3",
                [Milestone.Monster] = DEFAULT_ART_LOCATION + "/art/monster.mp3",
                [Milestone.Resource] = DEFAULT_ART_LOCATION + "/art/resource.mp3",
                [Milestone.MetheorUsed] = DEFAULT_ART_LOCATION + "/art/explosion.mp3",
                [Milestone.CharityClaimed] = DEFAULT_ART_LOCATION + "/art/bid.mp3",
                [Milestone.CardOnBidSwapped] = DEFAULT_ART_LOCATION + "/art/cardflip.mp3",
                [Milestone.Bid] = DEFAULT_ART_LOCATION + "/art/bid.mp3",
                [Milestone.CardWonSwapped] = DEFAULT_ART_LOCATION + "/art/cardflip.mp3",
                [Milestone.AuctionWon] = DEFAULT_ART_LOCATION + "/art/bell.mp3",
                [Milestone.Revival] = DEFAULT_ART_LOCATION + "/art/revival.mp3",
                [Milestone.Shipment] = DEFAULT_ART_LOCATION + "/art/shipment.mp3",
                [Milestone.Move] = DEFAULT_ART_LOCATION + "/art/move.mp3",
                [Milestone.Explosion] = DEFAULT_ART_LOCATION + "/art/explosion.mp3",
                [Milestone.LeaderKilled] = DEFAULT_ART_LOCATION + "/art/scream.mp3",
                [Milestone.Messiah] = "",
                [Milestone.TreacheryCalled] = DEFAULT_ART_LOCATION + "/art/laughter.mp3",
                [Milestone.FaceDanced] = DEFAULT_ART_LOCATION + "/art/laughter.mp3",
                [Milestone.Clairvoyance] = DEFAULT_ART_LOCATION + "/art/clairvoyance.mp3",
                [Milestone.Karma] = DEFAULT_ART_LOCATION + "/art/karma.mp3",
                [Milestone.Bribe] = DEFAULT_ART_LOCATION + "/art/bribe.mp3",
                [Milestone.GameWon] = DEFAULT_ART_LOCATION + "/art/win.mp3",
                [Milestone.Amal] = DEFAULT_ART_LOCATION + "/art/flute.mp3",
                [Milestone.Thumper] = DEFAULT_ART_LOCATION + "/art/thumping.mp3",
                [Milestone.Harvester] = DEFAULT_ART_LOCATION + "/art/engine.mp3",
                [Milestone.HmsMovement] = DEFAULT_ART_LOCATION + "/art/hms.mp3",
                [Milestone.RaiseDead] = DEFAULT_ART_LOCATION + "/art/bubbles.mp3",
                [Milestone.WeatherControlled] = DEFAULT_ART_LOCATION + "/art/thunder.mp3",
                [Milestone.Storm] = DEFAULT_ART_LOCATION + "/art/thunder.mp3",
                [Milestone.Voice] = DEFAULT_ART_LOCATION + "/art/voice.mp3",
                [Milestone.Prescience] = DEFAULT_ART_LOCATION + "/art/clairvoyance.mp3",

                [Milestone.ResourcesReceived] = DEFAULT_ART_LOCATION + "/art/bid.mp3",
                [Milestone.Economics] = DEFAULT_ART_LOCATION + "/art/bribe.mp3",
                [Milestone.CardTraded] = DEFAULT_ART_LOCATION + "/art/cardflip.mp3",
                [Milestone.Discard] = DEFAULT_ART_LOCATION + "/art/cardflip.mp3",
                [Milestone.SpecialUselessPlayed] = DEFAULT_ART_LOCATION + "/art/karma.mp3",
            },

            MapDimensions = new Point(4145, 4601),
            PlanetRadius = 1790,
            MapRadius = 1940,
            PlanetCenter = new Point(2070, 2287),
            PlayerTokenRadius = 95,

            SpiceDeckLocation = new Point(10, 4050),
            TreacheryDeckLocation = new Point(3300, 4050),

            BattleScreenWidth = 2037,
            BattleScreenHeight = 2037,

            BattleScreenHeroX = (int)(2.0329 * 173),
            BattleScreenHeroY = (int)(2.0329 * 551),
            BattleWheelHeroWidth = (int)(2.0329 * 317),
            BattleWheelHeroHeight = (int)(2.0329 * 317),

            BattleWheelForcesX = (int)(2.0329 * 505),
            BattleWheelForcesY = (int)(2.0329 * 100),

            BattleWheelCardX = (int)(2.0329 * 545),
            BattleWheelCardY = (int)(2.0329 * 375),
            BattleWheelCardWidth = (int)(2.0329 * 316),
            BattleWheelCardHeight = (int)(2.0329 * 440),

            //Monster token
            MONSTERTOKEN_RADIUS = 100,

            //Force tokens
            FORCETOKEN_FONT = "normal normal bolder 85px Calibri, Tahoma, sans-serif",
            FORCETOKEN_FONTCOLOR = "white",
            FORCETOKEN_SPECIAL_FONTCOLOR = "gold",
            FORCETOKEN_FONT_BORDERCOLOR = "black",
            FORCETOKEN_SPECIAL_BORDERCOLOR = "gold",
            FORCETOKEN_FONT_BORDERWIDTH = 3,
            FORCETOKEN_BORDERCOLOR = "white",
            FORCETOKEN_BORDERWIDTH = 5,
            FORCETOKEN_SPECIAL_BORDERWIDTH = 10,
            FORCETOKEN_RADIUS = 60,

            //Spice tokens
            RESOURCETOKEN_FONT = "normal normal bolder 85px Calibri, Tahoma, sans-serif",
            RESOURCETOKEN_FONTCOLOR = "white",
            RESOURCETOKEN_FONT_BORDERCOLOR = "black",
            RESOURCETOKEN_FONT_BORDERWIDTH = 3,
            RESOURCETOKEN_COLOR = "rgba(255,140,60,0.9)",
            RESOURCETOKEN_BORDERCOLOR = "white",
            RESOURCETOKEN_RADIUS = 80,

            //Other highlights
            HIGHLIGHT_OVERLAY_COLOR = "rgba(255,255,255,0.5)",
            METHEOR_OVERLAY_COLOR = "rgba(209,247,137,0.5)",
            BLOWNSHIELDWALL_OVERLAY_COLOR = "rgba(137,238,247,0.5)",
            STORM_OVERLAY_COLOR = "rgba(255,100,100,0.5)",
            STORM_PRESCIENCE_OVERLAY_COLOR = "rgba(255,100,100,0.2)",

            //Card piles
            CARDPILE_FONT = "normal normal normal 140px Advokat, Calibri, Tahoma, sans-serif",
            CARDPILE_FONTCOLOR = "white",
            CARDPILE_FONT_BORDERCOLOR = "black",
            CARDPILE_FONT_BORDERWIDTH = 3,

            //Phases
            PHASE_FONT = "normal normal bold 90px Advokat, Calibri, Tahoma, sans-serif",
            PHASE_ACTIVE_FONT = "normal normal normal 130px Advokat, Calibri, Tahoma, sans-serif",
            PHASE_FONTCOLOR = "white",
            PHASE_ACTIVE_FONTCOLOR = "rgb(231,191,60)",
            PHASE_FONT_BORDERCOLOR = "black",
            PHASE_FONT_BORDERWIDTH = 3,
            PHASE_ACTIVE_FONT_BORDERWIDTH = 3,

            //Player names
            PLAYERNAME_FONT = "normal normal bold 95px Advokat, Calibri, Tahoma, sans-serif",
            PLAYERNAME_FONTCOLOR = "white",
            PLAYERNAME_FONT_BORDERCOLOR = "black",
            PLAYERNAME_FONT_BORDERWIDTH = 3,

            TABLEPOSITION_BACKGROUNDCOLOR = "rgb(231,191,60)",

            //Turns
            TURN_FONT = "normal normal normal 130px Advokat, Calibri, Tahoma, sans-serif",
            TURN_FONT_COLOR = "white",
            TURN_FONT_BORDERCOLOR = "black",
            TURN_FONT_BORDERWIDTH = 3,

            //Wheel
            WHEEL_FONT = "normal normal normal 200px Advokat, Calibri, Tahoma, sans-serif",
            WHEEL_FONTCOLOR = "black",
            WHEEL_FONT_AGGRESSOR_BORDERCOLOR = "white",
            WHEEL_FONT_DEFENDER_BORDERCOLOR = "white",
            WHEEL_FONT_BORDERWIDTH = 6,

            //Shadows
            SHADOW_DARK = "black",
            SHADOW_LIGHT = "#505050",

            //General
            GAMEVERSION_FONT = "normal normal normal 16px Advokat, Calibri, Tahoma, sans-serif;",
            PLAYEDCARD_MESSAGE_FONT = "normal normal normal 16px Calibri, Tahoma, sans-serif",
            FACTION_INFORMATIONCARDSTYLE = "font: normal normal normal 14px Calibri, Tahoma, sans-serif; color: white; padding: 5px 5px 5px 5px; overflow: auto; line-height: 95%; background-color: rgba(32,32,32,0.95); border-color: grey; border-style: solid; border-width: 1px; border-radius: 3px;",
            TRACKER_FONT = "normal normal normal 12px Calibri, Tahoma, sans-serif;",
            JSPANEL_DEFAULTSTYLE = "font-family: Calibri, Tahoma, sans-serif"

        };

        public static Skin Current = Dune1979;

    }
}
