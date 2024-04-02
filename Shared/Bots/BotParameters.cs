/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared.Model;

public class BotParameters
{
    public const float PenaltyForAttackingBots = 6.0f;

    public int Bidding_ResourcesToKeepWhenCardIsPerfect { get; set; }
    public int Bidding_ResourcesToKeepWhenCardIsntPerfect { get; set; }
    public int Bidding_PassingTreshold { get; set; }

    public bool Karma_SaveCardToUseSpecialKarmaAbility { get; set; }

    public int Shipment_MinimumOtherPlayersITrustToPreventAWin { get; set; }
    public float Shipment_DialShortageToAccept { get; set; }
    public int Shipment_MinimumResourcesToKeepForBattle { get; set; }
    public float Shipment_MaxEnemyForceStrengthFightingForSpice { get; set; }
    public int Shipment_ExpectedStormMovesWhenUnknown { get; set; }
    public int Shipment_DialForExtraForcesToShip { get; set; }

    public int Battle_MaximumUnsupportedForces { get; set; }
    public float Battle_MimimumChanceToAssumeEnemyHeroSurvives { get; set; }
    public float Battle_MimimumChanceToAssumeMyLeaderSurvives { get; set; }
    public float Battle_DialShortageThresholdForThrowing { get; set; }

    public static readonly BotParameters BlackParameters = new()
    {
        Bidding_ResourcesToKeepWhenCardIsPerfect = 1,
        Bidding_ResourcesToKeepWhenCardIsntPerfect = 4,
        Bidding_PassingTreshold = 0,
        Karma_SaveCardToUseSpecialKarmaAbility = true,
        Shipment_MinimumOtherPlayersITrustToPreventAWin = 4,
        Shipment_DialShortageToAccept = 4,
        Shipment_MinimumResourcesToKeepForBattle = 1,
        Shipment_DialForExtraForcesToShip = 1,
        Shipment_MaxEnemyForceStrengthFightingForSpice = 3,
        Shipment_ExpectedStormMovesWhenUnknown = 3,
        Battle_MaximumUnsupportedForces = 3,
        Battle_MimimumChanceToAssumeEnemyHeroSurvives = 0.6f,
        Battle_MimimumChanceToAssumeMyLeaderSurvives = 0.4f,
        Battle_DialShortageThresholdForThrowing = 3
    };

    public static readonly BotParameters GreenParameters = new()
    {
        Bidding_ResourcesToKeepWhenCardIsPerfect = 1,
        Bidding_ResourcesToKeepWhenCardIsntPerfect = 4,
        Bidding_PassingTreshold = 2,
        Karma_SaveCardToUseSpecialKarmaAbility = false,
        Shipment_MinimumOtherPlayersITrustToPreventAWin = 3,
        Shipment_DialShortageToAccept = 2,
        Shipment_MinimumResourcesToKeepForBattle = 1,
        Shipment_DialForExtraForcesToShip = 1,
        Shipment_MaxEnemyForceStrengthFightingForSpice = 2,
        Shipment_ExpectedStormMovesWhenUnknown = 3,
        Battle_MaximumUnsupportedForces = 3,
        Battle_MimimumChanceToAssumeEnemyHeroSurvives = 0.5f,
        Battle_MimimumChanceToAssumeMyLeaderSurvives = 0.5f,
        Battle_DialShortageThresholdForThrowing = 3
    };

    public static readonly BotParameters YellowParameters = new()
    {
        Bidding_ResourcesToKeepWhenCardIsPerfect = 0,
        Bidding_ResourcesToKeepWhenCardIsntPerfect = 0,
        Bidding_PassingTreshold = 4,
        Karma_SaveCardToUseSpecialKarmaAbility = false,
        Shipment_MinimumOtherPlayersITrustToPreventAWin = 2,
        Shipment_DialShortageToAccept = 4,
        Shipment_MinimumResourcesToKeepForBattle = 0,
        Shipment_DialForExtraForcesToShip = 6,
        Shipment_MaxEnemyForceStrengthFightingForSpice = 8,
        Shipment_ExpectedStormMovesWhenUnknown = 0,
        Battle_MaximumUnsupportedForces = 20,
        Battle_MimimumChanceToAssumeEnemyHeroSurvives = 0.4f,
        Battle_MimimumChanceToAssumeMyLeaderSurvives = 0.5f,
        Battle_DialShortageThresholdForThrowing = 4
    };

    public static readonly BotParameters RedParameters = new()
    {
        Bidding_ResourcesToKeepWhenCardIsPerfect = 1,
        Bidding_ResourcesToKeepWhenCardIsntPerfect = 8,
        Bidding_PassingTreshold = 3,
        Karma_SaveCardToUseSpecialKarmaAbility = true,
        Shipment_MinimumOtherPlayersITrustToPreventAWin = 3,
        Shipment_DialShortageToAccept = 2,
        Shipment_MinimumResourcesToKeepForBattle = 2,
        Shipment_DialForExtraForcesToShip = 4,
        Shipment_MaxEnemyForceStrengthFightingForSpice = 0,
        Shipment_ExpectedStormMovesWhenUnknown = 6,
        Battle_MaximumUnsupportedForces = 1,
        Battle_MimimumChanceToAssumeEnemyHeroSurvives = 0.4f,
        Battle_MimimumChanceToAssumeMyLeaderSurvives = 0.6f,
        Battle_DialShortageThresholdForThrowing = 3
    };

