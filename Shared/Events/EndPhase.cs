/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Linq;

namespace Treachery.Shared
{
    public class EndPhase : GameEvent
    {
        #region Construction

        public EndPhase(Game game) : base(game)
        {
        }

        public EndPhase()
        {
        }

        #endregion Construction

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        #endregion Construction

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            switch (Game.CurrentPhase)
            {
                case Phase.SelectingFactions:
                    Game.AssignFactionsAndEnterFactionTrade();
                    break;

                case Phase.TradingFactions:
                    EstablishDecks();
                    break;

                case Phase.BeginningOfStorm:
                    Game.MoveHMSBeforeDiallingStorm();
                    break;

                case Phase.MetheorAndStormSpell:
                    EnterNormalStormPhase();
                    break;

                case Phase.StormReport:
                    Game.EnterSpiceBlowPhase();
                    break;

                case Phase.Thumper:
                    Game.EnterBlowA();
                    break;

                case Phase.HarvesterA:
                case Phase.HarvesterB:
                    Game.MoveToNextPhaseAfterResourceBlow();
                    break;

                case Phase.AllianceA:
                case Phase.AllianceB:
                    EndNexus();
                    break;

                case Phase.BlowReport:
                    if (Game.Applicable(Rule.HasCharityPhase))
                    {
                        EnterCharityPhase();
                    }
                    else
                    {
                        Game.EnterBiddingPhase();
                    }
                    break;

                case Phase.BeginningOfCharity:
                    StartClaimingCharity();
                    break;

                case Phase.ClaimingCharity:
                    EndCharityPhase();
                    break;

                case Phase.CharityReport:
                    Game.EnterBiddingPhase();
                    break;

                case Phase.BeginningOfBidding:
                    Game.StartBiddingPhase();
                    break;

                case Phase.ReplacingCardJustWon:
                    if (Game.CardJustWon == Game.CardSoldOnBlackMarket)
                    {
                        Game.EnterWhiteBidding();
                    }
                    else
                    {
                        Game.DetermineNextStepAfterCardWasSold();
                    }
                    break;

                case Phase.WaitingForNextBiddingRound:
                    Game.PutNextCardOnAuction();
                    break;

                case Phase.BiddingReport:
                    EnterRevivalPhase();
                    break;

                case Phase.BeginningOfResurrection:
                    Game.Enter(Phase.Resurrection);
                    break;

                case Phase.Resurrection:
                    EndResurrectionPhase();
                    break;

                case Phase.ResurrectionReport:
                    EnterShipmentAndMovePhase();
                    break;

                case Phase.BeginningOfShipAndMove:
                    Game.StartShipAndMoveSequence();
                    break;

                case Phase.ShipmentAndMoveConcluded:
                    EnterBattlePhase();
                    break;

                case Phase.BeginningOfBattle:
                    Game.Enter(Phase.BattlePhase);
                    break;

                case Phase.CancellingTraitor:
                    Game.Enter(Phase.CallTraitorOrPass);
                    Game.HandleRevealedBattlePlans();
                    break;

                case Phase.BattleReport:
                    ResetBattle();
                    Game.Enter(Game.NextPlayerToBattle != null, Phase.BattlePhase, EnterSpiceCollectionPhase);
                    break;

                case Phase.BeginningOfCollection:
                    Game.StartCollection();
                    break;

                case Phase.CollectionReport:
                    Game.EnterMentatPhase();
                    break;

                case Phase.Extortion:
                    Game.EndMentatPause();
                    break;

                case Phase.Contemplate:
                    Game.ContinueMentatPhase();
                    break;

                case Phase.TurnConcluded:
                    if (Game.Version < 108) Game.AddBribesToPlayerResources();
                    Game.EnterStormPhase();
                    break;
            }
        }

        private void EstablishDecks()
        {
            if (Game.IsPlaying(Faction.White))
            {
                Game.WhiteCache = TreacheryCardManager.GetWhiteCards();
            }

            Game.Enter(Game.Applicable(Rule.CustomDecks) && Game.Version >= 134, Phase.CustomizingDecks, Game.EnterSetupPhase);
        }

        private void EnterNormalStormPhase()
        {
            Log("The storm moves ", Game.NextStormMoves, " sectors");
            Game.MoveStormAndDetermineNext(Game.NextStormMoves);
            Game.EndStormPhase();
        }

