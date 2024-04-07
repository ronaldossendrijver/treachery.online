/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Treachery.Client;

public class Skin : IDescriber
{
    #region Attributes

    public const int CurrentVersion = 151;
    public const string DEFAULT_ART_LOCATION = ".";

    public string Description;
    public int Version;

    public Dictionary<Concept, string> Concept_STR;
    public Dictionary<MainPhase, string> MainPhase_STR;
    public Dictionary<int, string> PersonName_STR;
    public Dictionary<int, string> PersonImage_URL;
    public Dictionary<int, string> TerritoryName_STR;
    public Dictionary<int, string> TerritoryBorder_SVG;
    public Dictionary<int, PointD> LocationCenter_Point;
    public Dictionary<int, PointD> LocationSpice_Point;
    public Dictionary<Faction, string> FactionName_STR;
    public Dictionary<Faction, string> FactionImage_URL;
    public Dictionary<Faction, string> FactionTableImage_URL;
    public Dictionary<Faction, string> FactionFacedownImage_URL;
    public Dictionary<Faction, string> FactionForceImage_URL;
    public Dictionary<Faction, string> FactionSpecialForceImage_URL;
    public Dictionary<Faction, string> FactionColor;
    public Dictionary<Faction, string> ForceName_STR;
    public Dictionary<Faction, string> SpecialForceName_STR;
    public Dictionary<TechToken, string> TechTokenName_STR;
    public Dictionary<TechToken, string> TechTokenImage_URL;
    public Dictionary<Milestone, string> Sound;
    public Dictionary<TreacheryCardType, string> TreacheryCardType_STR;
    public Dictionary<int, string> TreacheryCardName_STR;
    public Dictionary<int, string> TreacheryCardDescription_STR;
    public Dictionary<int, string> TreacheryCardImage_URL;
    public Dictionary<TechToken, string> TechTokenDescription_STR;
    public Dictionary<int, string> ResourceCardImage_URL;
    public Dictionary<LeaderSkill, string> LeaderSkillCardName_STR;
    public Dictionary<LeaderSkill, string> LeaderSkillCardImage_URL;
    public Dictionary<int, string> StrongholdCardName_STR;
    public Dictionary<StrongholdAdvantage, string> StrongholdCardImage_URL;
    public Dictionary<World, string> HomeWorldImage_URL;
    public Dictionary<World, string> HomeWorldCardImage_URL;
    public Dictionary<Faction, string> NexusCardImage_URL;
    public Dictionary<TerrorType, string> TerrorTokenName_STR;
    public Dictionary<TerrorType, string> TerrorTokenDescription_STR;
    public Dictionary<DiscoveryToken, string> DiscoveryTokenName_STR;
    public Dictionary<DiscoveryToken, string> DiscoveryTokenDescription_STR;
    public Dictionary<DiscoveryToken, string> DiscoveryTokenImage_URL;
    public Dictionary<DiscoveryTokenType, string> DiscoveryTokenTypeName_STR;
    public Dictionary<DiscoveryTokenType, string> DiscoveryTokenTypeImage_URL;
    public Dictionary<Ambassador, string> AmbassadorImage_URL;
    public Dictionary<Ambassador, string> AmbassadorName_STR;
    public Dictionary<Ambassador, string> AmbassadorDescription_STR;

    public bool DrawResourceIconsOnMap { get; set; }
    public bool ShowVerboseToolipsOnMap { get; set; }
    public bool ShowArrowsForRecentMoves { get; set; }

    public string MusicGeneral_URL;
    public string MusicResourceBlow_URL;
    public string MusicSetup_URL;
    public string MusicBidding_URL;
    public string MusicShipmentAndMove_URL;
    public string MusicBattle_URL;
    public string MusicBattleClimax_URL;
    public string MusicMentat_URL;

    public string Sound_YourTurn_URL;
    public string Sound_Chatmessage_URL;

    public string Map_URL;
    public string Eye_URL;
    public string EyeSlash_URL;
    public string CardBack_ResourceCard_URL;
    public string CardBack_TreacheryCard_URL;
    public string BattleScreen_URL;
    public string Messiah_URL;
    public string Monster_URL;
    public string Harvester_URL;
    public string Resource_URL;
    public string HMS_URL;
    public string HighThreshold_URL;
    public string LowThreshold_URL;

    public PointD MapDimensions;
    public PointD PlanetCenter;
    public int PlanetRadius;
    public int MapRadius;
    public int PlayerTokenRadius;

    public PointD SpiceDeckLocation;
    public PointD TreacheryDeckLocation;
    public PointD CardSize;

    public int BattleScreenWidth;
    public int BattleScreenHeroX;
    public int BattleScreenHeroY;
    public int BattleWheelHeroWidth;
    public int BattleWheelHeroHeight;
    public int BattleWheelForcesX;
    public int BattleWheelForcesY;
    public int BattleWheelCardX;
    public int BattleWheelCardY;
    public int BattleWheelCardWidth;
    public int BattleWheelCardHeight;

    //Monster token
    public int MONSTERTOKEN_RADIUS;

    //Force tokens
    public string FORCETOKEN_FONT;
    public string FORCETOKEN_FONTCOLOR;
    public string FORCETOKEN_FONT_BORDERCOLOR;
    public int FORCETOKEN_FONT_BORDERWIDTH;
    public int FORCETOKEN_RADIUS;

    //Spice tokens
    public string RESOURCETOKEN_FONT;
    public string RESOURCETOKEN_FONTCOLOR;
    public string RESOURCETOKEN_FONT_BORDERCOLOR;
    public int RESOURCETOKEN_FONT_BORDERWIDTH;
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
    public string SHADOW;