    public static readonly BotParameters OrangeParameters = new()
    {
        Bidding_ResourcesToKeepWhenCardIsPerfect = 1,
        Bidding_ResourcesToKeepWhenCardIsntPerfect = 8,
        Bidding_PassingTreshold = 0,
        Karma_SaveCardToUseSpecialKarmaAbility = true,
        Shipment_MinimumOtherPlayersITrustToPreventAWin = 2,
        Shipment_DialShortageToAccept = 0,
        Shipment_MinimumResourcesToKeepForBattle = 4,
        Shipment_DialForExtraForcesToShip = 2,
        Shipment_MaxEnemyForceStrengthFightingForSpice = 0,
        Shipment_ExpectedStormMovesWhenUnknown = 6,
        Battle_MaximumUnsupportedForces = 0,
        Battle_MimimumChanceToAssumeEnemyHeroSurvives = 0.4f,
        Battle_MimimumChanceToAssumeMyLeaderSurvives = 0.6f,
        Battle_DialShortageThresholdForThrowing = 6
    };

    public static readonly BotParameters BlueParameters = new()
    {
        Bidding_ResourcesToKeepWhenCardIsPerfect = 1,
        Bidding_ResourcesToKeepWhenCardIsntPerfect = 7,
        Bidding_PassingTreshold = 0,
        Karma_SaveCardToUseSpecialKarmaAbility = false,
        Shipment_MinimumOtherPlayersITrustToPreventAWin = 2,
        Shipment_DialShortageToAccept = 4,
        Shipment_MinimumResourcesToKeepForBattle = 2,
        Shipment_DialForExtraForcesToShip = 2,
        Shipment_MaxEnemyForceStrengthFightingForSpice = 4,
        Shipment_ExpectedStormMovesWhenUnknown = 6,
        Battle_MaximumUnsupportedForces = 1,
        Battle_MimimumChanceToAssumeEnemyHeroSurvives = 0.4f,
        Battle_MimimumChanceToAssumeMyLeaderSurvives = 0.55f,
        Battle_DialShortageThresholdForThrowing = 6
    };

    public static readonly BotParameters GreyParameters = new()
    {
        Bidding_ResourcesToKeepWhenCardIsPerfect = 1,
        Bidding_ResourcesToKeepWhenCardIsntPerfect = 5,
        Bidding_PassingTreshold = 4,
        Karma_SaveCardToUseSpecialKarmaAbility = false,
        Shipment_MinimumOtherPlayersITrustToPreventAWin = 3,
        Shipment_DialShortageToAccept = 4,
        Shipment_MinimumResourcesToKeepForBattle = 0,
        Shipment_DialForExtraForcesToShip = 1,
        Shipment_MaxEnemyForceStrengthFightingForSpice = 4,
        Shipment_ExpectedStormMovesWhenUnknown = 5,
        Battle_MaximumUnsupportedForces = 2,
        Battle_MimimumChanceToAssumeEnemyHeroSurvives = 0.6f,
        Battle_MimimumChanceToAssumeMyLeaderSurvives = 0.4f,
        Battle_DialShortageThresholdForThrowing = 4
    };

    public static readonly BotParameters PurpleParameters = new()
    {
        Bidding_ResourcesToKeepWhenCardIsPerfect = 1,
        Bidding_ResourcesToKeepWhenCardIsntPerfect = 5,
        Bidding_PassingTreshold = 3,
        Karma_SaveCardToUseSpecialKarmaAbility = false,
        Shipment_MinimumOtherPlayersITrustToPreventAWin = 2,
        Shipment_DialShortageToAccept = 6,
        Shipment_MinimumResourcesToKeepForBattle = 0,
        Shipment_DialForExtraForcesToShip = 4,
        Shipment_MaxEnemyForceStrengthFightingForSpice = 6,
        Shipment_ExpectedStormMovesWhenUnknown = 0,
        Battle_MaximumUnsupportedForces = 4,
        Battle_MimimumChanceToAssumeEnemyHeroSurvives = 0.3f,
        Battle_MimimumChanceToAssumeMyLeaderSurvives = 0.6f,
        Battle_DialShortageThresholdForThrowing = 6
    };

