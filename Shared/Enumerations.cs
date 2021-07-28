/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;

namespace Treachery.Shared
{
    public static class Enumerations
    {
        public static IEnumerable<T> GetValues<T>(Type t)
        {
            var values = t.GetEnumValues();
            var result = new List<T>();

            for (int i = 0; i < values.Length; i++)
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

            for (int i = 0; i < values.Length; i++)
            {
                var value = (T)values.GetValue(i);
                if (!defaultValue.Equals(value))
                {
                    result.Add(value);
                }
            }

            return result;
        }
    }

    public enum Faction : int
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

    public enum TechToken : int
    {
        None = 0,
        Graveyard = 10,
        Ships = 20,
        Resources = 30
    }

    public enum MainPhase : int
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

    public enum MainPhaseMoment : int
    {
        None = 0,
        Start = 10,
        Middle = 20,
        End = 30
    }

    public enum BrownEconomicsStatus : int
    {
        None = 0,
        Double = 10,
        DoubleFlipped = 20,
        Cancel = 30,
        CancelFlipped = 40,
        RemovedFromGame = 50
    }

    /// <summary>
    /// When adding a new phase, make sure you add it to Treachery.online.Client.Handler_GameStatus and Treachery.Shared.Skin
    /// </summary>
    public enum Phase : int
    {
        None = 0,
        AwaitingPlayers = 10,
        SelectingFactions = 29,
        TradingFactions = 30,

        [PhaseProperty(true, Faction.Blue)]
        BluePredicting = 50,

        [PhaseProperty(true, Faction.Black)]
        BlackMulligan = 60,

        AssigningInitialSkills = 64,
        SelectingTraitors = 65,
        PerformCustomSetup = 66,

        [PhaseProperty(true, Faction.Yellow)]
        YellowSettingUp = 70,

        [PhaseProperty(true, Faction.Blue)]
        BlueSettingUp = 90,

        MetheorAndStormSpell = 95,

        [PhaseProperty(true, Faction.Grey)]
        HmsPlacement = 100,
        [PhaseProperty(true, Faction.Grey)]
        HmsMovement = 101,

        DiallingStorm = 105,
        StormLosses = 110,
        StormReport = 113,

        Thumper = 115,
        BlowA = 120,
        HarvesterA = 125,
        BlowB = 130,
        HarvesterB = 135,

        [PhaseProperty(true, Faction.Yellow)]
        YellowSendingMonsterA = 140,

        AllianceA = 150,
        YellowRidingMonsterA = 160,
        YellowSendingMonsterB = 170,
        AllianceB = 180,

        [PhaseProperty(true, Faction.Yellow)]
        YellowRidingMonsterB = 190,

        BlowReport = 194,
        ClaimingCharity = 195,

        [PhaseProperty(true, Faction.Grey)]
        GreySelectingCard = 196,
        [PhaseProperty(true, Faction.Grey)]
        GreyRemovingCardFromBid = 197,
        [PhaseProperty(true, Faction.Grey)]
        GreySwappingCard = 198,

        Bidding = 200,
        ReplacingCardJustWon = 201,
        WaitingForNextBiddingRound = 205,
        BiddingReport = 208,
        Resurrection = 210,

        BeginningOfShipAndMove = 219,

        NonOrangeShip = 220,

        [PhaseProperty(true, Faction.Orange)]
        OrangeShip = 230,

        [PhaseProperty(true, Faction.Blue)]
        BlueAccompaniesNonOrange = 240,
        [PhaseProperty(true, Faction.Blue)]
        BlueAccompaniesOrange = 250,
        [PhaseProperty(true, Faction.Blue)]
        BlueIntrudedByNonOrangeShip = 255,
        [PhaseProperty(true, Faction.Blue)]
        BlueIntrudedByOrangeShip = 256,

        NonOrangeMove = 260,

        [PhaseProperty(true, Faction.Orange)]
        OrangeMove = 270,

        [PhaseProperty(true, Faction.Blue)]
        BlueIntrudedByNonOrangeMove = 280,
        [PhaseProperty(true, Faction.Blue)]
        BlueIntrudedByOrangeMove = 290,
        [PhaseProperty(true, Faction.Blue)]
        BlueIntrudedByCaravan = 295,
        [PhaseProperty(true, Faction.Blue)]
        BlueIntrudedByYellowRidingMonsterA = 296,
        [PhaseProperty(true, Faction.Blue)]
        BlueIntrudedByYellowRidingMonsterB = 297,

        ShipmentAndMoveConcluded = 299,

        BeginningOfBattle = 301,
        BattlePhase = 300,
        CallTraitorOrPass = 310,
        AvoidingAudit = 312,

        [PhaseProperty(true, Faction.Brown)]
        Auditing = 313,

        [PhaseProperty(true, Faction.None)]
        MeltingRock = 314,

        BattleConclusion = 315,

        [PhaseProperty(true, Faction.Purple)]
        Facedancing = 320,

        BattleReport = 330,

        CollectionReport = 340,

        [PhaseProperty(true, Faction.Black)]
        PerformingKarmaHandSwap = 350,

        [PhaseProperty(true, Faction.Brown)]
        TradingCards = 351,

        Clairvoyance = 360,

        [PhaseProperty(true, Faction.None)]
        SearchingDiscarded = 370,

        [PhaseProperty(true, Faction.Purple)]
        ReplacingFaceDancer = 395,

        Contemplate = 398,
        TurnConcluded = 399,
        GameEnded = 400,

        [PhaseProperty(true, Faction.White)]
        BlackMarketAnnouncement = 500,

        BlackMarketBidding = 501,

