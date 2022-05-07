/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Treachery.Shared;

namespace Treachery.Client
{
    public class Skin : IDescriber
    {
        #region Attributes

        public const int CurrentVersion = 138;
        public const int MINIMUM_SUPPORTED_VERSION = 23;

        public const string DEFAULT_ART_LOCATION = "https://treachery.online"; //Used for debugging
        //public const string DEFAULT_ART_LOCATION = ".";

        public string Description = null;
        public int Version;

        public bool DrawResourceIconsOnMap = false;
        public bool ShowVerboseToolipsOnMap = true;
        public bool ShowArrowsForRecentMoves = true;

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
        public Dictionary<int, string> TerritoryBorder_SVG;
        public Dictionary<int, Point> LocationCenter_Point;
        public Dictionary<int, Point> LocationSpice_Point;
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

        public Point MapDimensions = new();
        public Point PlanetCenter = new();
        public int PlanetRadius = -1;
        public int MapRadius = -1;
        public int PlayerTokenRadius = -1;

        public Point SpiceDeckLocation = new(0, 0);
        public Point TreacheryDeckLocation = new(0, 0);
        public Point CardSize = new(40, 56);

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
        public string FORCETOKEN_FONT;
        public string FORCETOKEN_FONTCOLOR;
        public string FORCETOKEN_SPECIAL_FONTCOLOR;
        public string FORCETOKEN_FONT_BORDERCOLOR;
        public int FORCETOKEN_FONT_BORDERWIDTH;
        public string FORCETOKEN_SPECIAL_BORDERCOLOR;
        public string FORCETOKEN_BORDERCOLOR;
        public int FORCETOKEN_BORDERWIDTH;
        public int FORCETOKEN_SPECIAL_BORDERWIDTH;
        public int FORCETOKEN_RADIUS;

        //Spice tokens
        public string RESOURCETOKEN_FONT;
        public string RESOURCETOKEN_FONTCOLOR;
        public string RESOURCETOKEN_FONT_BORDERCOLOR;
        public int RESOURCETOKEN_FONT_BORDERWIDTH;
        public string RESOURCETOKEN_COLOR;
        public string RESOURCETOKEN_BORDERCOLOR;
        public int RESOURCETOKEN_RADIUS;

        //Other highlights
        public string HIGHLIGHT_OVERLAY_COLOR;
        public string METHEOR_OVERLAY_COLOR;
        public string BLOWNSHIELDWALL_OVERLAY_COLOR;
        public string STORM_OVERLAY_COLOR;
        public string STORM_PRESCIENCE_OVERLAY_COLOR;

        //Card piles
        public string CARDPILE_FONT;
        public string CARDPILE_FONTCOLOR;
        public string CARDPILE_FONT_BORDERCOLOR;
        public int CARDPILE_FONT_BORDERWIDTH;

        //Phases
        public string PHASE_FONT;
        public string PHASE_ACTIVE_FONT;
        public string PHASE_FONTCOLOR;
        public string PHASE_ACTIVE_FONTCOLOR;
        public string PHASE_FONT_BORDERCOLOR;
        public int PHASE_FONT_BORDERWIDTH;
        public int PHASE_ACTIVE_FONT_BORDERWIDTH;

        //Player names
        public string PLAYERNAME_FONT;
        public string PLAYERNAME_FONTCOLOR;
        public string PLAYERNAME_FONT_BORDERCOLOR;
        public int PLAYERNAME_FONT_BORDERWIDTH;

        //Skill names
        public string SKILL_FONT;
        public string SKILL_FONTCOLOR;
        public string SKILL_FONT_BORDERCOLOR;
        public int SKILL_FONT_BORDERWIDTH;

        //Player positions
        public string TABLEPOSITION_BACKGROUNDCOLOR;

        //Turns
        public string TURN_FONT;
        public string TURN_FONT_COLOR;
        public string TURN_FONT_BORDERCOLOR;
        public int TURN_FONT_BORDERWIDTH;

        //Wheel
        public string WHEEL_FONT;
        public string WHEEL_FONTCOLOR;
        public string WHEEL_FONT_AGGRESSOR_BORDERCOLOR;
        public string WHEEL_FONT_DEFENDER_BORDERCOLOR;
        public int WHEEL_FONT_BORDERWIDTH;

        //Shadows
        public string SHADOW = "black";

        //General
        public string GAMEVERSION_FONT;
        public string PLAYEDCARD_MESSAGE_FONT;
        public string FACTION_INFORMATIONCARDSTYLE;
        public string TRACKER_FONT;
        public string JSPANEL_DEFAULTSTYLE;

        #endregion Attributes

        #region Descriptions

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

        public Point GetCenter(Location location)
        {
            if (location is HiddenMobileStronghold hms)
            {
                if (hms.AttachedToLocation != null)
                {
                    var attachedToCenter = GetCenter(hms.AttachedToLocation);
                    return new Point(attachedToCenter.X + HmsDX, attachedToCenter.Y);
                }
                else
                {
                    return new Point(HmsDX, HmsDX);
                }
            }
            else
            {
                return LocationCenter_Point[location.Id];
            }
        }

        public int HmsDX => -4 * PlayerTokenRadius;

        public int HmsRadius => 2 * PlayerTokenRadius;

        public Point GetSpiceLocation(Location location) => location.SpiceBlowAmount != 0 ? LocationSpice_Point[location.Id] : new Point(0, 0);


        public string Describe(object value, bool capitalize = false)
        {
            string result;

            if (value == null) return "";

            result = (value) switch
            {
                string str => str,
                bool b => b ? "Yes" : "No",
                Message msg => Describe(msg),
                MessagePart part => Describe(part),
                Payment payment => Format("{0} {1}", payment.Amount, Concept.Resource),
                Concept c => Describe(c),
                Faction faction => Describe(faction),
                FactionForce ff => Describe(ff),
                FactionSpecialForce fsf => Describe(fsf),
                FactionAdvantage factionadvantage => Describe(factionadvantage),
                ResourceCard rc => Describe(rc),
                TreacheryCard tc => Describe(tc),
                TreacheryCardType tct => Describe(tct),
                LeaderSkill ls => Describe(ls),
                Ruleset r => Describe(r),
                RuleGroup rg => Describe(rg),
                Rule rule => Describe(rule),
                MainPhase m => Describe(m),
                TechToken tt => Describe(tt),
                Territory t => Describe(t),
                Location l => Describe(l),
                IHero hero => Describe(hero),
                WinMethod w => Describe(w),
                Phase p => Describe(p),
                BrownEconomicsStatus bes => Describe(bes),
                AuctionType at => Describe(at),
                JuiceType jt => Describe(jt),
                CaptureDecision cd => Describe(cd),
                StrongholdAdvantage sa => Describe(sa),
                IEnumerable ienum => Join(Enumerable.Cast<object>(ienum)),
                _ => value.ToString()
            };

            if (capitalize)
            {
                return FirstCharToUpper(result);
            }
            else
            {
                return result;
            }
        }

        public string Describe(object obj)
        {
            return Describe(obj, false);
        }

        private string[] Describe(object[] objects)
        {
            var result = new string[objects.Length];
            for (int i = 0; i < objects.Length; i++)
            {
                result[i] = Describe(objects[i]);
            }
            return result;
        }

        private static string FirstCharToUpper(string input)
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

        public string Describe(Territory t)
        {
            return GetLabel(TerritoryName_STR, t.SkinId);
        }

        public string Describe(IHero hero)
        {
            if (hero == null)
            {
                return "?";
            }
            else if (hero is Leader l)
            {
                return GetLabel(PersonName_STR, l.SkinId);
            }
            else if (hero is Messiah)
            {
                return Describe(Concept.Messiah);
            }
            else if (hero is TreacheryCard tc)
            {
                return Describe(tc);
            }
            else
            {
                return "?";
            }
        }

