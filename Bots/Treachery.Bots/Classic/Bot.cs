/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Bots;

public partial class ClassicBot(Game game, Player player, BotParameters param) : IBot
{
    private Game Game { get; } = game;

    private Player Player { get; } = player;
    
    private BotParameters Param { get; } = param;

    private Faction Faction => Player.Faction;

    private Faction Ally => Player.Ally;

    private Player? AlliedPlayer => Player.AlliedPlayer;

    private int Resources => Player.Resources;
    
    #region PublicInterface

    public GameEvent? DetermineHighestPriorityInPhaseAction(List<Type> events)
    {
        GameEvent? action = null;

        if (Do(DetermineNexusPlayed, ref action, events) ||
            Do(DetermineDealCancelled, ref action, events) ||
            Do(DetermineAcceptOrCancelPurpleRevival, ref action, events) ||
            Do(DetermineThoughtAnswered, ref action, events) ||
            Do(DetermineThought, ref action, events)) return action;

        return null;
    }

    public GameEvent? DetermineHighPriorityInPhaseAction(List<Type> events)
    {
        GameEvent? action = null;

        if (Do(DetermineVoice, ref action, events)) return action;

        return null;
    }

    public GameEvent? DetermineMiddlePriorityInPhaseAction(List<Type> events)
    {
        GameEvent? action = null;

        if (Do(DetermineMetheorPlayed, ref action, events) ||
            Do(DetermineAmalPlayed, ref action, events) ||
            Do(DetermineRecruitsPlayed, ref action, events) ||
            Do(DetermineSetIncreasedRevivalLimits, ref action, events) ||
            Do(DetermineSetShipmentPermission, ref action, events) ||
            Do(DetermineDealAccepted, ref action, events) ||
            Do(DetermineRequestPurpleRevival, ref action, events) ||
            Do(DetermineDistransUsed, ref action, events) ||
            Do(DetermineJuicePlayed, ref action, events) ||
            Do(DeterminePortableAntidoteUsed, ref action, events) ||
            Do(DetermineDiplomacy, ref action, events) ||
            Do(DetermineSwitchedSkilledLeader, ref action, events) ||
            Do(DetermineRetreat, ref action, events) ||
            Do(DetermineHmsAdvantageChosen, ref action, events) ||
            Do(DeterminePlanetology, ref action, events) ||
            Do(DeterminePrescience, ref action, events) ||
            Do(DetermineCardGiven, ref action, events) ||
            Do(DetermineKarmaShipmentPrevention, ref action, events)) return action;

        return null;
    }

    public GameEvent? DetermineLowPriorityInPhaseAction(List<Type> events)
    {
        GameEvent? action = null;

        if (Do(DetermineDealOffered, ref action, events) ||
            Do(DetermineFactionTradeOffered, ref action, events) ||
            Do(DetermineSkillAssigned, ref action, events) ||
            Do(DetermineStormDialled, ref action, events) ||
            Do(DetermineClairVoyanceAnswered, ref action, events) ||
            Do(DetermineTraitorsSelected, ref action, events) ||
            Do(DetermineStormSpellPlayed, ref action, events) ||
            Do(DetermineTestingStationUsed, ref action, events) ||
            Do(DetermineThumperPlayed, ref action, events) ||
            Do(DetermineHarvesterPlayed, ref action, events) ||
            Do(DetermineAllianceBroken, ref action, events) ||
            Do(DetermineAllianceOffered, ref action, events) ||
            Do(DetermineAlliancePermissions, ref action, events) ||
            Do(DetermineCharityClaimed, ref action, events) ||
            Do(DetermineBlackMarketBid, ref action, events) ||
            Do(DetermineBid, ref action, events) ||
            Do(DetermineRevival, ref action, events) ||
            Do(DetermineDelay, ref action, events) ||
            Do(DetermineRaiseDeadPlayed, ref action, events) ||
            Do(DetermineShipment, ref action, events) ||
            Do(DetermineCaravan, ref action, events) ||
            Do(DetermineMove, ref action, events) ||
            Do(DetermineBattleInitiated, ref action, events) ||
            Do(DetermineClairvoyance, ref action, events) ||
            Do(DetermineBattle, ref action, events) ||
            Do(DetermineTreacheryCalled, ref action, events) ||
            Do(DetermineBattleConcluded, ref action, events) ||
            Do(DetermineMulliganPerformed, ref action, events) ||
            Do(DetermineReplacedCardWon, ref action, events) ||
            Do(DetermineAudited, ref action, events) ||
            Do(DetermineAuditCancelled, ref action, events) ||
            Do(DetermineCardTraded, ref action, events) ||
            Do(DetermineRockWasMelted, ref action, events) ||
            Do(DetermineResidualPlayed, ref action, events) ||
            Do(DetermineFlightUsed, ref action, events) ||
            Do(DetermineFlightDiscoveryUsed, ref action, events) ||
            Do(DetermineDiscardedSearchedAnnounced, ref action, events) ||
            Do(DetermineDiscardedSearched, ref action, events) ||
            Do(DetermineDiscardedTaken, ref action, events) ||
            Do(DetermineBureaucracy, ref action, events) ||
            Do(DetermineNexusCardDrawn, ref action, events) ||
            Do(DetermineExtortionPrevented, ref action, events) ||
            Do(DetermineDiscarded, ref action, events) ||
            Do(DetermineTraitorDiscarded, ref action, events) ||
            Do(DetermineAllianceByTerror, ref action, events) ||
            Do(DetermineAllianceByAmbassador, ref action, events) ||
            Do(DetermineLoserConcluded, ref action, events) ||
            Do(DetermineDivideResources, ref action, events) ||
            Do(DetermineDivideResourcesAccepted, ref action, events) ||
            Do(DetermineBattleClaimed, ref action, events) ||
            Do(DetermineNexusVoted, ref action, events) ||
            Do(DetermineDiscoveryRevealed, ref action, events) ||
            Do(DetermineDiscoveryEntered, ref action, events) ||
            Do(DetermineResourcesTransferred, ref action, events) ||

            //Brown
            Do(DetermineBrownEconomics, ref action, events) ||
            Do(DetermineBrownRemoveForce, ref action, events) ||
            Do(DetermineResourcesAudited, ref action, events) ||

            //Black
            Do(DetermineCaptured, ref action, events) ||
            Do(DetermineKarmaHandSwapInitiated, ref action, events) ||
            Do(DetermineKarmaHandSwap, ref action, events) ||

            //Blue
            Do(DetermineBlueAccompanies, ref action, events) ||
            Do(DetermineBluePrediction, ref action, events) ||
            Do(DetermineBlueFlip, ref action, events) ||
            Do(DetermineBlueBattleAnnouncement, ref action, events) ||
            Do(DetermineBluePlacement, ref action, events) ||

            //Yellow
            Do(DetermineYellowRidesMonster, ref action, events) ||
            Do(DetermineYellowSentMonster, ref action, events) ||
            Do(DeterminePerformYellowSetup, ref action, events) ||
            Do(DetermineTakeLosses, ref action, events) ||

            //Grey
            Do(DetermineGreyRemovedCardFromAuction, ref action, events) ||
            Do(DetermineGreySelectedStartingCard, ref action, events) ||
            Do(DetermineGreySwappedCardOnBid, ref action, events) ||
            Do(DeterminePerformHmsPlacement, ref action, events) ||
            Do(DeterminePerformHmsMovement, ref action, events) ||

            //Red
            Do(DetermineKarmaFreeRevival, ref action, events) ||
            Do(DetermineRedDiscarded, ref action, events) ||

            //Cyan
            Do(DetermineTerrorPlanted, ref action, events) ||
            Do(DetermineTerrorRevealed, ref action, events) ||
            Do(DeterminePerformCyanSetup, ref action, events) ||

            //Pink
            Do(DetermineAmbassadorPlaced, ref action, events) ||
            Do(DetermineAmbassadorActivated, ref action, events) ||

            //Purple
            Do(DetermineFaceDancerRevealed, ref action, events) ||
            Do(DetermineFaceDanced, ref action, events) ||
            Do(DetermineFaceDancerReplaced, ref action, events) ||

            //White
            Do(DetermineWhiteAnnouncesBlackMarket, ref action, events) ||
            Do(DetermineWhiteAnnouncesAuction, ref action, events) ||
            Do(DetermineWhiteSpecifiesAuction, ref action, events) ||
            Do(DetermineWhiteKeepsUnsoldCard, ref action, events) ||
            Do(DetermineWhiteRevealedNoField, ref action, events)) return action;

        return null;
    }