        private void EnterCharityPhase()
        {
            Game.MainPhaseStart(MainPhase.Charity);
            Game.HasActedOrPassed.Clear();
            Game.Monsters.Clear();
            Game.ResourceTechTokenIncome = false;
            Game.Allow(FactionAdvantage.YellowControlsMonster);
            Game.Allow(FactionAdvantage.YellowProtectedFromMonster);

            if (Game.Version < 122)
            {
                StartClaimingCharity();
            }
            else
            {
                Game.Enter(Phase.BeginningOfCharity);
            }
        }

        private void StartClaimingCharity()
        {
            if (!Game.Prevented(FactionAdvantage.BrownControllingCharity))
            {
                var brown = GetPlayer(Faction.Brown);
                if (brown != null)
                {
                    int toCollect = Game.Players.Count * 2 * Game.CurrentCharityMultiplier;
                    brown.Resources += toCollect;
                    Log(Faction.Brown, " collect ", Payment.Of(toCollect));
                }
            }
            else
            {
                Game.LogPreventionByKarma(FactionAdvantage.BrownControllingCharity);
            }

            var blue = GetPlayer(Faction.Blue);
            if (blue != null && Game.Applicable(Rule.BlueAutoCharity))
            {
                if (!Game.Prevented(FactionAdvantage.BlueCharity))
                {
                    Game.HasActedOrPassed.Add(Faction.Blue);
                    Game.GiveCharity(blue, 2 * Game.CurrentCharityMultiplier);
                    Game.Stone(Milestone.CharityClaimed);
                }
                else
                {
                    Game.LogPreventionByKarma(FactionAdvantage.BlueCharity);
                    if (!Game.Applicable(Rule.FullPhaseKarma)) Game.Allow(FactionAdvantage.BlueCharity);
                }
            }

            Game.MainPhaseMiddle();
            Game.Enter(Phase.ClaimingCharity);
        }

        private void EndCharityPhase()
        {
            if (Game.ResourceTechTokenIncome) Game.ReceiveTechIncome(TechToken.Resources);

            if (Game.Version < 122)
            {
                Game.EnterBiddingPhase();
            }
            else
            {
                if (Game.Version >= 132) Game.MainPhaseEnd();
                Game.Enter(Phase.CharityReport);
            }
        }

        private void EnterRevivalPhase()
        {
            Game.MainPhaseStart(MainPhase.Resurrection);
            Game.Allow(FactionAdvantage.BlackFreeCard);
            Game.Allow(FactionAdvantage.RedReceiveBid);
            Game.Allow(FactionAdvantage.GreyAllyDiscardingCard);
            Game.RevivalTechTokenIncome = false;
            Game.AmbassadorsPlacedThisTurn = 0;
            Game.FactionsThatTookFreeRevival.Clear();
            Game.HasActedOrPassed.Clear();
            Game.PurpleStartedRevivalWithLowThreshold = Game.HasLowThreshold(Faction.Purple);

            if (Game.Version < 122)
            {
                Game.Enter(Phase.Resurrection);
            }
            else
            {
                Game.Enter(Phase.BeginningOfResurrection);
            }
        }

        private void EndResurrectionPhase()
        {
            if (Game.RevivalTechTokenIncome) Game.ReceiveTechIncome(TechToken.Graveyard);
            Game.CurrentKarmaRevivalPrevention = null;
            Game.CurrentYellowNexus = null;
            Game.CurrentRecruitsPlayed = null;

            if (Game.Version < 122)
            {
                EnterShipmentAndMovePhase();
            }
            else
            {
                if (Game.Version >= 132) Game.MainPhaseEnd();
                Game.Enter(Phase.ResurrectionReport);
            }
        }