    private static readonly BotParameters BrownParameters = new()
    {
        Bidding_ResourcesToKeepWhenCardIsPerfect = 1,
        Bidding_ResourcesToKeepWhenCardIsntPerfect = 8,
        Bidding_PassingTreshold = 3,
        Karma_SaveCardToUseSpecialKarmaAbility = true,
        Shipment_MinimumOtherPlayersITrustToPreventAWin = 2,
        Shipment_DialShortageToAccept = 4,
        Shipment_MinimumResourcesToKeepForBattle = 0,
        Shipment_DialForExtraForcesToShip = 2,
        Shipment_MaxEnemyForceStrengthFightingForSpice = 0,
        Shipment_ExpectedStormMovesWhenUnknown = 4,
        Battle_MaximumUnsupportedForces = 1,
        Battle_MimimumChanceToAssumeEnemyHeroSurvives = 0.4f,
        Battle_MimimumChanceToAssumeMyLeaderSurvives = 0.55f,
        Battle_DialShortageThresholdForThrowing = 3
    };

    private static readonly BotParameters WhiteParameters = new()
    {
        Bidding_ResourcesToKeepWhenCardIsPerfect = 1,
        Bidding_ResourcesToKeepWhenCardIsntPerfect = 5,
        Bidding_PassingTreshold = 4,
        Karma_SaveCardToUseSpecialKarmaAbility = false,
        Shipment_MinimumOtherPlayersITrustToPreventAWin = 4,
        Shipment_DialShortageToAccept = 4,
        Shipment_MinimumResourcesToKeepForBattle = 0,
        Shipment_DialForExtraForcesToShip = 1,
        Shipment_MaxEnemyForceStrengthFightingForSpice = 4,
        Shipment_ExpectedStormMovesWhenUnknown = 4,
        Battle_MaximumUnsupportedForces = 1,
        Battle_MimimumChanceToAssumeEnemyHeroSurvives = 0.4f,
        Battle_MimimumChanceToAssumeMyLeaderSurvives = 0.5f,
        Battle_DialShortageThresholdForThrowing = 6
    };

    public static readonly BotParameters PinkParameters = new()
    {
        Bidding_ResourcesToKeepWhenCardIsPerfect = 1,
        Bidding_ResourcesToKeepWhenCardIsntPerfect = 5,
        Bidding_PassingTreshold = 4,
        Karma_SaveCardToUseSpecialKarmaAbility = false,
        Shipment_MinimumOtherPlayersITrustToPreventAWin = 2,
        Shipment_DialShortageToAccept = 2,
        Shipment_MinimumResourcesToKeepForBattle = 0,
        Shipment_DialForExtraForcesToShip = 2,
        Shipment_MaxEnemyForceStrengthFightingForSpice = 0,
        Shipment_ExpectedStormMovesWhenUnknown = 4,
        Battle_MaximumUnsupportedForces = 2,
        Battle_MimimumChanceToAssumeEnemyHeroSurvives = 0.5f,
        Battle_MimimumChanceToAssumeMyLeaderSurvives = 0.5f,
        Battle_DialShortageThresholdForThrowing = 3
    };

    public static readonly BotParameters CyanParameters = new()
    {
        Bidding_ResourcesToKeepWhenCardIsPerfect = 1,
        Bidding_ResourcesToKeepWhenCardIsntPerfect = 5,
        Bidding_PassingTreshold = 4,
        Karma_SaveCardToUseSpecialKarmaAbility = false,
        Shipment_MinimumOtherPlayersITrustToPreventAWin = 4,
        Shipment_DialShortageToAccept = 4,
        Shipment_MinimumResourcesToKeepForBattle = 0,
        Shipment_DialForExtraForcesToShip = 2,
        Shipment_MaxEnemyForceStrengthFightingForSpice = 5,
        Shipment_ExpectedStormMovesWhenUnknown = 4,
        Battle_MaximumUnsupportedForces = 2,
        Battle_MimimumChanceToAssumeEnemyHeroSurvives = 0.4f,
        Battle_MimimumChanceToAssumeMyLeaderSurvives = 0.5f,
        Battle_DialShortageThresholdForThrowing = 4
    };

    public static BotParameters GetDefaultParameters(Faction f)
    {
        var result = f switch
        {
            Faction.Black => BlackParameters.MemberwiseClone(),
            Faction.Blue => BlueParameters.MemberwiseClone(),
            Faction.Green => GreenParameters.MemberwiseClone(),
            Faction.Yellow => YellowParameters.MemberwiseClone(),
            Faction.Red => RedParameters.MemberwiseClone(),
            Faction.Orange => OrangeParameters.MemberwiseClone(),
            Faction.Grey => GreyParameters.MemberwiseClone(),
            Faction.Purple => PurpleParameters.MemberwiseClone(),
            Faction.Brown => BrownParameters.MemberwiseClone(),
            Faction.White => WhiteParameters.MemberwiseClone(),
            Faction.Pink => PinkParameters.MemberwiseClone(),
            Faction.Cyan => CyanParameters.MemberwiseClone(),
            _ => BlackParameters.MemberwiseClone()
        };

        return (BotParameters)result;
    }
}