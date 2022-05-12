/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Player
    {
        #region PublicInterface

        public BotParameters Param { get; set; }

        public bool IsBot { get; set; }

        public bool AllyIsBot => Ally != Faction.None && AlliedPlayer.IsBot;

        public GameEvent DetermineHighPrioInPhaseAction(IEnumerable<Type> evts)
        {
            GameEvent action = null;

            if (Do(DetermineVoice, ref action, evts) ||
                Do(DetermineDealCancelled, ref action, evts) ||
                Do(DetermineAcceptOrCancelPurpleRevival, ref action, evts) ||
                Do(DetermineThoughtAnswered, ref action, evts) ||
                Do(DetermineThought, ref action, evts)) return action;

            return null;
        }

        public GameEvent DetermineMiddlePrioInPhaseAction(IEnumerable<Type> evts)
        {
            GameEvent action = null;

            if (Do(DetermineMetheorPlayed, ref action, evts) || 
                Do(DetermineAmalPlayed, ref action, evts) || 
                Do(DetermineSetIncreasedRevivalLimits, ref action, evts) || 
                Do(DetermineDealAccepted, ref action, evts) || 
                Do(DetermineRequestPurpleRevival, ref action, evts) ||
                Do(DetermineDistransUsed, ref action, evts) ||
                Do(DetermineJuicePlayed, ref action, evts) ||
                Do(DeterminePortableAntidoteUsed, ref action, evts) ||
                Do(DetermineDiplomacy, ref action, evts) ||
                Do(DetermineSwitchedSkilledLeader, ref action, evts) ||
                Do(DetermineRetreat, ref action, evts) ||
                Do(DetermineHMSAdvantageChosen, ref action, evts) ||
                Do(DeterminePlanetology, ref action, evts) ||
                Do(DeterminePrescience, ref action, evts) ||
                Do(DetermineKarmaShipmentPrevention, ref action, evts)) return action;

            return null;
        }

        public GameEvent DetermineLowPrioInPhaseAction(IEnumerable<Type> evts)
        {
            GameEvent action = null;

            if (Do(DetermineDealOffered, ref action, evts) ||
                Do(DetermineFactionTradeOffered, ref action, evts) ||
                Do(DetermineSkillAssigned, ref action, evts) ||
                Do(DetermineStormDialled, ref action, evts) ||
                Do(DetermineClairVoyanceAnswered, ref action, evts) ||
                Do(DetermineTraitorsSelected, ref action, evts) ||
                Do(DetermineStormSpellPlayed, ref action, evts) ||
                Do(DetermineThumperPlayed, ref action, evts) ||
                Do(DetermineHarvesterPlayed, ref action, evts) ||
                Do(DetermineAllianceBroken, ref action, evts) ||
                Do(DetermineAllianceOffered, ref action, evts) ||
                Do(DetermineAlliancePermissions, ref action, evts) ||
                Do(DetermineCharityClaimed, ref action, evts) ||
                Do(DetermineBlackMarketBid, ref action, evts) ||
                Do(DetermineBid, ref action, evts) ||
                Do(DetermineRevival, ref action, evts) ||
                Do(DetermineDelay, ref action, evts) ||
                Do(DetermineRaiseDeadPlayed, ref action, evts) ||
                Do(DetermineShipment, ref action, evts) ||
                Do(DetermineCaravan, ref action, evts) ||
                Do(DetermineMove, ref action, evts) ||
                Do(DetermineBattleInitiated, ref action, evts) ||
                Do(DetermineClairvoyance, ref action, evts) ||
                Do(DetermineBattle, ref action, evts) ||
                Do(DetermineTreacheryCalled, ref action, evts) ||
                Do(DetermineBattleConcluded, ref action, evts) ||
                Do(DetermineMulliganPerformed, ref action, evts) ||
                Do(DetermineReplacedCardWon, ref action, evts) ||
                Do(DetermineAudited, ref action, evts) ||
                Do(DetermineAuditCancelled, ref action, evts) ||
                Do(DetermineCardTraded, ref action, evts) ||
                Do(DetermineRockWasMelted, ref action, evts) ||
                Do(DetermineResidualPlayed, ref action, evts) ||
                Do(DetermineFlightUsed, ref action, evts) ||
                Do(DetermineDiscardedSearchedAnnounced, ref action, evts) ||
                Do(DetermineDiscardedSearched, ref action, evts) ||
                Do(DetermineDiscardedTaken, ref action, evts) ||
                Do(DetermineBureaucracy, ref action, evts) ||

                //Brown
                Do(DetermineBrownEconomics, ref action, evts) ||

                //Black
                Do(DetermineCaptured, ref action, evts) ||
                Do(DetermineKarmaHandSwapInitiated, ref action, evts) ||
                Do(DetermineKarmaHandSwap, ref action, evts) ||

                //Blue
                Do(DetermineBlueAccompanies, ref action, evts) ||
                Do(DetermineBluePrediction, ref action, evts) ||
                Do(DetermineBlueFlip, ref action, evts) ||
                Do(DetermineBlueBattleAnnouncement, ref action, evts) ||
                Do(DetermineBluePlacement, ref action, evts) ||

                //Yellow
                Do(DetermineYellowRidesMonster, ref action, evts) ||
                Do(DetermineYellowSentMonster, ref action, evts) ||
                Do(DeterminePerformYellowSetup, ref action, evts) ||
                Do(DetermineTakeLosses, ref action, evts) ||

                //Grey
                Do(DetermineGreyRemovedCardFromAuction, ref action, evts) ||
                Do(DetermineGreySelectedStartingCard, ref action, evts) ||
                Do(DetermineGreySwappedCardOnBid, ref action, evts) ||
                Do(DeterminePerformHmsPlacement, ref action, evts) ||
                Do(DeterminePerformHmsMovement, ref action, evts) ||

                //Red
                Do(DetermineKarmaFreeRevival, ref action, evts) ||

                //Purple
                Do(DetermineFaceDanced, ref action, evts) ||
                Do(DetermineFaceDancerReplaced, ref action, evts) ||

                //White
                Do(DetermineWhiteAnnouncesBlackMarket, ref action, evts) ||
                Do(DetermineWhiteAnnouncesAuction, ref action, evts) ||
                Do(DetermineWhiteSpecifiesAuction, ref action, evts) ||
                Do(DetermineWhiteKeepsUnsoldCard, ref action, evts) ||
                Do(DetermineWhiteRevealedNoField, ref action, evts)) return action;
          
            return null;
        }


        public GameEvent DetermineEndPhaseAction(IEnumerable<Type> evts)
        {
            GameEvent action = null;

            if (Do(DetermineEndPhase, ref action, evts)) return action;


            return null;
        }

        private EndPhase DetermineEndPhase() => new EndPhase(Game) { Initiator = Faction };

        private bool Do<T>(Func<T> method, ref GameEvent action, IEnumerable<Type> allowedActions) where T: GameEvent
        {
            if (typeof(T).Equals(typeof(GameEvent)))
            {
                throw new ArgumentException("Illegally typed method: " + method.ToString());
            }

            if (action == null && allowedActions.Contains(typeof(T)))
            {
                try
                {
                    action = method();
                }
                catch (Exception e)
                {
                    LogInfo("--error occured -->" + e.ToString());
                }

                if (action != null)
                {
                    var error = action.Validate();
                    if (error != null)
                    {
                        LogInfo("--invalid decision ({0})--> {1}: {2}", Resources, action.GetMessage(), error);
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

        protected void LogInfo(string msg, params object[] pars)
        {
            if (Game.BotInfologging)
            {
                if (Message.DefaultDescriber != null)
                {
                    Console.WriteLine(Name + ": " + Message.DefaultDescriber.Format(msg, pars));
                }
                else
                {
                    Console.WriteLine(Name + ": " + string.Format(msg, pars));
                }
            }
        }

        protected void LogInfo(Message message)
        {
            if (Game.BotInfologging && message != null)
            {
                if (Message.DefaultDescriber != null)
                {
                    Console.WriteLine(Name + ": " + message.ToString(Message.DefaultDescriber));
                }
                else
                {
                    Console.WriteLine(Name + ": " + message.ToString());
                }

            }
        }

        private readonly Random random = new Random();
        protected int D(int amount, int sides)
        {
            if (amount == 0 || sides == 0) return 0;

            int result = 0;
            for (int i = 0; i < amount; i++)
            {
                result += random.Next(sides) + 1;
            }
            return result;
        }

        #endregion SupportMethods
    }
}
