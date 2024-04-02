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

public static class Enumerations
{
    public static IEnumerable<T> GetValues<T>(Type t)
    {
        var values = t.GetEnumValues();
        var result = new List<T>();

        for (var i = 0; i < values.Length; i++)
        {
            var value = (T)values.GetValue(i);
            result.Add(value);
        }

        return result;
    }

    public static IEnumerable<T> GetValuesExceptDefault<T>(Type t, T defaultValue)
    {
        var values = t.GetEnumValues();
        var result = new List<T>();

        for (var i = 0; i < values.Length; i++)
        {
            var value = (T)values.GetValue(i);
            if (!defaultValue.Equals(value)) result.Add(value);
        }

        return result;
    }
}

public enum Faction
{
    None = 0,
    Yellow = 10,
    Green = 20,
    Black = 30,
    Red = 40,
    Orange = 50,
    Blue = 60,
    Grey = 70,
    Purple = 80,
    Brown = 90,
    White = 100,
    Pink = 110,
    Cyan = 120
}

public enum Ambassador
{
    None = 0,
    Yellow = 10,
    Green = 20,
    Black = 30,
    Red = 40,
    Orange = 50,
    Blue = 60,
    Grey = 70,
    Purple = 80,
    Brown = 90,
    White = 100,
    Pink = 110,
    Cyan = 120
}

public enum TechToken
{
    None = 0,
    Graveyard = 10,
    Ships = 20,
    Resources = 30
}

public enum MainPhase
{
    None = 0,
    Started = 10,
    Setup = 20,
    Storm = 30,
    Blow = 40,
    Charity = 50,
    Bidding = 60,
    Resurrection = 70,
    ShipmentAndMove = 80,
    Battle = 90,
    Collection = 95,
    Contemplate = 100,
    Ended = 110
}

public enum MainPhaseMoment
{
    None = 0,
    Start = 10,
    Middle = 20,
    End = 30
}

public enum BrownEconomicsStatus
{
    None = 0,
    Double = 10,
    DoubleFlipped = 20,
    Cancel = 30,
    CancelFlipped = 40,
    RemovedFromGame = 50
}

public enum Phase
{
    None = 00000,
    AwaitingPlayers = 10000,

    SelectingFactions = 29000,
    TradingFactions = 30000,
    CustomizingDecks = 31000,

    BluePredicting = 50000,

    BlackMulligan = 60000,

    AssigningInitialSkills = 64000,
    SelectingTraitors = 65000,
    PerformCustomSetup = 66000,

    YellowSettingUp = 70000,

    BlueSettingUp = 90000,

    CyanSettingUp = 91000,

    BeginningOfStorm = 93000,

    MetheorAndStormSpell = 95000,

    HmsPlacement = 100000,
    HmsMovement = 101000,

    DiallingStorm = 105000,
    StormLosses = 110000,
    StormReport = 113000,

    Thumper = 115000,
    BlowA = 120000,
    HarvesterA = 125000,
    BlowB = 130000,
    HarvesterB = 135000,

    YellowSendingMonsterA = 140000,

    VoteAllianceA = 149900,
    AllianceA = 150000,
    YellowRidingMonsterA = 160000,
    YellowSendingMonsterB = 170000,
    VoteAllianceB = 179900,
    AllianceB = 180000,

    YellowRidingMonsterB = 190000,
    NexusCards = 191000,

    BlowReport = 194000,

    BeginningOfCharity = 194500,
    ClaimingCharity = 195000,
    CharityReport = 195100,

    BeginningOfBidding = 195300,
    BlackMarketAnnouncement = 195500,
    BlackMarketBidding = 195600,
    WhiteAnnouncingAuction = 195700,
    WhiteSpecifyingAuction = 195800,
    WhiteKeepingUnsoldCard = 195900,

    GreySelectingCard = 196000,
    GreyRemovingCardFromBid = 197000,
    GreySwappingCard = 198000,

    Bidding = 200000,
    ReplacingCardJustWon = 201000,
    WaitingForNextBiddingRound = 205000,
    BiddingReport = 208000,

    BeginningOfResurrection = 209000,
    Resurrection = 210000,
    BlueIntrudedByRevival = 211000,
    TerrorTriggeredByRevival = 211100,
    AmbassadorTriggeredByRevival = 211110,
    ResurrectionReport = 215000,

    BeginningOfShipAndMove = 219000,

    NonOrangeShip = 220000,

    OrangeShip = 230000,