    //General
    public string FACTION_INFORMATIONCARDSTYLE;

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
            return "-";
        return result;
    }

    public string Join(IEnumerable items)
    {
        var itemsAsObjects = new List<object>();
        foreach (var item in items) itemsAsObjects.Add(item);

        return Join(itemsAsObjects);
    }

    public PointD GetCenter(Location location, Map map)
    {
        if (location is HiddenMobileStronghold hms)
        {
            if (hms.AttachedToLocation != null)
            {
                var dX = hms.AttachedToLocation.Territory == map.RockOutcroppings ? 22 : 0;
                var dY = hms.AttachedToLocation.Territory == map.RockOutcroppings ? -10 : 0;
                var attachedToCenter = GetCenter(hms.AttachedToLocation, map);
                return new PointD(attachedToCenter.X + (int)HmsDX + dX, attachedToCenter.Y + dY);
            }

            return new PointD((int)HmsDX, (int)HmsDX);
        }

        if (location is DiscoveredLocation ds)
        {
            if (ds.AttachedToLocation != null)
            {
                var attachedToCenter = GetCenter(ds.AttachedToLocation, map);
                return new PointD(attachedToCenter.X - 1.5f * FORCETOKEN_RADIUS, attachedToCenter.Y - 1.5f * FORCETOKEN_RADIUS);
            }

            return new PointD(-4 * FORCETOKEN_RADIUS, -4 * FORCETOKEN_RADIUS);
        }

        return LocationCenter_Point[location.Id];
    }

    public float HmsDX => -3 * PlayerTokenRadius;

    public float HmsRadius => 1.5f * PlayerTokenRadius;

    public PointD GetSpiceLocation(Location location)
    {
        return location.SpiceBlowAmount != 0 ? LocationSpice_Point[location.Id] : new PointD(0, 0);
    }

    public string Describe(object value)
    {
        return value switch
        {
            null => "",
            string str => str,
            bool b => b ? "Yes" : "No",
            Message msg => msg.ToString(this),
            MessagePart part => part.ToString(this),
            Payment payment => Format("{0} {1}", payment.Amount, Concept.Resource),
            Concept c => Describe(c),
            Faction faction => Describe(faction),
            Ambassador ambassador => Describe(ambassador),
            FactionForce ff => Describe(ff),
            FactionSpecialForce fsf => Describe(fsf),
            FactionAdvantage factionadvantage => Describe(factionadvantage),
            ResourceCard rc => Describe(rc),
            TreacheryCard tc => Describe(tc),
            TreacheryCardType tct => Describe(tct),
            LeaderSkill ls => Describe(ls),
            TerrorType terr => Describe(terr),
            DiscoveryToken dt => Describe(dt),
            DiscoveryTokenType dtd => Describe(dtd),
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
            ClairVoyanceAnswer cva => Describe(cva),
            IEnumerable ienum => Join(ienum.Cast<object>()),
            _ => value.ToString()
        };
    }

    private string[] Describe(object[] objects)
    {
        var result = new string[objects.Length];
        for (var i = 0; i < objects.Length; i++) result[i] = Describe(objects[i]);
        return result;
    }

    private static string FirstCharToUpper(string input)
    {
        if (input == null || input == "")
            return input;
        return input[0].ToString().ToUpper() + input[1..];
    }

    public string Describe(ClairVoyanceAnswer answer)
    {
        return answer switch
        {
            ClairVoyanceAnswer.No => "No.",
            ClairVoyanceAnswer.Yes => "Yes.",
            ClairVoyanceAnswer.Unknown => "I don't know...",
            _ => "?"
        };
    }

    public string Describe(Territory t)
    {
        return GetLabel(TerritoryName_STR, t.SkinId);
    }

    public string Describe(IHero hero)
    {
        return hero switch
        {
            null => "?",
            Leader l => GetLabel(PersonName_STR, l.SkinId),
            Messiah => Describe(Concept.Messiah),
            TreacheryCard tc => Describe(tc),
            _ => "?"
        };
    }

    public string Describe(Location l)
    {
        if (l.Orientation != "")
            return Format("{0} ({1} Sector)", l.Territory, l.Orientation);
        return Describe(l.Territory);
    }

    public string Describe(Faction f)
    {
        return GetLabel(FactionName_STR, f);
    }

    public string Describe(Concept c)
    {
        return GetLabel(Concept_STR, c);
    }

    public string Describe(TreacheryCardType t)
    {
        return GetLabel(TreacheryCardType_STR, t);
    }

    public string Describe(ResourceCard c)
    {
        if (c == null)
            return "?";
        if (c.IsShaiHulud)
            return Describe(Concept.Monster);
        if (c.IsGreatMaker)
            return Describe(Concept.GreatMonster);
        if (c.IsSandTrout)
            return Describe(Concept.BabyMonster);
        return Describe(c.Location.Territory);
    }

    public string Describe(TreacheryCard c)
    {
        return GetLabel(TreacheryCardName_STR, c.SkinId);
    }

    public string Describe(LeaderSkill l)
    {
        return GetLabel(LeaderSkillCardName_STR, l);
    }

    public string Describe(DiscoveryToken dt)
    {
        return GetLabel(DiscoveryTokenName_STR, dt);
    }

    public string GetDiscoveryTokenDescription(DiscoveryToken dt)
    {
        return GetLabel(DiscoveryTokenDescription_STR, dt);
    }

    public string GetImageURL(object obj)
    {
        return obj switch
        {
            null => "",
            TreacheryCard tc => GetImageURL(tc),
            ResourceCard rc => GetImageURL(rc),
            IHero h => GetImageURL(h),
            LeaderSkill ls => GetImageURL(ls),
            TechToken tt => GetImageURL(tt),
            Faction f => GetImageURL(f),
            Ambassador a => GetImageURL(a),
            FactionForce ff => GetImageURL(ff),
            FactionSpecialForce fsf => GetImageURL(fsf),
            StrongholdAdvantage adv => GetImageURL(adv),
            _ => ""
        };
    }

    public string GetImageURL(DiscoveryTokenType dtt)
    {
        return GetLabel(DiscoveryTokenTypeImage_URL, dtt);
    }

    public string GetImageURL(DiscoveryToken dt)
    {
        return GetLabel(DiscoveryTokenImage_URL, dt);
    }

    public string Describe(DiscoveryTokenType dtt)
    {
        return GetLabel(DiscoveryTokenTypeName_STR, dtt);
    }

    public string Describe(TerrorType terr)
    {
        return GetLabel(TerrorTokenName_STR, terr);
    }

    public string GetTerrorTypeDescription(TerrorType terr)
    {
        return GetLabel(TerrorTokenDescription_STR, terr);
    }

    public string Describe(Ambassador ambassador)
    {
        return GetLabel(AmbassadorName_STR, ambassador);
    }

    public string GetAmbassadorDescription(Ambassador ambassador)
    {
        return GetLabel(AmbassadorDescription_STR, ambassador);
    }

    public string GetImageURL(Ambassador ambassador)
    {
        return GetLabel(AmbassadorImage_URL, ambassador);
    }

    public string Describe(MainPhase p)
    {
        return GetLabel(MainPhase_STR, p);
    }

    public string Describe(TechToken tt)
    {
        return GetLabel(TechTokenName_STR, tt);
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
            FactionForce.Pink => Faction.Pink,
            FactionForce.Cyan => Faction.Cyan,
            _ => Faction.None
        };
    }

    public string Describe(FactionForce f)
    {
        return GetLabel(ForceName_STR, GetFaction(f));
    }

    private static Faction GetFaction(FactionSpecialForce f)
    {
        return f switch
        {
            FactionSpecialForce.Red => Faction.Red,
            FactionSpecialForce.Yellow => Faction.Yellow,
            FactionSpecialForce.Blue => Faction.Blue,
            FactionSpecialForce.Grey => Faction.Grey,
            FactionSpecialForce.White => Faction.White,
            _ => Faction.None
        };
    }

    public string Describe(FactionSpecialForce f)
    {
        return GetLabel(SpecialForceName_STR, GetFaction(f));
    }

    public string Describe(FactionAdvantage advantage)
    {
        return advantage switch
        {
            FactionAdvantage.None => "- Any stoppable advantage not listed below -",
            FactionAdvantage.GreenBiddingPrescience => Format("{0} seeing the card on auction", Faction.Green),
            FactionAdvantage.GreenSpiceBlowPrescience => Format("{0} seeing the top {1} card this turn", Faction.Green, Concept_STR[Concept.Resource]),
            FactionAdvantage.GreenBattlePlanPrescience => Format("{0} seeing part of the opponent battle plan", Faction.Green),
            FactionAdvantage.GreenUseMessiah => Format("{0} using the {1} in this battle", Faction.Green, Concept_STR[Concept.Messiah]),
            FactionAdvantage.BlackFreeCard => Format("{0} taking a free treachery card", Faction.Black),
            FactionAdvantage.BlackCaptureLeader => Format("{0} capturing a leader after this battle", Faction.Black),
            FactionAdvantage.BlackCallTraitorForAlly => Format("{0} calling TREACHERY for their ally in this battle", Faction.Black),
            FactionAdvantage.BlueAccompanies => Format("{0} accompanying shipment", Faction.Blue),
            FactionAdvantage.BlueAnnouncesBattle => Format("{0} flipping advisors to fighters*", Faction.Blue),
            FactionAdvantage.BlueIntrusion => Format("{0} becoming advisors on intrusion", Faction.Blue),
            FactionAdvantage.BlueUsingVoice => Format("{0} using Voice during this battle", Faction.Blue),
            FactionAdvantage.BlueWorthlessAsKarma => Format("{0} using a {1} as a {2} card*", Faction.Blue, TreacheryCardType.Useless, TreacheryCardType.Karma),
            FactionAdvantage.BlueCharity => Format("{0} receiving 2 {1} at {2}", Faction.Blue, Concept.Resource, MainPhase.Charity),
            FactionAdvantage.YellowControlsMonster => Format("{0} sending {1} when a subsequent {1} card is drawn", Faction.Yellow, Concept.Monster),
            FactionAdvantage.YellowRidesMonster => Format("{0} riding {1}", Faction.Yellow, Concept.Monster),
            FactionAdvantage.YellowNotPayingForBattles => Format("{0} not paying for forces in this battle", Faction.Yellow),
            FactionAdvantage.YellowSpecialForceBonus => Format("{0} counting {1} bonus in this battle", Faction.Yellow, FactionSpecialForce.Yellow),
            FactionAdvantage.YellowExtraMove => Format("{0} moving two territories as part of their move action", Faction.Yellow),
            FactionAdvantage.YellowProtectedFromStorm => Format("{0} not taking storm losses when hit", Faction.Yellow),
            FactionAdvantage.YellowProtectedFromMonster => Format("{0} not being devoured by {1}", Faction.Yellow, Concept.Monster),
            FactionAdvantage.YellowProtectedFromMonsterAlly => Format("{0} ally not being devoured by {1}", Faction.Yellow, Concept.Monster),
            FactionAdvantage.YellowStormPrescience => Format("{0} seeing the storm movement this turn", Faction.Yellow),
            FactionAdvantage.RedSpecialForceBonus => Format("{0} counting {1} bonus in this battle", Faction.Red, FactionSpecialForce.Red),
            FactionAdvantage.RedReceiveBid => Format("{0} receiving {1} for a treachery card", Faction.Red, Concept.Resource),
            FactionAdvantage.RedGiveSpiceToAlly => Format("{0} giving {1} to their ally*", Faction.Red, Concept.Resource),
            FactionAdvantage.RedLetAllyReviveExtraForces => Format("{0} allowing their ally to revive 3 extra forces", Faction.Red, Concept.Resource),
            FactionAdvantage.OrangeDetermineMoveMoment => Format("{0} shipping out of turn order (play before their normal turn)", Faction.Orange),
            FactionAdvantage.OrangeSpecialShipments => Format("{0} (and ally) shipping site-to-site or back to reserves", Faction.Orange),
            FactionAdvantage.OrangeShipmentsDiscount => Format("{0} shipping at half price", Faction.Orange),
            FactionAdvantage.OrangeShipmentsDiscountAlly => Format("{0} ally shipping at half price", Faction.Orange),
            FactionAdvantage.OrangeReceiveShipment => Format("{0} receiving {1} for a shipment", Faction.Orange, Concept.Resource),
            FactionAdvantage.PurpleRevivalDiscount => Format("{0} reviving at half price", Faction.Purple),
            FactionAdvantage.PurpleRevivalDiscountAlly => Format("{0} ally reviving at half price", Faction.Purple),
            FactionAdvantage.PurpleReplacingFaceDancer => Format("{0} replacing a face dancer at end of turn", Faction.Purple),
            FactionAdvantage.PurpleIncreasingRevivalLimits => Format("{0} increasing revival limits", Faction.Purple),
            FactionAdvantage.PurpleReceiveRevive => Format("{0} receiving {1} for revival", Faction.Purple, Concept.Resource),
            FactionAdvantage.PurpleEarlyLeaderRevive => Format("{0} allowing early revival of a leader*", Faction.Purple),
            FactionAdvantage.PurpleReviveGhola => Format("{0} reviving a leader as a Ghola*", Faction.Purple),
            FactionAdvantage.GreyMovingHMS => Format("{0} moving the Hidden Mobile Stronghold", Faction.Grey),
            FactionAdvantage.GreySpecialForceBonus => Format("{0} counting {1} bonus in this battle", Faction.Grey, FactionSpecialForce.Grey),
            FactionAdvantage.GreySelectingCardsOnAuction => Format("{0} selecting a card to go on top or bottom (play before they draw cards on auction)", Faction.Grey),
            FactionAdvantage.GreyCyborgExtraMove => Format("{0} moving {1} two territories", Faction.Grey, FactionSpecialForce.Grey),
            FactionAdvantage.GreyReplacingSpecialForces => Format("{0} replacing {1} lost in battle with {2}", Faction.Grey, FactionSpecialForce.Grey, FactionForce.Grey),
            FactionAdvantage.GreyAllyDiscardingCard => Format("{0} allowing their ally to replace a won card", Faction.Grey),
            FactionAdvantage.GreySwappingCard => Format("{0} replacing a treachery card during bidding", Faction.Grey),
            FactionAdvantage.BrownControllingCharity => Format("{0} receiving and giving {1} during {2}", Faction.Brown, Concept.Resource, MainPhase.Charity),
            FactionAdvantage.BrownDiscarding => Format("{0} discarding cards for {1} or a {2} card for its special effect*", Faction.Brown, Concept.Resource, TreacheryCardType.Useless),
            FactionAdvantage.BrownRevival => Format("{0} having unlimited force revival and reduced revival cost", Faction.Brown),
            FactionAdvantage.BrownEconomics => Format("{0} playing their Inflation token during {1}", Faction.Brown, MainPhase.Contemplate),
            FactionAdvantage.BrownReceiveForcePayment => Format("{0} collecting {1} payment for forces this battle", Faction.Brown, Concept.Resource),
            FactionAdvantage.BrownAudit => Format("{0} auditing their opponent after this battle", Faction.Brown),
            FactionAdvantage.WhiteAuction => Format("{0} auctioning a card from their card cache", Faction.White),
            FactionAdvantage.WhiteNofield => Format("{0} using a No-Field to ship", Faction.White),
            FactionAdvantage.WhiteBlackMarket => Format("{0} selling a card from their hand", Faction.White),

            _ => "Unknown"
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
            Ruleset.Expansion3BasicGame => "Ecaz & Moritani Expansion - Basic",
            Ruleset.Expansion3AdvancedGame => "Ecaz & Moritani Expansion - Advanced",
            Ruleset.AllExpansionsBasicGame => "All Expansions - Basic",
            Ruleset.AllExpansionsAdvancedGame => "All Expansions - Advanced",
            Ruleset.ServerClassic => "Server Classic",
            Ruleset.Custom => "Custom",

            _ => "unknown rule set"
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

            RuleGroup.ExpansionPinkAndCyanBasic => "Ecaz & Moritani Expansion",
            RuleGroup.ExpansionPinkAndCyanAdvanced => "Ecaz & Moritani Expansion, Advanced Rules",

            RuleGroup.House => "House Rules",

            _ => "unknown rule group"
        };
    }

    public string Describe(WinMethod m)
    {
        return m switch
        {
            WinMethod.Strongholds => "by number of victory points",
            WinMethod.Prediction => "by prediction",
            WinMethod.Timeout => "by running out of time",
            WinMethod.Forfeit => "by forfeit",
            WinMethod.YellowSpecial => Format("by {0} special victory", Faction.Yellow),
            WinMethod.OrangeSpecial => Format("by {0} special victory", Faction.Orange),
            _ => "None"
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
            Rule.YellowSeesStorm => Format("{0} determine storm movement with the Storm Deck", Faction.Yellow),
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
            Rule.StormDeckWithoutYellow => Format("Use the Storm Deck if {0} are not in play", Faction.Yellow),

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

            Rule.GreatMaker => "Great Maker",
            Rule.DiscoveryTokens => "Discovery Tokens",
            Rule.Homeworlds => "Homeworlds",
            Rule.NexusCards => "Nexus Cards",
            Rule.Expansion3TreacheryCards => "Expansion Treachery Cards",
            Rule.CyanAssassinate => Format("{0} can assassinate leaders", Faction.Cyan),
            Rule.PinkLoyalty => Format("{0} gain a loyal leader", Faction.Pink),

            Rule.CustomDecks => "Customized Treachery Card Deck",
            Rule.ExtraKaramaCards => Format("Add three extra {0} cards to the game", TreacheryCardType.Karma),
            Rule.FullPhaseKarma => Format("Full phase {0} (instead of single instance)", TreacheryCardType.Karma),
            Rule.YellowMayMoveIntoStorm => Format("{0} may move into storm", Faction.Yellow),
            Rule.BlueVoiceMustNameSpecialCards => "Voice must target special cards by name",
            Rule.BattlesUnderStorm => "Battles may happen under the storm",
            Rule.MovementBonusRequiresOccupationBeforeMovement => "Arrakeen/Carthag must be occupied before Ship&Move to grant ornithopters",
            Rule.AssistedNotekeeping => "Mentat: auto notekeeping of knowable info (spice owned, cards seen, ...)",
            Rule.AssistedNotekeepingForGreen => Format("Mentat: auto notekeeping for {0}", Faction.Green),
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

            Rule.BotsCannotAlly => "Bots may not initiate alliances",
            Rule.CardsCanBeTraded => Format("Allow players to give cards to each other"),
            Rule.PlayersChooseFactions => Format("Let players choose their factions at start"),
            Rule.RedSupportingNonAllyBids => Format("{0} may support bids of non-ally players", Faction.Red),
            Rule.BattleWithoutLeader => "Allow leaderless battles even if leaders are available",
            Rule.CapturedLeadersAreTraitorsToOwnFaction => "Captured leaders can be called as traitors by their original factions without a traitor card",
            Rule.DisableEndOfGameReport => "Disable end-of-game report (don't reveal player shields)",
            Rule.DisableOrangeSpecialVictory => Format("Disable {0} special victory condition", Faction.Orange),
            Rule.DisableResourceTransfers => Format("Only allow transfer of {0} by alliance rules", Concept.Resource),
            Rule.YellowAllyGetsDialedResourcesRefunded => Format("{0} ally may get {1} dialled in battles refunded in {2} phase", Faction.Yellow, Concept.Resource, MainPhase.Contemplate),

            _ => "unknown rule"
        };
    }

    #endregion Descriptions

    #region NamesAndImages

    public string GetTerritoryBorder(Territory t)
    {
        return t != null ? GetLabel(TerritoryBorder_SVG, t.SkinId) : "";
    }

    public string GetImageURL(TreacheryCard c)
    {
        return c != null ? GetLabel(TreacheryCardImage_URL, c.SkinId) : "";
    }

    public string GetImageURL(World w)
    {
        return GetLabel(HomeWorldImage_URL, w);
    }

    public string GetImageURL(StrongholdAdvantage a)
    {
        return GetLabel(StrongholdCardImage_URL, a);
    }

    public string GetImageURL(ResourceCard c)
    {
        return c != null ? GetLabel(ResourceCardImage_URL, c.SkinId) : "";
    }

    public string GetImageURL(LeaderSkill s)
    {
        return GetLabel(LeaderSkillCardImage_URL, s);
    }

    public string GetTreacheryCardDescription(TreacheryCard c)
    {
        return c != null ? GetLabel(TreacheryCardDescription_STR, c.SkinId) : "";
    }

    public object GetTechTokenDescription(TechToken t)
    {
        return GetLabel(TechTokenDescription_STR, t);
    }

    public string GetImageURL(IHero h)
    {
        if (h == null)
            return "";
        if (h is TreacheryCard)
            return GetLabel(TreacheryCardImage_URL, h.SkinId);
        if (h is Messiah)
            return Messiah_URL;
        return GetLabel(PersonImage_URL, h.SkinId);
    }
    public string GetImageURL(Faction faction)
    {
        return GetLabel(FactionImage_URL, faction);
    }

    public string GetFactionTableImageURL(Faction faction)
    {
        return GetLabel(FactionTableImage_URL, faction);
    }

    public string GetFactionFacedownImageURL(Faction faction)
    {
        return GetLabel(FactionFacedownImage_URL, faction);
    }

    public string GetFactionForceImageURL(Faction f)
    {
        return GetLabel(FactionForceImage_URL, f);
    }

    public string GetFactionSpecialForceImageURL(Faction f)
    {
        return GetLabel(FactionSpecialForceImage_URL, f);
    }

    public string GetImageURL(FactionForce ff)
    {
        return GetFactionForceImageURL(GetFaction(ff));
    }

    public string GetImageURL(FactionSpecialForce fsf)
    {
        return GetFactionSpecialForceImageURL(GetFaction(fsf));
    }

    public string GetHomeworldCardImageURL(World w)
    {
        return GetLabel(HomeWorldCardImage_URL, w);
    }

    public string GetNexusCardImageURL(Faction f)
    {
        return GetLabel(NexusCardImage_URL, f);
    }

    public string GetImageURL(TechToken tech)
    {
        return GetLabel(TechTokenImage_URL, tech);
    }

    private static string GetLabel<T>(Dictionary<T, string> labels, T key)
    {
        if (labels != null && labels.TryGetValue(key, out var result)) return result;

        return "";
    }

    public string GetSound(Milestone m)
    {
        return GetLabel(Sound, m);
    }

    public string GetFactionColor(Faction faction)
    {
        return GetLabel(FactionColor, faction);
    }

    public string GetFactionColorTransparant(Faction faction, string transparancy)
    {
        return GetLabel(FactionColor, faction) + transparancy;
    }

    #endregion NamesAndImages

    #region FactionManual

    public string GetFactionInfo_HTML(Game g, Faction f)
    {
        object[] parameters = {
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
            MainPhase_STR[MainPhase.Contemplate], //43
            PersonName_STR[1036], //44
            MainPhase_STR[MainPhase.Bidding], //45
            MainPhase_STR[MainPhase.Battle], //46
            MainPhase_STR[MainPhase.Charity], //47
            Faction.Pink, //48
            Faction.Cyan, //49
            g.Map.ImperialBasin // 50

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
            Faction.Pink => Format(GetPinkTemplate(g), parameters),
            Faction.Cyan => Format(GetCyanTemplate(g), parameters),
            _ => ""
        };
    }

    private static string SheetHeader(string atStart, int resources, int freeRevival, string theme)
    {
        return
            $"<div style='{{25}}'><p><strong>AT START:</strong> {atStart}. Start with {resources} {{16}}.</p><p><strong>FREE REVIVAL:</strong> {freeRevival}</p><h5>{theme}</h5>";
    }

    private static string Advantage(string advantageName, string advantageDescription, bool condition = true)
    {
        return condition ? $"<p><strong>{advantageName.ToUpper()}</strong> - {advantageDescription}" : "";
    }

    private static string Advantage(object title, string advantageDescription, bool condition = true)
    {
        return Advantage(Current.Describe(title), advantageDescription, condition);
    }

    private static string Advantage(Game g, Rule rule, string advantageName, string advantageDescription)
    {
        return g.Applicable(rule) ? $"<p><strong>{advantageName.ToUpper()}</strong> - {advantageDescription}" : "";
    }

    private static string Advantage(Game g, Rule rule, object title, string advantageDescription)
    {
        return Advantage(g, rule, Current.Describe(title), advantageDescription);
    }

    private static string AdvancedHeader(Game g, params Rule[] rules)
    {
        return rules.Any(r => g.Applicable(r)) ? "<h5>Advanced Game Advantages</h5>" : "";
    }

    private static string GetGreenTemplate(Game g)
    {
        return SheetHeader("10 tokens in {6} and 10 in reserve (off-planet)", 10, 2,
                   "You have limited prescience") +
               Advantage("Bidding Prescience", "you see the treachery card on bid.") +
               Advantage("Spice Blow Prescience", "you see the top card of the {16} deck.") +
               Advantage("Battle Prescience",
                   "you may force your opponent to tell you your choice of one of the four elements he will use in his battle plan against you; the leader, the weapon, the defense or the number dialed. If your opponent tells you that he is not playing a weapon or defense, you may not ask something else.") +
               AdvancedHeader(g, Rule.GreenMessiah, Rule.AdvancedKarama) +
               Advantage(g, Rule.GreenMessiah, Concept.Messiah,
                   "after losing a total of at least 7 forces in battle, you may use the {20}. It cannot be used alone in battle but may add its +2 strength to any one leader or {13} per turn. If the leader or {13} is killed, the {20} has no effect in the battle. {20} can only be killed if blown up by a {18}/{17} explosion. A leader accompanied by {20} cannot turn traitor. If killed, the {20} must be revived like any other leader. The {20} has no effect on {0} leader revival.") +
               Advantage(g, Rule.AdvancedKarama, TreacheryCardType.Karma,
                   "you may use {19} to see one player's entire battle plan.") +
               @"<h5>Alliance</h5>
                <p>You may assist your allies by forcing their opponents to tell them one element of their battle plan.</p>
                <h5>Strategy</h5>
                <p>You are handicapped by the fact that you must both purchase cards and ship onto the planet, and you have no source of income other than {16}. This will keep you in constant battles. Since you start from {6} you have the movement advantage of 3 from the outset, and it is wise to protect this. Your prescience allows you to avoid being devoured by {15} and helps you to get some slight head start on the {16} blow. In addition, you can gain some slight advantage over those who would do battle with you by your foreknowledge of one element of their battle plan.</p>
                </div>";
    }


    private static string GetBlackTemplate(Game g)
    {
        return SheetHeader("10 tokens in {7} and 10 tokens in reserve (off-planet)", 10, 2,
                   "You excel in treachery") +
               Advantage("Traitors", "you keep all traitors you draw at the start of the game.") +
               Advantage("Treachery",
                   "you may hold up to 8 treachery cards. At start of game, you are dealt 2 cards instead of 1, and every time you buy a card you get an extra card free from the deck (if you have less than 8 total).") +
               Advantage(g, Rule.BlackMulligan, "Mulligan",
                   "at start, when you draw 2 or more of your own leaders as traitors, you may shuffle them back and redraw four traitors.") +
               AdvancedHeader(g, Rule.BlackCapturesOrKillsLeaders, Rule.AdvancedKarama) +
               Advantage(g, Rule.BlackCapturesOrKillsLeaders, "Captured leaders",
                   "every time you win a battle you can select randomly one leader from the loser (including the leader used in battle, if not killed, but excluding all leaders already used elsewhere that turn). You can kill that leader for 2 {16}; or use the leader once in a battle after which you must return it to the original owner. If all your own leaders have been killed, all captured leaders are immediately returned to their original owners. Killed captured leaders are put in the 'tanks' from which the original owners can revive them (subject to the revival rules).") +
               Advantage(g, Rule.AdvancedKarama, TreacheryCardType.Karma,
                   "during the Bidding phase, you may use {19} to take at random up to all treachery cards of any one player of your choice, as long has your maximum hand size is not exceeded. Then, for each card you took you must give him one of your cards in return.") +
               @"<h5>Alliance</h5>
                <p>Your traitors may be used against your allies opponents.</p>
                <h5>Strategy</h5>
                <p>Your major handicap is your difficulty in obtaining {16}. You are at your greatest relative strength at the beginning of the game and should capitalize on this fact by quickly buying as many treachery cards as you can, and then surging into battle. Since you get 2 cards for every one you bid for, you can afford to bid a little higher than most, but if you spend too lavishly at first you will not have enough {16} to ship in forces or buy more cards later. The large number of cards you may hold will increase your chances of holding worthless cards. To counteract this you should pick your battles, both to unload cards and to flush out the traitors in your pay.</p>
                </div>";
    }

    private static string GetYellowTemplate(Game g)
    {
        return SheetHeader(
                   "10 tokens distributed as you like on {9}, {10}, and {11}; and 10 tokens in on-planet reserves",
                   3, 3, "You are native to this planet and know its ways") +
               Advantage("Shipment",
                   "instead of shipping like other factions, you may bring any number of forces from your reserves onto any territory within two territories of and including {12} (subject to storm and occupancy rules).") +
               Advantage("Movement", "you may move two territories instead of one.") +
               Advantage(Concept.Monster,
                   "if {15} appears in a territory where you have forces, they are not devoured but, immediately upon conclusion of the following nexus phase, may move from that territory to any one territory (subject to storm and occupancy rules).") +
               Advantage("Special victory condition",
                   "if no player has won by the end of the last turn and if you (or no one) occupies {9} and {22} and neither {1}, {0}, {2} nor {35} occupy {8}, you and your ally have prevented interference with your plans to alter the planet and win the game.") +
               Advantage("Special victory condition",
                   "if no player has been able to win the game by the end of play, you prevented control over the planet and you and your ally automatically win the game.",
                   !g.IsPlaying(Faction.Orange) && !g.Applicable(Rule.DisableOrangeSpecialVictory)) +
               AdvancedHeader(g, Rule.YellowSeesStorm, Rule.YellowSendingMonster, Rule.YellowStormLosses,
                   Rule.YellowSpecialForces, Rule.AdvancedCombat, Rule.AdvancedKarama) +
               Advantage(g, Rule.YellowSeesStorm, "Storm rule",
                   "you can see the number of sectors the next storm will move.") +
               Advantage(g, Rule.YellowSendingMonster, "Sandworms",
                   "during a {16} blow, each time {15} appears after the first time, you choose in which unprotected territory it appears.") +
               Advantage(g, Rule.YellowStormLosses, MainPhase.Storm,
                   "If caught in a storm, only half your forces are killed. You may rally forces into a storm at half loss.") +
               Advantage(g, Rule.YellowSpecialForces, FactionSpecialForce.Yellow,
                   "Your 3 {23} are worth two normal forces in battle and in taking losses. They are treated as one token in revival. Only one {23} can be revived per turn.") +
               Advantage(g, Rule.AdvancedCombat, MainPhase.Battle,
                   "Your forces do not require {16} to count at full strength.") +
               Advantage(g, Rule.AdvancedKarama, TreacheryCardType.Karma,
                   "during {16} blow, you may use {19} to cause {15} to appear in any unprotected territory that you wish.") +
               @"<h5>Alliance</h5>
                <p>You may protect your allies from being devoured by {15} and may let them revive 3 forces for free.</p>
                <h5>Strategy</h5>
                <p>Your major handicap is poverty. You won't be available to buy cards early game. You must be patient and move your forces into any vacant strongholds, avoiding battles until you are prepared. In battles you can afford to dial high and sacrifice your troops since they have a high revival rate and you can bring them back into play at no cost. You have better mobility than those without a city, and good fighting leaders. Bide your time and wait for an accessible {16} blow that no one else wants in order to build up your resources.</p>
                </div>";
    }

    private static string GetRedTemplate(Game g)
    {
        return SheetHeader("20 tokens in reserve (off-planet)", 10, 1, "You have access to great wealth") +
               Advantage(MainPhase.Bidding, "Payments for treachery cards go to you.") +
               Advantage(g, Rule.RedSupportingNonAllyBids, "Support",
                   "you may support bids of non-allied players. Any {16} paid this way flows back to you at the end of the bidding phase.") +
               AdvancedHeader(g, Rule.RedSpecialForces) +
               Advantage(g, Rule.RedSpecialForces, FactionSpecialForce.Red,
                   "your 5 {24} have a special fighting capability. They are worth two normal tokens in battle and in taking losses against all opponents except {3}. They are treated as one token in revival. Only one {24} can be revived per turn.") +
               Advantage(g, Rule.AdvancedKarama, TreacheryCardType.Karma,
                   "you may use {19} to revive up to three tokens or one leader for free.") +
               @"<h5>Alliance</h5>
                <p>Unlike other factions, you may give {16} to your allies which is received immediately. Their payment for any treachery card even with your own {16} comes right back to you. In addition, you may pay (directly to the bank) for the revival of up to 3 extra of their forces (for a possible total of 6).</p>
                <h5>Strategy</h5>
                <p>Your major handicap is that you must ship in all of your tokens at the start of the game and often this requires a battle before you are prepared. Even though you do not need to forage for {16} on the surface of the planet often, you still are quite subject to attack since you are likely to concentrate on the cities for the mobility they give you. On the plus side you will never need {16} badly, since the bidding will keep you supplied.</p>
                </div>";
    }

    private static string GetOrangeTemplate(Game g)
    {
        return SheetHeader("5 tokens in {8} and 15 tokens in reserve (off-planet)", 5, 1,
                   "You control all shipments onto the planet") +
               Advantage("Payment for shipment",
                   "Payments by other players for shipments onto the planet from off-planet reserves go to you.") +
               Advantage("Types of shipment",
                   "You can make one of three possible types of shipments; you may ship normally from reserves onto the planet; or you may site-to-site ship any number of tokens from any one territory to any other territory on the board; or you may ship any number of tokens from any one territory back to your reserves.") +
               Advantage("Half Price",
                   "You pay only half the fee when shipping. The cost for shipping to your reserves is one {16} for every two tokens shipped.") +
               Advantage("Special victory condition",
                   "If no player has been able to win the game by the end of play, you prevented control over the planet and you and your ally automatically win the game.",
                   !g.Applicable(Rule.DisableOrangeSpecialVictory)) +
               AdvancedHeader(g, Rule.OrangeDetermineShipment) +
               Advantage(g, Rule.OrangeDetermineShipment, "Ship when you wish",
                   "During Shipment & Move, you decide when you take your turn. You do not have to reveal when you intend to take your turn until the moment you wish to take it.") +
               Advantage(g, Rule.AdvancedKarama, TreacheryCardType.Karma,
                   "You may use {19} to stop one off-planet shipment of any one player. You may do this directly before or after the shipment occurs.") +
               @"<h5>Alliance</h5>
                <p>Allies may also perform site-to-site shipments and, like you, pay half the fee.</p>
                <h5>Strategy</h5>
                <p>Your major handicap is your weak array of leaders and your inability to revive quickly. In addition, you usually cannot buy treachery cards at the beginning of the game. You are vulnerable at this point and should make your stronger moves after building up your resources. If players do not ship on at a steady rate you will have to fight for {16} on the planet or collect only the isolated blows. Your major advantage is that you can ship onto the planet inexpensively and can ship from any one territory to any other. This mobility allows you to make surprise moves and is particularly useful when you are the last player in the movement round. If the game is out of reach and well along, try suicide battles against the strongest players to weaken them and prevent a win until the last turn: the victory is then yours.</p>
                </div>";
    }

    private static string GetBlueTemplate(Game g)
    {
        return SheetHeader("1 token in {26} and 19 tokens in reserve (off-planet)", 5, 1,
                   "You are adept in the ways of mind control") +
               Advantage("Prediction",
                   "At start of game you secretly predict which faction will win in which turn (you can't predict the special victory condition of {4} or {3} at the end of play). If that factions wins (alone or as an ally, even your own) when you have predicted, you alone win instead. You can also win normally.") +
               Advantage("Shipment", "Whenever any other player ships, you may ship for free one force to {26}.") +
               Advantage("Voice",
                   "You may Voice your opponent in battle to play or not to play a projectile/poison weapon or defense, a worthless card or a <i>specific</i> special card. If he can't comply with your command, he may do as he wishes.") +
               AdvancedHeader(g, Rule.BlueFirstForceInAnyTerritory, Rule.BlueAutoCharity, Rule.BlueAdvisors,
                   Rule.BlueWorthlessAsKarma) +
               Advantage(g, Rule.BlueAutoCharity, MainPhase.Charity,
                   "You automatically receive 2 charity during the {47} Phase.") +
               Advantage(g, Rule.BlueAdvisors, "Advisors",
                   "You can peacefully coexist as <i>spiritual advisors</i> with opponent forces in the same territory. Advisors cannot collect {16}, cannot be involved in combat, don't prevent opponent control of a stronghold and don't yield three territory movement bonus. They are still susceptible to storms, {15} and {18}/{17} explosions. You can never have both advisors and fighters in the same territory.") +
               Advantage(g, Rule.BlueAdvisors, "At Start",
                   "Instead of starting with 1 force in {26} you may start with an advisor in a territory of choice.") +
               Advantage(g, Rule.BlueAdvisors, "Advisor Shipment",
                   "You can ship advisors to a territory where you have advisors and whenever any other player ships, instead of shipping a force to {26}, you may ship an advisor to the same territory.") +
               Advantage(g, Rule.BlueAdvisors, "End peaceful coexistence",
                   "You may announce before the {21} phase all territories in which you no longer wish to peacefully coexist. Advisors there are flipped to fighters.") +
               Advantage(g, Rule.BlueAdvisors, "Flipping",
                   "When you move forces into an occupied territory where you don't have forces or when another player moves tokens into a territory where you have advisors, you decide to stay there as either advisors or fighters.") +
               Advantage("Not with ally",
                   "Allied forces may not willingly end up in a territory where you have advisors (and vice versa).",
                   g.Applicable(Rule.BlueAdvisors) && !g.Applicable(Rule.AdvisorsDontConflictWithAlly)) +
               Advantage(g, Rule.BlueWorthlessAsKarma, TreacheryCardType.Karma,
                   "You may use a worthless card as a {19} card.") +
               @"<h5>Alliance Power</h5>
                <p>You may Voice your ally's opponent.</p>
                <h5>Strategy</h5>
                <p>Your major handicap is your low revival rate. Don't allow large numbers of your tokens to be killed or you may find yourself without sufficient reserves to bring onto the planet. Your strengths are that you have the ability to win by correctly predicting another winner and the secretly working for that player. In addition, you can be quite effective in battles by voicing your opponent and leaving him weaponless or defenseless. You can afford to bide your time while casting subtle innuendoes about which player you have picked to win.</p>
                </div>";
    }

    private static string GetPurpleTemplate(Game g)
    {
        return SheetHeader("20 tokens in reserve (off-planet)", 5, 2,
                   "You have superior genetic engineering technology") +
               Advantage("Face dancers",
                   @"At the start of the game you are not dealt Traitor Cards. After traitors are selected, unused traitor cards are shuffled and you get the top 3 cards. These are your Face Dancers. After a faction has won a battle you may reveal the leader they used to be a Face Dancer, and the following occurs:
                  <ol>
                  <li>The battle is fully resolved a normal.</li>
                  <li>The Face Dancer leader is sent to the {31} if it was not already killed. You don't collect {16} for it.</li>
                  <li>The remaining amount of forces in the territory go back to their reserves and are replaced by up to the same amount of your forces from your reserves and/or from anywhere on the planet.</li>
                  </ol>
                  Once revealed you do not replace a Face Dancer (Traitor Card) until you have revealed all 3. When that happens, they are shuffled into the Traitor deck and you get 3 new Face Dancers.
                  During {43}, you may replace an unrevealed Face Dancer with one from the traitor deck (after shuffling it back).") +
               Advantage("Unlimited Revival",
                   "You have no revival limits and your revive at half price (rounded up). Payments by other players for revivals go to you. Each time a player uses free revival or a Tleilaxu Ghola card, you get 1 {16} from the bank.") +
               Advantage("Increased Revival",
                   "You may increase the 3 force revival limit for any other faction to 5.") +
               Advantage("Leader Revival",
                   "Upon request by a faction for a specific killed leader, you can set a price for its early revival. This can only be done when this leader cannot be revived according to normal revival rules.") +
               Advantage("Zoal",
                   "{44}’s value in battle matches the value of the opponent’s leader (0 for a Cheap Hero), and for collecting {16} when killed. {44} costs 3 to revive.") +
               AdvancedHeader(g, Rule.PurpleGholas, Rule.AdvancedKarama) +
               Advantage("Gholas",
                   "When you have fewer than five leaders available, you may revive dead leaders of other factions at your discounted rate and add them to your leader pool.") +
               Advantage(g, Rule.AdvancedKarama, TreacheryCardType.Karma,
                   "you may prevent a player from performing a revival (forces and/or leader).") +
               @"<h5>Alliance Power</h5>
                <p>Your ally may revive at half price (rounded up).</p>
                <h5>Strategy</h5>
                <p>You are handicapped by having no forces on the planet to start and only a small amount of {16} until you begin receiving {16} for revivals. You will have to bide your time as other factions battle, waiting until you start gaining {16} and giving your Face Dancers a chance to suddenly strike, or get into minor battles early to drive forces to the tanks, and possibly get a Face Dancer reveal. Use your ability to cycle through Face Dancers during the Mentat Pause to position yourself with a potentially more useful Face Dancer.</p>
                </div>";
    }

    private static string GetGreyTemplate(Game g)
    {
        return SheetHeader("3 {32} and 3 {33} in the {34}. 10 {32} and 4 {33} in reserve (off-planet)", 10, 1,
                   "You are skilled in technology and production") +
               Advantage("Start of game",
                   "During Setup you see all initially dealt Treachery Cards and choose your starting card from them.") +
               Advantage("Bidding",
                   "Before Bidding, one extra card is drawn and you see them all and put one of those cards on top or on the bottom of the Treachery Card deck. The remaining cards are shuffled for the bidding round.") +
               Advantage(FactionSpecialForce.Grey,
                   "Your 7 {33}s are each worth 2 normal forces in battle, are able to move 2 territories instead of 1 and can collect 3 {16}. Your {33} forces ship normally, but each costs 3 to revive.") +
               Advantage(FactionForce.Grey,
                   "Your 13 {32} ship normally but are worth ½ in battle. {32} can be used to absorb losses after a battle. After battle losses are calculated, any of your surviving {32} in that territory can be exchanged for {33} you lost in that battle. {32} can control strongholds and collect {16}. {32} move 2 if accompanied by a {33}, or 1 if they are not.") +
               Advantage("Hidden Mobile Stronghold",
                   "After the first storm movement, place your {34} in any non-stronghold territory. This stronghold counts towards the game win and is protected from worms and storms. Subsequently, before storms are revealed and as long as your forces occupy it, you may move your {34} up to 3 territories to any non-stronghold territory. You can't move it into or out of a storm. When you move into, from, or through a sector containing {16}, you may immediately collect 2 {16} per force in your stronghold. No other faction may ship forces directly into your {34}, or move it if they take control. Other factions must move or ship forces into the territory it is pointing at (including {26}), and then use one movement to enter.") +
               AdvancedHeader(g, Rule.GreySwappingCardOnBid, Rule.AdvancedCombat, Rule.AdvancedKarama) +
               Advantage(g, Rule.GreySwappingCardOnBid, "Tech",
                   "Once, during the Bidding phase, before {0} gets to look and bidding begins, you may take the card about to be bid on and replace it with a card from your hand.") +
               Advantage(g, Rule.AdvancedCombat, "{32} strength",
                   "{32} are worth ½ in battle. You can’t increase the effectiveness of {32} in battle by spending {16}.") +
               Advantage(g, Rule.AdvancedKarama, TreacheryCardType.Karma,
                   "During Shipment and Movement, you may move the {34} 2 territories on your turn as well as make your normal movement.") +
               @"<h5>Alliance Power</h5>
                <p>After your ally purchases a Treachery Card during bidding, they may immediately discard it and draw the top card from the deck.</p>
                <h5>Strategy</h5>
                <p>You are handicapped by having weaker forces in the half strength {32}, which make up the bulk of your forces. You have no regular source of {16} income. However, tactical placement of your {34} can position you to acquire {16} left behind on the planet. You also have an advantage over other factions because you know what Treachery cards are in play and you can mix in or suppress certain cards during the bidding phase.</p>
                </div>";
    }

    private static string GetWhiteTemplate(Game g)
    {
        return SheetHeader(
                   "20 tokens in reserve (off-planet). Start with 5 {16}. You have a separate deck of 10 {35} cards that are not part of your hand",
                   5, 2, "You have alternative technology") +
               Advantage(MainPhase.Bidding,
                   @"When drawing Treachery cards up for bid, one less card is drawn and instead you choose to auction a card from your {35} cards first or last. When it is time to auction that card, you choose and reveal the card and the type of auction (Once Around or Silent). Payments for your cards by other players go to you. If you buy any, the {16} goes to the {2} or the {16} Bank normally. Whenever discarded, these cards go to the normal discard pile. They can’t be bought with {19}.
              <ul>
              <li><i>Once Around:</i> pick clockwise or counter-clockwise. Starting with the faction on one side of you, each player able to bid may pass or bid higher. You bid last and the highest bidder gets the card. If everyone else passes, you may either get the card for free or remove it from the game.</li>
              <li><i>Silent:</i> all factions able to bid secretly choose an amount to bid. Choices are revealed simultaneously. The faction that bid to most wins the card (ties break according to Storm order). If all factions bid zero {16}, you may either get the card for free or remove it from the game.</li>
              </ul>") +
               Advantage(MainPhase.ShipmentAndMove,
                   @"Instead of normal shipping, you may ship one of your No-Field tokens (0, 3 or 5) face-down, paying for one force. Other factions do not know how many of your forces are located there and proceed as if at least one is there. You may reveal a No-Field token at any time before the Battle phase, placing the indicated number of forces from your reserves (or up to that amount if you have fewer forces left). You may move a No-Field token like a force.<br>
              You may not have two No-Field tokens on the planet at the same time. The last revealed token stays face-up in front of your shield until another one is revealed.<br>
              When you are in a battle, you must reveal the value of a No-Field token in that territory and place the indicated number of forces from your reserves (or up to that amount if you have fewer forces left). When you are in a Battle with a No-Field token, {0} may not see your force dial.") +
               AdvancedHeader(g, Rule.WhiteBlackMarket, Rule.AdvancedCombat) +
               Advantage(g, Rule.WhiteBlackMarket, "Black Market",
                   "At the start of the Bidding Round, you may auction one card from your hand. You may tell what you are selling (you may lie) and keep the card face-down ({0} may still look). You may use any type of auction. If no player bids, you keep the card. If it is sold, one fewer card is put up for auction as part of the normal Bidding Round. Payments by other players go to you. Black market cards cannot be bought with {19}. If the normal bidding type was used, the regular bidding round resumes wherever it left off.") +
               Advantage(g, Rule.AdvancedKarama, TreacheryCardType.Karma,
                   "You may buy a card from your separate cache for 3 {16} at any time.") +
               @"<h5>Alliance</h5>
                <p>Your ally may ship using one of your available No-Field tokens, revealing it immediately upon shipping. Place the used No-Field token face-up in front of your shield until you reveal another No-Field token.</p>
                <p>You may give your ally a {35} card from your hand at any time.</p>
                <h5>Strategy</h5>
                <p>You are at a disadvantage by having no forces on the planet, and not much spice to operate. Try to be aware if factions would be inclined to buy one of your special Cards either for their use or to keep it out of the hands of another faction. Selling your cards will be your one regular form of income until you have gained enough spice. Use your No-Field tokens to get forces on the planet cheaply and confuse your opponents.</p>
                </div>";
    }

    private static string GetBrownTemplate(Game g)
    {
        return SheetHeader("20 tokens in reserve (off-planet)", 2, 0,
                   "You control economic affairs across the Imperium") +
               Advantage(MainPhase.Charity,
                   "Each turn, you collect 2 {16} for each faction in the game during Charity before any factions collect. If another faction collects Charity, it is paid to them from your {16}.") +
               Advantage(MainPhase.Resurrection,
                   "You have no limit to the number of forces you may pay to revive and it only costs you 1 for each force.") +
               Advantage("Treachery",
                   @"You may hold up to 5 Treachery Cards. At the end of any phase, you may reveal duplicates of the same card from your hand for 3 {16} each. You may also discard {14} cards for 2 {16} each. Alternatively, you may use {14} cards as follows:
                <ul>
                <li><i>{37}</i>: Prevent a player from moving forces into a territory you occupy during {21}. They may ship in normally.</li>
                <li><i>{38}</i>: Prevent a loss of your forces in one territory to the Storm when it moves.</li>
                <li><i>{39}</i>: Prevent a player from playing a {19} card this phase as they attempt to do so.</li>
                <li><i>{40}</i>: Move your forces one extra territory on your turn during {21}.</li>
                <li><i>{41}</i>: Prevent a player from taking Free Revival.</li>
                <li><i>{42}</i>: Force a player to send 1 force back to reserves during {43}.</li>
                </ul>") +
               Advantage("Inflation",
                   "You may play your Inflation token during {43} with either the Double or Cancel side face-up. In the following game turn, Charity is either doubled or canceled for that turn (including {5} in the advanced game). While Charity is doubled, no bribes can be made. In the next Mentat the Inflation token is flipped to the other side. If the token has already been flipped it is removed from the game instead.") +
               AdvancedHeader(g, Rule.BrownAuditor, Rule.AdvancedCombat, Rule.AdvancedKarama) +
               Advantage(g, Rule.AdvancedCombat, "Forces",
                   "When other players pay for their forces in battle, half of the {16} (rounded down) goes to you.") +
               Advantage(g, Rule.BrownAuditor, "Auditor",
                   "You gain the Auditor Leader and it is added to the Traitor deck at the start of the game. Whenever you use the Auditor as a leader in a battle, you may audit your opponent. You may look at two cards in your opponent's hand at random (not counting cards used in battle) if the Auditor survived, or one card if killed. That faction may pay you 1 {16} per card you would get to see to cancel the audit. The Auditor may be revived as if all of your leaders were in the Tanks. The Auditor can't be revived as a ghola, nor be captured by {1}. The Auditor can't have a Leader Skill.") +
               Advantage(g, Rule.AdvancedKarama, TreacheryCardType.Karma,
                   "You may discard any Treachery cards from your hand and gain 3 {16} each.") +
               @"<h5>Alliance</h5>
                <p>Once per turn at the end of a phase, you may trade a Treachery Card with your ally. This trade is simultaneous and two-way.</p>
                <p>You may pay for your ally’s forces in battle.</p>
                <h5>Strategy</h5>
                <p>Your leaders are weak, but you have a steady income. Stockpile Treachery Cards. You start with no forces on the planet and must ship them all in. For this reason, you may want to wait until you can attack with a large force. Use your Inflation token at a key moment, especially at a time when others aren’t collecting Charity.</p>
                </div>";
    }

    private static string GetPinkTemplate(Game g)
    {
        return SheetHeader("6 tokens in {50} and 14 in reserve (off-planet)", 12, 2, "You forge strong alliances") +
               Advantage("Ambassadors",
                   "At start you get the {48} Ambassador token and 5 random Ambassadors for other factions. At the end of Revival you may place Ambassadors in any stronghold not in storm that does not have one for 1 spice (cost increases by 1 spice for each subsequent Ambassador that turn). When another faction (other than your ally or the faction matching the marker) enters a stronghold with an Ambassador, you may trigger its effect, setting it aside. After triggering all non-{48} Ambassadors, you get 5 random new ones. Ambassadors are vulnerable to game effects like storm or explosions, returning to your supply.") +
               Advantage("Occupy",
                   "When you are in an alliance, you and your ally’s forces are considered the same faction, and may enter and occupy the same territory. If you both collect spice from a desert territory, you split the it however you both agree (as evenly as possible if you can’t agree). If both in a battle, you decide which of you is considered the faction that fights. Regardless of who fights, your ally's forces are the ones dialed, and half of your forces in the territory (rounded up) are added to the number dialed. After the battle, half of your forces (rounded down) remain in the territory if your side wins, and the rest go to the Tanks. If you and your ally both occupy a stronghold at the end of a turn, it only counts as one stronghold for both of you, and you control it. It only takes three strongholds for you and your ally to win, if you both are co-occupied in all three.") +
               Advantage(MainPhase.Resurrection,
                   "You may always revive Duke Vidal for 5 spice, no matter how many of your leaders are in the Tanks.  You may revive leaders normally when at least 5 are in the Tanks (counting Duke Vidal).") +
               AdvancedHeader(g, Rule.PinkLoyalty, Rule.IncreasedResourceFlow, Rule.AdvancedKarama) +
               Advantage(g, Rule.PinkLoyalty, "Loyalty",
                   "At the start of the game, one of your leader (selected randomly) is removed from the Traitor deck.") +
               Advantage(g, Rule.IncreasedResourceFlow, MainPhase.Collection,
                   "You and your ally both gain income from strongholds that yield {16} during collection.") +
               Advantage(g, Rule.AdvancedKarama, TreacheryCardType.Karma,
                   "If you played neither a weapon nor a defense, you may add the difference between your leader disc and your opponent’s leader disc to your number dialed.") +
               @"<h5>Alliance</h5>
                <p>You may have your ally benefit from a triggered Ambassador's effect.</p>
                <h5>Strategy</h5>
                <p>Much of your strength comes from being in an alliance as soon as possible.  Your Occupy advantage can provide a significant boost when you send several forces into a stronghold your ally occupies.  Your Ambassadors can provide you with useful perks, and placing them requires careful consideration, not only with the order you select them, but in how their presence on the map can influence the other factions.</p>
                </div>";
    }

    private static string GetCyanTemplate(Game g)
    {
        return SheetHeader(
                   "6 tokens in any unoccupied territory when all other factions set up and 14 in reserve (off-planet)",
                   12, 2, "You resort to terrorism") +
               Advantage("Terrorize",
                   "During the Mentat Pause, you may place a Terror token face down in any stronghold that doesn't have one (other than the Ixian Hidden Mobile Stronghold), even one under storm, or else move one to a stronghold that doesn't have one.  You may reveal and activate a Terror token when another faction (other than your ally) enters a stronghold containing one (either moving or shipping in, including advisors), applying effects to that faction. Revealed tokens are removed from the game.") +
               Advantage("Duke Vidal",
                   "You gain Duke Vidal at the end of Shipping and Movement if you are in at least two battles in strongholds (not counting battles involving Ecaz) if he is not captured, a ghola, or in the Tanks, taking him from any faction currently controlling him.  Set him aside at the end of the turn if he’s not in the Tanks or captured.") +
               Advantage("Enemy of my Enemy",
                   "When a faction (other than Ecaz) would trigger a Terror token, you may offer to enter into an alliance with that faction before the token is revealed.  If that faction accepts, you both are now allied (breaking existing alliances either or both of you were in).  Your Terror token is not revealed, and returns to your supply.  If that faction does not accept, the Terror token must be revealed.") +
               AdvancedHeader(g, Rule.CyanAssassinate, Rule.AdvancedKarama) +
               Advantage(g, Rule.CyanAssassinate, "Assassinate Leaders",
                   "When you lose a battle in which the opposing player had a leader disc that was not killed (and no Traitor was called), you may reveal a Traitor Card for the same faction (other than the leader you opposed).  If they are not in the Tanks, kill that leader and collect spice for them.  During the Mentat Pause, set the revealed card aside face up as a marker, then draw a new Traitor Card.  You may only use this advantage once against each other faction in the game. You may reveal a Traitor Card normally, but then this advantage is lost.") +
               Advantage(g, Rule.AdvancedKarama, TreacheryCardType.Karma,
                   "If you lose a battle, force your opponent to discard or keep any or all Treachery Cards they played.") +
               @"<h5>Alliance</h5>
                <p>When your ally loses a battle that had a winner, they may keep one Treachery Card they played in the battle that they would have been able to keep had they won.</p>
                <h5>Strategy</h5>
                <p>You are handicapped by having to wait a few turns before you can get multiple Terror tokens onto the board, and you can only gain Duke Vidal by getting into battles in strongholds.  It’s to your advantage to prolong the game until you can maneuver into a situation where you can either utilize Enemy of My Enemy to gain a useful ally at a critical moment, or the other factions have been weakened enough by your Terror tokens that you can go for the win alone.</p>
                </div>";
    }

    #endregion FactionManual

    #region LoadingAndSaving

    public static Skin Load(string data)
    {
        var serializer = JsonSerializer.CreateDefault();
        serializer.Formatting = Formatting.Indented;
        var textReader = new StringReader(data);
        var jsonReader = new JsonTextReader(textReader);
        var result = serializer.Deserialize<Skin>(jsonReader);
        Fix(result, Default);
        return result;
    }

    public static void Fix(Skin toFix, Skin donor)
    {
        toFix.Concept_STR = FixDictionary(toFix.Concept_STR, donor.Concept_STR);
        toFix.MainPhase_STR = FixDictionary(toFix.MainPhase_STR, donor.MainPhase_STR);
        toFix.PersonName_STR = FixDictionary(toFix.PersonName_STR, donor.PersonName_STR);
        toFix.PersonImage_URL = FixDictionary(toFix.PersonImage_URL, donor.PersonImage_URL);
        toFix.TerritoryName_STR = FixDictionary(toFix.TerritoryName_STR, donor.TerritoryName_STR);
        toFix.TerritoryBorder_SVG = FixDictionary(toFix.TerritoryBorder_SVG, donor.TerritoryBorder_SVG);
        toFix.LocationCenter_Point = FixDictionary(toFix.LocationCenter_Point, donor.LocationCenter_Point);
        toFix.LocationSpice_Point = FixDictionary(toFix.LocationSpice_Point, donor.LocationSpice_Point);
        toFix.FactionName_STR = FixDictionary(toFix.FactionName_STR, donor.FactionName_STR);
        toFix.FactionImage_URL = FixDictionary(toFix.FactionImage_URL, donor.FactionImage_URL);
        toFix.FactionTableImage_URL = FixDictionary(toFix.FactionTableImage_URL, donor.FactionTableImage_URL);
        toFix.FactionFacedownImage_URL = FixDictionary(toFix.FactionFacedownImage_URL, donor.FactionFacedownImage_URL);
        toFix.FactionForceImage_URL = FixDictionary(toFix.FactionForceImage_URL, donor.FactionForceImage_URL);
        toFix.FactionSpecialForceImage_URL = FixDictionary(toFix.FactionSpecialForceImage_URL, donor.FactionSpecialForceImage_URL);
        toFix.FactionColor = FixDictionary(toFix.FactionColor, donor.FactionColor);
        toFix.ForceName_STR = FixDictionary(toFix.ForceName_STR, donor.ForceName_STR);
        toFix.SpecialForceName_STR = FixDictionary(toFix.SpecialForceName_STR, donor.SpecialForceName_STR);
        toFix.TechTokenName_STR = FixDictionary(toFix.TechTokenName_STR, donor.TechTokenName_STR);
        toFix.TechTokenImage_URL = FixDictionary(toFix.TechTokenImage_URL, donor.TechTokenImage_URL);
        toFix.Sound = FixDictionary(toFix.Sound, donor.Sound);
        toFix.TreacheryCardType_STR = FixDictionary(toFix.TreacheryCardType_STR, donor.TreacheryCardType_STR);
        toFix.TreacheryCardName_STR = FixDictionary(toFix.TreacheryCardName_STR, donor.TreacheryCardName_STR);
        toFix.TreacheryCardDescription_STR = FixDictionary(toFix.TreacheryCardDescription_STR, donor.TreacheryCardDescription_STR);
        toFix.TreacheryCardImage_URL = FixDictionary(toFix.TreacheryCardImage_URL, donor.TreacheryCardImage_URL);
        toFix.TechTokenDescription_STR = FixDictionary(toFix.TechTokenDescription_STR, donor.TechTokenDescription_STR);
        toFix.ResourceCardImage_URL = FixDictionary(toFix.ResourceCardImage_URL, donor.ResourceCardImage_URL);
        toFix.LeaderSkillCardName_STR = FixDictionary(toFix.LeaderSkillCardName_STR, donor.LeaderSkillCardName_STR);
        toFix.LeaderSkillCardImage_URL = FixDictionary(toFix.LeaderSkillCardImage_URL, donor.LeaderSkillCardImage_URL);
        toFix.StrongholdCardName_STR = FixDictionary(toFix.StrongholdCardName_STR, donor.StrongholdCardName_STR);
        toFix.StrongholdCardImage_URL = FixDictionary(toFix.StrongholdCardImage_URL, donor.StrongholdCardImage_URL);
        toFix.HomeWorldImage_URL = FixDictionary(toFix.HomeWorldImage_URL, donor.HomeWorldImage_URL);
        toFix.HomeWorldCardImage_URL = FixDictionary(toFix.HomeWorldCardImage_URL, donor.HomeWorldCardImage_URL);
        toFix.NexusCardImage_URL = FixDictionary(toFix.NexusCardImage_URL, donor.NexusCardImage_URL);
        toFix.TerrorTokenName_STR = FixDictionary(toFix.TerrorTokenName_STR, donor.TerrorTokenName_STR);
        toFix.TerrorTokenDescription_STR = FixDictionary(toFix.TerrorTokenDescription_STR, donor.TerrorTokenDescription_STR);
        toFix.DiscoveryTokenName_STR = FixDictionary(toFix.DiscoveryTokenName_STR, donor.DiscoveryTokenName_STR);
        toFix.DiscoveryTokenDescription_STR = FixDictionary(toFix.DiscoveryTokenDescription_STR, donor.DiscoveryTokenDescription_STR);
        toFix.DiscoveryTokenImage_URL = FixDictionary(toFix.DiscoveryTokenImage_URL, donor.DiscoveryTokenImage_URL);
        toFix.DiscoveryTokenTypeName_STR = FixDictionary(toFix.DiscoveryTokenTypeName_STR, donor.DiscoveryTokenTypeName_STR);
        toFix.DiscoveryTokenTypeImage_URL = FixDictionary(toFix.DiscoveryTokenTypeImage_URL, donor.DiscoveryTokenTypeImage_URL);
        toFix.AmbassadorImage_URL = FixDictionary(toFix.AmbassadorImage_URL, donor.AmbassadorImage_URL);
        toFix.AmbassadorName_STR = FixDictionary(toFix.AmbassadorName_STR, donor.AmbassadorName_STR);
        toFix.AmbassadorDescription_STR = FixDictionary(toFix.AmbassadorDescription_STR, donor.AmbassadorDescription_STR);

        FixValue(ref toFix.MusicGeneral_URL, donor.MusicGeneral_URL);
        FixValue(ref toFix.MusicResourceBlow_URL, donor.MusicResourceBlow_URL);
        FixValue(ref toFix.MusicSetup_URL, donor.MusicSetup_URL);
        FixValue(ref toFix.MusicBidding_URL, donor.MusicBidding_URL);
        FixValue(ref toFix.MusicShipmentAndMove_URL, donor.MusicShipmentAndMove_URL);
        FixValue(ref toFix.MusicBattle_URL, donor.MusicBattle_URL);
        FixValue(ref toFix.MusicBattleClimax_URL, donor.MusicBattleClimax_URL);
        FixValue(ref toFix.MusicMentat_URL, donor.MusicMentat_URL);
        FixValue(ref toFix.Sound_YourTurn_URL, donor.Sound_YourTurn_URL);
        FixValue(ref toFix.Sound_Chatmessage_URL, donor.Sound_Chatmessage_URL);
        FixValue(ref toFix.Map_URL, donor.Map_URL);
        FixValue(ref toFix.Eye_URL, donor.Eye_URL);
        FixValue(ref toFix.EyeSlash_URL, donor.EyeSlash_URL);
        FixValue(ref toFix.HighThreshold_URL, donor.HighThreshold_URL);
        FixValue(ref toFix.LowThreshold_URL, donor.LowThreshold_URL);
        FixValue(ref toFix.CardBack_ResourceCard_URL, donor.CardBack_ResourceCard_URL);
        FixValue(ref toFix.CardBack_TreacheryCard_URL, donor.CardBack_TreacheryCard_URL);
        FixValue(ref toFix.BattleScreen_URL, donor.BattleScreen_URL);
        FixValue(ref toFix.Messiah_URL, donor.Messiah_URL);
        FixValue(ref toFix.Monster_URL, donor.Monster_URL);
        FixValue(ref toFix.Harvester_URL, donor.Harvester_URL);
        FixValue(ref toFix.Resource_URL, donor.Resource_URL);
        FixValue(ref toFix.HMS_URL, donor.HMS_URL);
        FixValue(ref toFix.MapDimensions, donor.MapDimensions);
        FixValue(ref toFix.PlanetCenter, donor.PlanetCenter);
        FixValue(ref toFix.PlanetRadius, donor.PlanetRadius);
        FixValue(ref toFix.MapRadius, donor.MapRadius);
        FixValue(ref toFix.PlayerTokenRadius, donor.PlayerTokenRadius);
        FixValue(ref toFix.SpiceDeckLocation, donor.SpiceDeckLocation);
        FixValue(ref toFix.TreacheryDeckLocation, donor.TreacheryDeckLocation);
        FixValue(ref toFix.CardSize, donor.CardSize);
        FixValue(ref toFix.BattleScreenWidth, donor.BattleScreenWidth);
        FixValue(ref toFix.BattleScreenHeroX, donor.BattleScreenHeroX);
        FixValue(ref toFix.BattleScreenHeroY, donor.BattleScreenHeroY);
        FixValue(ref toFix.BattleWheelHeroWidth, donor.BattleWheelHeroWidth);
        FixValue(ref toFix.BattleWheelHeroHeight, donor.BattleWheelHeroHeight);
        FixValue(ref toFix.BattleWheelForcesX, donor.BattleWheelForcesX);
        FixValue(ref toFix.BattleWheelForcesY, donor.BattleWheelForcesY);
        FixValue(ref toFix.BattleWheelCardX, donor.BattleWheelCardX);
        FixValue(ref toFix.BattleWheelCardY, donor.BattleWheelCardY);
        FixValue(ref toFix.BattleWheelCardWidth, donor.BattleWheelCardWidth);
        FixValue(ref toFix.BattleWheelCardHeight, donor.BattleWheelCardHeight);
        FixValue(ref toFix.MONSTERTOKEN_RADIUS, donor.MONSTERTOKEN_RADIUS);
        FixValue(ref toFix.FORCETOKEN_FONT, donor.FORCETOKEN_FONT);
        FixValue(ref toFix.FORCETOKEN_FONTCOLOR, donor.FORCETOKEN_FONTCOLOR);
        FixValue(ref toFix.FORCETOKEN_FONT_BORDERCOLOR, donor.FORCETOKEN_FONT_BORDERCOLOR);
        FixValue(ref toFix.FORCETOKEN_FONT_BORDERWIDTH, donor.FORCETOKEN_FONT_BORDERWIDTH);
        FixValue(ref toFix.FORCETOKEN_RADIUS, donor.FORCETOKEN_RADIUS);
        FixValue(ref toFix.RESOURCETOKEN_FONT, donor.RESOURCETOKEN_FONT);
        FixValue(ref toFix.RESOURCETOKEN_FONTCOLOR, donor.RESOURCETOKEN_FONTCOLOR);
        FixValue(ref toFix.RESOURCETOKEN_FONT_BORDERCOLOR, donor.RESOURCETOKEN_FONT_BORDERCOLOR);
        FixValue(ref toFix.RESOURCETOKEN_FONT_BORDERWIDTH, donor.RESOURCETOKEN_FONT_BORDERWIDTH);
        FixValue(ref toFix.RESOURCETOKEN_RADIUS, donor.RESOURCETOKEN_RADIUS);
        FixValue(ref toFix.HIGHLIGHT_OVERLAY_COLOR, donor.HIGHLIGHT_OVERLAY_COLOR);
        FixValue(ref toFix.METHEOR_OVERLAY_COLOR, donor.METHEOR_OVERLAY_COLOR);
        FixValue(ref toFix.BLOWNSHIELDWALL_OVERLAY_COLOR, donor.BLOWNSHIELDWALL_OVERLAY_COLOR);
        FixValue(ref toFix.STORM_OVERLAY_COLOR, donor.STORM_OVERLAY_COLOR);
        FixValue(ref toFix.STORM_PRESCIENCE_OVERLAY_COLOR, donor.STORM_PRESCIENCE_OVERLAY_COLOR);
        FixValue(ref toFix.CARDPILE_FONT, donor.CARDPILE_FONT);
        FixValue(ref toFix.CARDPILE_FONTCOLOR, donor.CARDPILE_FONTCOLOR);
        FixValue(ref toFix.CARDPILE_FONT_BORDERCOLOR, donor.CARDPILE_FONT_BORDERCOLOR);
        FixValue(ref toFix.CARDPILE_FONT_BORDERWIDTH, donor.CARDPILE_FONT_BORDERWIDTH);
        FixValue(ref toFix.PHASE_FONT, donor.PHASE_FONT);
        FixValue(ref toFix.PHASE_ACTIVE_FONT, donor.PHASE_ACTIVE_FONT);
        FixValue(ref toFix.PHASE_FONTCOLOR, donor.PHASE_FONTCOLOR);
        FixValue(ref toFix.PHASE_ACTIVE_FONTCOLOR, donor.PHASE_ACTIVE_FONTCOLOR);
        FixValue(ref toFix.PHASE_FONT_BORDERCOLOR, donor.PHASE_FONT_BORDERCOLOR);
        FixValue(ref toFix.PHASE_FONT_BORDERWIDTH, donor.PHASE_FONT_BORDERWIDTH);
        FixValue(ref toFix.PHASE_ACTIVE_FONT_BORDERWIDTH, donor.PHASE_ACTIVE_FONT_BORDERWIDTH);
        FixValue(ref toFix.PLAYERNAME_FONT, donor.PLAYERNAME_FONT);
        FixValue(ref toFix.PLAYERNAME_FONTCOLOR, donor.PLAYERNAME_FONTCOLOR);
        FixValue(ref toFix.PLAYERNAME_FONT_BORDERCOLOR, donor.PLAYERNAME_FONT_BORDERCOLOR);
        FixValue(ref toFix.PLAYERNAME_FONT_BORDERWIDTH, donor.PLAYERNAME_FONT_BORDERWIDTH);
        FixValue(ref toFix.SKILL_FONT, donor.SKILL_FONT);
        FixValue(ref toFix.SKILL_FONTCOLOR, donor.SKILL_FONTCOLOR);
        FixValue(ref toFix.SKILL_FONT_BORDERCOLOR, donor.SKILL_FONT_BORDERCOLOR);
        FixValue(ref toFix.SKILL_FONT_BORDERWIDTH, donor.SKILL_FONT_BORDERWIDTH);
        FixValue(ref toFix.TABLEPOSITION_BACKGROUNDCOLOR, donor.TABLEPOSITION_BACKGROUNDCOLOR);
        FixValue(ref toFix.TURN_FONT, donor.TURN_FONT);
        FixValue(ref toFix.TURN_FONT_COLOR, donor.TURN_FONT_COLOR);
        FixValue(ref toFix.TURN_FONT_BORDERCOLOR, donor.TURN_FONT_BORDERCOLOR);
        FixValue(ref toFix.TURN_FONT_BORDERWIDTH, donor.TURN_FONT_BORDERWIDTH);
        FixValue(ref toFix.WHEEL_FONT, donor.WHEEL_FONT);
        FixValue(ref toFix.WHEEL_FONTCOLOR, donor.WHEEL_FONTCOLOR);
        FixValue(ref toFix.WHEEL_FONT_AGGRESSOR_BORDERCOLOR, donor.WHEEL_FONT_AGGRESSOR_BORDERCOLOR);
        FixValue(ref toFix.WHEEL_FONT_DEFENDER_BORDERCOLOR, donor.WHEEL_FONT_DEFENDER_BORDERCOLOR);
        FixValue(ref toFix.WHEEL_FONT_BORDERWIDTH, donor.WHEEL_FONT_BORDERWIDTH);
        FixValue(ref toFix.SHADOW, donor.SHADOW);
        FixValue(ref toFix.FACTION_INFORMATIONCARDSTYLE, donor.FACTION_INFORMATIONCARDSTYLE);
    }

    private static Dictionary<TKey, TValue> FixDictionary<TKey, TValue>(Dictionary<TKey, TValue> toFix, Dictionary<TKey, TValue> donor)
    {
        if (toFix == null || donor.Keys.Any(k => !toFix.ContainsKey(k)))
            return donor;
        return toFix;
    }

    private static void FixValue<T>(ref T toFix, T donor)
    {
        if (toFix == null || toFix.Equals(default(T))) toFix = donor;
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

    #region DefaultSkin

    public static Skin Default { get; } = new Skin
    {
        Description = "1979 Art",
        Version = CurrentVersion,

        Map_URL = DEFAULT_ART_LOCATION + "/art/map.svg",
        Eye_URL = DEFAULT_ART_LOCATION + "/art/eye.svg",
        EyeSlash_URL = DEFAULT_ART_LOCATION + "/art/eyeslash.svg",
        HighThreshold_URL = DEFAULT_ART_LOCATION + "/art/arrow-up-circle-fill.svg",
        LowThreshold_URL = DEFAULT_ART_LOCATION + "/art/arrow-down-circle-fill.svg",
        CardBack_ResourceCard_URL = DEFAULT_ART_LOCATION + "/art/SpiceBack.gif",
        CardBack_TreacheryCard_URL = DEFAULT_ART_LOCATION + "/art/TreacheryBack.gif",
        BattleScreen_URL = DEFAULT_ART_LOCATION + "/art/wheel.png",
        Messiah_URL = DEFAULT_ART_LOCATION + "/art/messiah.svg",
        Monster_URL = DEFAULT_ART_LOCATION + "/art/monster.svg",
        DrawResourceIconsOnMap = true,
        ShowVerboseToolipsOnMap = true,
        ShowArrowsForRecentMoves = true,
        Resource_URL = DEFAULT_ART_LOCATION + "/art/PassiveSpice.svg",
        Harvester_URL = DEFAULT_ART_LOCATION + "/art/ActiveSpice.svg",
        HMS_URL = DEFAULT_ART_LOCATION + "/art/hms.svg",

        Concept_STR = new Dictionary<Concept, string>
        {
            [Concept.Messiah] = "Kwisatz Haderach",
            [Concept.Monster] = "Shai Hulud",
            [Concept.Resource] = "Spice",
            [Concept.Graveyard] = "Tleilaxu Tanks"

        }.Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<Concept, string>>() : new Dictionary<Concept, string>
        {
            [Concept.BabyMonster] = "Sandtrout"

        }).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<Concept, string>>() : new Dictionary<Concept, string>
        {
            [Concept.GreatMonster] = "Great Maker"

        }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

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
            [TreacheryCardType.Clairvoyance] = "Truthtrance"

        }.Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<TreacheryCardType, string>>() : new Dictionary<TreacheryCardType, string>
        {
            [TreacheryCardType.ArtilleryStrike] = "Artillery Strike",
            [TreacheryCardType.Harvester] = "Harvester",
            [TreacheryCardType.Thumper] = "Thumper",
            [TreacheryCardType.Amal] = "Amal"

        }).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<TreacheryCardType, string>>() : new Dictionary<TreacheryCardType, string>
        {
            [TreacheryCardType.Distrans] = "Distrans",
            [TreacheryCardType.Juice] = "Juice Of Sapho",
            [TreacheryCardType.MirrorWeapon] = "Mirror Weapon",
            [TreacheryCardType.PortableAntidote] = "Portable Snooper",
            [TreacheryCardType.Flight] = "Ornithopter",
            [TreacheryCardType.SearchDiscarded] = "Nullentropy",
            [TreacheryCardType.TakeDiscarded] = "Semuta Drug",
            [TreacheryCardType.Residual] = "Residual Poison",
            [TreacheryCardType.Rockmelter] = "Stone Burner"

        }).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<TreacheryCardType, string>>() : new Dictionary<TreacheryCardType, string>
        {
            [TreacheryCardType.Recruits] = "Recruits",
            [TreacheryCardType.Reinforcements] = "Reinforcements",
            [TreacheryCardType.HarassAndWithdraw] = "Harass and Withdraw"

        }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

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
                [32] = "Trip to Gamont"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
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
                    [44] = "Kull Wahad"
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [45] = "Distrans",
                    [46] = "Juice Of Sapho",
                    [47] = "Mirror Weapon",
                    [48] = "Portable Snooper",
                    [49] = "Ornithopter",
                    [50] = "Nullentropy Box",
                    [51] = "Semuta Drug",
                    [52] = "Residual Poison",
                    [53] = "Stone Burner",
                    [54] = "Karama"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [55] = "Recruits",
                    [56] = "Reinforcements",
                    [57] = "Harass and Withdraw"
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

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
                [32] = "Worthless Card - Play as part of your Battle Plan, in place of a weapon, defense, or both. This card has no value in play, and you can discard it only by playing it in your Battle plan."
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
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
                    [44] = "Worthless Card - Play as part of your Battle Plan, in place of a weapon, defense, or both. This card has no value in play, and you can discard it only by playing it in your Battle plan."
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [45] = "Distrans - Give another player a treachery card from your hand at any time except during a bid and if their hand size permits. Discard after use.",
                    [46] = "Choose one: (1) be considered aggressor in a battle or (2) play at the beginning of a phase or action that requires turn order to go first or (3) go last in a phase or action that requires turn order. Discard after use.",
                    [47] = "Weapon - Special - Play as part of your Battle Plan. Becomes a copy of your opponent's weapon. Discard after use.",
                    [48] = "Defense - Poison - You may play this after revealing your battle plan if you did not play a defense and if Voice permits. Discard after use.",
                    [49] = "Ornithopter - As part of your movement you may move one group of forces up to 3 territories or two groups of forces up to your normal move distance. Discard after use.",
                    [50] = "Nullentropy Box - At any time, pay 2 spice to secretly search and take one card from the treachery discard pile. Then shuffle the discard pile, discarding this card on top.",
                    [51] = "Semuta Drug - Add a treachery card to your hand immediately after another player discards it. You choose if multiple cards are discarded at the same time. Discard after use.",
                    [52] = "Residual Poison - Play against your opponent in battle before making battle plans. Kills one of their available leaders at random. No spice is collected for it. Discard after use.",
                    [53] = "Weapon - Special - Play as part of your Battle Plan. You choose after pland are revealed to either kill both leaders or reduce the strength of both leaders to 0. The player with the highest number of undialed forces wins the battle. Dialed forces are lost normally. Discard after use.",
                    [54] = "Allows you to prevent use of a Faction Advantage. Allows you to bid any amount of spice on a card or immediately win a card on bid. Allows you to ship at half price. In the advanced game, allows use of your Special Karama Power once during the game. Discard after use."
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [55] = "Play during Revival. All factions double their Free Revival rates. The revival limits is increased to 7. Discard after use.",
                    [56] = "Play as part of your battle plan in place of a weapon or defense if you have at least 3 forces in reserves. Add 2 to your dialed number, then send 3 forces from reserves to the Tanks. Discard after use.",
                    [57] = "Play as part of your battle plan in place of a weapon or defense when not on your own Home World. Your undialed forces return to your reserves. Your leader may be killed as normal. If your opponent calls traitor, this effect is cancelled. Discard after use."
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        TechTokenDescription_STR = new Dictionary<TechToken, string>
        {
            [TechToken.Graveyard] = "At the end of the Revival phase, yields 1 spice per tech token you own if any player except Tleilaxu used free revival.",
            [TechToken.Resources] = "At the end of the Charity phase, yields 1 spice per tech token you own if any player except Bene Gesserit claimed charity.",
            [TechToken.Ships] = "At the end of the Shipment & Move phase, yields 1 spice per tech token you own if any player except Guild shipped forces from off-planet."
        },

        TreacheryCardImage_URL = new Dictionary<int, string>
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
                [32] = DEFAULT_ART_LOCATION + "/art/TripToGamont.gif"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
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
                    [44] = DEFAULT_ART_LOCATION + "/art/KullWahad.gif"
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [45] = DEFAULT_ART_LOCATION + "/art/Distrans.gif",
                    [46] = DEFAULT_ART_LOCATION + "/art/JuiceOfSapho.gif",
                    [47] = DEFAULT_ART_LOCATION + "/art/MirrorWeapon.gif",
                    [48] = DEFAULT_ART_LOCATION + "/art/PortableSnooper.gif",
                    [49] = DEFAULT_ART_LOCATION + "/art/Ornithopter.gif",
                    [50] = DEFAULT_ART_LOCATION + "/art/Nullentropy.gif",
                    [51] = DEFAULT_ART_LOCATION + "/art/SemutaDrug.gif",
                    [52] = DEFAULT_ART_LOCATION + "/art/ResidualPoison.gif",
                    [53] = DEFAULT_ART_LOCATION + "/art/StoneBurner.gif",
                    [54] = DEFAULT_ART_LOCATION + "/art/WhiteKarama.gif"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [55] = DEFAULT_ART_LOCATION + "/art/Recruits.gif",
                    [56] = DEFAULT_ART_LOCATION + "/art/Reinforcements.gif",
                    [57] = DEFAULT_ART_LOCATION + "/art/HarassAndWithdraw.gif"
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        ResourceCardImage_URL = new Dictionary<int, string>
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
                [98] = DEFAULT_ART_LOCATION + "/art/Shai-Hulud.gif"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [99] = DEFAULT_ART_LOCATION + "/art/Sandtrout.gif"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [41] = DEFAULT_ART_LOCATION + "/art/SihayaRidgeDiscovery.gif",
                    [42] = DEFAULT_ART_LOCATION + "/art/RockOutcroppingsDiscovery.gif",
                    [43] = DEFAULT_ART_LOCATION + "/art/HaggaBasinDiscovery.gif",
                    [44] = DEFAULT_ART_LOCATION + "/art/FuneralPlainDiscovery.gif",
                    [45] = DEFAULT_ART_LOCATION + "/art/WindPassNorthDiscovery.gif",
                    [46] = DEFAULT_ART_LOCATION + "/art/OHGapDiscovery.gif",
                    [100] = DEFAULT_ART_LOCATION + "/art/GreatMaker.gif"
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        PersonName_STR = new Dictionary<int, string>
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
                [1010] = "Wanna Yueh",
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
                [1030] = "Umman Kudu"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [1031] = "Dominic Vernius",
                    [1032] = "C'Tair Pilru",
                    [1033] = "Tessia Vernius",
                    [1034] = "Kailea Vernius",
                    [1035] = "Cammar Pilru",
                    [1036] = "Zoal",
                    [1037] = "Hidar Fen Ajidica",
                    [1038] = "Master Zaaf",
                    [1039] = "Wykk",
                    [1040] = "Blin"
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
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
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [1052] = "Sanya Ecaz",
                    [1053] = "Rivvy Dinari",
                    [1054] = "Ilesa Ecaz",
                    [1055] = "Bindikk Narvi",
                    [1056] = "Whitmore Bludd",
                    [1057] = "Duke Prad Vidal",
                    [1058] = "Lupino Ord",
                    [1059] = "Hiih Resser",
                    [1060] = "Trin Kronos",
                    [1061] = "Grieu Kronos",
                    [1062] = "Vando Terboli"
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        PersonImage_URL = new Dictionary<int, string>
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
                [1030] = DEFAULT_ART_LOCATION + "/art/person9.png"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [1031] = DEFAULT_ART_LOCATION + "/art/person30.png",
                    [1032] = DEFAULT_ART_LOCATION + "/art/person31.png",
                    [1033] = DEFAULT_ART_LOCATION + "/art/person32.png",
                    [1034] = DEFAULT_ART_LOCATION + "/art/person33.png",
                    [1035] = DEFAULT_ART_LOCATION + "/art/person34.png",
                    [1036] = DEFAULT_ART_LOCATION + "/art/person35.png",
                    [1037] = DEFAULT_ART_LOCATION + "/art/person36.png",
                    [1038] = DEFAULT_ART_LOCATION + "/art/person37.png",
                    [1039] = DEFAULT_ART_LOCATION + "/art/person38.png",
                    [1040] = DEFAULT_ART_LOCATION + "/art/person39.png"
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
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
                    [1051] = DEFAULT_ART_LOCATION + "/art/person1051.gif"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
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
                    [1062] = DEFAULT_ART_LOCATION + "/art/person1062.png"
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        TerritoryName_STR = new Dictionary<int, string>
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
                [41] = "Cielago West"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [42] = "Hidden Mobile Stronghold"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
            {
                [43] = "Caladan",
                [44] = "Giedi Prime",
                [45] = "Southern Hemisphere",
                [46] = "Kaitain",
                [47] = "Salusa Secundus",
                [48] = "Junction",
                [49] = "Wallach IX",
                [50] = "Ix",
                [51] = "Tleilax",
                [52] = "Tupile",
                [53] = "Richese",
                [54] = "Ecaz",
                [55] = "Grumman",
                [56] = "Jacurutu Sietch",
                [57] = "Cistern",
                [58] = "Ecological Testing Station",
                [59] = "Shrine",
                [60] = "Orgiz Processing Station"

            }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        TerritoryBorder_SVG = new Dictionary<int, string>
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
            [41] = "M193.6 384.7 L186.5 417.4 L176.5 428.8 L178.4 459.3 L183.1 466.8 L179.5 487.3 L195.1 486.8 L208.8 474 L210.1 462.8 L217.3 453.4 L219.7 438.3 L218 432.7 L224.5 427.7 L221.1 424.6 L219.8 417.5 L224.1 410 L213.3 388.3 L212.5 368.9 L193.6 384.7 z"
        },

        LocationCenter_Point = new Dictionary<int, PointD>
        {
            [0] = new(277, 314), //Polar Sink
            [1] = new(327, 243), //Imperial Basin (East Sector)
            [2] = new(323, 204), //Imperial Basin (Center Sector)
            [3] = new(291, 204), //Imperial Basin (West Sector)
            [4] = new(274, 169), //Carthag
            [5] = new PointD(348, 146), //Arrakeen
            [6] = new PointD(451, 407), //Tuek's Sietch
            [7] = new PointD(110, 201), //Sietch Tabr
            [8] = new PointD(109, 408), //Habbanya Sietch
            [9] = new PointD(241, 417), //Cielago North (West Sector)
            [10] = new PointD(279, 421), //Cielago North (Center Sector)
            [11] = new PointD(316, 416), //Cielago North (East Sector)
            [12] = new PointD(236, 457), //Cielago Depression (West Sector)
            [13] = new PointD(277, 454), //Cielago Depression (Center Sector)
            [14] = new PointD(324, 461), //Cielago Depression (East Sector)
            [15] = new PointD(217, 511), //Meridian (West Sector)
            [16] = new PointD(253, 523), //Meridian (East Sector)
            [17] = new PointD(296, 498), //Cielago South (West Sector)
            [18] = new PointD(326, 497), //Cielago South (East Sector)
            [19] = new PointD(358, 462), //Cielago East (West Sector)
            [20] = new PointD(380, 468), //Cielago East (East Sector)
            [21] = new PointD(312, 355), //Harg Pass (West Sector)
            [22] = new PointD(333, 346), //Harg Pass (East Sector)
            [23] = new PointD(388, 434), //False Wall South (West Sector)
            [24] = new PointD(394, 393), //False Wall South (East Sector)
            [25] = new PointD(319, 333), //False Wall East (Far South Sector)
            [26] = new PointD(326, 320), //False Wall East (South Sector)
            [27] = new PointD(332, 303), //False Wall East (Middle Sector)
            [28] = new PointD(325, 284), //False Wall East (North Sector)
            [29] = new PointD(316, 276), //False Wall East (Far North Sector)
            [30] = new PointD(367, 355), //The Minor Erg (Far South Sector)
            [31] = new PointD(361, 326), //The Minor Erg (South Sector)
            [32] = new PointD(379, 295), //The Minor Erg (North Sector)
            [33] = new PointD(371, 268), //The Minor Erg (Far North Sector)
            [34] = new PointD(408, 365), //Pasty Mesa (Far South Sector)
            [35] = new PointD(433, 336), //Pasty Mesa (South Sector)
            [36] = new PointD(439, 288), //Pasty Mesa (North Sector)
            [37] = new PointD(431, 234), //Pasty Mesa (Far North Sector)
            [38] = new PointD(489, 279), //Red Chasm
            [39] = new PointD(446, 473), //South Mesa (South Sector)
            [40] = new PointD(460, 449), //South Mesa (Middle Sector)
            [41] = new PointD(507, 346), //South Mesa (North Sector)
            [42] = new PointD(428, 129), //Basin
            [43] = new PointD(367, 179), //Rim Wall West
            [44] = new PointD(393, 187), //Hole In The Rock
            [45] = new PointD(428, 159), //Sihaya Ridge
            [46] = new PointD(383, 235), //Shield Wall (South Sector)
            [47] = new PointD(355, 235), //Shield Wall (North Sector)
            [48] = new PointD(477, 190), //Gara Kulon
            [49] = new PointD(400, 118), //OH Gap (East Sector)
            [50] = new PointD(357, 93), //OH Gap (Middle Sector)
            [51] = new PointD(312, 80), //OH Gap (West Sector)
            [52] = new PointD(278, 82), //Broken Land (East Sector)
            [53] = new PointD(219, 97), //Broken Land (West Sector)
            [54] = new PointD(272, 114), //Tsimpo (East Sector)
            [55] = new PointD(230, 137), //Tsimpo (Middle Sector)
            [56] = new PointD(192, 178), //Tsimpo (West Sector)
            [57] = new PointD(275, 223), //Arsunt (East Sector)
            [58] = new PointD(266, 268), //Arsunt (West Sector)
            [59] = new PointD(124, 156), //Rock Outcroppings (North Sector)
            [60] = new PointD(81, 180), //Rock Outcroppings (South Sector)
            [61] = new PointD(198, 132), //Plastic Basin (North Sector)
            [62] = new PointD(165, 165), //Plastic Basin (Middle Sector)
            [63] = new PointD(172, 254), //Plastic Basin (South Sector)
            [64] = new PointD(235, 188), //Hagga Basin (East Sector)
            [65] = new PointD(199, 203), //Hagga Basin (West Sector)
            [66] = new PointD(71, 214), //Bight Of The Cliff (North Sector)
            [67] = new PointD(67, 244), //Bight Of The Cliff (South Sector)
            [68] = new PointD(85, 261), //Funeral Plain
            [69] = new PointD(143, 295), //The Great Flat
            [70] = new PointD(236, 287), //Wind Pass (Far North Sector)
            [71] = new PointD(227, 303), //Wind Pass (North Sector)
            [72] = new PointD(213, 327), //Wind Pass (South Sector)
            [73] = new PointD(207, 357), //Wind Pass (Far South Sector)
            [74] = new PointD(155, 327), //The Greater Flat
            [75] = new PointD(84, 371), //Habbanya Erg (West Sector)
            [76] = new PointD(145, 375), //Habbanya Erg (East Sector)
            [77] = new PointD(192, 334), //False Wall West (North Sector)
            [78] = new PointD(196, 374), //False Wall West (Middle Sector)
            [79] = new PointD(163, 449), //False Wall West (South Sector)
            [80] = new PointD(241, 336), //Wind Pass North (North Sector)
            [81] = new PointD(247, 355), //Wind Pass North (South Sector)
            [82] = new PointD(79, 408), //Habbanya Ridge Flat (West Sector)
            [83] = new PointD(152, 485), //Habbanya Ridge Flat (East Sector)
            [84] = new PointD(205, 402), //Cielago West (North Sector)
            [85] = new PointD(201, 463) //Cielago West (South Sector)
        },

        LocationSpice_Point = new Dictionary<int, PointD>
        {
            [11] = new PointD(309, 393), //Cielago North (East Sector)
            [17] = new PointD(290, 539), //Cielago South (West Sector)
            [33] = new PointD(362, 272), //The Minor Erg (Far North Sector)
            [38] = new PointD(509, 292), //Red Chasm
            [40] = new PointD(491, 401), //South Mesa (Middle Sector)
            [45] = new PointD(442, 144), //Sihaya Ridge
            [50] = new PointD(335, 86), //OH Gap (Middle Sector)
            [53] = new PointD(196, 99), //Broken Land (West Sector)
            [60] = new PointD(94, 176), //Rock Outcroppings (South Sector)
            [65] = new PointD(216, 233), //Hagga Basin (West Sector)
            [68] = new PointD(59, 266), //Funeral Plain
            [69] = new PointD(55, 293), //The Great Flat
            [75] = new PointD(60, 367), //Habbanya Erg (West Sector)
            [80] = new PointD(233, 340), //Wind Pass North (North Sector)
            [83] = new PointD(133, 483) //Habbanya Ridge Flat (East Sector)
        },

        FactionName_STR = new Dictionary<Faction, string>
            {
                [Faction.None] = "None",
                [Faction.Green] = "Atreides",
                [Faction.Black] = "Harkonnen",
                [Faction.Yellow] = "Fremen",
                [Faction.Red] = "Emperor",
                [Faction.Orange] = "Guild",
                [Faction.Blue] = "Bene Gesserit"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Grey] = "Ixian",
                    [Faction.Purple] = "Tleilaxu"
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Brown] = "CHOAM",
                    [Faction.White] = "Richese"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Pink] = "Ecaz",
                    [Faction.Cyan] = "Moritani"
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        FactionImage_URL = new Dictionary<Faction, string>
            {
                [Faction.Green] = DEFAULT_ART_LOCATION + "/art/faction1.svg",
                [Faction.Black] = DEFAULT_ART_LOCATION + "/art/faction2.svg",
                [Faction.Yellow] = DEFAULT_ART_LOCATION + "/art/faction3.svg",
                [Faction.Red] = DEFAULT_ART_LOCATION + "/art/faction4.svg",
                [Faction.Orange] = DEFAULT_ART_LOCATION + "/art/faction5.svg",
                [Faction.Blue] = DEFAULT_ART_LOCATION + "/art/faction6.svg"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Grey] = DEFAULT_ART_LOCATION + "/art/faction7.svg",
                    [Faction.Purple] = DEFAULT_ART_LOCATION + "/art/faction8.svg"
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Brown] = DEFAULT_ART_LOCATION + "/art/faction9.svg",
                    [Faction.White] = DEFAULT_ART_LOCATION + "/art/faction10.svg"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Pink] = DEFAULT_ART_LOCATION + "/art/faction11.svg",
                    [Faction.Cyan] = DEFAULT_ART_LOCATION + "/art/faction12.svg"
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        FactionTableImage_URL = new Dictionary<Faction, string>
            {
                [Faction.Green] = DEFAULT_ART_LOCATION + "/art/faction1.svg",
                [Faction.Black] = DEFAULT_ART_LOCATION + "/art/faction2.svg",
                [Faction.Yellow] = DEFAULT_ART_LOCATION + "/art/faction3.svg",
                [Faction.Red] = DEFAULT_ART_LOCATION + "/art/faction4.svg",
                [Faction.Orange] = DEFAULT_ART_LOCATION + "/art/faction5.svg",
                [Faction.Blue] = DEFAULT_ART_LOCATION + "/art/faction6.svg"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Grey] = DEFAULT_ART_LOCATION + "/art/faction7.svg",
                    [Faction.Purple] = DEFAULT_ART_LOCATION + "/art/faction8.svg"
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Brown] = DEFAULT_ART_LOCATION + "/art/faction9.svg",
                    [Faction.White] = DEFAULT_ART_LOCATION + "/art/faction10.svg"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Pink] = DEFAULT_ART_LOCATION + "/art/faction11.svg",
                    [Faction.Cyan] = DEFAULT_ART_LOCATION + "/art/faction12.svg"
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        FactionFacedownImage_URL = new Dictionary<Faction, string>
            {
                [Faction.Green] = DEFAULT_ART_LOCATION + "/art/faction1.svg",
                [Faction.Black] = DEFAULT_ART_LOCATION + "/art/faction2.svg",
                [Faction.Yellow] = DEFAULT_ART_LOCATION + "/art/faction3.svg",
                [Faction.Red] = DEFAULT_ART_LOCATION + "/art/faction4.svg",
                [Faction.Orange] = DEFAULT_ART_LOCATION + "/art/faction5.svg",
                [Faction.Blue] = DEFAULT_ART_LOCATION + "/art/faction6.svg"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Grey] = DEFAULT_ART_LOCATION + "/art/faction7.svg",
                    [Faction.Purple] = DEFAULT_ART_LOCATION + "/art/faction8.svg"
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Brown] = DEFAULT_ART_LOCATION + "/art/faction9.svg",
                    [Faction.White] = DEFAULT_ART_LOCATION + "/art/faction10.svg"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Pink] = DEFAULT_ART_LOCATION + "/art/faction11.svg",
                    [Faction.Cyan] = DEFAULT_ART_LOCATION + "/art/faction12.svg"
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        FactionForceImage_URL = new Dictionary<Faction, string>
            {
                [Faction.Green] = DEFAULT_ART_LOCATION + "/art/faction1force.svg",
                [Faction.Black] = DEFAULT_ART_LOCATION + "/art/faction2force.svg",
                [Faction.Yellow] = DEFAULT_ART_LOCATION + "/art/faction3force.svg",
                [Faction.Red] = DEFAULT_ART_LOCATION + "/art/faction4force.svg",
                [Faction.Orange] = DEFAULT_ART_LOCATION + "/art/faction5force.svg",
                [Faction.Blue] = DEFAULT_ART_LOCATION + "/art/faction6force.svg"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Grey] = DEFAULT_ART_LOCATION + "/art/faction7force.svg",
                    [Faction.Purple] = DEFAULT_ART_LOCATION + "/art/faction8force.svg"
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Brown] = DEFAULT_ART_LOCATION + "/art/faction9force.svg",
                    [Faction.White] = DEFAULT_ART_LOCATION + "/art/faction10force.svg"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Pink] = DEFAULT_ART_LOCATION + "/art/faction11force.svg",
                    [Faction.Cyan] = DEFAULT_ART_LOCATION + "/art/faction12force.svg"
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        FactionSpecialForceImage_URL = new Dictionary<Faction, string>
            {
                { Faction.Yellow, DEFAULT_ART_LOCATION + "/art/faction3specialforce.svg" },
                { Faction.Red, DEFAULT_ART_LOCATION + "/art/faction4specialforce.svg" },
                { Faction.Blue, DEFAULT_ART_LOCATION + "/art/faction6specialforce.svg" }
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    { Faction.Grey, DEFAULT_ART_LOCATION + "/art/faction7specialforce.svg" }
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    { Faction.White, DEFAULT_ART_LOCATION + "/art/faction10specialforce.svg" }
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        FactionColor = new Dictionary<Faction, string>
            {
                [Faction.None] = "#646464",
                [Faction.Green] = "#63842e",
                [Faction.Black] = "#2c2c2c",
                [Faction.Yellow] = "#d29422",
                [Faction.Red] = "#b33715",
                [Faction.Orange] = "#c85b20",
                [Faction.Blue] = "#385884"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Grey] = "#b0b079",
                    [Faction.Purple] = "#602d8b"
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Brown] = "#582d1b",
                    [Faction.White] = "#b3afa4"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Pink] = "#a85f9c",
                    [Faction.Cyan] = "#289caa"
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        ForceName_STR = new Dictionary<Faction, string>
            {
                [Faction.None] = "-",
                [Faction.Green] = "forces",
                [Faction.Black] = "forces",
                [Faction.Yellow] = "forces",
                [Faction.Red] = "forces",
                [Faction.Orange] = "forces",
                [Faction.Blue] = "fighters"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Grey] = "suboids",
                    [Faction.Purple] = "forces"
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Brown] = "forces",
                    [Faction.White] = "forces"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Pink] = "forces",
                    [Faction.Cyan] = "forces"
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        SpecialForceName_STR = new Dictionary<Faction, string>
            {
                [Faction.None] = "-",
                [Faction.Green] = "-",
                [Faction.Black] = "-",
                [Faction.Yellow] = "Fedaykin",
                [Faction.Red] = "Sardaukar",
                [Faction.Orange] = "-",
                [Faction.Blue] = "advisors"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Grey] = "cyborgs",
                    [Faction.Purple] = "-"
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Brown] = "-",
                    [Faction.White] = "No-Field"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Pink] = "-",
                    [Faction.Cyan] = "-"
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        AmbassadorName_STR = Game.ExpansionLevel < 3 ? new() : new Dictionary<Ambassador, string>
        {
            [Ambassador.None] = "None",
            [Ambassador.Green] = "Atreides",
            [Ambassador.Black] = "Harkonnen",
            [Ambassador.Yellow] = "Fremen",
            [Ambassador.Red] = "Emperor",
            [Ambassador.Orange] = "Guild",
            [Ambassador.Blue] = "Bene Gesserit",
            [Ambassador.Grey] = "Ixian",
            [Ambassador.Purple] = "Tleilaxu",
            [Ambassador.Brown] = "CHOAM",
            [Ambassador.White] = "Richese",
            [Ambassador.Pink] = "Ecaz",
            [Ambassador.Cyan] = "Moritani"
        },

        AmbassadorImage_URL = Game.ExpansionLevel < 3 ? new() : new Dictionary<Ambassador, string>
        {
            [Ambassador.Green] = DEFAULT_ART_LOCATION + "/art/faction1ambassador.svg",
            [Ambassador.Black] = DEFAULT_ART_LOCATION + "/art/faction2ambassador.svg",
            [Ambassador.Yellow] = DEFAULT_ART_LOCATION + "/art/faction3ambassador.svg",
            [Ambassador.Red] = DEFAULT_ART_LOCATION + "/art/faction4ambassador.svg",
            [Ambassador.Orange] = DEFAULT_ART_LOCATION + "/art/faction5ambassador.svg",
            [Ambassador.Blue] = DEFAULT_ART_LOCATION + "/art/faction6ambassador.svg",
            [Ambassador.Grey] = DEFAULT_ART_LOCATION + "/art/faction7ambassador.svg",
            [Ambassador.Purple] = DEFAULT_ART_LOCATION + "/art/faction8ambassador.svg",

            [Ambassador.Brown] = DEFAULT_ART_LOCATION + "/art/faction9ambassador.svg",
            [Ambassador.White] = DEFAULT_ART_LOCATION + "/art/faction10ambassador.svg",

            [Ambassador.Pink] = DEFAULT_ART_LOCATION + "/art/faction11ambassador.svg",
            [Ambassador.Cyan] = DEFAULT_ART_LOCATION + "/art/faction12ambassador.svg"
        },

        AmbassadorDescription_STR = Game.ExpansionLevel < 3 ? new() : new Dictionary<Ambassador, string>
        {
            [Ambassador.None] = "None",

            [Ambassador.Green] = "Atreides - See visitor's hand",
            [Ambassador.Black] = "Harkonnen - Look at a random Traitor Card the visiting faction holds",
            [Ambassador.Yellow] = "Fremen - Move a group of your forces on the board to any territory (subject to storm and occupancy rules)",
            [Ambassador.Red] = "Emperor - Gain 5 spice",
            [Ambassador.Orange] = "Guild - Send up to 4 of your forces in reserves to any territory not in storm for free",
            [Ambassador.Blue] = "Bene Gesserit - Trigger the effect of any Ambassador that was not part of your supply",

            [Ambassador.Grey] = "Ixian - Discard a Treachery Card and draw from the deck",
            [Ambassador.Purple] = "Tleilaxu - Revive one of your leaders or up to 4 of your forces for free",

            [Ambassador.Brown] = "CHOAM - Discard any of your Treachery Cards and gain 3 spice for each one",
            [Ambassador.White] = "Richese - Pay 3 spice to the Bank for a Treachery Card",

            [Ambassador.Pink] = "Ecaz - Gain Vidal if he is not in the Tanks, captured, or a ghola until used in a battle, or form an alliance with the visiting faction (if neither of you are allied); they may take control of Vidal instead. This token returns to your available supply"
        },

        TechTokenName_STR = Game.ExpansionLevel < 1 ? new() : new Dictionary<TechToken, string>
        {
            { TechToken.Graveyard, "Axlotl Tanks" },
            { TechToken.Ships, "Heighliners" },
            { TechToken.Resources, "Spice Production" }
        },

        TechTokenImage_URL = Game.ExpansionLevel < 1 ? new() : new Dictionary<TechToken, string>
        {
            { TechToken.Graveyard, DEFAULT_ART_LOCATION + "/art/techtoken0.svg" },
            { TechToken.Ships, DEFAULT_ART_LOCATION + "/art/techtoken1.svg" },
            { TechToken.Resources, DEFAULT_ART_LOCATION + "/art/techtoken2.svg" }
        },

        LeaderSkillCardName_STR = Game.ExpansionLevel < 2 ? new() : new Dictionary<LeaderSkill, string>
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

        LeaderSkillCardImage_URL = Game.ExpansionLevel < 2 ? new() : new Dictionary<LeaderSkill, string>
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

        HomeWorldImage_URL = new Dictionary<World, string>
        {
            [World.Green] = DEFAULT_ART_LOCATION + "/art/faction1planet.svg",
            [World.Black] = DEFAULT_ART_LOCATION + "/art/faction2planet.svg",
            [World.Yellow] = DEFAULT_ART_LOCATION + "/art/faction3planet.svg",
            [World.Red] = DEFAULT_ART_LOCATION + "/art/faction4planet.svg",
            [World.RedStar] = DEFAULT_ART_LOCATION + "/art/faction4planet2.svg",
            [World.Orange] = DEFAULT_ART_LOCATION + "/art/faction5planet.svg",
            [World.Blue] = DEFAULT_ART_LOCATION + "/art/faction6planet.svg",

            [World.Grey] = DEFAULT_ART_LOCATION + "/art/faction7planet.svg",
            [World.Purple] = DEFAULT_ART_LOCATION + "/art/faction8planet.svg",

            [World.Brown] = DEFAULT_ART_LOCATION + "/art/faction9planet.svg",
            [World.White] = DEFAULT_ART_LOCATION + "/art/faction10planet.svg",

            [World.Pink] = DEFAULT_ART_LOCATION + "/art/faction11planet.svg",
            [World.Cyan] = DEFAULT_ART_LOCATION + "/art/faction12planet.svg"
        },

        HomeWorldCardImage_URL = Game.ExpansionLevel < 3 ? new() : new Dictionary<World, string>
        {
            [World.Green] = DEFAULT_ART_LOCATION + "/art/CaladanCard.gif",
            [World.Black] = DEFAULT_ART_LOCATION + "/art/GiediPrimeCard.gif",
            [World.Yellow] = DEFAULT_ART_LOCATION + "/art/ArrakisCard.gif",
            [World.Red] = DEFAULT_ART_LOCATION + "/art/KaitainCard.gif",
            [World.RedStar] = DEFAULT_ART_LOCATION + "/art/SalusaSecundusCard.gif",
            [World.Orange] = DEFAULT_ART_LOCATION + "/art/JunctionCard.gif",
            [World.Blue] = DEFAULT_ART_LOCATION + "/art/WallachIXCard.gif",

            [World.Grey] = DEFAULT_ART_LOCATION + "/art/IxCard.gif",
            [World.Purple] = DEFAULT_ART_LOCATION + "/art/TleilaxCard.gif",

            [World.Brown] = DEFAULT_ART_LOCATION + "/art/TupileCard.gif",
            [World.White] = DEFAULT_ART_LOCATION + "/art/RicheseCard.gif",

            [World.Pink] = DEFAULT_ART_LOCATION + "/art/EcazCard.gif",
            [World.Cyan] = DEFAULT_ART_LOCATION + "/art/GrummanCard.gif"
        },

        NexusCardImage_URL = Game.ExpansionLevel < 3 ? new() : new Dictionary<Faction, string>
        {
            [Faction.Green] = DEFAULT_ART_LOCATION + "/art/faction1nexus.gif",
            [Faction.Black] = DEFAULT_ART_LOCATION + "/art/faction2nexus.gif",
            [Faction.Yellow] = DEFAULT_ART_LOCATION + "/art/faction3nexus.gif",
            [Faction.Red] = DEFAULT_ART_LOCATION + "/art/faction4nexus.gif",
            [Faction.Orange] = DEFAULT_ART_LOCATION + "/art/faction5nexus.gif",
            [Faction.Blue] = DEFAULT_ART_LOCATION + "/art/faction6nexus.gif",

            [Faction.Grey] = DEFAULT_ART_LOCATION + "/art/faction7nexus.gif",
            [Faction.Purple] = DEFAULT_ART_LOCATION + "/art/faction8nexus.gif",

            [Faction.Brown] = DEFAULT_ART_LOCATION + "/art/faction9nexus.gif",
            [Faction.White] = DEFAULT_ART_LOCATION + "/art/faction10nexus.gif",

            [Faction.Pink] = DEFAULT_ART_LOCATION + "/art/faction11nexus.gif",
            [Faction.Cyan] = DEFAULT_ART_LOCATION + "/art/faction12nexus.gif"
        },

        TerrorTokenName_STR = Game.ExpansionLevel < 3 ? new() : new Dictionary<TerrorType, string>
        {
            [TerrorType.None] = "None",
            [TerrorType.Assassination] = "Assassination",
            [TerrorType.Atomics] = "Atomics",
            [TerrorType.Extortion] = "Extortion",
            [TerrorType.Robbery] = "Robbery",
            [TerrorType.Sabotage] = "Sabotage",
            [TerrorType.SneakAttack] = "Sneak Attack"
        },

        TerrorTokenDescription_STR = Game.ExpansionLevel < 3 ? new() : new Dictionary<TerrorType, string>
        {
            [TerrorType.None] = "None",
            [TerrorType.Assassination] = "Choose a random leader from that player, send it to the Tanks and collect spice for it",
            [TerrorType.Atomics] = "All forces in the territory go to the Tanks. Place the Atomics Aftermath token in the territory. No forces may ever ship into this territory (even Fremen). From this turn forward, your hand limit is reduced by 1 (as well as your ally’s), discarding a random card if a hand exceeds the limit",
            [TerrorType.Extortion] = "Gain 5 spice from the Spice Bank, placed in front of your shield. Collect it in the Mentat Pause, then regain this Terror token unless any one player in storm order pays you 3 spice",
            [TerrorType.Robbery] = "Steal half the spice (rounded up) from that player or take the top card of the Treachery Deck (then discarding a card of your choice if you exceed your hand size)",
            [TerrorType.Sabotage] = "Draw a random Treachery Card from that player and discard it if possible. Then you may give that player a Treachery Card of your choice from your hand",
            [TerrorType.SneakAttack] = "Send up to 5 of your forces in reserves into that territory at no cost (subject to storm and occupancy rules), even if the Atomics Aftermath token is there"
        },

        DiscoveryTokenName_STR = Game.ExpansionLevel < 3 ? new() : new Dictionary<DiscoveryToken, string>
        {
            [DiscoveryToken.Jacurutu] = "Jacurutu Sietch",
            [DiscoveryToken.Shrine] = "Shrine",
            [DiscoveryToken.TestingStation] = "Ecological Testing Station",
            [DiscoveryToken.Cistern] = "Cistern",
            [DiscoveryToken.ProcessingStation] = "Orgiz Processing Station",
            [DiscoveryToken.CardStash] = "Treachery Card Stash",
            [DiscoveryToken.ResourceStash] = "Spice Stash",
            [DiscoveryToken.Flight] = "Ornithopter"
        },

        DiscoveryTokenDescription_STR = Game.ExpansionLevel < 3 ? new() : new Dictionary<DiscoveryToken, string>
        {
            [DiscoveryToken.Jacurutu] = "Counts as a normal stronghold.  If you win a battle here, gain 1 spice for each of your opponent’s undialed forces that go to the Tanks",
            [DiscoveryToken.Shrine] = "If occupied, you may play Truthtrance as a Karama card, and vice versa",
            [DiscoveryToken.TestingStation] = "If occupied during Storm, you may add or subtract the movement of the storm by 1, not affecting Weather Control",
            [DiscoveryToken.Cistern] = "If occupied during Collection, gain 2 spice from the bank",
            [DiscoveryToken.ProcessingStation] = "If occupied during Collection, steal 1 spice of each spice blow collected",
            [DiscoveryToken.CardStash] = "Gain a treachery card, remove this token from the game and discard a card if you exceed your maximum hand size",
            [DiscoveryToken.ResourceStash] = "Gain 7 spice from the bank and remove this token from the game",
            [DiscoveryToken.Flight] = "Gain the token and remove it from the game to gain 3 movement for one movement action."
        },

        DiscoveryTokenTypeName_STR = Game.ExpansionLevel < 3 ? new() : new Dictionary<DiscoveryTokenType, string>
        {
            [DiscoveryTokenType.Yellow] = "Hiereg",
            [DiscoveryTokenType.Orange] = "Smuggler"
        },

        DiscoveryTokenTypeImage_URL = Game.ExpansionLevel < 3 ? new() : new Dictionary<DiscoveryTokenType, string>
        {
            [DiscoveryTokenType.Yellow] = "/art/discoverytype1.png",
            [DiscoveryTokenType.Orange] = "/art/discoverytype2.png"
        },

        DiscoveryTokenImage_URL = Game.ExpansionLevel < 3 ? new() : new Dictionary<DiscoveryToken, string>
        {
            [DiscoveryToken.Jacurutu] = "/art/discovery1.png",
            [DiscoveryToken.Cistern] = "/art/discovery2.png",
            [DiscoveryToken.TestingStation] = "/art/discovery3.png",
            [DiscoveryToken.Shrine] = "/art/discovery4.png",
            [DiscoveryToken.ProcessingStation] = "/art/discovery5.png"
        },

        StrongholdCardName_STR = Game.ExpansionLevel < 2 ? new() : new Dictionary<int, string>
        {
            [2] = "Carthag",
            [3] = "Arrakeen",
            [4] = "Tuek's Sietch",
            [5] = "Sietch Tabr",
            [6] = "Habbanya Sietch",
            [42] = "Hidden Mobile Stronghold"
        },

        StrongholdCardImage_URL = Game.ExpansionLevel < 2 ? new() : new Dictionary<StrongholdAdvantage, string>
        {
            [StrongholdAdvantage.CountDefensesAsAntidote] = DEFAULT_ART_LOCATION + "/art/Carthag.gif",
            [StrongholdAdvantage.FreeResourcesForBattles] = DEFAULT_ART_LOCATION + "/art/Arrakeen.gif",
            [StrongholdAdvantage.CollectResourcesForUseless] = DEFAULT_ART_LOCATION + "/art/TueksSietch.gif",
            [StrongholdAdvantage.CollectResourcesForDial] = DEFAULT_ART_LOCATION + "/art/SietchTabr.gif",
            [StrongholdAdvantage.WinTies] = DEFAULT_ART_LOCATION + "/art/HabbanyaSietch.gif",
            [StrongholdAdvantage.AnyOtherAdvantage] = DEFAULT_ART_LOCATION + "/art/HMS.gif"
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

        Sound = new Dictionary<Milestone, string>
        {
            [Milestone.GameStarted] = DEFAULT_ART_LOCATION + "/art/intro.mp3",
            [Milestone.Shuffled] = DEFAULT_ART_LOCATION + "/art/shuffleanddeal.mp3",
            [Milestone.BabyMonster] = DEFAULT_ART_LOCATION + "/art/monster.mp3",
            [Milestone.Monster] = DEFAULT_ART_LOCATION + "/art/monster.mp3",
            [Milestone.GreatMonster] = DEFAULT_ART_LOCATION + "/art/monster.mp3",
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
            [Milestone.Discard] = DEFAULT_ART_LOCATION + "/art/crumple.mp3",
            [Milestone.SpecialUselessPlayed] = DEFAULT_ART_LOCATION + "/art/karma.mp3",
            [Milestone.Bureaucracy] = DEFAULT_ART_LOCATION + "/art/typewriter.mp3",
            [Milestone.Audited] = DEFAULT_ART_LOCATION + "/art/typewriter.mp3",
            [Milestone.TerrorPlanted] = DEFAULT_ART_LOCATION + "/art/terror.mp3",
            [Milestone.TerrorRevealed] = DEFAULT_ART_LOCATION + "/art/terror.mp3",
            [Milestone.AmbassadorPlaced] = DEFAULT_ART_LOCATION + "/art/ambassador.mp3",
            [Milestone.AmbassadorActivated] = DEFAULT_ART_LOCATION + "/art/ambassador.mp3",
            [Milestone.NexusPlayed] = DEFAULT_ART_LOCATION + "/art/fairybell.mp3",
            [Milestone.DiscoveryAppeared] = DEFAULT_ART_LOCATION + "/art/discoveryhorn.mp3",
            [Milestone.DiscoveryRevealed] = DEFAULT_ART_LOCATION + "/art/discoveryhorn.mp3",
            [Milestone.Assassination] = DEFAULT_ART_LOCATION + "/art/scream.mp3"
        },

        MapDimensions = new PointD(563, 626),
        PlanetRadius = 242,
        MapRadius = 260,
        PlanetCenter = new PointD(281, 311),
        PlayerTokenRadius = 11,

        SpiceDeckLocation = new PointD(0, 540),
        TreacheryDeckLocation = new PointD(475, 540),
        CardSize = new PointD(40, 56),

        BattleScreenWidth = 273,

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
        FORCETOKEN_FONT_BORDERCOLOR = "black",
        FORCETOKEN_FONT_BORDERWIDTH = 1,
        FORCETOKEN_RADIUS = 8,

        //Spice tokens
        RESOURCETOKEN_FONT = "normal normal bold 8px Verdana, Open Sans, Calibri, Tahoma, sans-serif",
        RESOURCETOKEN_FONTCOLOR = "white",
        RESOURCETOKEN_FONT_BORDERCOLOR = "black",
        RESOURCETOKEN_FONT_BORDERWIDTH = 1,
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
        FACTION_INFORMATIONCARDSTYLE = "font: normal normal normal 14px Calibri, Tahoma, sans-serif; color: white; padding: 5px 5px 5px 5px; overflow: auto; background-color: rgba(32,32,32,0.95); border-color: grey; border-style: solid; border-width: 1px; border-radius: 3px;"
    };

    #endregion

    public static Skin Current { get; set; } = Default;
}