        [PhaseProperty(true, Faction.White)]
        WhiteAnnouncingAuction = 505,

        [PhaseProperty(true, Faction.White)]
        WhiteSpecifyingAuction = 506,

        [PhaseProperty(true, Faction.White)]
        WhiteKeepingUnsoldCard = 507,

        [PhaseProperty(true, Faction.None)]
        Bureaucracy = 508,
        AssigningSkill = 509,
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class PhasePropertyAttribute : Attribute
    {
        public bool Urgent;
        public Faction FactionSpecific;

        public PhasePropertyAttribute(bool urgent, Faction factionSpecific)
        {
            Urgent = urgent;
            FactionSpecific = factionSpecific;
        }
    }

    public enum Milestone : int
    {
        None = 0,
        GameStarted = 50,
        Monster = 100,
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
        SpecialUselessPlayed = 3100
    }


    public enum FactionAdvantage : int
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
        BlueNoFlipOnIntrusion = 72,
        BlueCharity = 73,

        YellowControlsMonster = 80,
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
        WhiteBlackMarket = 152
    }

    public enum FactionForce : int
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

    public enum FactionSpecialForce : int
    {
        None = 0,
        Yellow = 10,
        Red = 20,
        Blue = 30,
        Grey = 40,
        White = 50
    }

    public enum Ruleset : int
    {
        None = 0,
        BasicGame = 10,
        AdvancedGameWithoutPayingForBattles = 20,
        AdvancedGame = 30,

        ExpansionBasicGame = 110,
        ExpansionAdvancedGameWithoutPayingForBattles = 120,
        ExpansionAdvancedGame = 130,

        ServerClassic = 140,

        Expansion2BasicGame = 150,
        Expansion2AdvancedGameWithoutPayingForBattles = 160,
        Expansion2AdvancedGame = 170,

        AllExpansionsBasicGame = 180,
        AllExpansionsAdvancedGameWithoutPayingForBattles = 190,
        AllExpansionsAdvancedGame = 200,

        Custom = 1000
    }

    public enum Rule : int
    {
        None = 0,

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

        BlueFirstForceInAnyTerritory = 55,
        BlueAutoCharity = 60,
        BlueWorthlessAsKarma = 65,
        BlueAdvisors = 70,
        BlueAccompaniesToShipmentLocation = 75,

        OrangeDetermineShipment = 80,

        RedSpecialForces = 85,

        //Exceptions
        BribesAreImmediate = 90,
        ContestedStongholdsCountAsOccupied = 95,
        AdvisorsDontConflictWithAlly = 96,
        OrangeShipmentContributionsFlowBack = 97,

        //Expansion
        GreyAndPurpleExpansionTechTokens = 200,
        GreyAndPurpleExpansionTreacheryCardsExceptPBandSSandAmal = 210,
        GreyAndPurpleExpansionTreacheryCardsPBandSS = 211,
        GreyAndPurpleExpansionTreacheryCardsAmal = 212,
        GreyAndPurpleExpansionCheapHeroTraitor = 220,
        GreyAndPurpleExpansionSandTrout = 230,

        //Expansion, Advanced Game
        GreyAndPurpleExpansionGreySwappingCardOnBid = 300,
        GreyAndPurpleExpansionPurpleGholas = 301,

        //Expansion 2
        BrownAndWhiteLeaderSkills = 400,
        BrownAndWhiteStrongholdBonus = 401,
        WhiteTreacheryCards = 402,

        //Expansion 2, Advanced Game
        BrownAuditor = 500,
        WhiteBlackMarket = 501, 

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

        //House Rules
        CustomInitialForcesAndResources = 100,
        HMSwithoutGrey = 104,
        SSW = 105,
        BlackMulligan = 106,
        FullPhaseKarma = 107,
        YellowMayMoveIntoStorm = 108,
        BlueVoiceMustNameSpecialCards = 109,
        BattlesUnderStorm = 110,
        MovementBonusRequiresOccupationBeforeMovement = 111,
        AssistedNotekeeping = 112,

        ExtraKaramaCards = 999,

        CardsCanBeTraded = 1010,
        PlayersChooseFactions = 1011,
        RedSupportingNonAllyBids = 1012
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
        House = 1000,
        Bots = 2000
    }

    public enum TreacheryCardType : int
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
        Rockmelter = 260
    }

    public enum Concept : int
    {
        None = 0,
        Messiah = 10,
        Resource = 20,
        Monster = 30,
        Graveyard = 40,
        BabyMonster = 41
    }

    public enum WinMethod : int
    {
        None = 0,
        Strongholds = 10,
        Prediction = 20,
        YellowSpecial = 30,
        OrangeSpecial = 40,
        Forfeit = 50,
        Timeout = 60
    }

    public enum HeroType : int
    {
        None = 0,
        Normal = 10,
        Messiah = 20,
        Mercenary = 30,
        Auditor = 40,
        InitiallyUnclaimed = 50
    }

    public enum AuctionType : int
    {
        None = 0,
        Normal = 10,
        BlackMarketNormal = 20,
        BlackMarketOnceAround = 30,
        BlackMarketSilent = 40,
        WhiteNormal = 50,
        WhiteOnceAround = 60,
        WhiteSilent = 70
    }

    public enum JuiceType : int
    {
        None = 0,
        GoFirst = 10,
        GoLast = 20,
        Aggressor = 30
    }

    public enum LeaderSkill : int
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

    public enum StrongholdAdvantage : int
    {
        None = 0,
        FreeResourcesForBattles = 10,
        CollectResourcesForUseless = 20,
        CountDefensesAsSnooper = 30,
        WinTies = 40,
        CollectResourcesForDial = 50
    }
}