    BlueAccompaniesNonOrangeShip = 240000,
    TerrorTriggeredByBlueAccompaniesNonOrangeShip = 241000,
    AmbassadorTriggeredByBlueAccompaniesNonOrangeShip = 241010,
    BlueAccompaniesOrangeShip = 250000,
    TerrorTriggeredByBlueAccompaniesOrangeShip = 251000,
    AmbassadorTriggeredByBlueAccompaniesOrangeShip = 251010,
    BlueIntrudedByNonOrangeShip = 255000,
    TerrorTriggeredByOrangeShip = 255100,
    AmbassadorTriggeredByOrangeShip = 255110,
    BlueIntrudedByOrangeShip = 256000,
    TerrorTriggeredByNonOrangeShip = 256100,
    AmbassadorTriggeredByNonOrangeShip = 256110,

    NonOrangeMove = 260000,

    OrangeMove = 270000,

    BlueIntrudedByNonOrangeMove = 280000,
    TerrorTriggeredByNonOrangeMove = 281000,
    AmbassadorTriggeredByNonOrangeMove = 281010,
    BlueIntrudedByOrangeMove = 290000,
    TerrorTriggeredByOrangeMove = 291000,
    AmbassadorTriggeredByOrangeMove = 291010,
    BlueIntrudedByCaravan = 295000,
    TerrorTriggeredByCaravan = 295100,
    AmbassadorTriggeredByCaravan = 295110,
    BlueIntrudedByYellowRidingMonsterA = 296000,
    TerrorTriggeredByYellowRidingMonsterA = 296100,
    AmbassadorTriggeredByYellowRidingMonsterA = 296110,
    BlueIntrudedByYellowRidingMonsterB = 297000,
    TerrorTriggeredByYellowRidingMonsterB = 297100,
    AmbassadorTriggeredByYellowRidingMonsterB = 297110,

    AllianceByTerror = 298000,
    AllianceByAmbassador = 298010,

    ShipmentAndMoveConcluded = 299000,

    BeginningOfBattle = 299900,
    BattlePhase = 300000,
    ClaimingBattle = 305000,
    CallTraitorOrPass = 310000,
    CancellingTraitor = 310100,
    CaptureDecision = 311000,
    AvoidingAudit = 312000,

    Auditing = 313000,
    Retreating = 314000,

    BattleConclusion = 315000,

    RevealingFacedancer = 319000,
    Facedancing = 320000,

    BattleReport = 330000,

    BeginningOfCollection = 339000,
    DividingCollectedResources = 339100,
    AcceptingResourceDivision = 339150,
    CollectionReport = 340000,

    PerformingKarmaHandSwap = 350000,

    TradingCards = 351000,

    Clairvoyance = 360000,

    SearchingDiscarded = 370000,

    ReplacingFaceDancer = 395000,

    Extortion = 397000,
    Contemplate = 398000,
    TurnConcluded = 399000,
    GameEnded = 400000,

    Bureaucracy = 508000,
    AssigningSkill = 509000,
    Thought = 510000,
    MeltingRock = 511000,
    Discarding = 600000,
    DiscardingTraitor = 700000
}

public enum Milestone
{
    None = 0,
    GameStarted = 50,
    Monster = 100,
    GreatMonster = 110,
    BabyMonster = 150,
    Resource = 200,
    MetheorUsed = 300,
    AuctionWon = 500,
    Shipment = 550,
    LeaderKilled = 600,
    Messiah = 650,
    Explosion = 700,
    GameWon = 800,
    CharityClaimed = 850,
    CardOnBidSwapped = 890,
    Bid = 900,
    CardWonSwapped = 910,
    Shuffled = 1100,
    Revival = 1200,
    Move = 1300,
    TreacheryCalled = 1400,
    FaceDanced = 1450,
    Clairvoyance = 1500,
    Karma = 1600,
    Bribe = 1700,
    Amal = 1800,
    Thumper = 1900,
    Harvester = 2000,
    HmsMovement = 2100,
    RaiseDead = 2200,
    WeatherControlled = 2300,
    Storm = 2400,
    Voice = 2500,
    Prescience = 2600,
    ResourcesReceived = 2700,
    Economics = 2800,
    CardTraded = 2900,
    Discard = 3000,
    SpecialUselessPlayed = 3100,
    TerrorPlanted = 3200,
    TerrorRevealed = 3300,
    AmbassadorPlaced = 3400,
    AmbassadorActivated = 3500,
    NexusPlayed = 3600,
    DiscoveryAppeared = 3700,
    DiscoveryRevealed = 3800,
    Assassination = 3900,
    Bureaucracy = 4000,
    Audited = 4100
}

