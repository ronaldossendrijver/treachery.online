/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Collections;

namespace Treachery.Shared;

public class Skin : IDescriber
{
    #region Attributes
    
    // ReSharper disable InconsistentNaming

    public const int LatestVersion = 151;

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
    public Dictionary<Nexus, string> NexusCardImage_URL;
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
    
    // ReSharper restore InconsistentNaming

    #endregion Attributes

    #region Descriptions

    public string Format(string m, params object[] list)
    {
        try
        {
            return list.Length == 0 ? m : string.Format(m, Describe(list));
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

    public PointD GetCenter(Location location, Map map)
    {
        if (location is HiddenMobileStronghold hms)
        {
            if (hms.AttachedToLocation != null)
            {
                var dX = hms.AttachedToLocation.Territory == map.RockOutcroppings ? 22 : 0;
                var dY = hms.AttachedToLocation.Territory == map.RockOutcroppings ? -10 : 0;
                var attachedToCenter = GetCenter(hms.AttachedToLocation, map);
                return new PointD(attachedToCenter.X + (int)HmsDx + dX, attachedToCenter.Y + dY);
            }

            return new PointD((int)HmsDx, (int)HmsDx);
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

    public float HmsDx => -3 * PlayerTokenRadius;

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
            UserStatus us => Describe(us),
            SubscriptionType st => Describe(st),
            Concept c => Describe(c),
            Faction faction => Describe(faction),
            Ambassador ambassador => Describe(ambassador),
            FactionForce ff => Describe(ff),
            FactionSpecialForce fsf => Describe(fsf),
            FactionAdvantage factionAdvantage => Describe(factionAdvantage),
            ResourceCard rc => Describe(rc),
            TreacheryCard tc => Describe(tc),
            TreacheryCardType tct => Describe(tct),
            LeaderSkill ls => Describe(ls),
            TerrorType terr => Describe(terr),
            DiscoveryToken dt => Describe(dt),
            DiscoveryTokenType dtd => Describe(dtd),
            Ruleset r => Describe(r),
            AutomationRuleType r => Describe(r),
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
            IEnumerable iEnum => Join(iEnum.Cast<object>()),
            _ => value.ToString()
        };
    }

    private object[] Describe(object[] objects)
    {
        var result = new object[objects.Length];
        for (var i = 0; i < objects.Length; i++) result[i] = Describe(objects[i]);
        return result;
    }

    private static string Describe(ClairVoyanceAnswer answer)
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

    private static string Describe(UserStatus us)
    {
        return us switch
        {
            UserStatus.None => "No status",
            UserStatus.Online => "Online",
            UserStatus.Away => "Away (not available)",
            UserStatus.Lfg => "Looking for game",
            UserStatus.Lfm => "Looking for others to join",
            UserStatus.InGame => "In game",
            _ => "Unknown status"
        };
    }
    
    private static string Describe(SubscriptionType st)
    {
        return st switch
        {
            SubscriptionType.DontParticipate => "no",
            SubscriptionType.MaybeAsPlayer => "maybe",
            SubscriptionType.CertainAsPlayer => "yes",
            SubscriptionType.MaybeAsHost => "maybe*",
            SubscriptionType.CertainAsHost => "yes*",
            _ => "unknown"
        };
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

    private static string GetPopup(string c)
        => $"<div style='background-color:white;border-color:black;border-width:1px;border-style:solid;color:black;padding:2px;'>{c}</div>";

    public string GetPopup(ResourceCard c)
        => GetImageHoverHtml(GetImageUrl(c));

    public string GetPopup(TerrorType t)
        => GetPopup(GetTerrorTypeDescription(t));

    public string GetPopup(LeaderSkill c)
        => GetImageHoverHtml(GetImageUrl(c));

    public string GetPopup(TechToken tt)
        => GetImageHoverHtml(GetImageUrl(tt));

    public string GetNexusCardPopup(Nexus n)
        => GetImageHoverHtml(GetImageUrl(n));

    public string GetPopup(Homeworld w)
        => $"<div style='position:relative'><img width=480 style='position:relative;filter:drop-shadow(-3px 3px 2px black);' src='{GetHomeworldCardImageUrl(w.World)}'/></div>";

    public string GetPopup(TreacheryCard c)
    {
        if (c == null)
            return "";
        return
            $"<div style='filter:drop-shadow(-3px 3px 2px black);'><img src='{GetImageUrl(c)}' width=300 class='img-fluid'/></div><div class='bg-dark text-white text-center' style='width:300px'>{GetTreacheryCardDescription(c)}</div>";
    }

    public string GetPopup(IHero h)
    {
        return GetImageHoverHtml(GetImageUrl(h));
    }

    public string GetPopup(IHero h, Game g)
    {
        if (h == null) return "";

        var skill = g.Skill(h);

        if (skill == LeaderSkill.None)
        {
            return GetImageHoverHtml(GetImageUrl(h));
        }

        return GetPopup(h as Leader, skill);
    }

    public string GetPopup(Leader l, LeaderSkill s)
        => l == null ? "" : $"<div style='position:relative'><img style='position:relative;filter:drop-shadow(-3px 3px 2px black);' src='{GetImageUrl(s)}' width=300/><img src='{GetImageUrl(l)}' width=140 style='position:absolute;left:200px;top:120px;filter:drop-shadow(-2px 2px 2px black);'/></div>";
    
    public string GetPopup(StrongholdAdvantage adv)
        => GetImageHoverHtml(GetImageUrl(adv));

    public string GetPopup(StrongholdAdvantage adv, Faction f)
        => $"<div style='position:relative'><img style='position:relative;filter:drop-shadow(-3px 3px 2px black);' src='{GetImageUrl(adv)}' width=300/><img src='{GetImageUrl(f)}' width=100 style='position:absolute;left:220px;top:40px;filter:drop-shadow(-3px 3px 2px black);'/></div>";

    private string GetImageHoverHtml(string imageUrl)
        => $"<img src='{imageUrl}' width=300 class='img-fluid' style='filter:drop-shadow(-3px 3px 2px black);'/>";

    public string GetImageUrl(object obj)
    {
        return obj switch
        {
            null => "",
            TreacheryCard tc => GetImageUrl(tc),
            ResourceCard rc => GetImageUrl(rc),
            IHero h => GetImageUrl(h),
            LeaderSkill ls => GetImageUrl(ls),
            TechToken tt => GetImageUrl(tt),
            Faction f => GetImageUrl(f),
            Nexus n => GetImageUrl(n),
            Ambassador a => GetImageUrl(a),
            FactionForce ff => GetImageUrl(ff),
            FactionSpecialForce fsf => GetImageUrl(fsf),
            StrongholdAdvantage adv => GetImageUrl(adv),
            _ => ""
        };
    }

    public string GetImageUrl(DiscoveryTokenType dtt)
    {
        return GetLabel(DiscoveryTokenTypeImage_URL, dtt);
    }

    public string GetImageUrl(DiscoveryToken dt)
    {
        return GetLabel(DiscoveryTokenImage_URL, dt);
    }

    public string Describe(DiscoveryTokenType dtt)
    {
        return GetLabel(DiscoveryTokenTypeName_STR, dtt);
    }

    private string Describe(TerrorType terr)
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

    public string GetImageUrl(Ambassador ambassador)
    {
        return GetLabel(AmbassadorImage_URL, ambassador);
    }

    public string Describe(MainPhase p)
    {
        return GetLabel(MainPhase_STR, p);
    }

    private string Describe(TechToken tt)
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

    private string Describe(FactionAdvantage advantage)
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
            FactionAdvantage.PurpleReviveGhola => Format("{0} reviving a leader as a Ghola", Faction.Purple),
            FactionAdvantage.GreyMovingHms => Format("{0} moving the Hidden Mobile Stronghold", Faction.Grey),
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

    private static string Describe(Ruleset s)
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
    
    private string Describe(AutomationRuleType s)
    {
        return s switch
        {
            AutomationRuleType.CharityAutoClaim => "Claim charity if possible",
            AutomationRuleType.BiddingPassWhenGreenOrGreenAllyPassed => $"Pass if most recent bid by {Describe(Faction.Green)} or their ally passed",
            AutomationRuleType.BiddingPassAboveAmount => "Pass if current bid is equal to or higher than...",
            AutomationRuleType.BiddingPassWhenHighestBidByFaction => "Pass if current highest bid by...",
            AutomationRuleType.RevivalAutoClaimFreeRevival => "Auto claim free revival",
            AutomationRuleType.ShipmentOrangeAutoDelay => "Auto delay shipment until last",
            _ => "unknown automation rule"
        };
    }

    private static string Describe(RuleGroup s)
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

    private static string Describe(Phase p)
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

    private static string Describe(BrownEconomicsStatus p)
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

    private static string Describe(AuctionType t)
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

    private static string Describe(JuiceType jt)
    {
        return jt switch
        {
            JuiceType.GoFirst => "be considered first in storm order",
            JuiceType.GoLast => "be considered last in storm order",
            JuiceType.Aggressor => "be considered aggressor in this battle",
            _ => "None"
        };
    }

    private static string Describe(CaptureDecision c)
    {
        return c switch
        {
            CaptureDecision.Capture => "Keep",
            CaptureDecision.Kill => "Kill to gain 2 spice",
            CaptureDecision.DontCapture => "Neither keep nor kill",
            _ => "None"
        };
    }

    private string Describe(StrongholdAdvantage sa)
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
            Rule.YellowDeterminesStorm => Format("{0} determine storm movement with the Storm Deck", Faction.Yellow),
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
            Rule.HmSwithoutGrey => Format("Use the {0} if {1} are not in play", "Hidden Mobile Stronghold", Faction.Grey),
            Rule.StormDeckWithoutYellow => Format("Use the Storm Deck if {0} are not in play", Faction.Yellow),

            Rule.Ssw => Format("SSW: {0} counts for victory after fourth {1}", "Shield Wall", Concept.Monster),
            Rule.BlackMulligan => Format("{0} mulligan traitors when they drew > 1 of their own", Faction.Black),

            Rule.TechTokens => "Tech Tokens",
            Rule.ExpansionTreacheryCards => "Expansion Treachery Cards",
            Rule.ExpansionTreacheryCardsExceptPBandSSandAmal => Format("Treachery Cards: all except {0}, {1} and {2}", TreacheryCardType.ProjectileAndPoison, TreacheryCardType.ShieldAndAntidote, TreacheryCardType.Amal),
            Rule.ExpansionTreacheryCardsPBandSs => Format("Treachery Cards: {0} and {1}", TreacheryCardType.ProjectileAndPoison, TreacheryCardType.ShieldAndAntidote),
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
            Rule.DisableNovaFlipping  => "No Nova flipping (advisors will not auto flip after shipment phase)",
            
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
    
    public string Describe(ErrorType e)
    {
        return e switch
        {
            ErrorType.None => string.Empty,
            ErrorType.UserNotFound => "User not found",
            ErrorType.GameNotFound => "Game not found",
            ErrorType.ScheduledGameNotFound => "Game not found",
            ErrorType.UserNameTooShort => "Username must be more than 3 characters",
            ErrorType.PlayerNameTooShort => "Player name must be more than 3 characters",
            ErrorType.UserNameTooLong => "Username must be 40 characters or less",
            ErrorType.PlayerNameTooLong => "Player name must be 40 characters or less",
            ErrorType.UserNameExists => "This username already exists",
            ErrorType.EmailExists => "This e-mail address is already in use",
            ErrorType.UserCreationFailed => "User creation failed",
            ErrorType.InvalidGameVersion => "Invalid game version",
            ErrorType.InvalidUserNameOrPassword => "Invalid user name or password",
            ErrorType.UnknownUsernameOrEmailAddress => "Unknown user name or email address",
            ErrorType.ResetRequestTooSoon => "Please wait at least 5 minutes before requesting another password reset",
            ErrorType.UnknownUserName => "Unknown user name",
            ErrorType.InvalidResetToken => "Invalid password reset token",
            ErrorType.ResetTokenExpired => "Your password reset token has expired",
            ErrorType.NoHost => "You are not a host",
            ErrorType.InvalidGameEvent => "Invalid game action",
            ErrorType.TooManyGames => "You cannot have more than 3 active games, delete a game first",
            ErrorType.TooManyScheduledGames => "You cannot schedule more than 10 games, delete a game first",
            ErrorType.IncorrectGamePassword => "Incorrect password",
            ErrorType.SeatNotAvailableNotOpen => "Seat is not available",
            ErrorType.SeatNotAvailableKicked => "You were removed from this game",
            ErrorType.AlreadyObserver => "You are already an observer in this game",
            ErrorType.CannotRemoveLastHost => "You cannot remove the only remaining host from the game",
            ErrorType.NoCreator => "You are not the creator of this game",
            ErrorType.AlreadyPlayer => "You are already a player in this game",
            ErrorType.UserNotInGame => "User not found in game",
            
            _ => throw new ArgumentOutOfRangeException(nameof(e), e, null)
        };
    }

    #endregion Descriptions

    #region NamesAndImages

    public string GetTerritoryBorder(Territory t)
    {
        return t != null ? GetLabel(TerritoryBorder_SVG, t.SkinId) : "";
    }

    public string GetImageUrl(TreacheryCard c)
    {
        return c != null ? GetLabel(TreacheryCardImage_URL, c.SkinId) : "";
    }

    public string GetImageUrl(World w)
    {
        return GetLabel(HomeWorldImage_URL, w);
    }

    public string GetImageUrl(StrongholdAdvantage a)
    {
        return GetLabel(StrongholdCardImage_URL, a);
    }

    public string GetImageUrl(ResourceCard c)
    {
        return c != null ? GetLabel(ResourceCardImage_URL, c.SkinId) : "";
    }

    public string GetImageUrl(LeaderSkill s)
    {
        return GetLabel(LeaderSkillCardImage_URL, s);
    }

    private string GetTreacheryCardDescription(TreacheryCard c)
    {
        return c != null ? GetLabel(TreacheryCardDescription_STR, c.SkinId) : "";
    }

    public string GetImageUrl(IHero h)
    {
        if (h == null)
            return "";
        if (h is TreacheryCard)
            return GetLabel(TreacheryCardImage_URL, h.SkinId);
        if (h is Messiah)
            return Messiah_URL;
        return GetLabel(PersonImage_URL, h.SkinId);
    }
    public string GetImageUrl(Faction faction)
    {
        return GetLabel(FactionImage_URL, faction);
    }

    public string GetFactionTableImageUrl(Faction faction)
    {
        return GetLabel(FactionTableImage_URL, faction);
    }

    public string GetFactionFacedownImageUrl(Faction faction)
    {
        return GetLabel(FactionFacedownImage_URL, faction);
    }

    public string GetFactionForceImageUrl(Faction f)
    {
        return GetLabel(FactionForceImage_URL, f);
    }

    public string GetFactionSpecialForceImageUrl(Faction f)
    {
        return GetLabel(FactionSpecialForceImage_URL, f);
    }

    private string GetImageUrl(FactionForce ff)
    {
        return GetFactionForceImageUrl(GetFaction(ff));
    }

    private string GetImageUrl(FactionSpecialForce fsf)
    {
        return GetFactionSpecialForceImageUrl(GetFaction(fsf));
    }

    public string GetHomeworldCardImageUrl(World w)
    {
        return GetLabel(HomeWorldCardImage_URL, w);
    }

    public string GetImageUrl(Nexus n)
    {
        return GetLabel(NexusCardImage_URL, n);
    }

    public string GetImageUrl(TechToken tech)
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

    public string GetFactionColorTransparent(Faction faction, string transparency)
    {
        return GetLabel(FactionColor, faction) + transparency;
    }

    #endregion NamesAndImages

    #region FactionManual

    public string GetFactionInfo_HTML(Game g, Faction f)
    {
        object[] parameters =
        [
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
            TreacheryCardName_STR[TreacheryCardManager.Lookup.Find(TreacheryCardManager.CardBaliset).SkinId], //37
            TreacheryCardName_STR[TreacheryCardManager.Lookup.Find(TreacheryCardManager.CardJubbaCloak).SkinId], //38
            TreacheryCardName_STR[TreacheryCardManager.Lookup.Find(TreacheryCardManager.CardKullWahad).SkinId], //39
            TreacheryCardName_STR[TreacheryCardManager.Lookup.Find(TreacheryCardManager.CardKulon).SkinId], //40
            TreacheryCardName_STR[TreacheryCardManager.Lookup.Find(TreacheryCardManager.CardLalala).SkinId], //41
            TreacheryCardName_STR[TreacheryCardManager.Lookup.Find(TreacheryCardManager.CardTripToGamont).SkinId], //42
            MainPhase_STR[MainPhase.Contemplate], //43
            PersonName_STR[1036], //44
            MainPhase_STR[MainPhase.Bidding], //45
            MainPhase_STR[MainPhase.Battle], //46
            MainPhase_STR[MainPhase.Charity], //47
            Faction.Pink, //48
            Faction.Cyan, //49
            g.Map.ImperialBasin // 50

        ];

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

    private string Advantage(object title, string advantageDescription, bool condition = true)
    {
        return Advantage(Describe(title), advantageDescription, condition);
    }

    private string Advantage(Game g, Rule rule, string advantageName, string advantageDescription)
    {
        return g.Applicable(rule) ? $"<p><strong>{advantageName.ToUpper()}</strong> - {advantageDescription}" : "";
    }

    private string Advantage(Game g, Rule rule, object title, string advantageDescription)
    {
        return Advantage(g, rule, Describe(title), advantageDescription);
    }

    private static string AdvancedHeader(Game g, params Rule[] rules)
    {
        return rules.Any(g.Applicable) ? "<h5>Advanced Game Advantages</h5>" : "";
    }

    private string GetGreenTemplate(Game g)
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


    private string GetBlackTemplate(Game g)
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

    private string GetYellowTemplate(Game g)
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
               AdvancedHeader(g, Rule.YellowDeterminesStorm, Rule.YellowSendingMonster, Rule.YellowStormLosses,
                   Rule.YellowSpecialForces, Rule.AdvancedCombat, Rule.AdvancedKarama) +
               Advantage(g, Rule.YellowDeterminesStorm, "Storm rule",
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

    private string GetRedTemplate(Game g)
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

    private string GetOrangeTemplate(Game g)
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

    private string GetBlueTemplate(Game g)
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

    private string GetPurpleTemplate(Game g)
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
                  <p>Once revealed you do not replace a Face Dancer (Traitor Card) until you have revealed all 3. When that happens, they are shuffled into the Traitor deck and you get 3 new Face Dancers.
                  During {43}, you may replace an unrevealed Face Dancer with one from the traitor deck (after shuffling it back).</p>") +
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

    private string GetGreyTemplate(Game g)
    {
        return SheetHeader("3 {32} and 3 {33} in the {34}. 10 {32} and 4 {33} in reserve (off-planet)", 10, 1,
                   "You are skilled in technology and production") +
               Advantage("Start of game",
                   "During Setup you see all initially dealt Treachery Cards and choose your starting card from them.") +
               Advantage("Bidding",
                   "Before Bidding, one extra card is drawn and you see them all and put one of those cards on top or on the bottom of the Treachery Card deck. The remaining cards are shuffled for the bidding round.") +
               Advantage(FactionSpecialForce.Grey,
                   "Your 7 {33} are each worth 2 normal forces in battle, are able to move 2 territories instead of 1 and can collect 3 {16}. Your {33} forces ship normally, but each costs 3 to revive.") +
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

    private string GetWhiteTemplate(Game g)
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

    private string GetBrownTemplate(Game g)
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

    private string GetPinkTemplate(Game g)
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

    private string GetCyanTemplate(Game g)
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

    public static Skin Load(string data, Skin donor)
    {
        var result = Utilities.Deserialize<Skin>(data);
        Fix(result, donor);
        return result;
    }

    private static void Fix(Skin toFix, Skin donor)
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

    public string SkinToString() => Utilities.Serialize(this);

    #endregion LoadingAndSaving
}