        private void EnterShipmentAndMovePhase()
        {
            Game.MainPhaseStart(MainPhase.ShipmentAndMove);
            Game.FactionsWithOrnithoptersAtStartOfMovement = Game.Players.Where(p => Game.OccupiesArrakeenOrCarthag(p)).Select(p => p.Faction).ToList();
            Game.RecentMoves.Clear();
            Game.BeginningOfShipmentAndMovePhase = true;
            Game.FactionsWithIncreasedRevivalLimits = Array.Empty<Faction>();
            Game.EarlyRevivalsOffers.Clear();

            Game.ShipsTechTokenIncome = false;
            Game.CurrentFreeRevivalPrevention = null;
            Game.Allow(FactionAdvantage.BlueAnnouncesBattle);
            Game.Allow(FactionAdvantage.RedLetAllyReviveExtraForces);
            Game.Allow(FactionAdvantage.PurpleReceiveRevive);
            Game.Allow(FactionAdvantage.BrownRevival);

            Game.HasActedOrPassed.Clear();
            Game.LastShipmentOrMovement = null;

            Game.ShipmentAndMoveSequence = new PlayerSequence(Game);

            Game.Enter(Game.Version >= 107, Phase.BeginningOfShipAndMove, Game.StartShipAndMoveSequence);
        }

        private void EnterBattlePhase()
        {
            Game.MainPhaseStart(MainPhase.Battle);
            Game.NrOfBattlesFought = 0;
            Game.BattleSequence = new PlayerSequence(Game);
            if (Game.KarmaHmsMovesLeft != 2) Game.KarmaHmsMovesLeft = 0;
            ResetBattle();
            Game.Enter(Game.NextPlayerToBattle == null, EnterSpiceCollectionPhase, Game.Version >= 107, Phase.BeginningOfBattle, Phase.BattlePhase);
        }

        private void ResetBattle()
        {
            Game.FacedancerWasCancelled = false;
            Game.CurrentBattle = null;
            Game.CurrentPrescience = null;
            Game.CurrentThought = null;
            Game.CurrentVoice = null;
            Game.CurrentGreenNexus = null;
            Game.CurrentBlueNexus = null;
            Game.CurrentRedNexus = null;
            Game.CurrentGreyNexus = null;
            Game.BlackVictim = null;
            Game.AggressorBattleAction = null;
            Game.DefenderBattleAction = null;
            Game.AggressorTraitorAction = null;
            Game.DefenderTraitorAction = null;
            Game.PoisonToothCancelled = false;
            Game.GreySpecialForceLossesToTake = 0;
            Game.BattleWinner = Faction.None;
            Game.BattleLoser = Faction.None;
            Game.HasActedOrPassed.Clear();
            Game.BattleTriggeredBureaucracy = null;
            Game.CardsToBeDiscardedByLoserAfterBattle.Clear();
            Game.BattleWasConcludedByWinner = false;
            Game.LoserMayTryToAssassinate = false;
            Game.BattleWinnerMayChooseToDiscard = true;
            Game.BattleAboutToStart = null;
            Game.CurrentPinkOrAllyFighter = Faction.None;
            Game.CurrentPinkBattleContribution = 0;
            Game.GreenKarma = false;
            Game.PinkKarmaBonus = 0;
        }

        private void EnterSpiceCollectionPhase()
        {
            Game.MainPhaseStart(MainPhase.Collection);
            Game.ResourcesCollectedByYellow = 0;
            Game.ResourcesCollectedByBlackFromDesertOrHomeworld = 0;
            Game.CallHeroesHome();
            Game.PendingDiscoveries = Game.DiscoveriesOnPlanet.Where(kvp => Game.AnyForcesIn(kvp.Key.Territory)).Select(kvp => kvp.Value.Token).ToList();

            if (Game.Version < 122)
            {
                Game.StartCollection();
            }
            else
            {
                Game.Enter(Phase.BeginningOfCollection);
            }
        }

        private void EndNexus()
        {
            Game.NexusHasOccured = true;
            Game.CurrentAllianceOffers.Clear();

            if (YellowRidesMonster.IsApplicable(Game))
            {
                Game.Enter(Game.CurrentPhase == Phase.AllianceA, Phase.YellowRidingMonsterA, Phase.YellowRidingMonsterB);
            }
            else
            {
                if (Game.CurrentPhase == Phase.AllianceA)
                {
                    Game.Enter(Game.Applicable(Rule.IncreasedResourceFlow), Game.EnterBlowB, Game.StartNexusCardPhase);
                }
                else if (Game.CurrentPhase == Phase.AllianceB)
                {
                    Game.StartNexusCardPhase();
                }
            }
        }

        public override Message GetMessage()
        {
            return Message.Express("Phase ended by ", Initiator);
        }

        #endregion Execution
    }
}