public enum FactionAdvantage
{
    None = 0,

    GreenBiddingPrescience = 20,
    GreenSpiceBlowPrescience = 30,
    GreenBattlePlanPrescience = 35,
    GreenUseMessiah = 38,

    BlackFreeCard = 40,
    BlackCaptureLeader = 50,
    BlackCallTraitorForAlly = 51,

    BlueAccompanies = 60,
    BlueUsingVoice = 65,
    BlueWorthlessAsKarma = 70,
    BlueAnnouncesBattle = 71,
    BlueIntrusion = 72,
    BlueCharity = 73,

    YellowControlsMonster = 80,
    YellowRidesMonster = 81,
    YellowNotPayingForBattles = 85,
    YellowSpecialForceBonus = 90,
    YellowExtraMove = 95,
    YellowProtectedFromStorm = 96,
    YellowProtectedFromMonster = 97,
    YellowProtectedFromMonsterAlly = 98,
    YellowStormPrescience = 99,

    RedSpecialForceBonus = 100,
    RedReceiveBid = 105,
    RedGiveSpiceToAlly = 106,
    RedLetAllyReviveExtraForces = 107,

    OrangeDetermineMoveMoment = 110,
    OrangeSpecialShipments = 111,
    OrangeShipmentsDiscount = 112,
    OrangeShipmentsDiscountAlly = 113,
    OrangeReceiveShipment = 114,

    PurpleRevivalDiscount = 120,
    PurpleRevivalDiscountAlly = 121,
    PurpleReplacingFaceDancer = 122,//*
    PurpleIncreasingRevivalLimits = 123,//*
    PurpleReceiveRevive = 124,
    PurpleEarlyLeaderRevive = 125, //*
    PurpleReviveGhola = 126, //*

    GreyMovingHMS = 130,
    GreySpecialForceBonus = 131,
    GreySelectingCardsOnAuction = 132,
    GreyCyborgExtraMove = 133,
    GreyReplacingSpecialForces = 134, //*
    GreyAllyDiscardingCard = 135, //*
    GreySwappingCard = 136, //*

    BrownControllingCharity = 140,
    BrownDiscarding = 141,
    BrownRevival = 142,
    BrownEconomics = 143,
    BrownReceiveForcePayment = 144,
    BrownAudit = 145,

    WhiteAuction = 150,
    WhiteNofield = 151,
    WhiteBlackMarket = 152,

    PinkAmbassadors = 160,
    PinkOccupation = 161,
    PinkCollection = 162,

    CyanPlantingTerror = 170,
    CyanGainingVidal = 171,
    CyanEnemyOfEnemy = 172
}

public enum FactionForce
{
    None = 0,
    Yellow = 10,
    Green = 20,
    Black = 30,
    Red = 40,
    Orange = 50,
    Blue = 60,
    Grey = 70,
    Purple = 80,
    Brown = 90,
    White = 100,
    Pink = 110,
    Cyan = 120
}

public enum FactionSpecialForce
{
    None = 0,
    Yellow = 10,
    Red = 20,
    Blue = 30,
    Grey = 40,
    White = 50
}

public enum Ruleset
{
    None = 0,
    BasicGame = 10,

    AdvancedGame = 30,

    ExpansionBasicGame = 110,
    ExpansionAdvancedGame = 130,

    ServerClassic = 140,

    Expansion2BasicGame = 150,
    Expansion2AdvancedGame = 170,

    Expansion3BasicGame = 300,
    Expansion3AdvancedGame = 310,

    AllExpansionsBasicGame = 180,
    AllExpansionsAdvancedGame = 200,

    Custom = 1000
}

public enum DiscoveryToken
{
    None = 0,
    Jacurutu = 10,
    Shrine = 20,
    TestingStation = 30,
    Cistern = 40,
    ProcessingStation = 50,
    CardStash = 60,
    ResourceStash = 70,
    Flight = 80
}

public enum DiscoveryTokenType
{
    None = 0,
    Yellow = 10,
    Orange = 20
}

public enum Rule
{
    None = 0,

    //Basic classic game
    BasicTreacheryCards = 10000,
    HasCharityPhase = 10001,





    //Advanced Game
    AdvancedCombat = 10,
    IncreasedResourceFlow = 15,
    AdvancedKarama = 20,

    YellowSeesStorm = 25,
    YellowStormLosses = 30,
    YellowSendingMonster = 35,
    YellowSpecialForces = 40,

    GreenMessiah = 45,

    BlackCapturesOrKillsLeaders = 50,