        public string Describe(Location l)
        {
            if (l.Orientation != "")
            {
                return Format("{0} ({1} Sector)", l.Territory, l.Orientation);
            }
            else
            {
                return Describe(l.Territory);
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

        public string Describe(ResourceCard c)
        {
            if (c == null)
            {
                return "?";
            }
            else if (c.IsShaiHulud)
            {
                return Describe(Concept.Monster);
            }
            else if (c.IsSandTrout)
            {
                return Describe(Concept.BabyMonster);
            }
            else
            {
                return Describe(c.Location.Territory);
            }
        }

        public string Describe(TreacheryCard c)
        {
            return GetLabel(TreacheryCardName_STR, c.SkinId);
        }

        public string Describe(LeaderSkill l)
        {
            return LeaderSkillCardName_STR[l];
        }

        public string Describe(MainPhase p)
        {
            return MainPhase_STR[p];
        }

        public string Describe(TechToken tt)
        {
            return TechTokenName_STR[tt];
        }

        private static Faction GetFaction(FactionForce f)
        {
            return f switch
            {
                FactionForce.Green => Faction.Green,
                FactionForce.Black => Faction.Black,
                FactionForce.Yellow => Faction.Yellow,
                FactionForce.Red => Faction.Red,
                FactionForce.Orange => Faction.Orange,
                FactionForce.Blue => Faction.Blue,
                FactionForce.Grey => Faction.Grey,
                FactionForce.Purple => Faction.Purple,
                FactionForce.Brown => Faction.Brown,
                FactionForce.White => Faction.White,
                _ => Faction.None,
            };
        }

        public string Describe(FactionForce f) => ForceName_STR[GetFaction(f)];

        private static Faction GetFaction(FactionSpecialForce f)
        {
            return f switch
            {
                FactionSpecialForce.Red => Faction.Red,
                FactionSpecialForce.Yellow => Faction.Yellow,
                FactionSpecialForce.Blue => Faction.Blue,
                FactionSpecialForce.Grey => Faction.Grey,
                FactionSpecialForce.White => Faction.White,
                _ => Faction.None,
            };
        }

        public string Describe(FactionSpecialForce f) => SpecialForceName_STR[GetFaction(f)];

        public string Describe(Message msg)
        {
            return msg.ToString(this);
        }

        public string Describe(MessagePart part)
        {
            return part.ToString(this);
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
                FactionAdvantage.BlueAccompanies => Format("{0} accompanying shipment", Faction.Blue),
                FactionAdvantage.BlueAnnouncesBattle => Format("{0} flipping advisors to fighters*", Faction.Blue),
                FactionAdvantage.BlueIntrusion => Format("{0} becoming advisors on intrusion", Faction.Blue),
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
                FactionAdvantage.RedLetAllyReviveExtraForces => Format("{0} allowing their ally to revive 3 extra forces", Faction.Red, Concept.Resource),
                FactionAdvantage.OrangeDetermineMoveMoment => Format("{0} shipping out of turn order", Faction.Orange),
                FactionAdvantage.OrangeSpecialShipments => Format("{0} (and ally) shipping site-to-site or back to reserves", Faction.Orange),
                FactionAdvantage.OrangeShipmentsDiscount => Format("{0} shipping at half price", Faction.Orange),
                FactionAdvantage.OrangeShipmentsDiscountAlly => Format("{0} ally shipping at half price", Faction.Orange),
                FactionAdvantage.OrangeReceiveShipment => Format("{0} receiving {1} for a shipment", Faction.Orange, Concept.Resource),
                FactionAdvantage.PurpleRevivalDiscount => Format("{0} reviving at half price", Faction.Purple),
                FactionAdvantage.PurpleRevivalDiscountAlly => Format("{0} ally reviving at half price", Faction.Purple),
                FactionAdvantage.PurpleReplacingFaceDancer => Format("{0} replacing a face dancer at end of turn", Faction.Purple),
                FactionAdvantage.PurpleIncreasingRevivalLimits => Format("{0} increasing revival limits", Faction.Purple),
                FactionAdvantage.PurpleReceiveRevive => Format("{0} receiving {1} for a revival", Faction.Purple, Concept.Resource),
                FactionAdvantage.PurpleEarlyLeaderRevive => Format("{0} allowing early revival of a leader*", Faction.Purple),
                FactionAdvantage.PurpleReviveGhola => Format("{0} reviving a leader as a Ghola*", Faction.Purple),
                FactionAdvantage.GreyMovingHMS => Format("{0} moving the Hidden Mobile Stronghold", Faction.Grey),
                FactionAdvantage.GreySpecialForceBonus => Format("{0} counting {1} bonus in one battle", Faction.Grey, FactionSpecialForce.Grey),
                FactionAdvantage.GreySelectingCardsOnAuction => Format("{0} selecting the cards on auction", Faction.Grey),
                FactionAdvantage.GreyCyborgExtraMove => Format("{0} moving {1} two territories", Faction.Grey, FactionSpecialForce.Grey),
                FactionAdvantage.GreyReplacingSpecialForces => Format("{0} replacing {1} lost in battle with {2}", Faction.Grey, FactionSpecialForce.Grey, FactionForce.Grey),
                FactionAdvantage.GreyAllyDiscardingCard => Format("{0} allowing their ally to replace a won card", Faction.Grey),
                FactionAdvantage.GreySwappingCard => Format("{0} replacing a treachery card during bidding", Faction.Grey),
                FactionAdvantage.BrownControllingCharity => Format("{0} receiving and giving {1} during {2}", Faction.Brown, Concept.Resource, MainPhase.Charity),
                FactionAdvantage.BrownDiscarding => Format("{0} discarding cards for {1} or a {2} card for its special effect*", Faction.Brown, Concept.Resource, TreacheryCardType.Useless),
                FactionAdvantage.BrownRevival => Format("{0} having unlimited force revival and reduced revival cost", Faction.Brown),
                FactionAdvantage.BrownEconomics => Format("{0} playing their Inflation token during {1}", Faction.Brown, MainPhase.Contemplate),
                FactionAdvantage.BrownReceiveForcePayment => Format("{0} collecting {1} payment for forces for one battle", Faction.Brown, Concept.Resource),
                FactionAdvantage.BrownAudit => Format("{0} auditing their opponent after a battle", Faction.Brown),
                FactionAdvantage.WhiteAuction => Format("{0} auctioning a card from their card cache", Faction.White),
                FactionAdvantage.WhiteNofield => Format("{0} using a No-Field to ship", Faction.White),
                FactionAdvantage.WhiteBlackMarket => Format("{0} selling a card from their hand", Faction.White),

                _ => "Unknown",
            };
        }


        public static string Describe(Ruleset s)
        {
            return s switch
            {
                Ruleset.BasicGame => "Standard Dune - Basic",
                Ruleset.AdvancedGame => "Standard Dune - Advanced",
                Ruleset.ExpansionBasicGame => "Ixians & Tleilaxu Expansion - Basic",
                Ruleset.ExpansionAdvancedGame => "Ixians & Tleilaxu Expansion - Advanced",
                Ruleset.Expansion2BasicGame => "CHOAM & Richese Expansion - Basic",
                Ruleset.Expansion2AdvancedGame => "CHOAM & Richese Expansion - Advanced",
                Ruleset.AllExpansionsBasicGame => "Both Expansions - Basic",
                Ruleset.AllExpansionsAdvancedGame => "Both Expansions - Advanced",
                Ruleset.ServerClassic => "Server Classic",
                Ruleset.Custom => "Custom",

                _ => "unknown rule set",
            };
        }

        public static string Describe(RuleGroup s)
        {
            return s switch
            {
                RuleGroup.CoreAdvanced => "Core Game, Advanced Rules",
                RuleGroup.CoreBasicExceptions => "Core Game, Exceptions to Basic Rules",
                RuleGroup.CoreAdvancedExceptions => "Core Game, Exceptions to Advanced Rules",

                RuleGroup.ExpansionIxAndBtBasic => "Ixians & Tleilaxu Expansion",
                RuleGroup.ExpansionIxAndBtAdvanced => "Ixians & Tleilaxu Expansion, Advanced Rules",

                RuleGroup.ExpansionBrownAndWhiteBasic => "CHOAM & Richese Expansion",
                RuleGroup.ExpansionBrownAndWhiteAdvanced => "CHOAM & Richese Expansion, Advanced Rules",

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

        public static string Describe(Phase p)
        {
            return p switch
            {
                Phase.TurnConcluded => "End of turn",
                Phase.Bidding => "Next bidding round",
                Phase.BiddingReport => "End of bidding phase",
                Phase.ShipmentAndMoveConcluded => "End of movement phase",
                Phase.BattleReport => "End of battle phase",
                Phase.GameEnded => "End of game",
                _ => "unknown"
            };
        }

        public static string Describe(BrownEconomicsStatus p)
        {
            return p switch
            {
                BrownEconomicsStatus.Cancel => "Cancel",
                BrownEconomicsStatus.CancelFlipped => "Cancel",
                BrownEconomicsStatus.Double => "Double",
                BrownEconomicsStatus.DoubleFlipped => "Double",
                BrownEconomicsStatus.RemovedFromGame => "Removed from game",
                _ => "None"
            };
        }

        public static string Describe(AuctionType t)
        {
            return t switch
            {
                AuctionType.Normal => "Normal",
                AuctionType.BlackMarketNormal => "Normal",
                AuctionType.BlackMarketSilent => "Silent",
                AuctionType.BlackMarketOnceAround => "Once Around",
                AuctionType.WhiteSilent => "Silent",
                AuctionType.WhiteOnceAround => "Once Around",
                _ => "None"
            };
        }

        public string DescribeDetailed(AuctionType t)
        {
            return t switch
            {
                AuctionType.Normal => "Normal",
                AuctionType.BlackMarketNormal => "Black Market / Normal",
                AuctionType.BlackMarketSilent => "Black Market / Silent",
                AuctionType.BlackMarketOnceAround => "Black Market / Once Around",
                AuctionType.WhiteSilent => Format("{0} / Silent", Faction.White),
                AuctionType.WhiteOnceAround => Format("{0} / Once Around", Faction.White),
                _ => "None"
            };
        }

        public static string Describe(JuiceType jt)
        {
            return jt switch
            {
                JuiceType.GoFirst => "be considered first in storm order",
                JuiceType.GoLast => "be considered last in storm order",
                JuiceType.Aggressor => "be considered aggressor in this battle",
                _ => "None"
            };
        }

        public static string Describe(CaptureDecision c)
        {
            return c switch
            {
                CaptureDecision.Capture => "Keep",
                CaptureDecision.Kill => "Kill to gain 2 spice",
                CaptureDecision.DontCapture => "Neither keep nor kill",
                _ => "None"
            };
        }

        public string Describe(StrongholdAdvantage sa)
        {
            return sa switch
            {
                StrongholdAdvantage.CollectResourcesForDial => StrongholdCardName_STR[5],
                StrongholdAdvantage.CollectResourcesForUseless => StrongholdCardName_STR[4],
                StrongholdAdvantage.CountDefensesAsAntidote => StrongholdCardName_STR[2],
                StrongholdAdvantage.FreeResourcesForBattles => StrongholdCardName_STR[3],
                StrongholdAdvantage.WinTies => StrongholdCardName_STR[6],
                _ => "None"
            };
        }

        public string Describe(Rule r)
        {
            return r switch
            {
                Rule.BasicTreacheryCards => "Basic Treachery Cards",
                Rule.HasCharityPhase => "Charity Phase",



                Rule.AdvancedCombat => "Advanced Combat",
                Rule.IncreasedResourceFlow => "Increased Spice Flow",
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

                Rule.TechTokens => "Tech Tokens",
                Rule.ExpansionTreacheryCards => "Expansion Treachery Cards",
                Rule.ExpansionTreacheryCardsExceptPBandSSandAmal => Format("Treachery Cards: all except {0}, {1} and {2}", TreacheryCardType.ProjectileAndPoison, TreacheryCardType.ShieldAndAntidote, TreacheryCardType.Amal),
                Rule.ExpansionTreacheryCardsPBandSS => Format("Treachery Cards: {0} and {1}", TreacheryCardType.ProjectileAndPoison, TreacheryCardType.ShieldAndAntidote),
                Rule.ExpansionTreacheryCardsAmal => Format("Treachery Card: {0}", TreacheryCardType.Amal),
                Rule.CheapHeroTraitor => "Cheap Hero Traitor",
                Rule.SandTrout => Describe(Concept.BabyMonster),
                Rule.PurpleGholas => Format("{0} may revive leaders as Gholas", Faction.Purple),
                Rule.GreySwappingCardOnBid => Format("{0} may swap one card on bid with a card from their hand", Faction.Grey),

                Rule.LeaderSkills => "Leader Skills",
                Rule.Expansion2TreacheryCards => Format("Treachery Cards: {0} and {1}", TreacheryCardType.ArtilleryStrike, TreacheryCardType.PoisonTooth),
                Rule.StrongholdBonus => "Stronghold Bonus",
                Rule.BrownAuditor => Format("{0} gains the Auditor leader", Faction.Brown),
                Rule.WhiteBlackMarket => Format("{0} Black Market bidding", Faction.White),

                Rule.CustomDecks => "Customized Treachery Card Deck",
                Rule.ExtraKaramaCards => Format("Add three extra {0} cards to the game", TreacheryCardType.Karma),
                Rule.FullPhaseKarma => Format("Full phase {0} (instead of single instance)", TreacheryCardType.Karma),
                Rule.YellowMayMoveIntoStorm => Format("{0} may move into storm", Faction.Yellow),
                Rule.BlueVoiceMustNameSpecialCards => "Voice must target special cards by name",
                Rule.BattlesUnderStorm => "Battles may happen under the storm",
                Rule.MovementBonusRequiresOccupationBeforeMovement => "Arrakeen/Carthag must be occupied before Ship&Move to grant ornithopters",
                Rule.AssistedNotekeeping => "Mentat: auto notekeeping of knowable info (spice owned, cards seen, ...)",
                Rule.ResourceBonusForStrongholds => "Stronghold spice bonus (even without increased spice flow)",

                Rule.FillWithBots => "Fill empty seats with random Bots",
                Rule.OrangeBot => Format("{0}Bot", Faction.Orange),
                Rule.RedBot => Format("{0}Bot", Faction.Red),
                Rule.BlackBot => Format("{0}Bot", Faction.Black),
                Rule.PurpleBot => Format("{0}Bot", Faction.Purple),
                Rule.GreenBot => Format("{0}Bot", Faction.Green),
                Rule.BlueBot => Format("{0}Bot", Faction.Blue),
                Rule.YellowBot => Format("{0}Bot", Faction.Yellow),
                Rule.GreyBot => Format("{0}Bot", Faction.Grey),
                //Rule.BrownBot => Format("{0}Bot", Faction.Brown),
                //Rule.WhiteBot => Format("{0}Bot", Faction.White),
                Rule.BotsCannotAlly => "Bots may not initiate alliances",
                Rule.CardsCanBeTraded => Format("Allow players to give cards to each other"),
                Rule.PlayersChooseFactions => Format("Let players choose their factions at start"),
                Rule.RedSupportingNonAllyBids => Format("{0} may support bids of non-ally players", Faction.Red),
                Rule.BattleWithoutLeader => "Allow leaderless battles even if leaders are available",
                Rule.CapturedLeadersAreTraitorsToOwnFaction => "Captured leaders can be called as traitors by their original factions",
                Rule.DisableEndOfGameReport => "Disable end-of-game report (don't reveal player shields)",
                Rule.DisableOrangeSpecialVictory => Format("Disable {0} special victory condition", Faction.Orange),
                Rule.DisableResourceTransfers => Format("Only allow transfer of {0} by alliance rules", Concept.Resource),
                _ => "unknown rule",
            };
        }





        #endregion Descriptions

        #region NamesAndImages

        public string GetTerritoryBorder(Territory t)
        {
            return GetLabel(TerritoryBorder_SVG, t.SkinId);
        }

        public string GetImageURL(TreacheryCard c)
        {
            return GetURL(TreacheryCardImage_URL, c.SkinId);
        }

        public string GetImageURL(Location stronghold)
        {
            return GetURL(StrongholdCardImage_URL, stronghold.Territory.SkinId);
        }

        public string GetImageURL(ResourceCard c)
        {
            return GetURL(ResourceCardImage_URL, c.SkinId);
        }

        public string GetImageURL(FactionForce ff)
        {
            return GetFactionForceImageURL(GetFaction(ff));
        }

        public string GetImageURL(FactionSpecialForce fsf)
        {
            return GetFactionSpecialForceImageURL(GetFaction(fsf));
        }


        public string GetImageURL(LeaderSkill s)
        {
            return GetURL(LeaderSkillCardImage_URL, s);
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

        private static string GetLabel<T>(Dictionary<T, string> labels, T key)
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

        private static string GetURL<T>(Dictionary<T, string> labels, T key)
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
                g.Map.HiddenMobileStronghold, // 34
                Faction.White, //35
                Faction.Brown, //36
                TreacheryCardName_STR[TreacheryCardManager.Lookup.Find(TreacheryCardManager.CARD_BALISET).SkinId], //37
                TreacheryCardName_STR[TreacheryCardManager.Lookup.Find(TreacheryCardManager.CARD_JUBBACLOAK).SkinId], //38
                TreacheryCardName_STR[TreacheryCardManager.Lookup.Find(TreacheryCardManager.CARD_KULLWAHAD).SkinId], //39
                TreacheryCardName_STR[TreacheryCardManager.Lookup.Find(TreacheryCardManager.CARD_KULON).SkinId], //40
                TreacheryCardName_STR[TreacheryCardManager.Lookup.Find(TreacheryCardManager.CARD_LALALA).SkinId], //41
                TreacheryCardName_STR[TreacheryCardManager.Lookup.Find(TreacheryCardManager.CARD_TRIPTOGAMONT).SkinId], //42
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
                Faction.Brown => Format(GetBrownTemplate(g), parameters),
                Faction.White => Format(GetWhiteTemplate(g), parameters),
                _ => "",
            };
        }

        private static string GetGreenTemplate(Game g)
        {
            return
              @"<div style='{25}'>
                <p><strong>At start:</strong> 10 tokens in {6} and 10 in reserve (off-planet). Start with 10 {16}.</p>
                <p><strong>Free revival:</strong> 2.</p>
                <h5>Basic Advantages</h5>
                <p>You have limited prescience.</p>
                <p>During Bidding you see the treachery card on bid.</p>
                <p>During Shipment & Move you see the top card of the {16} deck.</p>
                <p>During Battle, you may force your opponent to tell you your choice of one of the four elements he will use in his battle plan against you; the leader, the weapon, the defense or the number dialed. If your opponent tells you that he is not playing a weapon or defense, you may not ask something else.</p>" +

              (g.Applicable(Rule.GreenMessiah) ?
              @"<h5>Advanced Advantages</h5>
                <p>After losing a at least 7 forces in battle(s), you may use the {20}. It cannot be used alone in battle but may add its +2 strength to any one leader or {13} per turn. If the leader or {13} is killed, the {20} has no effect in the battle. {20} can only be killed if blown up by a {18}/{17} explosion. A leader accompanied by {20} cannot turn traitor. If killed, the {20} must be revived like any other leader. The {20} has no effect on {0} leader revival.</p>" : "") +

              (g.Applicable(Rule.AdvancedKarama) ?
              @"<p><strong>Special {19}:</strong> you may use a {19} card to ask one player's entire battle plan.</p>" : "") +

              @"<h5>Alliance Power</h5>
                <p>You may assist your allies by forcing their opponents to tell them one element of their battle plan.</p>
                <h5>Strategy</h5>
                <p>You are handicapped by the fact that you must both purchase cards and ship onto the planet, and you have no source of income other than {16}. This will keep you in constant battles. Since you start from {6} you have the movement advantage of 3 from the outset, and it is wise to protect this. Your prescience allows you to avoid being devoured by {15} and helps you to get some slight head start on the {16} blow. In addition, you can gain some slight advantage over those who would do battle with you by your foreknowledge of one element of their battle plan.</p>
                </div>";
        }

        private static string GetBlackTemplate(Game g)
        {

            return
              @"<div style='{25}'>
                <p><strong>At start:</strong> 10 tokens in {7} and 10 tokens in reserve (off-planet). Start with 10 {16}.</p>
                <p><strong>Free revival:</strong> 2.</p>
                <h5>Basic Advantages</h5>
                <p>You excel in treachery.</p>
                <p>You keep all traitors you draw at the start of the game.</p>
                <p>You may hold up to 8 treachery cards. At start of game, you are dealt 2 cards instead of 1, and every time you buy a card you get an extra card free from the deck (if you have less than 8 total).</p>" +

             (g.Applicable(Rule.BlackMulligan) ? "<p>At start, when you draw 2 or more of your own leaders as traitors, you may shuffle them back and redraw four traitors.</p>" : "") +

             (g.Applicable(Rule.BlackCapturesOrKillsLeaders) ?
              @"<h5>Advanced Advantages</h5>
                <p>Every time you win a battle you can select randomly one leader from the loser (including the leader used in battle, if not killed, but excluding all leaders already used elsewhere that turn). You can kill that leader for 2 {16}; or use the leader once in a battle after which you must return him (her) to the original owner. If all your own leaders have been killed, all captured leaders are immediately returned to their original owners. Killed captured leaders are put in the 'tanks' from which the original owners can revive them (subject to the revival rules).</p>" : "") +

             (g.Applicable(Rule.AdvancedKarama) ?
              @"<p><strong>Special {19}:</strong> during the Bidding phase, you may use a {19} card to take at random up to all treachery cards of any one player of your choice, as long has your maximum hand size is not exceeded. Then, for each card you took you must give him one of your cards in return.</p>" : "") +

              @"<h5>Alliance Power</h5>
                <p>Leaders in your pay may betray your allies opponents, too.</p>
                <h5>Strategy</h5>
                <p>Your major handicap is your difficulty in obtaining {16}. You are at your greatest relative strength at the beginning of the game and should capitalize on this fact by quickly buying as many treachery cards as you can, and then surging into battle. Since you get 2 cards for every one you bid for, you can afford to bid a little higher than most, but if you spend too lavishly at first you will not have enough {16} to ship in tokens or buy more cards at a later date. The large number of cards you may hold will increase your chances of holding worthless cards. To counteract this you should pick your battles, both to unload cards and to flush out the traitors in your pay.</p>
                </div>";
        }

        private static string GetYellowTemplate(Game g)
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
                <p>Special Victory Condition: If no player has won by the end of the last turn and if you (or no one) occupies {9} and {22} and neither {1}, {0}, {2} nor {35} occupies {8}, you have prevented interference with your plans to alter the planet and win the game.</p>
                <p>If no player has won by the end of the last turn and {4} is not playing, you win the game.</p>" +
              (advancedApplies ? "<h5>Advanced Advantages</h5>" : "") +
              (g.Applicable(Rule.YellowSeesStorm) ? "<p>You can see the number of sectors the next storm will move.</p>" : "") +
              (g.Applicable(Rule.YellowSendingMonster) ? "<p>During {16} blow, each time {15} appears after the first time, you choose in which unprotected territory it appears.</p>" : "") +
              (g.Applicable(Rule.YellowStormLosses) ? "<p>If caught in a storm, only half your forces are killed. You may rally forces into a storm at half loss.</p>" : "") +
              (g.Applicable(Rule.YellowSpecialForces) ? "<p>Your 3 {23} are worth two normal tokens in battle and in taking losses. They are treated as one token in revival. Only one {23} token can be revived per turn.</p>" : "") +

              (g.Applicable(Rule.AdvancedKarama) ?
              @"<p><strong>Special {19}:</strong> during {16} blow, you may use a {19} card to cause {15} to appear in any unprotected territory that you wish.</p>" : "") +

              @"<h5>Alliance Powers</h5>
                <p>You may protect your allies from being devoured by {15} and may let them revive 3 forces for free. They win with you if you win with your special victory condition.</p>
                <h5>Strategy</h5>
                <p>Your major handicap is poverty. You won't be available to buy cards early game. You must be patient and move your forces into any vacant strongholds, avoiding battles until you are prepared. In battles you can afford to dial high and sacrifice your troops since they have a high revival rate and you can bring them back into play at no cost. You have better mobility than those without a city, and good fighting leaders. Bide your time and wait for an accessible {16} blow that no one else wants in order to build up your resources.<p>
                </div>";
        }

        private static string GetRedTemplate(Game g)
        {
            return
              @"<div style='{25}'>
                <p><strong>At start:</strong> 20 tokens in reserve (off-planet). Start with 10 {16}.</p>
                <p><strong>Free revival:</strong> 1.</p>
                <h5>Basic Advantages</h5>
                <p>You have access to great wealth.</p>
                <p>Whenever any other player pays {16} for a treachery card, he pays it to you instead of to the {16} bank.</p>" +

              (g.Applicable(Rule.RedSupportingNonAllyBids) ? "<p>You may support bids of non-allied players. Any {16} paid this way flows back to you at the end of the bidding phase.</p>" : "") +

              (g.Applicable(Rule.RedSpecialForces) ?
              @"<h5>Advanced Advantages</h5>
                <p>Your 5 {24} have a special fighting capability. They are worth two normal tokens in battle and in taking losses against all opponents except {3}. Your starred tokens are worth just one against {3}. They are treated as one token in revival. Only one starred token can be revived per turn.</p>" : "") +

              (g.Applicable(Rule.AdvancedKarama) ?
              @"<p><strong>Special {19}:</strong> you may use a {19} card to revive up to three tokens or one leader for free.</p>" : "") +

              @"<h5>Alliance Powers</h5>
                <p>Unlike other factions, you may give {16} to your allies which they receive immediately. Their payment for any treachery card even with your own {16} comes right back to you. In addition, you may pay (directly to the bank) for the revival of up to 3 extra of their forces (for a possible total of 6).</p>
                <h5>Strategy</h5>
                <p>Your major handicap is that you must ship in all of your tokens at the start of the game, and often this move requires a battle before you are prepared. Even though you do not need to forage for {16} on the surface of the planet often, you still are quite subject to attack since you are likely to concentrate on the cities for the mobility they give you. On the plus side you will never need {16} badly, since the bidding will keep you supplied.</p>
                </div>";
        }

        private static string GetOrangeTemplate(Game g)
        {
            return
              @"<div style='{25}'>
                <p><strong>At start:</strong> 5 tokens in {8} and 15 tokens in reserve (off-planet). Start with 5 {16}.</p>
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

              (g.Applicable(Rule.AdvancedKarama) ?
              @"<p><strong>Special {19}:</strong> you may use a {19} card to stop one off-planet shipment of any one player. You may do this directly before or after the shipment occurs.</p>" : "") +

              @"<h5>Alliance Powers</h5>
                <p>Allies may also perform site-to-site shipments and may ship at the same fee as you. They win with you if no one else wins. Ally payments for shipments, even with your own {16}, come right back to you.</p>
                <h5>Strategy</h5>
                <p>Your major handicap is your weak array of leaders and your inability to revive quickly. In addition, you usually cannot buy treachery cards at the beginning of the game. You are vulnerable at this point and should make your stronger moves after building up your resources. If players do not ship on at a steady rate you will have to fight for {16} on the planet or collect only the isolated blows. Your major advantage is that you can ship onto the planet inexpensively and can ship from any one territory to any other. This mobility allows you to make surprise moves and is particularly useful when you are the last player in the movement round. If the game is out of reach and well along, try suicide battles against the strongest players to weaken them and prevent a win until the last turn: the victory is then yours.</p>
                </div>";
        }

        private static string GetBlueTemplate(Game g)
        {
            bool advancedApplies = g.Applicable(Rule.BlueFirstForceInAnyTerritory) || g.Applicable(Rule.BlueAutoCharity) || g.Applicable(Rule.BlueAdvisors) || g.Applicable(Rule.BlueAccompaniesToShipmentLocation) || g.Applicable(Rule.BlueWorthlessAsKarma);

            return
              @"<div style='{25}'>
                <p><strong>At start:</strong> 1 token in {26} and 19 tokens in reserve (off-planet). Start with 5 {16}.</p>
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

        private static string GetPurpleTemplate(Game g)
        {
            return
              @"<div style='{25}'>
                <p><strong>At start:</strong> 20 tokens in reserve (off-planet). Start with 5 {16}.</p>
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

              (g.Applicable(Rule.PurpleGholas) ?
              @"<h5>Advanced Advantages</h5>
                <p>When you have fewer than five leaders available, you may revive dead leaders of other factions at your discounted rate and add them to your leader pool.</p>" : "") +

              (g.Applicable(Rule.AdvancedKarama) ?
              @"<p><strong>Special {19}:</strong> you may prevent a player from performing a revival (forces and/or leader).</p>" : "") +

              @"<h5>Alliance Power</h5>
                <p>Your ally may revive at half price (rounded up).</p>
                <h5>Strategy</h5>
                <p>You are handicapped by having no forces on the planet to start and only a small amount of {16} until you begin receiving {16} for revivals. You will have to bide your time as other factions battle, waiting until you start gaining {16} and giving your Face Dancers a chance to suddenly strike, or get into minor battles early to drive forces to the tanks, and possibly get a Face Dancer reveal. Use your ability to cycle through Face Dancers during the Mentat Pause to position yourself with a potentially more useful Face Dancer.</p>
                </div>";
        }

        private static string GetGreyTemplate(Game g)
        {
            bool advancedApplies = g.Applicable(Rule.GreySwappingCardOnBid) || g.Applicable(Rule.AdvancedCombat);

            return
              @"<div style='{25}'>
                <p><strong>At start:</strong> 3 {32} and 3 {33} in the {34}. 10 {32} and 4 {33} in reserve (off-planet). Start with 10 {16}.</p>
                <p><strong>Free revival:</strong> 1.</p>
                <h5>Basic Advantages</h5>
                <p>You are skilled in technology and production.</p>
                <p>During Setup you see all initially dealt Treachery Cards and choose your starting card from them.</p>
                <p>Before Bidding, one extra card is drawn and you see them all and put one of those cards on top or on the bottom of the Treachery Card deck. The remaining cards are shuffled for the bidding round.</p>
                <p>Your 7 {33} forces are each worth 2 normal forces in battle, are able to move 2 territories instead of 1 and can collect 3 {16}. Your {33} forces ship normally, but each costs 3 to revive.</p>
                <p>Your 13 {32} forces ship normally but are worth ½ in battle. {32} can be used to absorb losses after a battle. After battle losses are calculated, any of your surviving {32} in that territory can be exchanged for {33} you lost in that battle. {32} can control strongholds and collect {16}. {32} move 2 if accompanied by a {33}, or 1 if they are not.</p>
                <p>After the first storm movement, place your {34} in any non-stronghold territory. This stronghold counts towards the game win and is protected from worms and storms.</p>
                <p>Subsequently, before storms are revealed and as long as your forces occupy it, you may move your {34} up to 3 territories to any non-stronghold territory. You can't move it into or out of a storm. When you move into, from, or through a sector containing {16}, you may immediately collect 2 {16} per force in your stronghold.</p>
                <p>No other faction may ship forces directly into your {34}, or move it if they take control. Other factions must move or ship forces into the territory it is pointing at (including {26}), and then use one movement to enter.</p>" +

              (advancedApplies ? @"<h5>Advanced Advantages</h5>" : "") +

              (g.Applicable(Rule.GreySwappingCardOnBid) ? @"<p>Once, during the bidding round, before bidding begins on a card and before {0} gets to look at the card, you may take the Treachery Card about to be bid on, replacing it with one from your hand.</p>" : "") +

              (g.Applicable(Rule.AdvancedCombat) ? @"<p>{32} are always considered half strength for dialing. You can’t increase the effectiveness of {32} in battle by spending {16}.</p>" : "") +

              (g.Applicable(Rule.AdvancedKarama) ?
              @"<p><strong>Special {19}:</strong> during Shipment and Movement, you may move the {34} 2 territories on your turn as well as make your normal movement.</p>" : "") +

              @"<h5>Alliance Power</h5>
                <p>After an ally purchases a Treachery Card during bidding, they may immediately discard it and draw the top card from the deck.</p>
                <h5>Strategy</h5>
                <p>You are handicapped by having weaker forces in the halfstrength {32}, which make up the bulk of your forces. You have no regular source of {16} income. However, tactical placement of your {34} can position you to acquire {16} left behind on the planet. You also have an advantage over other factions because you know what Treachery cards are in play and you can mix in or suppress certain cards during the bidding phase.</p>
                </div>";
        }

        private static string GetWhiteTemplate(Game g)
        {
            bool advancedApplies = g.Applicable(Rule.WhiteBlackMarket) || g.Applicable(Rule.AdvancedCombat);

            return
              @"<div style='{25}'>
                <p><strong>At start:</strong> 20 tokens in reserve (off-planet). Start with 5 {16}. You have a separate deck of 10 {35} cards that are not part of your hand.</p>
                <p><strong>Free revival:</strong> 2.</p>
                <h5>Basic Advantages</h5>
                <p>You have alternative technology.</p>
                <p><strong>Bidding:</strong> when drawing Treachery cards up for bid, one less card is drawn and instead you choose to auction a card from your {35} deck first or last. When it is time to auction that card, you choose and reveal the card and the type of auction (Once Around or Silent). You collect payment on your cards sold to other factions. If you buy any, the {16} goes to the {2} or the {16} Bank normally. Whenever discarded, these cards go to the normal discard pile. They can’t be bought with {19}.</p>
                <p><i>Once Around Auction:</i> pick clockwise or counter-clockwise. Starting with the faction on one side of you, each player able to bid may pass or bid higher. You bid last and the highest bidder gets the card. If everyone else passes, you may either get the card for free or remove it from the game.</p>
                <p><i>Silent Auction:</i> all factions able to bid secretly choose an amount to bid. Choices are revealed simultaneously. The faction that bid to most wins the card (ties break according to Storm order). If all factions bid zero {16}, you may either get the card for free or remove it from the game.</p>
                <p><strong>Shipping:</strong> instead of normal shipping, you may ship one of your No-Field tokens (0, 3 or 5) face-down, paying for one force. Other factions do not know how many of your forces are located there and proceed as if at least one is there. You may reveal a No-Field token at any time before the Battle phase, placing the indicated number of forces from your reserves (or up to that amount if you have fewer forces left). You may move a No-Field token like a force.</p>
                <p>You may not have two No-Field tokens on the planet at the same time. The last revealed token stays face-up in front of your shield until another one is revealed.</p>
                <p>When you are in a battle, you must reveal the value of a No-Field token in that territory and place the indicated number of forces from your reserves (or up to that amount if you have fewer forces left). When you are in a Battle with a No-Field token, {0} may not see your force dial.</p>
                <p><strong>{3} Special Victory Condition:</strong> {35} counts as one of the factions that can not occupy {8} in order to fulfill the {3} Special Victory Condition.</p>" +

              (advancedApplies ?
              @"<h5>Advanced Advantages</h5>" : "") +

              (g.Applicable(Rule.WhiteBlackMarket) ?
              @"<p><strong>Black Market:</strong> at the start of the Bidding Round, you may auction one card from your hand. You may tell what you are selling (you may lie) and keep the card face-down ({0} may still look). You may use any type of auction. If no player bids, you keep the card. If it is sold, one fewer card is put up for auction as part of the normal Bidding Round. You collect payments on cards sold to other factions. If you buy any, the {16} goes to the {2} or the {16} Bank normally. They can’t be bought with {19}. If the normal bidding type was used, the regular bidding round resumes wherever it left off.</p>" : "") +

              (g.Applicable(Rule.AdvancedKarama) ?
              @"<p><strong>Special {19}:</strong> you may buy a card from your separate cache for 3 {16} at any time.</p>" : "") +

              @"<h5>Alliance Powers</h5>
                <p>Your ally may ship using one of your available No-Field tokens, revealing it immediately upon shipping. Place the used No-Field token face-up in front of your shield until you reveal another No-Field token.</p>
                <p>You may give your ally a {35} Card from your hand at any time.</p>
                <h5>Strategy</h5>
                <p>You are at a disadvantage by having no forces on the planet, and not much spice to operate. Try to be aware if factions would be inclined to buy one of your special Cards either for their use or to keep it out of the hands of another faction. Selling your cards will be your one regular form of income until you have gained enough spice. Use your No-Field tokens to get forces on the planet cheaply and confuse your opponents.</p>
                </div>";
        }

        private static string GetBrownTemplate(Game g)
        {
            bool advancedApplies = g.Applicable(Rule.BrownAuditor) || g.Applicable(Rule.AdvancedCombat);

            return
              @"<div style='{25}'>
                <p><strong>At start:</strong> 20 tokens in reserve (off-planet). Start with 2 {16}.</p>
                <p><strong>Free revival:</strong> 0.</p>
                <h5>Basic Advantages</h5>
                <p>You control economic affairs across the Imperium.</p>
                <p><strong>Charity:</strong> each turn, you collect 2 {16} for each faction in the game during Charity before any factions collect. If another faction collects Charity, it is paid to them from your {16}.</p>
                <p><strong>Revival:</strong> you have no free revival, but you have no limit to the number of forces you may pay to revive and it only costs you 1 for each force.</p>
                <p><strong>Treachery:</strong> You may hold up to 5 Treachery Cards. At the end of any phase, you may reveal duplicates of the same card from your hand for 3 {16} each. You may also discard {14} cards for 2 {16} each. Alternatively, you may use {14} cards as follows:</p>
                <p mt-0 mb-0><i>{37}</i> - Prevent a player from moving forces into a territory you occupy during Ship & Move. They may ship in normally.</p>
                <p mt-0 mb-0><i>{38}</i> - Prevent a loss of your forces in one territory to the Storm when it moves.</p>
                <p mt-0 mb-0><i>{39}</i> - Prevent a player from playing a {19} card this phase as they attempt to do so.</p>
                <p mt-0 mb-0><i>{40}</i> - Move your forces one extra territory on your turn during Ship & Move.</p>
                <p mt-0 mb-0><i>{41}</i> - Prevent a player from taking Free Revival.</p>
                <p mt-0 mb-0><i>{42}</i> - Force a player to send 1 force back to reserves during Mentat.</p>
                <p><p>Inflation:</strong> during Mentat, you may play your Inflation token with either the Double or Cancel side face-up. In the following game turn, Charity is either doubled or canceled for that turn (including {5} in the advanced game). While Charity is doubled, no bribes can be made. In the next Mentat the Inflation token is flipped to the other side. If the token has already been flipped it is removed from the game instead.</p>" +

              (advancedApplies ?
              @"<h5>Advanced Advantages</h5>" : "") +

              (g.Applicable(Rule.AdvancedCombat) ?
              @"<p><strong>Forces:</strong> when other players pay for their forces in battle, half of the {16} (rounded down) goes to you.</p>" : "") +

              (g.Applicable(Rule.BrownAuditor) ?
              @"<p><strong>Auditor:</strong> you gain the Auditor Leader and it is added to the Traitor deck at the start of the game. Whenever you use the Auditor as a leader in a battle, you may audit your opponent. You may look at two cards in your opponent's hand at random (not counting cards used in battle) if the Auditor survived, or one card if killed. That faction may pay you 1 {16} per card you would get to see to cancel the audit. The Auditor may be revived as if all of your leaders were in the Tanks. The Auditor can't be revived as a ghola, nor be captured by {1}. The Auditor can't have a Leader Skill.</p>" : "") +

              (g.Applicable(Rule.AdvancedKarama) ?
              @"<p><strong>Special {19}:</strong> you may discard any Treachery cards from your hand and gain 3 {16} each.</p>" : "") +

              @"<h5>Alliance Powers</h5>
                <p>Once per turn at the end of a phase, you may trade a Treachery Card with your ally. This trade is simultaneous and two-way.</p>
                <p>You may pay for your ally’s forces in battle.</p>

                <h5>Strategy</h5>
                <p>Your leaders are weak, but you have a steady income. Stockpile Treachery Cards. You start with no forces on the planet and must ship them all in. For this reason, you may want to wait until you can attack with a large force. Use your Inflation token at a key moment, especially at a time when others aren’t collecting Charity.</p>
                </div>";
        }

        #endregion FactionManual

        //#region SkinValidationAndFixing

        //public async Task<IEnumerable<string>> ValidateAndFix(Func<string, Task<bool>> UrlExists)
        //{
        //    var errors = new List<string>();

        //    /*
        //    var tReportBackground_ShipmentAndMove_URL = FixIfMissing("ReportBackground_ShipmentAndMove_URL", true, ReportBackground_ShipmentAndMove_URL, Dune1979.ReportBackground_ShipmentAndMove_URL, errors, UrlExists);
        //    var tReportBackground_StormAndResourceBlow_URL = FixIfMissing("ReportBackground_StormAndResourceBlow_URL", true, ReportBackground_StormAndResourceBlow_URL, Dune1979.ReportBackground_StormAndResourceBlow_URL, errors, UrlExists);
        //    var tReportBackground_EndOfTurn_URL = FixIfMissing("ReportBackground_EndOfTurn_URL", true, ReportBackground_EndOfTurn_URL, Dune1979.ReportBackground_EndOfTurn_URL, errors, UrlExists);
        //    var tReportBackground_Resurrection_URL = FixIfMissing("ReportBackground_Resurrection_URL", true, ReportBackground_Resurrection_URL, Dune1979.ReportBackground_Resurrection_URL, errors, UrlExists);
        //    var tPanelBackground_Bidding_URL = FixIfMissing("PanelBackground_Bidding_URL", true, PanelBackground_Bidding_URL, Dune1979.PanelBackground_Bidding_URL, errors, UrlExists);
        //    var tPanelBackground_EndOfGame_URL = FixIfMissing("PanelBackground_EndOfGame_URL", true, PanelBackground_EndOfGame_URL, Dune1979.PanelBackground_EndOfGame_URL, errors, UrlExists);
        //    var tPanelBackground_Battle_URL = FixIfMissing("PanelBackground_Battle_URL", true, PanelBackground_Battle_URL, Dune1979.PanelBackground_Battle_URL, errors, UrlExists);
        //    */

        //    var tMap_URL = FixIfMissing("Map_URL", true, Map_URL, Dune1979.Map_URL, errors, UrlExists);
        //    var tEye_URL = FixIfMissing("Eye_URL", true, Eye_URL, Dune1979.Eye_URL, errors, UrlExists);
        //    var tEyeSlash_URL = FixIfMissing("EyeSlash_URL", true, EyeSlash_URL, Dune1979.EyeSlash_URL, errors, UrlExists);
        //    var tPlanet_URL = FixIfMissing("Planet_URL", true, Planet_URL, Dune1979.Planet_URL, errors, UrlExists);
        //    var tCardBack_ResourceCard_URL = FixIfMissing("CardBack_ResourceCard_URL", true, CardBack_ResourceCard_URL, Dune1979.CardBack_ResourceCard_URL, errors, UrlExists);
        //    var tCardBack_TreacheryCard_URL = FixIfMissing("CardBack_TreacheryCard_URL", true, CardBack_TreacheryCard_URL, Dune1979.CardBack_TreacheryCard_URL, errors, UrlExists);
        //    var tBattleScreen_URL = FixIfMissing("BattleScreen_URL", true, BattleScreen_URL, Dune1979.BattleScreen_URL, errors, UrlExists);
        //    var tMessiah_URL = FixIfMissing("Messiah_URL", true, Messiah_URL, Dune1979.Messiah_URL, errors, UrlExists);
        //    var tMonster_URL = FixIfMissing("Monster_URL", true, Monster_URL, Dune1979.Monster_URL, errors, UrlExists);
        //    var tHarvester_URL = FixIfMissing("Harvester_URL", true, Harvester_URL, Dune1979.Harvester_URL, errors, UrlExists);
        //    var tResource_URL = FixIfMissing("Resource_URL", true, Resource_URL, Dune1979.Resource_URL, errors, UrlExists);
        //    var tHMS_URL = FixIfMissing("HMS_URL", true, HMS_URL, Dune1979.HMS_URL, errors, UrlExists);

        //    /*
        //    var tMusicGeneral_URL = FixIfMissing("MusicGeneral_URL", false, MusicGeneral_URL, Dune1979.MusicGeneral_URL, errors, UrlExists);
        //    var tMusicSetup_URL = FixIfMissing("MusicSetup_URL", false, MusicSetup_URL, Dune1979.MusicSetup_URL, errors, UrlExists);
        //    var tMusicResourceBlow_URL = FixIfMissing("MusicResourceBlow_URL", false, MusicResourceBlow_URL, Dune1979.MusicResourceBlow_URL, errors, UrlExists);
        //    var tMusicBidding_URL = FixIfMissing("MusicBidding_URL", false, MusicBidding_URL, Dune1979.MusicBidding_URL, errors, UrlExists);
        //    var tMusicShipmentAndMove_URL = FixIfMissing("MusicShipmentAndMove_URL", false, MusicShipmentAndMove_URL, Dune1979.MusicShipmentAndMove_URL, errors, UrlExists);
        //    var tMusicBattle_URL = FixIfMissing("MusicBattle_URL", false, MusicBattle_URL, Dune1979.MusicBattle_URL, errors, UrlExists);
        //    var tMusicBattleClimax_URL = FixIfMissing("MusicBattleClimax_URL", false, MusicBattleClimax_URL, Dune1979.MusicBattleClimax_URL, errors, UrlExists);
        //    var tMusicMentat_URL = FixIfMissing("MusicMentat_URL", false, MusicMentat_URL, Dune1979.MusicMentat_URL, errors, UrlExists);
        //    */

        //    var tMapDimensions = FixIfMissing("MapDimensions", false, MapDimensions, Dune1979.MapDimensions, errors, UrlExists);
        //    var tPlanetCenter = FixIfMissing("PlanetCenter", false, PlanetCenter, Dune1979.PlanetCenter, errors, UrlExists);
        //    var tPlanetRadius = FixIfMissing("PlanetRadius", false, PlanetRadius, Dune1979.PlanetRadius, errors, UrlExists);
        //    var tMapRadius = FixIfMissing("MapRadius", false, MapRadius, Dune1979.MapRadius, errors, UrlExists);
        //    var tPlayerTokenRadius = FixIfMissing("PlayerTokenRadius", false, PlayerTokenRadius, Dune1979.PlayerTokenRadius, errors, UrlExists);
        //    var tSpiceDeckLocation = FixIfMissing("SpiceDeckLocation", false, SpiceDeckLocation, Dune1979.SpiceDeckLocation, errors, UrlExists);
        //    var tTreacheryDeckLocation = FixIfMissing("TreacheryDeckLocation", false, TreacheryDeckLocation, Dune1979.TreacheryDeckLocation, errors, UrlExists);
        //    var tBattleScreenWidth = FixIfMissing("BattleScreenWidth", false, BattleScreenWidth, Dune1979.BattleScreenWidth, errors, UrlExists);
        //    var tBattleScreenHeight = FixIfMissing("BattleScreenHeight", false, BattleScreenHeight, Dune1979.BattleScreenHeight, errors, UrlExists);
        //    var tBattleScreenHeroX = FixIfMissing("BattleScreenHeroX", false, BattleScreenHeroX, Dune1979.BattleScreenHeroX, errors, UrlExists);
        //    var tBattleScreenHeroY = FixIfMissing("BattleScreenHeroY", false, BattleScreenHeroY, Dune1979.BattleScreenHeroY, errors, UrlExists);
        //    var tBattleWheelHeroWidth = FixIfMissing("BattleWheelHeroWidth", false, BattleWheelHeroWidth, Dune1979.BattleWheelHeroWidth, errors, UrlExists);
        //    var tBattleWheelHeroHeight = FixIfMissing("BattleWheelHeroHeight", false, BattleWheelHeroHeight, Dune1979.BattleWheelHeroHeight, errors, UrlExists);
        //    var tBattleWheelForcesX = FixIfMissing("BattleWheelForcesX", false, BattleWheelForcesX, Dune1979.BattleWheelForcesX, errors, UrlExists);
        //    var tBattleWheelForcesY = FixIfMissing("BattleWheelForcesY", false, BattleWheelForcesY, Dune1979.BattleWheelForcesY, errors, UrlExists);
        //    var tBattleWheelCardX = FixIfMissing("BattleWheelCardX", false, BattleWheelCardX, Dune1979.BattleWheelCardX, errors, UrlExists);
        //    var tBattleWheelCardY = FixIfMissing("BattleWheelCardY", false, BattleWheelCardY, Dune1979.BattleWheelCardY, errors, UrlExists);
        //    var tBattleWheelCardWidth = FixIfMissing("BattleWheelCardWidth", false, BattleWheelCardWidth, Dune1979.BattleWheelCardWidth, errors, UrlExists);
        //    var tBattleWheelCardHeight = FixIfMissing("BattleWheelCardHeight", false, BattleWheelCardHeight, Dune1979.BattleWheelCardHeight, errors, UrlExists);

        //    /*
        //    var tSound_YourTurn_URL = FixIfMissing("Sound_YourTurn_URL", false, Sound_YourTurn_URL, Dune1979.Sound_YourTurn_URL, errors, UrlExists);
        //    var tSound_Chatmessage_URL = FixIfMissing("Sound_Chatmessage_URL", false, Sound_Chatmessage_URL, Dune1979.Sound_Chatmessage_URL, errors, UrlExists);
        //    var tSound = FixDictionaryIfMissing("Sound", false, Sound, Dune1979.Sound, errors, UrlExists);
        //    */

        //    var tTreacheryCardType_STR = FixDictionaryIfMissing("TreacheryCardType_STR", false, TreacheryCardType_STR, Dune1979.TreacheryCardType_STR, errors, UrlExists);
        //    var tTreacheryCardName_STR = FixDictionaryIfMissing("TreacheryCardName_STR", false, TreacheryCardName_STR, Dune1979.TreacheryCardName_STR, errors, UrlExists);
        //    var tTreacheryCardDescription_STR = FixDictionaryIfMissing("TreacheryCardDescription_STR", false, TreacheryCardDescription_STR, Dune1979.TreacheryCardDescription_STR, errors, UrlExists);
        //    var tTechTokenDescription_STR = FixDictionaryIfMissing("TechTokenDescription_STR", false, TechTokenDescription_STR, Dune1979.TechTokenDescription_STR, errors, UrlExists);
        //    var tTreacheryCardImage_URL = FixDictionaryIfMissing("TreacheryCardImage_URL", true, TreacheryCardImage_URL, Dune1979.TreacheryCardImage_URL, errors, UrlExists);
        //    var tResourceCardImage_URL = FixDictionaryIfMissing("ResourceCardImage_URL", true, ResourceCardImage_URL, Dune1979.ResourceCardImage_URL, errors, UrlExists);
        //    var tConcept_STR = FixDictionaryIfMissing("Concept_STR", false, Concept_STR, Dune1979.Concept_STR, errors, UrlExists);
        //    var tMainPhase_STR = FixDictionaryIfMissing("MainPhase_STR", false, MainPhase_STR, Dune1979.MainPhase_STR, errors, UrlExists);
        //    var tPersonName_STR = FixDictionaryIfMissing("PersonName_STR", false, PersonName_STR, Dune1979.PersonName_STR, errors, UrlExists);
        //    var tPersonImage_URL = FixDictionaryIfMissing("PersonImage_URL", true, PersonImage_URL, Dune1979.PersonImage_URL, errors, UrlExists);
        //    var tTerritoryName_STR = FixDictionaryIfMissing("TerritoryName_STR", false, TerritoryName_STR, Dune1979.TerritoryName_STR, errors, UrlExists);
        //    var tTerritoryBorder_SVG = FixDictionaryIfMissing("TerritoryBorder_SVG", false, TerritoryBorder_SVG, Dune1979.TerritoryBorder_SVG, errors, UrlExists);
        //    var tLocationCenter_Point = FixDictionaryIfMissing("LocationCenter_Point", false, LocationCenter_Point, Dune1979.LocationCenter_Point, errors, UrlExists);
        //    var tLocationSpice_Point = FixDictionaryIfMissing("LocationSpice_Point", false, LocationSpice_Point, Dune1979.LocationSpice_Point, errors, UrlExists);
        //    var tFactionName_STR = FixDictionaryIfMissing("FactionName_STR", false, FactionName_STR, Dune1979.FactionName_STR, errors, UrlExists);
        //    var tFactionImage_URL = FixDictionaryIfMissing("FactionImage_URL", true, FactionImage_URL, Dune1979.FactionImage_URL, errors, UrlExists);
        //    var tFactionForceImage_URL = FixDictionaryIfMissing("FactionForceImage_URL", true, FactionForceImage_URL, Dune1979.FactionForceImage_URL, errors, UrlExists);
        //    var tFactionSpecialForceImage_URL = FixDictionaryIfMissing("FactionSpecialForceImage_URL", true, FactionSpecialForceImage_URL, Dune1979.FactionSpecialForceImage_URL, errors, UrlExists);
        //    var tFactionTableImage_URL = FixDictionaryIfMissing("FactionTableImage_URL", true, FactionTableImage_URL, Dune1979.FactionTableImage_URL, errors, UrlExists);
        //    var tFactionFacedownImage_URL = FixDictionaryIfMissing("FactionFacedownImage_URL", true, FactionFacedownImage_URL, Dune1979.FactionFacedownImage_URL, errors, UrlExists);
        //    var tFactionColorTransparant = FixDictionaryIfMissing("FactionColorTransparant", false, FactionColorTransparant, Dune1979.FactionColorTransparant, errors, UrlExists);
        //    var tFactionColor = FixDictionaryIfMissing("FactionColor", false, FactionColor, Dune1979.FactionColor, errors, UrlExists);
        //    var tSpecialForceName_STR = FixDictionaryIfMissing("SpecialForceName_STR", false, SpecialForceName_STR, Dune1979.SpecialForceName_STR, errors, UrlExists);
        //    var tForceName_STR = FixDictionaryIfMissing("ForceName_STR", false, ForceName_STR, Dune1979.ForceName_STR, errors, UrlExists);
        //    var tTechTokenName_STR = FixDictionaryIfMissing("TechTokenName_STR", false, TechTokenName_STR, Dune1979.TechTokenName_STR, errors, UrlExists);
        //    var tTechTokenImage_URL = FixDictionaryIfMissing("TechTokenImage_URL", true, TechTokenImage_URL, Dune1979.TechTokenImage_URL, errors, UrlExists);

        //    var tStrongholdCardImage_URL = FixDictionaryIfMissing("StrongholdCardImage_URL", true, StrongholdCardImage_URL, Dune1979.StrongholdCardImage_URL, errors, UrlExists);
        //    var tStrongholdCardName_STR = FixDictionaryIfMissing("StrongholdCardImage_STR", false, StrongholdCardName_STR, Dune1979.StrongholdCardName_STR, errors, UrlExists);
        //    var tLeaderSkillCardImage_URL = FixDictionaryIfMissing("LeaderSkillCardImage_URL", true, LeaderSkillCardImage_URL, Dune1979.LeaderSkillCardImage_URL, errors, UrlExists);
        //    var tLeaderSkillCardName_STR = FixDictionaryIfMissing("LeaderSkillCardName_STR", false, LeaderSkillCardName_STR, Dune1979.LeaderSkillCardName_STR, errors, UrlExists);

        //    /*
        //    ReportBackground_ShipmentAndMove_URL = await tReportBackground_ShipmentAndMove_URL;
        //    ReportBackground_StormAndResourceBlow_URL = await tReportBackground_StormAndResourceBlow_URL;
        //    ReportBackground_EndOfTurn_URL = await tReportBackground_EndOfTurn_URL;
        //    ReportBackground_Resurrection_URL = await tReportBackground_Resurrection_URL;
        //    PanelBackground_Bidding_URL = await tPanelBackground_Bidding_URL;
        //    PanelBackground_EndOfGame_URL = await tPanelBackground_EndOfGame_URL;
        //    PanelBackground_Battle_URL = await tPanelBackground_Battle_URL;
        //    */

        //    Map_URL = await tMap_URL;
        //    Eye_URL = await tEye_URL;
        //    EyeSlash_URL = await tEyeSlash_URL;
        //    Planet_URL = await tPlanet_URL;
        //    CardBack_ResourceCard_URL = await tCardBack_ResourceCard_URL;
        //    CardBack_TreacheryCard_URL = await tCardBack_TreacheryCard_URL;
        //    BattleScreen_URL = await tBattleScreen_URL;
        //    Messiah_URL = await tMessiah_URL;
        //    Monster_URL = await tMonster_URL;
        //    Harvester_URL = await tHarvester_URL;
        //    Resource_URL = await tResource_URL;
        //    HMS_URL = await tHMS_URL;

        //    /*
        //    MusicGeneral_URL = await tMusicGeneral_URL;
        //    MusicSetup_URL = await tMusicSetup_URL;
        //    MusicResourceBlow_URL = await tMusicResourceBlow_URL;
        //    MusicBidding_URL = await tMusicBidding_URL;
        //    MusicShipmentAndMove_URL = await tMusicShipmentAndMove_URL;
        //    MusicBattle_URL = await tMusicBattle_URL;
        //    MusicBattleClimax_URL = await tMusicBattleClimax_URL;
        //    MusicMentat_URL = await tMusicMentat_URL;
        //    */

        //    /*
        //    Sound_YourTurn_URL = await tSound_YourTurn_URL;
        //    Sound_Chatmessage_URL = await tSound_Chatmessage_URL;
        //    Sound = await tSound;
        //    */

        //    MapDimensions = await tMapDimensions;
        //    PlanetCenter = await tPlanetCenter;
        //    PlanetRadius = await tPlanetRadius;
        //    MapRadius = await tMapRadius;
        //    PlayerTokenRadius = await tPlayerTokenRadius;
        //    SpiceDeckLocation = await tSpiceDeckLocation;
        //    TreacheryDeckLocation = await tTreacheryDeckLocation;
        //    BattleScreenWidth = await tBattleScreenWidth;
        //    BattleScreenHeight = await tBattleScreenHeight;
        //    BattleScreenHeroX = await tBattleScreenHeroX;
        //    BattleScreenHeroY = await tBattleScreenHeroY;
        //    BattleWheelHeroWidth = await tBattleWheelHeroWidth;
        //    BattleWheelHeroHeight = await tBattleWheelHeroHeight;
        //    BattleWheelForcesX = await tBattleWheelForcesX;
        //    BattleWheelForcesY = await tBattleWheelForcesY;
        //    BattleWheelCardX = await tBattleWheelCardX;
        //    BattleWheelCardY = await tBattleWheelCardY;
        //    BattleWheelCardWidth = await tBattleWheelCardWidth;
        //    BattleWheelCardHeight = await tBattleWheelCardHeight;

        //    TreacheryCardType_STR = await tTreacheryCardType_STR;
        //    TreacheryCardName_STR = await tTreacheryCardName_STR;
        //    TreacheryCardDescription_STR = await tTreacheryCardDescription_STR;
        //    TechTokenDescription_STR = await tTechTokenDescription_STR;
        //    TreacheryCardImage_URL = await tTreacheryCardImage_URL;
        //    ResourceCardImage_URL = await tResourceCardImage_URL;
        //    Concept_STR = await tConcept_STR;
        //    MainPhase_STR = await tMainPhase_STR;
        //    PersonName_STR = await tPersonName_STR;
        //    PersonImage_URL = await tPersonImage_URL;
        //    TerritoryName_STR = await tTerritoryName_STR;
        //    TerritoryBorder_SVG = await tTerritoryBorder_SVG;
        //    FactionName_STR = await tFactionName_STR;
        //    FactionImage_URL = await tFactionImage_URL;
        //    FactionForceImage_URL = await tFactionForceImage_URL;
        //    FactionSpecialForceImage_URL = await tFactionSpecialForceImage_URL;
        //    FactionTableImage_URL = await tFactionTableImage_URL;
        //    FactionFacedownImage_URL = await tFactionFacedownImage_URL;
        //    FactionColorTransparant = await tFactionColorTransparant;
        //    FactionColor = await tFactionColor;
        //    SpecialForceName_STR = await tSpecialForceName_STR;
        //    ForceName_STR = await tForceName_STR;
        //    TechTokenName_STR = await tTechTokenName_STR;
        //    TechTokenImage_URL = await tTechTokenImage_URL;
        //    StrongholdCardImage_URL = await tStrongholdCardImage_URL;
        //    StrongholdCardName_STR = await tStrongholdCardName_STR;
        //    LeaderSkillCardImage_URL = await tLeaderSkillCardImage_URL;
        //    LeaderSkillCardName_STR = await tLeaderSkillCardName_STR;

        //    return errors;
        //}

        //private static async Task<T> FixIfMissing<T>(string propertyName, bool checkIfUrlExists, T toCheck, T referenceValue, List<string> errors, Func<string, Task<bool>> UrlExists)
        //{
        //    if (toCheck == null)
        //    {
        //        errors.Add(propertyName + " is missing.");
        //        return referenceValue;
        //    }
        //    else if (checkIfUrlExists && !await UrlExists(toCheck.ToString()))
        //    {
        //        errors.Add("Resource \"" + toCheck + "\" (" + propertyName + ") not found.");
        //        return referenceValue;
        //    }

        //    return toCheck;
        //}

        //private static async Task<Dictionary<K, V>> FixDictionaryIfMissing<K, V>(string propertyName, bool checkIfUrlExists, Dictionary<K, V> toCheck, Dictionary<K, V> referenceValues, List<string> errors, Func<string, Task<bool>> UrlExists)
        //{
        //    if (toCheck == null)
        //    {
        //        errors.Add(propertyName + " is missing.");
        //        return referenceValues;
        //    }

        //    foreach (var key in referenceValues.Keys)
        //    {
        //        if (!toCheck.ContainsKey(key))
        //        {
        //            errors.Add(string.Format("{0} does not contain \"{1}\"", propertyName, key));
        //            toCheck.Add(key, referenceValues[key]);
        //        }
        //        else if (checkIfUrlExists && !await UrlExists(toCheck[key].ToString()))
        //        {
        //            errors.Add("Resource \"" + toCheck[key] + "\" (" + propertyName + "." + key + ") not found.");
        //            toCheck.Remove(key);
        //            toCheck.Add(key, referenceValues[key]);
        //        }
        //    }

        //    return toCheck;
        //}

        //#endregion SkinValidationAndFixing

        #region LoadingAndSaving

        public static Skin Load(string data)
        {
            var serializer = JsonSerializer.CreateDefault();
            serializer.Formatting = Formatting.Indented;
            var textReader = new StringReader(data);
            var jsonReader = new JsonTextReader(textReader);
            return serializer.Deserialize<Skin>(jsonReader);
        }

        public string SkinToString()
        {
            var serializer = JsonSerializer.CreateDefault();
            serializer.Formatting = Formatting.Indented;
            var writer = new StringWriter();
            serializer.Serialize(writer, this);
            writer.Close();
            return writer.ToString();
        }

        #endregion LoadingAndSaving

        public static Skin Dune1979 { get; private set; } = new Skin()
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
            HMS_URL = DEFAULT_ART_LOCATION + "/art/hms.svg",

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
                [MainPhase.Collection] = "Collection",
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
                [50] = "Nullentropy Box",
                [51] = "Semuta Drug",
                [52] = "Residual Poison",
                [53] = "Stone Burner",
                [54] = "Karama",
            },

            TreacheryCardDescription_STR = new Dictionary<int, string>
            {
                [0] = "Weapon - Special - Play as part of your Battle Plan. Automatically kills your opponent's leader. Causes an explosion when a Shield is used in the same battle, killing both leaders and all forces in the territory, cause both factions to loose the battle.",
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
                [23] = "Allows you to prevent use of a Faction Advantage. Allows you to bid any amount of spice on a card or immediately win a card on bid. Allows you to ship at half price. In the advanced game, allows use of your Special Karama Power once during the game. Discard after use.",
                [25] = "Publicly ask one player a yes or no question about the game. That question must be answered truthfully.",
                [27] = "Can be played after turn 1 just before the storm moves. Instead of normal storm moved, you can move the storm 0 to 10 sectors.",
                [28] = "Worthless Card - Play as part of your Battle Plan, in place of a weapon, defense, or both. This card has no value in play, and you can discard it only by playing it in your Battle plan.",
                [29] = "Worthless Card - Play as part of your Battle Plan, in place of a weapon, defense, or both. This card has no value in play, and you can discard it only by playing it in your Battle plan.",
                [30] = "Worthless Card - Play as part of your Battle Plan, in place of a weapon, defense, or both. This card has no value in play, and you can discard it only by playing it in your Battle plan.",
                [31] = "Worthless Card - Play as part of your Battle Plan, in place of a weapon, defense, or both. This card has no value in play, and you can discard it only by playing it in your Battle plan.",
                [32] = "Worthless Card - Play as part of your Battle Plan, in place of a weapon, defense, or both. This card has no value in play, and you can discard it only by playing it in your Battle plan.",

                [33] = "Weapon - Special - Play as part of your Battle Plan. This weapon counts as both projectile and poison. You may keep this card if you win this battle.",
                [34] = "Weapon - Projectile - Play as part of your Battle Plan. Kills opponent's leader before battle is resolved. Opponent may protect leader with a Shield. You may keep this card if you win the battle.",
                [35] = "Weapon - Poison - Play as part of your Battle Plan. Kills opponent's leader before battle is resolved. Opponent may protect leader with a Snooper. You may keep this card if you win the battle.",
                [36] = "Weapon - Special - Play as part of your Battle Plan. Counts as a projectile weapon but has the same effect as a projectile defense when played as a defense with another weapon. You may keep this card if you win this battle.",
                [37] = "Weapon - Special - Play as part of your Battle Plan. Kills both leaders, and is not stopped by a Snooper. After seeing the battle results, you may choose not to use this weapon in which case you don't need to discard it if you win the battle.",
                [38] = "Defense - Special - Play as part of your Battle Plan. Counts as both a Shield (projectile defense) and Snooper (poison defense). You may keep this card if you win this battle.",
                [39] = "Defense - Special - Play as part of your Battle Plan. Counts as a poison defense but has the same effect as a poison weapon when played as a weapon with another defense. You may keep this card if you win this battle.",
                [40] = "Weapon - Special - Play as part of your Battle Plan. Kills both leaders (no spice is paid for them). Both players may use Shields to protect their leader against the Artillery Strike. Surviving (shielded) leaders do not count towards the battle total, the side that dialed higher wins the battle. Discard after use.",
                [41] = "Play just after a spice blow comes up. Doubles the Spice blow. Place double the amount of spice in the territory.",
                [42] = "Play at beginning of Spice Blow Phase instead of revealing the Spice Blow card. Causes a Shai-Hulud to appear. Play proceeds as though Shai-Hulud has been revealed.",
                [43] = "At the beginning of any phase, cause all players to discard half of the spice behind their shields, rounded up, to the Spice Bank.",
                [44] = "Worthless Card - Play as part of your Battle Plan, in place of a weapon, defense, or both. This card has no value in play, and you can discard it only by playing it in your Battle plan.",

                [45] = "Distrans - Give another player a treachery card from your hand at any time except during a bid and if their hand size permits. Discard after use.",
                [46] = "Choose one: (1) be considered aggressor in a battle or (2) play at the beginning of a phase or action that requires turn order to go first or (3) go last in a phase or action that requires turn order. Discard after use.",
                [47] = "Weapon - Special - Play as part of your Battle Plan. Becomes a copy of your opponent's weapon. Discard after use.",
                [48] = "Defense - Poison - You may play this after revealing your battle plan if you did not play a defense and if Voice permits. Discard after use.",
                [49] = "Ornithopter - As part of your movement you may move one group of forces up to 3 territories or two groups of forces up to your normal move distance. Discard after use.",
                [50] = "Nullentropy Box - At any time, pay 2 spice to secretly search and take one card from the treachery discard pile. Then shuffle the discard pilem discarding this card on top.",
                [51] = "Semuta Drug - Add a treachery card to your hand immediately after another player discards it. You choose if multiple cards are discarded at the same time. Discard after use.",
                [52] = "Residual Poison - Play against your opponent in battle before making battle plans. Kills one of their available leaders at random. No spice is collected for it. Discard after use.",
                [53] = "Weapon - Special - Play as part of your Battle Plan. You choose after pland are revealed to either kill both leaders or reduce the strength of both leaders to 0. The player with the highest number of undialed forces wins the battle. Dialed forces are lost normally. Discard after use.",
                [54] = "Allows you to prevent use of a Faction Advantage. Allows you to bid any amount of spice on a card or immediately win a card on bid. Allows you to ship at half price. In the advanced game, allows use of your Special Karama Power once during the game. Discard after use.",
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
                [10] = DEFAULT_ART_LOCATION + "/art/Shield.gif",
                [11] = DEFAULT_ART_LOCATION + "/art/Shield.gif",
                [12] = DEFAULT_ART_LOCATION + "/art/Shield.gif",
                [13] = DEFAULT_ART_LOCATION + "/art/Snooper.gif",
                [14] = DEFAULT_ART_LOCATION + "/art/Snooper.gif",
                [15] = DEFAULT_ART_LOCATION + "/art/Snooper.gif",
                [16] = DEFAULT_ART_LOCATION + "/art/Snooper.gif",
                [17] = DEFAULT_ART_LOCATION + "/art/CheapHero.gif",
                [18] = DEFAULT_ART_LOCATION + "/art/CheapHero.gif",
                [19] = DEFAULT_ART_LOCATION + "/art/CheapHeroine.gif",
                [20] = DEFAULT_ART_LOCATION + "/art/TleilaxuGhola.gif",
                [21] = DEFAULT_ART_LOCATION + "/art/FamilyAtomics.gif",
                [22] = DEFAULT_ART_LOCATION + "/art/Hajr.gif",
                [23] = DEFAULT_ART_LOCATION + "/art/Karama.gif",
                [24] = DEFAULT_ART_LOCATION + "/art/Karama.gif",
                [25] = DEFAULT_ART_LOCATION + "/art/Truthtrance.gif",
                [26] = DEFAULT_ART_LOCATION + "/art/Truthtrance.gif",
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
                [54] = DEFAULT_ART_LOCATION + "/art/WhiteKarama.gif",
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
                [1051] = "Premier Ein Calimar"
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

                [1041] = DEFAULT_ART_LOCATION + "/art/person1041.gif",
                [1042] = DEFAULT_ART_LOCATION + "/art/person1042.gif",
                [1043] = DEFAULT_ART_LOCATION + "/art/person1043.gif",
                [1044] = DEFAULT_ART_LOCATION + "/art/person1044.gif",
                [1045] = DEFAULT_ART_LOCATION + "/art/person1045.gif",
                [1046] = DEFAULT_ART_LOCATION + "/art/person1046.gif",
                [1047] = DEFAULT_ART_LOCATION + "/art/person1047.gif",
                [1048] = DEFAULT_ART_LOCATION + "/art/person1048.gif",
                [1049] = DEFAULT_ART_LOCATION + "/art/person1049.gif",
                [1050] = DEFAULT_ART_LOCATION + "/art/person1050.gif",
                [1051] = DEFAULT_ART_LOCATION + "/art/person1051.gif",
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
                [25] = "Old Gap",
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

            TerritoryBorder_SVG = new Dictionary<int, string>()
            {
                [0] = "M243.4 297.6 L236.2 311.3 L233.7 326.2 L238.9 332 L242.4 332.3 L248.8 338.5 L256.8 341.8 L264.1 340.8 L268.6 348.9 L273.6 353.9 L288.7 354.5 L301 345.8 L301.4 337.3 L305.7 332 L310.1 321.9 L317.7 311.3 L320.2 297.1 L312 285.4 L302.7 285.5 L298.3 287.3 L291.4 287.1 L286.2 281.9 L281 281.9 L276.5 285.3 L267.7 288.3 L254.9 289.4 L243.4 297.6 z",
                [1] = "M345.8 221.8 L355.3 195.8 L353.8 193.3 L355.8 190.1 L354.6 183.9 L361.1 172.7 L354.7 170.7 L347.7 171.9 L343.9 162.8 L330.4 152.6 L329.5 145 L336.4 137.1 L342.3 126.1 L343.4 121 L340.8 110.7 L341.9 106.9 L336.6 99.3 L330.8 98.6 L320 90.4 L306.4 107.4 L297.6 127.7 L292.9 132.2 L297.6 140.5 L297.5 159 L287.6 182.5 L283.8 207.7 L286.4 217 L286.2 235.3 L288.3 237.8 L287.8 241.9 L287.9 260.2 L289.3 264.5 L286.2 281.9 L291.4 287.1 L298.3 287.3 L302.7 285.5 L302.7 280.5 L312.5 271.2 L317.2 262 L333.1 243.9 L331.6 240.5 L342.5 222.4 L345.8 221.8 z",
                [2] = "M287.6 182.5 L297.5 159 L297.6 140.5 L292.9 132.2 L277.5 129.2 L269.4 133.8 L250.7 139.1 L257.5 178 L261.6 178.3 L267.9 185.3 L275.2 182.6 L287.6 182.5 z",
                [3] = "M341.9 106.9 L340.8 110.7 L343.4 121 L342.3 126.1 L336.4 137.1 L329.5 145 L330.4 152.6 L343.9 162.8 L347.7 171.9 L354.7 170.7 L361.1 172.7 L383.3 134.2 L383.1 124.6 L370.6 110.3 L358 106.3 L341.9 106.9",
                [4] = "M451.4 447.4 L470.7 430.2 L479.3 414.8 L479.1 402.4 L482.6 390.3 L469.5 386.4 L464.4 381.1 L453.8 379.9 L443.5 377.4 L434.9 392.5 L433.7 404 L437.2 420.2 L435.7 433.8 L437.4 441 L451.4 447.4",
                [5] = "M128.8 234 L133.9 216.7 L131 213.6 L130.5 208.7 L127.9 206.7 L127.7 200.5 L122 191 L120 181.5 L111.1 177.2 L91.1 195.1 L90.4 200 L86.2 207.7 L81 209.4 L79.5 218 L89.4 228.9 L94.9 229.7 L100.2 226.6 L112.8 232 L120.6 231.9 L128.8 234 z",
                [6] = "M139.6 430 L129.1 409 L128.6 401.4 L125.9 395 L112.3 391.7 L97.8 396.8 L91.2 407.5 L96.8 427.1 L117 449 L139.6 430",
                [7] = "M224.1 410 L219.8 417.5 L221.1 424.6 L233.7 436.2 L263.8 439.1 L268.6 435.7 L280 435.6 L305.8 451.9 L314 446.6 L323.8 431.9 L330.4 424.6 L336 406.5 L301 345.8 L288.7 354.5 L273.6 353.9 L268.6 348.9 L264.1 340.8 L224.1 410 z",
                [8] = "M336.2 473.2 L324.9 474.3 L311 480.9 L301.1 481.1 L277.8 486.5 L245.5 486.1 L228.7 478.4 L218 479.5 L208.8 474 L210.1 462.8 L217.3 453.4 L219.7 438.3 L218 432.7 L224.5 427.7 L233.7 436.2 L263.8 439.1 L268.6 435.7 L280 435.6 L305.8 451.9 L314 446.6 L323.8 431.9 L327.9 446.4 L340.9 463.4 L336.2 473.2 z",
                [9] = "M259.7 552.8 L259.8 536.1 L264.3 529.4 L259.8 515.6 L260.2 494.4 L267 486.4 L245.5 486.1 L228.7 478.4 L218 479.5 L208.8 474 L195.1 486.8 L179.5 487.3 L159.8 521.3 C191.7 539.8 226.7 550 259.7 552.8 z",
                [10] = "M336.2 473.2 L324.9 474.3 L311 480.9 L301.1 481.1 L277.8 486.5 L267 486.4 L260.2 494.4 L259.8 515.6 L264.3 529.4 L259.8 536.1 L259.7 552.8 C268.3 553.6 276.8 553.9 285.2 553.7 C304.8 553.4 325.5 550.6 346.3 544.8 L342.6 539.1 L342.5 520.6 L346.1 510.3 L344.6 494.6 L337 481.3 L336.2 473.2 z",
                [11] = "M346.3 544.8 C372.2 537.6 398.2 525.6 421.6 508.9 L422 487.5 L420.8 485.6 L414 486 L403.3 483.9 L398.7 477 L396.1 465.4 L387.2 455.7 L373.2 449 L357.5 434 L336 406.5 L330.4 424.6 L323.8 431.9 L327.9 446.4 L340.9 463.4 L336.2 473.2 L337 481.3 L344.6 494.6 L346.1 510.3 L342.5 520.6 L342.6 539.1 L346.3 544.8 z",
                [12] = "M321.6 381.5 L325.3 369.6 L325 357.5 L327.5 357.4 L330.9 360.1 L339.7 360.6 L344.6 342 L334 336.9 L319.2 343.4 L305.7 332 L301.4 337.3 L301 345.8 L321.6 381.5 z",
                [13] = "M451.4 447.4 L437.4 441 L435.7 433.8 L437.2 420.2 L433.7 404 L434.9 392.5 L420.3 381.2 L413.4 381.6 L400 372.9 L393 374 L379.5 362.5 L372.4 376.6 L368.4 376.5 L361.8 366.5 L356.7 347.4 L349.8 341.5 L344.6 342 L339.7 360.6 L330.9 360.1 L327.5 357.4 L325 357.5 L325.3 369.6 L321.6 381.5 L336 406.5 L357.5 434 L373.2 449 L387.2 455.7 L396.1 465.4 L398.7 477 L403.3 483.9 L414 486 L420.8 485.6 L426 483.3 L427.2 480 L431.8 473.4 L433.4 462.8 L440.2 455.2 L451.4 447.4 z",
                [14] = "M334 336.9 L341 319.9 L341 314 L343 311.3 L342.2 289.1 L329.5 270.7 L317.2 262 L312.5 271.2 L302.7 280.5 L302.7 285.5 L312 285.4 L320.2 297.1 L317.7 311.3 L310.1 321.9 L305.7 332 L319.2 343.4 L334 336.9 z",
                [15] = "M344.6 342 L349.8 341.5 L356.7 347.4 L361.8 366.5 L368.4 376.5 L372.4 376.6 L379.5 362.5 L376.1 345.9 L388.7 311.3 L392.5 295.2 L391.9 273.8 L385.7 260.7 L385.6 245.2 L378.7 248.4 L375.5 252.5 L371.6 254.4 L368.3 261.3 L362 260.9 L349.1 265.7 L345.8 269.8 L333.3 276.3 L342.2 289.1 L343 311.3 L341 314 L341 319.9 L334 336.9 L344.6 342 z",
                [16] = "M482.6 390.3 L491.5 364.8 L495.8 356.2 L496.5 342.4 L494.6 336 L495.7 334.1 L493.6 328.8 L495.6 319.7 L501.6 311.3 L502.1 303.7 L495.9 303.7 L486.8 292.2 L484.5 279.6 L488.2 269.5 L497 262.8 L497.9 258.8 L513.4 242.2 C510 230.6 505.6 219.1 500.3 207.8 L490.8 208.1 L487.8 210.3 L484.4 208.6 L475.4 215 L468.8 210.9 L453.9 215.4 L449 210 L437 210.5 L433.3 207.7 L430.8 199 L412.5 213.9 L407.2 227.3 L400.3 233.9 L396 235.2 L395.6 238.6 L392.6 242.1 L385.6 245.2 L385.7 260.7 L391.9 273.8 L392.5 295.2 L388.7 311.3 L376.1 345.9 L379.5 362.5 L393 374 L400 372.9 L413.4 381.6 L420.3 381.2 L434.9 392.5 L443.5 377.4 L453.8 379.9 L464.4 381.1 L469.5 386.4 L482.6 390.3 z",
                [17] = "M523.5 307.3 C523.1 286.3 520 264.2 513.4 242.2 L497.9 258.8 L497 262.8 L488.2 269.5 L484.5 279.6 L486.8 292.2 L495.9 303.7 L502.1 303.7 L501.6 311.3 L523.5 311.3 L523.5 307.3 z",
                [18] = "M421.6 508.9 C454.2 486 481.1 453.8 498.4 418.7 C515.8 383.7 523.6 345.7 523.5 311.3 L501.6 311.3 L495.6 319.7 L493.6 328.8 L495.7 334.1 L494.6 336 L496.5 342.4 L495.8 356.2 L491.5 364.8 L482.6 390.3 L479.1 402.4 L479.3 414.8 L470.7 430.2 L451.4 447.4 L440.2 455.2 L433.4 462.8 L431.8 473.4 L427.2 480 L426 483.3 L420.8 485.6 L422 487.5 L421.6 508.9 z",
                [19] = "M383.7 163.9 L388.7 164.5 L397.6 160.1 L405 158.9 L415 153 L424 152 L428.3 143.7 L442.7 130.6 C434.1 123 425 115.9 415.5 109.6 L404.1 126.1 L393.5 132.1 L393.5 142.1 L389.1 147.4 L386.9 158.4 L383.7 163.9 z",
                [20] = "M355.3 195.8 L367.4 187.2 L367.3 183.9 L374.5 179.6 L375 176.7 L378.2 176.5 L378.9 172.2 L386.9 158.4 L389.1 147.4 L393.5 142.1 L393.5 132.1 L390.2 130.6 L386.5 134.2 L383.3 134.2 L361.1 172.7 L354.6 183.9 L355.8 190.1 L353.8 193.3 L355.3 195.8 z",
                [21] = "M418.9 168.3 L424.2 161.4 L424 152 L415 153 L405 158.9 L397.6 160.1 L388.7 164.5 L383.7 163.9 L378.9 172.2 L378.2 176.5 L375 176.7 L374.5 179.6 L367.3 183.9 L367.4 187.2 L355.3 195.8 L345.8 221.8 L350.4 220.9 L384.3 201.4 L391 200.7 L407.4 188.2 L407.8 182.8 L418.9 168.3 z",
                [22] = "M418.9 168.3 L435.6 165.8 L449 170.4 L466.8 155.5 C459.4 146.7 451.3 138.3 442.7 130.6 L428.3 143.7 L424 152 L424.2 161.4 L418.9 168.3 z",
                [23] = "M333.3 276.3 L345.8 269.8 L349.1 265.7 L362 260.9 L368.3 261.3 L371.6 254.4 L375.5 252.5 L378.7 248.4 L392.6 242.1 L395.6 238.6 L396 235.2 L400.3 233.9 L407.2 227.3 L412.5 213.9 L439.7 191.8 L448.9 174.6 L449 170.4 L435.6 165.8 L418.9 168.3 L407.8 182.8 L407.4 188.2 L391 200.7 L384.3 201.4 L350.4 220.9 L342.5 222.4 L331.6 240.5 L333.1 243.9 L317.2 262 L329.5 270.7 L333.3 276.3 z",
                [24] = "M449 170.4 L448.9 174.6 L439.7 191.8 L430.8 199 L433.3 207.7 L437 210.5 L449 210 L453.9 215.4 L468.8 210.9 L475.4 215 L484.4 208.6 L487.8 210.3 L490.8 208.1 L500.3 207.8 C491.6 189.4 480.3 171.6 466.8 155.5 L449 170.4 z",
                [25] = "M320 90.4 L330.8 98.6 L336.6 99.3 L341.9 106.9 L358 106.3 L370.6 110.3 L383.1 124.6 L383.3 134.2 L386.5 134.2 L390.2 130.6 L393.5 132.1 L404.1 126.1 L415.5 109.6 C380.6 86.2 340.6 73.2 303.3 69.9 L303.3 79.1 L297.8 88.1 L305 86.7 L320 90.4 z",
                [26] = "M166 112.1 L179 116.4 L198.1 116.4 L207.8 108.2 L223.8 105.9 L243.5 98.2 L266.5 93.1 L286.1 94.3 L290.7 89.5 L297.8 88.1 L303.3 79.1 L303.3 69.9 C294.4 69.1 285.6 68.8 277 68.9 C239.3 69.5 197.5 79.5 159.8 101.4 L166 112.1 z",
                [27] = "M320 90.4 L305 86.7 L290.7 89.5 L286.1 94.3 L266.5 93.1 L243.5 98.2 L223.8 105.9 L222.6 109.6 L222.1 120.2 L220 121 L212.6 136.5 L210.3 136.6 L208.3 141.4 L202.9 145.1 L199.9 149.3 L195.4 151.5 L193.4 159.5 L186.6 170.4 L181.8 180.9 L183.2 184.6 L185.6 187.3 L194.8 184.1 L201.2 173 L218.1 159.2 L233.5 156.5 L237.7 159.1 L252.9 151.8 L250.7 139.1 L269.4 133.8 L277.5 129.2 L292.9 132.2 L297.6 127.7 L306.4 107.4 z",
                [28] = "M257.5 178 L261.6 178.3 L267.9 185.3 L275.2 182.6 L287.6 182.5 L283.8 207.7 L286.4 217 L286.2 235.3 L288.3 237.8 L287.8 241.9 L287.9 260.2 L289.3 264.5 L286.2 281.9 L281 281.9 L276.5 285.3 L267.7 288.3 L258.4 272.1 L259.4 259.7 L257.7 254.1 L253.7 249.6 L253 232.3 L254.8 215.2 L261.7 201.4 L257.5 178 z",
                [29] = "M159.8 101.4 C126 120.8 96 149.6 75.3 183.1 L84.6 192.7 L89.3 193 L91.1 195.1 L111.1 177.2 L120 181.5 L130.2 170.5 L139.8 141.9 L148 129.2 L157.9 123.1 L166 112.1 L159.8 101.4 z",
                [30] = "M235.5 273.1 L188.3 233.5 L188.4 227 L184.2 222.9 L182 212.2 L179.1 204.6 L180.7 201.7 L181.2 192.2 L183.2 184.6 L181.8 180.9 L186.6 170.4 L193.4 159.5 L195.4 151.5 L199.9 149.3 L202.9 145.1 L208.3 141.4 L210.3 136.6 L212.6 136.5 L220 121 L222.1 120.2 L222.6 109.6 L223.8 105.9 L207.8 108.2 L198.1 116.4 L179 116.4 L166 112.1 L157.9 123.1 L148 129.2 L139.8 141.9 L130.2 170.5 L120 181.5 L122 191 L127.7 200.5 L127.9 206.7 L130.5 208.7 L131 213.6 L133.9 216.7 L128.8 234 L126.2 242.8 L123.1 243.1 L121 247.5 L120.5 252.9 L185 276.4 L220 289.1 L235.5 273.1 z",
                [31] = "M267.7 288.3 L258.4 272.1 L259.4 259.7 L257.7 254.1 L253.7 249.6 L253 232.3 L254.8 215.2 L261.7 201.4 L252.9 151.8 L237.7 159.1 L233.5 156.5 L218.1 159.2 L201.2 173 L194.8 184.1 L185.6 187.3 L183.2 184.6 L181.2 192.2 L180.7 201.7 L179.1 204.6 L182 212.2 L184.2 222.9 L188.4 227 L188.3 233.5 L235.5 273.1 L254.9 289.4 L267.7 288.3 z",
                [32] = "M47 248.2 L55.4 249.7 L60.3 255 L71.3 254 L85.8 249.4 L88.5 251.2 L95.9 254.8 L99.6 258.6 L110.9 257.8 L120.5 252.9 L121 247.5 L123.1 243.1 L126.2 242.8 L128.8 234 L120.6 231.9 L112.8 232 L100.2 226.6 L94.9 229.7 L89.4 228.9 L79.5 218 L81 209.4 L86.2 207.7 L90.4 200 L91.1 195.1 L89.3 193 L84.6 192.7 L75.3 183.1 C62.4 203.7 53 225.9 47 248.2 z",
                [33] = "M42.1 270.3 L53.5 278.2 L81.4 279.1 L96.3 283.5 L115.1 282.7 L142.5 277.2 L161 279.3 L167.4 283.5 L185.2 280 L185 276.4 L120.5 252.9 L110.9 257.8 L99.6 258.6 L95.9 254.8 L88.5 251.2 L85.8 249.4 L71.3 254 L60.3 255 L55.4 249.7 L47 248.2 C45 255.5 43.4 262.9 42.1 270.3 z",
                [34] = "M185 276.4 L185.2 280 L167.4 283.5 L161 279.3 L142.5 277.2 L115.1 282.7 L96.3 283.5 L81.4 279.1 L53.5 278.2 L42.1 270.3 C39.7 284.2 38.6 298 38.6 311.3 L205.6 311.3 L216.8 300.7 L220 289.1 L185 276.4 z",
                [35] = "M212.5 368.9 L225.7 331.5 L235.3 328 L233.7 326.2 L236.2 311.3 L243.4 297.6 L254.9 289.4 L235.5 273.1 L220 289.1 L216.8 300.7 L205.6 311.3 L203 321.8 L202.8 339.8 L193.6 384.7 L212.5 368.9 z",
                [36] = "M41.8 350.3 L48.7 347.8 L71 351.3 L90 352.2 L106.7 348 L128.5 346.4 L138.2 344.2 L146.1 347 L157.5 346.2 L168.8 352.2 L169 338.4 L170.7 336.9 L171.4 334.5 L181.7 329.7 L184 323.6 L203 321.8 L205.6 311.3 L38.6 311.3 L38.7 315.4 C38.9 326.8 39.9 338.5 41.8 350.3 z",
                [37] = "M168.8 352.2 L157.5 346.2 L146.1 347 L138.2 344.2 L128.5 346.4 L106.7 348 L90 352.2 L71 351.3 L48.7 347.8 L41.8 350.3 C44.2 364.9 47.9 379.7 53.2 394.3 L106.3 375 L117.3 375.9 L123.7 381.8 L129.2 381.5 L140.1 387.4 L149.2 389.6 L153.5 379 L169.2 362 L169.5 354.8 L168.8 352.2 z",
                [38] = "M203 321.8 L184 323.6 L181.7 329.7 L171.4 334.5 L170.7 336.9 L169 338.4 L168.8 352.2 L169.5 354.8 L169.2 362 L153.5 379 L149.2 389.6 L151.8 392.5 L154.6 397.9 L154.7 405.1 L149.7 409.5 L150.7 420.7 L159 438.5 L160.5 453.3 L169.6 459.3 L178.4 459.3 L176.5 428.8 L186.5 417.4 L193.6 384.7 L202.8 339.8 L203 321.8 z",
                [39] = "M235.3 328 L225.7 331.5 L212.5 368.9 L213.3 388.3 L224.1 410 L264.1 340.8 L256.8 341.8 L248.8 338.5 L242.4 332.3 L238.9 332 L235.3 328 z",
                [40] = "M178.419 459.259L169.587 459.259L160.505 453.319L158.971 438.467L150.68 420.751L149.705 409.503L154.737 405.115L154.583 397.874L151.821 392.536L149.214 389.651L140.11 387.414L129.214 381.509L123.695 381.768L117.344 375.877L106.295 374.956L53.249 394.264C71.319 445.451 110.68 493.501 159.847 521.302L179.48 487.295L183.096 466.835L178.419 459.259ZM139.629 430.025L139.629 430.025L117.001 449.011L96.807 427.145L91.241 407.55L97.789 396.841L112.334 391.744L125.942 395.017L128.561 401.424L129.122 408.954Z",
                [41] = "M193.6 384.7 L186.5 417.4 L176.5 428.8 L178.4 459.3 L183.1 466.8 L179.5 487.3 L195.1 486.8 L208.8 474 L210.1 462.8 L217.3 453.4 L219.7 438.3 L218 432.7 L224.5 427.7 L221.1 424.6 L219.8 417.5 L224.1 410 L213.3 388.3 L212.5 368.9 L193.6 384.7 z",
            },

            LocationCenter_Point = new Dictionary<int, Point>()
            {
                [0] = new Point(277, 314), //Polar Sink
                [1] = new Point(327, 243), //Imperial Basin (East Sector)
                [2] = new Point(323, 204), //Imperial Basin (Center Sector)
                [3] = new Point(291, 204), //Imperial Basin (West Sector)
                [4] = new Point(274, 169), //Carthag
                [5] = new Point(348, 146), //Arrakeen
                [6] = new Point(451, 407), //Tuek's Sietch
                [7] = new Point(110, 201), //Sietch Tabr
                [8] = new Point(109, 408), //Habbanya Sietch
                [9] = new Point(241, 417), //Cielago North (West Sector)
                [10] = new Point(279, 421), //Cielago North (Center Sector)
                [11] = new Point(316, 416), //Cielago North (East Sector)
                [12] = new Point(236, 457), //Cielago Depression (West Sector)
                [13] = new Point(277, 454), //Cielago Depression (Center Sector)
                [14] = new Point(324, 461), //Cielago Depression (East Sector)
                [15] = new Point(215, 509), //Meridian (West Sector)
                [16] = new Point(253, 523), //Meridian (East Sector)
                [17] = new Point(296, 498), //Cielago South (West Sector)
                [18] = new Point(326, 497), //Cielago South (East Sector)
                [19] = new Point(353, 462), //Cielago East (West Sector)
                [20] = new Point(380, 468), //Cielago East (East Sector)
                [21] = new Point(312, 355), //Harg Pass (West Sector)
                [22] = new Point(333, 346), //Harg Pass (East Sector)
                [23] = new Point(388, 434), //False Wall South (West Sector)
                [24] = new Point(394, 393), //False Wall South (East Sector)
                [25] = new Point(319, 333), //False Wall East (Far South Sector)
                [26] = new Point(326, 320), //False Wall East (South Sector)
                [27] = new Point(332, 303), //False Wall East (Middle Sector)
                [28] = new Point(325, 284), //False Wall East (North Sector)
                [29] = new Point(316, 276), //False Wall East (Far North Sector)
                [30] = new Point(367, 355), //The Minor Erg (Far South Sector)
                [31] = new Point(361, 326), //The Minor Erg (South Sector)
                [32] = new Point(379, 295), //The Minor Erg (North Sector)
                [33] = new Point(371, 268), //The Minor Erg (Far North Sector)
                [34] = new Point(408, 365), //Pasty Mesa (Far South Sector)
                [35] = new Point(433, 336), //Pasty Mesa (South Sector)
                [36] = new Point(439, 278), //Pasty Mesa (North Sector)
                [37] = new Point(431, 234), //Pasty Mesa (Far North Sector)
                [38] = new Point(489, 279), //Red Chasm
                [39] = new Point(446, 473), //South Mesa (South Sector)
                [40] = new Point(460, 449), //South Mesa (Middle Sector)
                [41] = new Point(507, 346), //South Mesa (North Sector)
                [42] = new Point(428, 129), //Basin
                [43] = new Point(367, 179), //Rim Wall West
                [44] = new Point(393, 187), //Hole In The Rock
                [45] = new Point(428, 159), //Sihaya Ridge
                [46] = new Point(383, 235), //Shield Wall (South Sector)
                [47] = new Point(355, 235), //Shield Wall (North Sector)
                [48] = new Point(464, 185), //Gara Kulon
                [49] = new Point(400, 118), //OH Gap (East Sector)
                [50] = new Point(357, 93), //OH Gap (Middle Sector)
                [51] = new Point(312, 80), //OH Gap (West Sector)
                [52] = new Point(278, 82), //Broken Land (East Sector)
                [53] = new Point(219, 97), //Broken Land (West Sector)
                [54] = new Point(272, 114), //Tsimpo (East Sector)
                [55] = new Point(230, 137), //Tsimpo (Middle Sector)
                [56] = new Point(192, 178), //Tsimpo (West Sector)
                [57] = new Point(275, 223), //Arsunt (East Sector)
                [58] = new Point(266, 268), //Arsunt (West Sector)
                [59] = new Point(124, 156), //Rock Outcroppings (North Sector)
                [60] = new Point(81, 180), //Rock Outcroppings (South Sector)
                [61] = new Point(198, 132), //Plastic Basin (North Sector)
                [62] = new Point(161, 161), //Plastic Basin (Middle Sector)
                [63] = new Point(172, 254), //Plastic Basin (South Sector)
                [64] = new Point(235, 188), //Hagga Basin (East Sector)
                [65] = new Point(199, 203), //Hagga Basin (West Sector)
                [66] = new Point(71, 214), //Bight Of The Cliff (North Sector)
                [67] = new Point(67, 244), //Bight Of The Cliff (South Sector)
                [68] = new Point(85, 261), //Funeral Plain
                [69] = new Point(143, 295), //The Great Flat
                [70] = new Point(236, 287), //Wind Pass (Far North Sector)
                [71] = new Point(227, 303), //Wind Pass (North Sector)
                [72] = new Point(213, 327), //Wind Pass (South Sector)
                [73] = new Point(207, 357), //Wind Pass (Far South Sector)
                [74] = new Point(155, 327), //The Greater Flat
                [75] = new Point(84, 371), //Habbanya Erg (West Sector)
                [76] = new Point(145, 375), //Habbanya Erg (East Sector)
                [77] = new Point(192, 334), //False Wall West (North Sector)
                [78] = new Point(182, 364), //False Wall West (Middle Sector)
                [79] = new Point(163, 449), //False Wall West (South Sector)
                [80] = new Point(241, 336), //Wind Pass North (North Sector)
                [81] = new Point(247, 355), //Wind Pass North (South Sector)
                [82] = new Point(79, 408), //Habbanya Ridge Flat (West Sector)
                [83] = new Point(152, 485), //Habbanya Ridge Flat (East Sector)
                [84] = new Point(205, 402), //Cielago West (North Sector)
                [85] = new Point(201, 463), //Cielago West (South Sector)
            },

            LocationSpice_Point = new Dictionary<int, Point>()
            {
                [11] = new Point(309, 393), //Cielago North (East Sector)
                [17] = new Point(290, 539), //Cielago South (West Sector)
                [33] = new Point(362, 272), //The Minor Erg (Far North Sector)
                [38] = new Point(510, 292), //Red Chasm
                [40] = new Point(491, 401), //South Mesa (Middle Sector)
                [45] = new Point(443, 144), //Sihaya Ridge
                [50] = new Point(335, 86), //OH Gap (Middle Sector)
                [53] = new Point(196, 99), //Broken Land (West Sector)
                [60] = new Point(94, 176), //Rock Outcroppings (South Sector)
                [65] = new Point(216, 233), //Hagga Basin (West Sector)
                [68] = new Point(59, 266), //Funeral Plain
                [69] = new Point(55, 293), //The Great Flat
                [75] = new Point(60, 367), //Habbanya Erg (West Sector)
                [80] = new Point(233, 340), //Wind Pass North (North Sector)
                [83] = new Point(133, 483), //Habbanya Ridge Flat (East Sector)
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
                [Faction.White] = "Richese"
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

                { Faction.Brown, DEFAULT_ART_LOCATION + "/art/faction9force.svg" },
                { Faction.White, DEFAULT_ART_LOCATION + "/art/faction10force.svg" },
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
                [Faction.White] = "#b3afa4bb"
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
                [Faction.White] = "#b3afa4"
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
                [Faction.White] = "forces"
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
                [Faction.White] = "No-Field"
            },

            LeaderSkillCardName_STR = new Dictionary<LeaderSkill, string>()
            {
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
                [LeaderSkill.Bureaucrat] = DEFAULT_ART_LOCATION + "/art/Bureaucrat.gif",
                [LeaderSkill.Diplomat] = DEFAULT_ART_LOCATION + "/art/Diplomat.gif",
                [LeaderSkill.Decipherer] = DEFAULT_ART_LOCATION + "/art/Decipherer.gif",
                [LeaderSkill.Smuggler] = DEFAULT_ART_LOCATION + "/art/Smuggler.gif",
                [LeaderSkill.Graduate] = DEFAULT_ART_LOCATION + "/art/Graduate.gif",
                [LeaderSkill.Planetologist] = DEFAULT_ART_LOCATION + "/art/Planetologist.gif",
                [LeaderSkill.Warmaster] = DEFAULT_ART_LOCATION + "/art/Warmaster.gif",
                [LeaderSkill.Adept] = DEFAULT_ART_LOCATION + "/art/Adept.gif",
                [LeaderSkill.Swordmaster] = DEFAULT_ART_LOCATION + "/art/Swordmaster.gif",
                [LeaderSkill.KillerMedic] = DEFAULT_ART_LOCATION + "/art/KillerMedic.gif",
                [LeaderSkill.MasterOfAssassins] = DEFAULT_ART_LOCATION + "/art/MasterOfAssassins.gif",
                [LeaderSkill.Sandmaster] = DEFAULT_ART_LOCATION + "/art/Sandmaster.gif",
                [LeaderSkill.Thinker] = DEFAULT_ART_LOCATION + "/art/Mentat.gif",
                [LeaderSkill.Banker] = DEFAULT_ART_LOCATION + "/art/Banker.gif"
            },

            StrongholdCardName_STR = new Dictionary<int, string>()
            {
                [2] = "Carthag",
                [3] = "Arrakeen",
                [4] = "Tuek's Sietch",
                [5] = "Sietch Tabr",
                [6] = "Habbanya Sietch",
                [42] = "Hidden Mobile Stronghold"
            },

            StrongholdCardImage_URL = new Dictionary<int, string>()
            {
                [2] = DEFAULT_ART_LOCATION + "/art/Carthag.gif",
                [3] = DEFAULT_ART_LOCATION + "/art/Arrakeen.gif",
                [4] = DEFAULT_ART_LOCATION + "/art/TueksSietch.gif",
                [5] = DEFAULT_ART_LOCATION + "/art/SietchTabr.gif",
                [6] = DEFAULT_ART_LOCATION + "/art/HabbanyaSietch.gif",
                [42] = DEFAULT_ART_LOCATION + "/art/HMS.gif"
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

            MapDimensions = new Point(563, 626),
            PlanetRadius = 242,
            MapRadius = 260,
            PlanetCenter = new Point(281, 311),
            PlayerTokenRadius = 11,

            SpiceDeckLocation = new Point(0, 540),
            TreacheryDeckLocation = new Point(475, 540),
            CardSize = new Point(40, 56),

            BattleScreenWidth = 273,
            BattleScreenHeight = 273,

            BattleScreenHeroX = 47,
            BattleScreenHeroY = 150,
            BattleWheelHeroWidth = 86,
            BattleWheelHeroHeight = 86,

            BattleWheelForcesX = 137,
            BattleWheelForcesY = 22,

            BattleWheelCardX = 148,
            BattleWheelCardY = 102,
            BattleWheelCardWidth = 86,
            BattleWheelCardHeight = 120,

            //Monster token
            MONSTERTOKEN_RADIUS = 13,

            //Force tokens
            FORCETOKEN_FONT = "normal normal bold 8px Verdana, Open Sans, Calibri, Tahoma, sans-serif",
            FORCETOKEN_FONTCOLOR = "white",
            FORCETOKEN_SPECIAL_FONTCOLOR = "gold",
            FORCETOKEN_FONT_BORDERCOLOR = "black",
            FORCETOKEN_SPECIAL_BORDERCOLOR = "gold",
            FORCETOKEN_FONT_BORDERWIDTH = 1,
            FORCETOKEN_BORDERCOLOR = "white",
            FORCETOKEN_BORDERWIDTH = 1,
            FORCETOKEN_SPECIAL_BORDERWIDTH = 2,
            FORCETOKEN_RADIUS = 8,

            //Spice tokens
            RESOURCETOKEN_FONT = "normal normal bold 8px Verdana, Open Sans, Calibri, Tahoma, sans-serif",
            RESOURCETOKEN_FONTCOLOR = "white",
            RESOURCETOKEN_FONT_BORDERCOLOR = "black",
            RESOURCETOKEN_FONT_BORDERWIDTH = 1,
            RESOURCETOKEN_COLOR = "rgba(255,140,60,0.9)",
            RESOURCETOKEN_BORDERCOLOR = "white",
            RESOURCETOKEN_RADIUS = 8,

            //Other highlights
            HIGHLIGHT_OVERLAY_COLOR = "rgba(255,255,255,0.5)",
            METHEOR_OVERLAY_COLOR = "rgba(209,247,137,0.5)",
            BLOWNSHIELDWALL_OVERLAY_COLOR = "rgba(137,238,247,0.5)",
            STORM_OVERLAY_COLOR = "rgba(255,100,100,0.5)",
            STORM_PRESCIENCE_OVERLAY_COLOR = "rgba(255,100,100,0.2)",

            //Card piles
            CARDPILE_FONT = "normal normal normal 20px Advokat, Calibri, Tahoma, sans-serif",
            CARDPILE_FONTCOLOR = "white",
            CARDPILE_FONT_BORDERCOLOR = "black",
            CARDPILE_FONT_BORDERWIDTH = 1,

            //Phases
            PHASE_FONT = "normal normal normal 10px Advokat, Calibri, Tahoma, sans-serif",
            PHASE_ACTIVE_FONT = "normal normal normal 18px Advokat, Calibri, Tahoma, sans-serif",
            PHASE_FONTCOLOR = "white",
            PHASE_ACTIVE_FONTCOLOR = "rgb(231,191,60)",
            PHASE_FONT_BORDERCOLOR = "black",
            PHASE_FONT_BORDERWIDTH = 1,
            PHASE_ACTIVE_FONT_BORDERWIDTH = 1,

            //Player names
            PLAYERNAME_FONT = "normal normal normal 10px Advokat, Calibri, Tahoma, sans-serif",
            PLAYERNAME_FONTCOLOR = "white",
            PLAYERNAME_FONT_BORDERCOLOR = "black",
            PLAYERNAME_FONT_BORDERWIDTH = 1,

            SKILL_FONT = "normal normal normal 7px Advokat, Calibri, Tahoma, sans-serif",
            SKILL_FONTCOLOR = "white",
            SKILL_FONT_BORDERCOLOR = "black",
            SKILL_FONT_BORDERWIDTH = 1,

            TABLEPOSITION_BACKGROUNDCOLOR = "rgb(231,191,60)",

            //Turns
            TURN_FONT = "normal normal normal 18px Advokat, Calibri, Tahoma, sans-serif",
            TURN_FONT_COLOR = "white",
            TURN_FONT_BORDERCOLOR = "black",
            TURN_FONT_BORDERWIDTH = 1,

            //Wheel
            WHEEL_FONT = "normal normal normal 24px Advokat, Calibri, Tahoma, sans-serif",
            WHEEL_FONTCOLOR = "black",
            WHEEL_FONT_AGGRESSOR_BORDERCOLOR = "white",
            WHEEL_FONT_DEFENDER_BORDERCOLOR = "white",
            WHEEL_FONT_BORDERWIDTH = 2,

            //Shadows
            SHADOW = "#000000AA",

            //General
            GAMEVERSION_FONT = "normal normal normal 16px Advokat, Calibri, Tahoma, sans-serif;",
            PLAYEDCARD_MESSAGE_FONT = "normal normal normal 20px Advokat, Calibri, Tahoma, sans-serif",
            FACTION_INFORMATIONCARDSTYLE = "font: normal normal normal 14px Verdana, Open Sans, Calibri, Tahoma, sans-serif; color: white; padding: 5px 5px 5px 5px; overflow: auto; line-height: 95%; background-color: rgba(32,32,32,0.95); border-color: grey; border-style: solid; border-width: 1px; border-radius: 3px;",
            TRACKER_FONT = "normal normal normal 12px Verdana, Open Sans, Calibri, Tahoma, sans-serif;",
            JSPANEL_DEFAULTSTYLE = "font-family: Verdana, Open Sans, Calibri, Tahoma, sans-serif"
        };

        public static Skin Current { get; set; } = Dune1979;
    }
}
