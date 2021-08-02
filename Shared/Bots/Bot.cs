/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
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

        public GameEvent DetermineHighPrioInPhaseAction(IEnumerable<Type> possibleEvents)
        {
            GameEvent action = null;

            try
            {
                if (action == null && possibleEvents.Contains(typeof(MetheorPlayed))) action = DetermineMetheorPlayed();
                if (action == null && possibleEvents.Contains(typeof(AmalPlayed))) action = DetermineAmalPlayed();
                if (action == null && possibleEvents.Contains(typeof(SetIncreasedRevivalLimits))) action = DetermineSetIncreasedRevivalLimits();
                if (action == null && possibleEvents.Contains(typeof(Voice))) action = DetermineVoice();
                if (action == null && possibleEvents.Contains(typeof(DealOffered))) action = DetermineDealOffered();
                if (action == null && possibleEvents.Contains(typeof(DealAccepted))) action = DetermineDealAccepted();
                if (action == null && possibleEvents.Contains(typeof(AcceptOrCancelPurpleRevival))) action = DetermineAcceptOrCancelPurpleRevival();
                if (action == null && possibleEvents.Contains(typeof(RequestPurpleRevival))) action = DetermineRequestPurpleRevival();
                if (action == null && possibleEvents.Contains(typeof(DistransUsed))) action = DetermineDistransUsed();
                if (action == null && possibleEvents.Contains(typeof(JuicePlayed))) action = DetermineJuicePlayed();
                if (action == null && possibleEvents.Contains(typeof(PortableAntidoteUsed))) action = DeterminePortableAntidoteUsed();
                if (action == null && possibleEvents.Contains(typeof(Diplomacy))) action = DetermineDiplomacy();
                if (action == null && possibleEvents.Contains(typeof(SwitchedSkilledLeader))) action = DetermineSwitchedSkilledLeader();
                if (action == null && possibleEvents.Contains(typeof(Thought))) action = DetermineThought();
                if (action == null && possibleEvents.Contains(typeof(ThoughtAnswered))) action = DetermineThoughtAnswered();
                if (action == null && possibleEvents.Contains(typeof(Retreat))) action = DetermineRetreat();
                if (action == null && possibleEvents.Contains(typeof(HMSAdvantageChosen))) action = DetermineHMSAdvantageChosen();
                if (action == null && possibleEvents.Contains(typeof(Planetology))) action = DeterminePlanetology();
            }
            catch (Exception e)
            {
                LogInfo("--error occured -->" + e.ToString());
            }

            if (action != null)
            {
                var result = action.Validate();
                if (result != "")
                {
                    LogInfo("--invalid decision ({0},{1})--> {2}: {3}", Resources, string.Join(",", TreacheryCards), action.GetMessage(), result);
                    action = null;
                }
                else
                {
                    LogInfo("--valid decision ({0},{1})--> {2}", Resources, string.Join(",", TreacheryCards), action.GetMessage());
                }
            }

            return action;
        }

        public GameEvent DetermineLowPrioInPhaseAction(IEnumerable<Type> possibleEvents)
        {
            GameEvent action = null;

            try
            {
                //Other
                if (action == null && possibleEvents.Contains(typeof(FactionTradeOffered))) action = DetermineFactionTradeOffered();
                if (action == null && possibleEvents.Contains(typeof(SkillAssigned))) action = DetermineSkillAssigned();
                if (action == null && possibleEvents.Contains(typeof(StormDialled))) action = DetermineStormDialled();
                if (action == null && possibleEvents.Contains(typeof(ClairVoyanceAnswered))) action = DetermineClairVoyanceAnswered(); 
                if (action == null && possibleEvents.Contains(typeof(TraitorsSelected))) action = DetermineTraitorsSelected();
                if (action == null && possibleEvents.Contains(typeof(StormSpellPlayed))) action = DetermineStormSpellPlayed();
                if (action == null && possibleEvents.Contains(typeof(ThumperPlayed))) action = new ThumperPlayed(Game) { Initiator = Faction };
                if (action == null && possibleEvents.Contains(typeof(HarvesterPlayed))) action = DetermineHarvesterPlayed();
                if (action == null && possibleEvents.Contains(typeof(AllianceBroken))) action = DetermineAllianceBroken();
                if (action == null && possibleEvents.Contains(typeof(AllianceOffered))) action = DetermineAllianceOffered();
                if (action == null && possibleEvents.Contains(typeof(AllyPermission))) action = DetermineAlliancePermissions();
                if (action == null && possibleEvents.Contains(typeof(CharityClaimed))) action = new CharityClaimed(Game) { Initiator = Faction };
                if (action == null && possibleEvents.Contains(typeof(BlackMarketBid))) action = DetermineBlackMarketBid();
                if (action == null && possibleEvents.Contains(typeof(Bid))) action = DetermineBid();
                if (action == null && possibleEvents.Contains(typeof(Revival))) action = DetermineRevival();
                if (action == null && possibleEvents.Contains(typeof(OrangeDelay))) action = DetermineDelay();
                if (action == null && possibleEvents.Contains(typeof(RaiseDeadPlayed))) action = DetermineRaiseDeadPlayed();
                if (action == null && possibleEvents.Contains(typeof(Shipment))) action = DetermineShipment();
                if (action == null && possibleEvents.Contains(typeof(BlueAccompanies))) action = DetermineBlueAccompanies();
                if (action == null && possibleEvents.Contains(typeof(Caravan))) action = DetermineCaravan();
                if (action == null && possibleEvents.Contains(typeof(Move))) action = DetermineMove();
                if (action == null && possibleEvents.Contains(typeof(BattleInitiated))) action = DetermineBattleInitiated();
                if (action == null && possibleEvents.Contains(typeof(Prescience))) action = DeterminePrescience();
                if (action == null && possibleEvents.Contains(typeof(ClairVoyancePlayed))) action = DetermineClairvoyance();
                if (action == null && possibleEvents.Contains(typeof(Battle))) action = DetermineBattle(true);
                if (action == null && possibleEvents.Contains(typeof(TreacheryCalled))) action = DetermineTreacheryCalled();
                if (action == null && possibleEvents.Contains(typeof(BattleConcluded))) action = DetermineBattleConcluded();
                if (action == null && possibleEvents.Contains(typeof(FaceDanced))) action = DetermineFaceDanced();
                if (action == null && possibleEvents.Contains(typeof(FaceDancerReplaced))) action = DetermineFaceDancerReplaced();
                if (action == null && possibleEvents.Contains(typeof(MulliganPerformed))) action = DetermineMulliganPerformed();
                if (action == null && possibleEvents.Contains(typeof(ReplacedCardWon))) action = DetermineReplacedCardWon();
                if (action == null && possibleEvents.Contains(typeof(Audited))) action = DetermineAudited();
                if (action == null && possibleEvents.Contains(typeof(AuditCancelled))) action = DetermineAuditCancelled();
                if (action == null && possibleEvents.Contains(typeof(CardTraded))) action = DetermineCardTraded();
                if (action == null && possibleEvents.Contains(typeof(RockWasMelted))) action = DetermineRockWasMelted();
                if (action == null && possibleEvents.Contains(typeof(ResidualPlayed))) action = DetermineResidualPlayed();
                if (action == null && possibleEvents.Contains(typeof(FlightUsed))) action = DetermineFlightUsed();
                if (action == null && possibleEvents.Contains(typeof(DiscardedSearchedAnnounced))) action = DetermineDiscardedSearchedAnnounced();
                if (action == null && possibleEvents.Contains(typeof(DiscardedSearched))) action = DetermineDiscardedSearched();
                if (action == null && possibleEvents.Contains(typeof(DiscardedTaken))) action = DetermineDiscardedTaken();
                if (action == null && possibleEvents.Contains(typeof(Bureaucracy))) action = DetermineBureaucracy();

                //Blue
                if (action == null && possibleEvents.Contains(typeof(BluePrediction))) action = DetermineBluePrediction();
                if (action == null && possibleEvents.Contains(typeof(BlueFlip))) action = DetermineBlueFlip();
                if (action == null && possibleEvents.Contains(typeof(BlueBattleAnnouncement))) action = DetermineBlueBattleAnnouncement();
                if (action == null && possibleEvents.Contains(typeof(PerformBluePlacement))) action = DetermineBluePlacement();

                //Yellow
                if (action == null && possibleEvents.Contains(typeof(YellowRidesMonster))) action = DetermineYellowRidesMonster();
                if (action == null && possibleEvents.Contains(typeof(YellowSentMonster))) action = DetermineYellowSentMonster();
                if (action == null && possibleEvents.Contains(typeof(PerformYellowSetup))) action = DeterminePerformYellowSetup();
                if (action == null && possibleEvents.Contains(typeof(TakeLosses))) action = DetermineTakeLosses();

                //Grey
                if (action == null && possibleEvents.Contains(typeof(GreyRemovedCardFromAuction))) action = DetermineGreyRemovedCardFromAuction();
                if (action == null && possibleEvents.Contains(typeof(GreySelectedStartingCard))) action = DetermineGreySelectedStartingCard();
                if (action == null && possibleEvents.Contains(typeof(GreySwappedCardOnBid))) action = DetermineGreySwappedCardOnBid();
                if (action == null && possibleEvents.Contains(typeof(PerformHmsPlacement))) action = DeterminePerformHmsPlacement();
                if (action == null && possibleEvents.Contains(typeof(PerformHmsMovement))) action = DeterminePerformHmsMovement();

                //Black
                if (action == null && possibleEvents.Contains(typeof(KarmaHandSwapInitiated))) action = DetermineKarmaHandSwapInitiated();
                if (action == null && possibleEvents.Contains(typeof(KarmaHandSwap))) action = DetermineKarmaHandSwap();

                //Red
                if (action == null && possibleEvents.Contains(typeof(KarmaFreeRevival))) action = DetermineKarmaFreeRevival();

                //White
                if (action == null && possibleEvents.Contains(typeof(WhiteAnnouncesBlackMarket))) action = DetermineWhiteAnnouncesBlackMarket();
                if (action == null && possibleEvents.Contains(typeof(WhiteAnnouncesAuction))) action = DetermineWhiteAnnouncesAuction();
                if (action == null && possibleEvents.Contains(typeof(WhiteSpecifiesAuction))) action = DetermineWhiteSpecifiesAuction();
                if (action == null && possibleEvents.Contains(typeof(WhiteKeepsUnsoldCard))) action = DetermineWhiteKeepsUnsoldCard();

                Speak();
            }
            catch (Exception e)
            {
                LogInfo("--error occured -->" + e.ToString());
            }

            if (action != null)
            {
                var result = action.Validate();
                if (result != "")
                {
                    LogInfo("--invalid decision ({0},{1})--> {2}: {3}", Resources, string.Join(",", TreacheryCards), action.GetMessage(), result);
                    action = null;
                }
                else
                {
                    LogInfo("--valid decision ({0},{1})--> {2}", Resources, string.Join(",", TreacheryCards), action.GetMessage());
                }
            }

            return action;
        }

        public GameEvent DetermineEndPhaseAction(IEnumerable<Type> possibleEvents)
        {
            GameEvent action = null;

            try
            {
                if (possibleEvents.Contains(typeof(EndPhase))) action = new EndPhase(Game) { Initiator = Faction };
            }
            catch (Exception e)
            {
                LogInfo("--error occured -->" + e.ToString());
            }

            if (action != null)
            {
                var result = action.Validate();
                if (result != "")
                {
                    LogInfo("--invalid decision--> " + action.GetMessage() + ":" + result);
                    action = null;
                }
                else
                {
                    LogInfo("--valid decision----> " + action.GetMessage());
                }
            }

            return action;
        }

        public bool IsBot { get; set; }

        #endregion PublicInterface

        #region SupportMethods

        protected void LogInfo(string msg, params object[] pars)
        {
            if (Game.BotInfologging)
            {
                Console.WriteLine(Name + ": " + Skin.Current.Format(msg, pars));
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