    BlueFirstForceInAnyTerritory = 55, //needed for versions < 144
    BlueAutoCharity = 60,
    BlueWorthlessAsKarma = 65,
    BlueAdvisors = 70,
    BlueAccompaniesToShipmentLocation = 75,  //needed for versions < 144

    OrangeDetermineShipment = 80,

    RedSpecialForces = 85,

    //Exceptions
    BribesAreImmediate = 90,
    ContestedStongholdsCountAsOccupied = 95,
    AdvisorsDontConflictWithAlly = 96,
    OrangeShipmentContributionsFlowBack = 97,

    //Expansion
    TechTokens = 200,
    ExpansionTreacheryCards = 209,
    ExpansionTreacheryCardsExceptPBandSSandAmal = 210,
    ExpansionTreacheryCardsPBandSS = 211,
    ExpansionTreacheryCardsAmal = 212,
    CheapHeroTraitor = 220,
    SandTrout = 230,

    //Expansion, Advanced Game
    GreySwappingCardOnBid = 300,
    PurpleGholas = 301,

    //Expansion 2
    LeaderSkills = 400,
    StrongholdBonus = 401,
    WhiteTreacheryCards = 402,
    Expansion2TreacheryCards = 403,

    //Expansion 2, Advanced Game
    BrownAuditor = 500,
    WhiteBlackMarket = 501,

    //Expansion 3
    GreatMaker = 600,
    DiscoveryTokens = 601,
    Homeworlds = 602,
    NexusCards = 603,
    Expansion3TreacheryCards = 604,

    //Expansion 3, Advanced Game
    CyanAssassinate = 700,
    PinkLoyalty = 701,

    //Bots
    FillWithBots = 998,
    OrangeBot = 1000,
    RedBot = 1001,
    BlackBot = 1002,
    PurpleBot = 1003,
    BlueBot = 1004,
    GreenBot = 1005,
    YellowBot = 1006,
    GreyBot = 1007,
    BotsCannotAlly = 1008,
    //BrownBot = 1009,
    //WhiteBot = 1013,

    //House Rules
    CustomInitialForcesAndResources = 100,
    CustomDecks = 101,
    HMSwithoutGrey = 104,
    SSW = 105,
    BlackMulligan = 106,
    FullPhaseKarma = 107,
    YellowMayMoveIntoStorm = 108,
    BlueVoiceMustNameSpecialCards = 109,
    BattlesUnderStorm = 110,
    MovementBonusRequiresOccupationBeforeMovement = 111,
    AssistedNotekeeping = 112,
    ResourceBonusForStrongholds = 113,
    DisableOrangeSpecialVictory = 114,
    DisableResourceTransfers = 115,
    StormDeckWithoutYellow = 116,
    AssistedNotekeepingForGreen = 117,
    YellowAllyGetsDialedResourcesRefunded = 118,

    ExtraKaramaCards = 999,

    CardsCanBeTraded = 1010,
    PlayersChooseFactions = 1011,
    RedSupportingNonAllyBids = 1012,
    BattleWithoutLeader = 1013,
    CapturedLeadersAreTraitorsToOwnFaction = 1014,
    DisableEndOfGameReport = 1015

}

public enum RuleGroup
{
    None = 0,
    CoreBasic = 100,
    CoreBasicExceptions = 101,
    CoreAdvanced = 110,
    CoreAdvancedExceptions = 111,
    ExpansionIxAndBtBasic = 200,
    ExpansionIxAndBtBasicExceptions = 201,
    ExpansionIxAndBtAdvanced = 210,
    ExpansionIxAndBtAdvancedExceptions = 211,
    ExpansionBrownAndWhiteBasic = 300,
    ExpansionBrownAndWhiteBasicExceptions = 301,
    ExpansionBrownAndWhiteAdvanced = 310,
    ExpansionBrownAndWhiteAdvancedExceptions = 311,
    ExpansionPinkAndCyanBasic = 400,
    ExpansionPinkAndCyanBasicExceptions = 401,
    ExpansionPinkAndCyanAdvanced = 410,
    ExpansionPinkAndCyanAdvancedExceptions = 411,
    House = 1000,
    Bots = 2000
}

public enum TreacheryCardType
{
    None = 0,
    Laser = 10,

    ProjectileDefense = 19,
    Projectile = 20,
    WeirdingWay = 25,

    PoisonDefense = 29,
    Poison = 30,
    PoisonTooth = 31,
    Chemistry = 35,

    Shield = 40,
    Antidote = 50,

    ProjectileAndPoison = 55,
    ShieldAndAntidote = 56,