    public GameEvent? DetermineEndPhaseAction(List<Type> events)
    {
        GameEvent? action = null;

        if (Do(DetermineEndPhase, ref action, events)) return action;


        return null;
    }

    private EndPhase DetermineEndPhase()
    {
        return new EndPhase(Game, Faction);
    }

    private bool Do<T>(Func<T> method, ref GameEvent? action, IEnumerable<Type> allowedActions) where T : GameEvent?
    {
        if (typeof(T) == typeof(GameEvent)) throw new ArgumentException("Illegally typed method: " + method);

        if (action == null && allowedActions.Contains(typeof(T)))
        {
            try
            {
                action = method();
            }
            catch (Exception e)
            {
                LogError("--error occured -->" + e);
            }

            if (action != null)
            {
                var error = action.Validate();
                if (error != null)
                {
                    LogError("--invalid decision ({0})--> {1}: {2}", Resources, action.GetMessage(), error);
                }
                else
                {
                    LogInfo("--valid decision ({0})--> {1}", Resources, action.GetMessage());
                    return true;
                }
            }
        }

        return false;
    }

    #endregion PublicInterface

    #region SupportMethods

    private static void LogInfo(string msg, params object?[] pars)
    {
        #if (DEBUG)
        {
            if (Message.DefaultDescriber != null)
                Console.WriteLine(Message.DefaultDescriber.Format(msg, pars));
            else
                Console.WriteLine(msg, pars);
        }
        #endif
    }

    private static void LogError(Message? message)
    {
        if (message == null) return;
    
        if (Message.DefaultDescriber != null)
            Console.WriteLine(message.ToString(Message.DefaultDescriber));
        else
            Console.WriteLine(message);
    }
    
    private static void LogError(string msg, params object?[] pars)
    {
        {
            if (Message.DefaultDescriber != null)
                Console.WriteLine(Message.DefaultDescriber.Format(msg, pars));
            else
                Console.WriteLine(msg, pars);
        }
    }

    private static void LogInfo(Message? message)
    {
#if (DEBUG) 
        {
            if (message == null) return;
        
            if (Message.DefaultDescriber != null)
                Console.WriteLine(message.ToString(Message.DefaultDescriber));
            else
                Console.WriteLine(message);
        }
#endif
    }

    private readonly LoggedRandom _random = new();

    private int D(int amount, int sides)
    {
        if (amount == 0 || sides == 0) return 0;

        var result = 0;
        for (var i = 0; i < amount; i++) result += _random.Next(sides) + 1;
        return result;
    }

    #endregion SupportMethods
}