    Mercenary = 60,
    Karma = 70,
    Useless = 80,
    StormSpell = 90,
    RaiseDead = 100,
    Metheor = 110,
    Caravan = 120,
    Clairvoyance = 130,

    Amal = 140,
    ArtilleryStrike = 150,
    Thumper = 160,
    Harvester = 170,

    Distrans = 180,
    Juice = 190,
    MirrorWeapon = 200,
    PortableAntidote = 210,
    Flight = 220,
    SearchDiscarded = 230,
    TakeDiscarded = 240,
    Residual = 250,
    Rockmelter = 260,
    Recruits = 261,
    Reinforcements = 262,
    HarassAndWithdraw = 263
}

public enum Concept
{
    None = 0,
    Messiah = 10,
    Resource = 20,
    Monster = 30,
    Graveyard = 40,
    BabyMonster = 50,
    GreatMonster = 60
}

public enum WinMethod
{
    None = 0,
    Strongholds = 10,
    Prediction = 20,
    YellowSpecial = 30,
    OrangeSpecial = 40,
    Forfeit = 50,
    Timeout = 60
}

public enum HeroType
{
    None = 0,
    Normal = 10,
    Messiah = 20,
    Mercenary = 30,
    Auditor = 40,
    Vidal = 50,
    VariableValue = 60
}

public enum AuctionType
{
    None = 0,
    Normal = 10,
    BlackMarketNormal = 20,
    BlackMarketOnceAround = 30,
    BlackMarketSilent = 40,
    WhiteOnceAround = 60,
    WhiteSilent = 70
}

public enum JuiceType
{
    None = 0,
    GoFirst = 10,
    GoLast = 20,
    Aggressor = 30
}

public enum LeaderSkill
{
    None = 0,
    Bureaucrat = 10,
    Diplomat = 20,
    Decipherer = 30,
    Smuggler = 40,
    Graduate = 50,
    Planetologist = 60,
    Warmaster = 70,
    Adept = 80,
    Swordmaster = 90,
    KillerMedic = 100,
    MasterOfAssassins = 110,
    Sandmaster = 120,
    Thinker = 130,
    Banker = 140
}

public enum StrongholdAdvantage
{
    None = 0,
    FreeResourcesForBattles = 10,
    CollectResourcesForUseless = 20,
    CountDefensesAsAntidote = 30,
    WinTies = 40,
    CollectResourcesForDial = 50,
    AnyOtherAdvantage = 60
}

public enum World
{
    None = 0,
    Yellow = 10,
    Green = 20,
    Black = 30,
    Red = 40,
    RedStar = 45,
    Orange = 50,
    Blue = 60,
    Grey = 70,
    Purple = 80,
    Brown = 90,
    White = 100,
    Pink = 110,
    Cyan = 120
}


public enum CaptureDecision
{
    None = 0,
    DontCapture = 10,
    Capture = 20,
    Kill = 30
}

public enum DealType
{
    None = 0,
    DontShipOrMoveTo = 10,
    ShareBiddingPrescience = 30,
    ShareResourceDeckPrescience = 50,
    ShareStormPrescience = 60,
    ForfeitBattle = 70,
    TellDiscardedTraitors = 80
}

public enum TerrorType
{
    None = 0,
    Assassination = 10,
    Atomics = 30,
    Extortion = 50,
    Robbery = 60,
    Sabotage = 70,
    SneakAttack = 80
}

public enum IntrusionType
{
    None = 0,
    BlueIntrusion = 10,
    Terror = 20,
    Ambassador = 30
}

public enum VidalMoment
{
    None = 0,
    AfterUsedInBattle = 10,
    EndOfTurn = 20,
    WhilePinkWorldIsOccupied = 30
}

public enum ClairVoyanceAnswer
{
    None = 0,
    Yes = 10,
    No = 20,
    Unknown = 30
}

public enum ClairvoyanceQuestion
{
    None = 0,

    Prediction = 10,
    LeaderAsTraitor = 20,
    LeaderAsFacedancer = 30,
    HasCardTypeInHand = 40,

    LeaderInBattle = 100,
    CardTypeInBattle = 110,
    CardTypeAsDefenseInBattle = 111,
    CardTypeAsWeaponInBattle = 112,
    DialOfMoreThanXInBattle = 120,

    WillAttackX = 200
}

public enum PrescienceAspect
{
    None = 0,
    Dial = 10,
    Leader = 20,
    Weapon = 30,
    Defense = 40
}

[Flags]
public enum ShipmentPermission
{
    None = 0,
    Cross = 1,
    ToHomeworld = 2,
    OrangeRate = 4
}