/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        public IList<Type> GetApplicableEvents(Player player, bool isHost)
        {
            List<Type> result = new List<Type>();

            if (player != null && (CurrentPhase == Phase.SelectingFactions || player.Faction != Faction.None))
            {
                AddPlayerActions(player, isHost, result);
            }

            if (isHost)
            {
                AddHostActions(result);
            }

            return new List<Type>(result);
        }

        private void AddHostActions(List<Type> result)
        {
            switch (CurrentPhase)
            {
                case Phase.AwaitingPlayers:
                    result.Add(typeof(EstablishPlayers));
                    break;
                case Phase.CustomizingDecks:
                    result.Add(typeof(CardsDetermined));
                    break;
                case Phase.PerformCustomSetup:
                    result.Add(typeof(PerformSetup));
                    break;
                case Phase.HmsPlacement:
                    if (!IsPlaying(Faction.Grey) && Applicable(Rule.HMSwithoutGrey)) result.Add(typeof(PerformHmsPlacement));
                    break;
                case Phase.SelectingFactions:
                case Phase.TradingFactions:
                case Phase.MetheorAndStormSpell:
                case Phase.StormReport:
                case Phase.Thumper:
                case Phase.HarvesterA:
                case Phase.HarvesterB:
                case Phase.AllianceA:
                case Phase.AllianceB:
                case Phase.BlowReport:
                case Phase.BeginningOfCharity:
                case Phase.ClaimingCharity:
                case Phase.CharityReport:
                case Phase.BeginningOfBidding:
                case Phase.ReplacingCardJustWon when Version > 150:
                case Phase.WaitingForNextBiddingRound:
                case Phase.BiddingReport:
                case Phase.BeginningOfResurrection:
                case Phase.Resurrection:
                case Phase.ResurrectionReport:
                case Phase.BeginningOfShipAndMove:
                case Phase.ShipmentAndMoveConcluded:
                case Phase.BeginningOfBattle:
                case Phase.BattleReport:
                case Phase.BeginningOfCollection:
                case Phase.CollectionReport:
                case Phase.Extortion:
                case Phase.Contemplate:
                case Phase.TurnConcluded:
                case Phase.CancellingTraitor:
                    result.Add(typeof(EndPhase));
                    break;
            }

            if (CurrentMainPhase >= MainPhase.Setup)
            {
                result.Add(typeof(PlayerReplaced));
            }
        }

        private void AddPlayerActions(Player player, bool isHost, List<Type> result)
        {
            var faction = player.Faction;

            switch (CurrentPhase)
            {
                case Phase.Discarding:
                    if (FactionsThatMustDiscard.Contains(faction)) result.Add(typeof(Discarded));
                    break;
                case Phase.DiscardingTraitor:
                    if (faction == FactionThatMustDiscardTraitor) result.Add(typeof(TraitorDiscarded));
                    break;
                case Phase.AllianceByTerror:
                    if (faction == AllianceByTerrorOfferedTo) result.Add(typeof(AllianceByTerror));
                    break;
                case Phase.AllianceByAmbassador:
                    if (faction == AllianceByAmbassadorOfferedTo) result.Add(typeof(AllianceByAmbassador));
                    break;
                case Phase.Bureaucracy:
                    if (player == PlayerSkilledAs(LeaderSkill.Bureaucrat)) result.Add(typeof(Bureaucracy));
                    break;
                case Phase.AssigningInitialSkills:
                case Phase.AssigningSkill:
                    if (player.SkillsToChooseFrom.Any()) result.Add(typeof(SkillAssigned));
                    break;
                case Phase.SelectingFactions:
                    if (player.Faction == Faction.None) result.Add(typeof(FactionSelected));
                    break;
                case Phase.DiallingStorm:
                    if (HasBattleWheel.Contains(player.Faction) && !HasActedOrPassed.Contains(player.Faction)) result.Add(typeof(StormDialled));
                    break;
                case Phase.TradingFactions:
                    if (Players.Count > 1) result.Add(typeof(FactionTradeOffered));
                    break;
                case Phase.PerformCustomSetup:
                    break;
                case Phase.BluePredicting:
                    if (faction == Faction.Blue) result.Add(typeof(BluePrediction));
                    break;
                case Phase.YellowSettingUp:
                    if (faction == Faction.Yellow) result.Add(typeof(PerformYellowSetup));
                    break;
                case Phase.BlueSettingUp:
                    if (faction == Faction.Blue) result.Add(typeof(PerformBluePlacement));
                    break;
                case Phase.CyanSettingUp:
                    if (faction == Faction.Cyan) result.Add(typeof(PerformCyanSetup));
                    break;
                case Phase.BlackMulligan:
                    if (faction == Faction.Black) result.Add(typeof(MulliganPerformed));
                    break;
                case Phase.Loyalty:
                    if (faction == Faction.Pink) result.Add(typeof(LoyaltyDecided));
                    break;
                case Phase.SelectingTraitors:
                    if (faction != Faction.Black && faction != Faction.Purple && !HasActedOrPassed.Contains(faction)) result.Add(typeof(TraitorsSelected));
                    break;
                case Phase.GreySelectingCard:
                    if (faction == Faction.Grey) result.Add(typeof(GreySelectedStartingCard));
                    break;
                case Phase.HmsPlacement:
                    if (faction == Faction.Grey) result.Add(typeof(PerformHmsPlacement));
                    break;
                case Phase.HmsMovement:
                    if (faction == Faction.Grey) result.Add(typeof(PerformHmsMovement));
                    break;
                case Phase.MetheorAndStormSpell:
                    if (player.Has(TreacheryCardType.StormSpell) && CurrentTurn > 1) result.Add(typeof(StormSpellPlayed));
                    if (MetheorPlayed.MayPlayMetheor(this, player)) result.Add(typeof(MetheorPlayed));
                    break;
                case Phase.Thumper:
                    if (player.Has(TreacheryCardType.Thumper) && CurrentTurn > 1) result.Add(typeof(ThumperPlayed));
                    if (Version < 103 && player.Has(TreacheryCardType.Amal)) result.Add(typeof(AmalPlayed));
                    break;
                case Phase.VoteAllianceA:
                case Phase.VoteAllianceB:
                    if (!NexusVotes.Any(v => v.Initiator == faction)) result.Add(typeof(NexusVoted));
                    break;
                case Phase.HarvesterA:
                case Phase.HarvesterB:
                    if (player.Has(TreacheryCardType.Harvester)) result.Add(typeof(HarvesterPlayed));
                    break;
                case Phase.StormLosses:
                    if (faction == TakeLosses.LossesToTake(this).Faction) result.Add(typeof(TakeLosses));
                    break;
                case Phase.YellowSendingMonsterA:
                case Phase.YellowSendingMonsterB:
                    if (faction == Faction.Yellow) result.Add(typeof(YellowSentMonster));
                    break;
                case Phase.YellowRidingMonsterA:
                case Phase.YellowRidingMonsterB:
                    if (faction == Faction.Yellow) result.Add(typeof(YellowRidesMonster));
                    break;
                case Phase.AllianceA:
                case Phase.AllianceB:
                    if (!HasActedOrPassed.Contains(faction))
                    {
                        if (player.Ally == Faction.None && Players.Count > 1) result.Add(typeof(AllianceOffered));
                        if (player.Ally != Faction.None) result.Add(typeof(AllianceBroken));
                    }
                    break;
                case Phase.NexusCards:
                    if (NexusCardDrawn.Applicable(this, player)) result.Add(typeof(NexusCardDrawn));
                    break;
                case Phase.BlowReport:
                    break;
                case Phase.ClaimingCharity:
                    if (!isHost && faction == Faction.Green) result.Add(typeof(EndPhase));
                    if (player.Resources <= 1 && !HasActedOrPassed.Contains(faction) && (Version <= 139 || !CharityIsCancelled)) result.Add(typeof(CharityClaimed));
                    if (Version < 103 && player.Has(TreacheryCardType.Amal) && (Version <= 82 || HasActedOrPassed.Count == 0)) result.Add(typeof(AmalPlayed));
                    break;
                case Phase.BlackMarketAnnouncement:
                    if (faction == Faction.White) result.Add(typeof(WhiteAnnouncesBlackMarket));
                    break;
                case Phase.BlackMarketBidding:
                    if (BlackMarketBid.MayBePlayed(this, player))
                    {
                        result.Add(typeof(BlackMarketBid));
                    }
                    if (faction == Faction.Red && Applicable(Rule.RedSupportingNonAllyBids)) result.Add(typeof(RedBidSupport));
                    break;
                case Phase.WhiteAnnouncingAuction:
                    if (faction == Faction.White) result.Add(typeof(WhiteAnnouncesAuction));
                    break;
                case Phase.WhiteSpecifyingAuction:
                    {
                        if (WhiteSpecifiesAuction.MaySpecifyCard(this, player) || WhiteSpecifiesAuction.MaySpecifyTypeOfAuction(this, player))
                        {
                            result.Add(typeof(WhiteSpecifiesAuction));
                        }
                        break;
                    }
                case Phase.Bidding:
                    if (Bid.MayBePlayed(this, player))
                    {
                        result.Add(typeof(Bid));
                    }
                    if (Version < 103 && player.Has(TreacheryCardType.Amal) && CardNumber == 1 && !Bids.Any()) result.Add(typeof(AmalPlayed));
                    if (faction == Faction.Red && Applicable(Rule.RedSupportingNonAllyBids)) result.Add(typeof(RedBidSupport));
                    break;
                case Phase.WhiteKeepingUnsoldCard:
                    if (faction == Faction.White) result.Add(typeof(WhiteKeepsUnsoldCard));
                    break;
                case Phase.GreyRemovingCardFromBid:
                    {
                        var occupierOfGreyHomeworld = OccupierOf(World.Grey);
                        if (occupierOfGreyHomeworld == null && faction == Faction.Grey || occupierOfGreyHomeworld == player) result.Add(typeof(GreyRemovedCardFromAuction));
                        if (Version < 103 && player.Has(TreacheryCardType.Amal) && CardNumber == 1 && !Bids.Any()) result.Add(typeof(AmalPlayed));
                        break;
                    }
                case Phase.GreySwappingCard:
                    {
                        var occupierOfGreyHomeworld = OccupierOf(World.Grey);
                        if (occupierOfGreyHomeworld == null && faction == Faction.Grey || occupierOfGreyHomeworld == player) result.Add(typeof(GreySwappedCardOnBid));
                        if (Version < 103 && player.Has(TreacheryCardType.Amal) && CardNumber == 1 && !Bids.Any()) result.Add(typeof(AmalPlayed));
                        break;
                    }
                case Phase.ReplacingCardJustWon:
                    if (faction == FactionThatMayReplaceBoughtCard) result.Add(typeof(ReplacedCardWon));
                    break;
                case Phase.WaitingForNextBiddingRound:
                    if (!isHost && faction == Faction.Green) result.Add(typeof(EndPhase));
                    break;
                case Phase.BiddingReport:
                    if (faction == Faction.Purple && Players.Count > 1 && (Version < 113 || !Prevented(FactionAdvantage.PurpleIncreasingRevivalLimits))) result.Add(typeof(SetIncreasedRevivalLimits));
                    if (faction == Faction.Red && player.HasHighThreshold() && player.Resources >= 2) result.Add(typeof(RedDiscarded));
                    break;

                case Phase.BeginningOfResurrection:
                    if (faction == Faction.Purple && Players.Count > 1 && (Version < 113 || !Prevented(FactionAdvantage.PurpleIncreasingRevivalLimits))) result.Add(typeof(SetIncreasedRevivalLimits));
                    break;

                case Phase.Resurrection:
                    if (IsPlaying(Faction.Purple) && faction != Faction.Purple &&
                        (Version <= 78 || !HasActedOrPassed.Contains(faction)) &&
                        KilledHeroes(player).Any() &&
                        !Revival.NormallyRevivableHeroes(this, player).Any()) result.Add(typeof(RequestPurpleRevival));

                    if (faction == Faction.Purple && (CurrentRevivalRequests.Any() || EarlyRevivalsOffers.Any())) result.Add(typeof(AcceptOrCancelPurpleRevival));
                    if (!HasActedOrPassed.Contains(faction) && HasSomethingToRevive(player) && !PreventedFromReviving(faction)) result.Add(typeof(Revival));
                    if (faction == Faction.Purple && Players.Count > 1 && (Version < 113 || !Prevented(FactionAdvantage.PurpleIncreasingRevivalLimits))) result.Add(typeof(SetIncreasedRevivalLimits));
                    if (Version < 103 && player.Has(TreacheryCardType.Amal) && (Version <= 82 || HasActedOrPassed.Count == 0)) result.Add(typeof(AmalPlayed));
                    break;

                case Phase.ResurrectionReport:
                    if (faction == Faction.Pink && AmbassadorPlaced.IsApplicable(this, player)) result.Add(typeof(AmbassadorPlaced));
                    break;

                case Phase.OrangeShip:
                    if (faction == Faction.Orange)
                    {
                        if (OrangeMayDelay) result.Add(typeof(OrangeDelay));
                        result.Add(typeof(Shipment));
                        if (player.TreacheryCards.Any(c => c.Type == TreacheryCardType.Caravan) && Caravan.TerritoriesWithAnyForcesNotInStorm(this, player).Any()) result.Add(typeof(Caravan));
                    }
                    if (Version < 103 && Version <= 96 && player.Has(TreacheryCardType.Amal) && HasActedOrPassed.Count == 0) result.Add(typeof(AmalPlayed));
                    if (Version < 103 && Version >= 97 && player.Has(TreacheryCardType.Amal) && BeginningOfShipmentAndMovePhase) result.Add(typeof(AmalPlayed));
                    break;
                case Phase.BlueAccompaniesOrangeShip:
                case Phase.BlueAccompaniesNonOrangeShip:
                    if (faction == Faction.Blue) result.Add(typeof(BlueAccompanies));
                    break;
                case Phase.NonOrangeShip:
                    if (player == ShipmentAndMoveSequence.CurrentPlayer)
                    {
                        result.Add(typeof(Shipment));
                        if (player.TreacheryCards.Any(c => c.Type == TreacheryCardType.Caravan)) result.Add(typeof(Caravan));
                    }
                    if (Version < 103 && Version <= 96 && player.Has(TreacheryCardType.Amal) && HasActedOrPassed.Count == 0) result.Add(typeof(AmalPlayed));
                    if (Version < 103 && Version >= 97 && player.Has(TreacheryCardType.Amal) && BeginningOfShipmentAndMovePhase) result.Add(typeof(AmalPlayed));
                    break;
                case Phase.OrangeMove:
                    if (faction == Faction.Orange)
                    {
                        if (player.TreacheryCards.Any(c => c.Type == TreacheryCardType.Caravan)) result.Add(typeof(Caravan));
                        result.Add(typeof(Move));
                        if (FlightUsed.IsAvailable(player)) result.Add(typeof(FlightUsed));
                        if (Planetology.CanBePlayed(this, player)) result.Add(typeof(Planetology));
                    }
                    break;
                case Phase.NonOrangeMove:
                    if (player == ShipmentAndMoveSequence.CurrentPlayer)
                    {
                        if (player.TreacheryCards.Any(c => c.Type == TreacheryCardType.Caravan)) result.Add(typeof(Caravan));
                        result.Add(typeof(Move));
                        if (FlightUsed.IsAvailable(player)) result.Add(typeof(FlightUsed));
                        if (Planetology.CanBePlayed(this, player)) result.Add(typeof(Planetology));
                    }
                    break;

                case Phase.BlueIntrudedByOrangeShip:
                case Phase.BlueIntrudedByNonOrangeShip:
                case Phase.BlueIntrudedByOrangeMove:
                case Phase.BlueIntrudedByNonOrangeMove:
                case Phase.BlueIntrudedByYellowRidingMonsterA:
                case Phase.BlueIntrudedByYellowRidingMonsterB:
                case Phase.BlueIntrudedByCaravan:
                    if (faction == Faction.Blue) result.Add(typeof(BlueFlip));
                    break;

                case Phase.TerrorTriggeredByOrangeShip:
                case Phase.TerrorTriggeredByBlueAccompaniesOrangeShip:
                case Phase.TerrorTriggeredByNonOrangeShip:
                case Phase.TerrorTriggeredByBlueAccompaniesNonOrangeShip:
                case Phase.TerrorTriggeredByOrangeMove:
                case Phase.TerrorTriggeredByNonOrangeMove:
                case Phase.TerrorTriggeredByYellowRidingMonsterA:
                case Phase.TerrorTriggeredByYellowRidingMonsterB:
                case Phase.TerrorTriggeredByCaravan:
                    if (faction == Faction.Cyan) result.Add(typeof(TerrorRevealed));
                    break;

                case Phase.AmbassadorTriggeredByOrangeShip:
                case Phase.AmbassadorTriggeredByBlueAccompaniesOrangeShip:
                case Phase.AmbassadorTriggeredByNonOrangeShip:
                case Phase.AmbassadorTriggeredByBlueAccompaniesNonOrangeShip:
                case Phase.AmbassadorTriggeredByOrangeMove:
                case Phase.AmbassadorTriggeredByNonOrangeMove:
                case Phase.AmbassadorTriggeredByYellowRidingMonsterA:
                case Phase.AmbassadorTriggeredByYellowRidingMonsterB:
                case Phase.AmbassadorTriggeredByCaravan:
                    if (faction == Faction.Pink || player.Ally == Faction.Pink && PinkSharesAmbassadors) result.Add(typeof(AmbassadorActivated));
                    break;

                case Phase.ShipmentAndMoveConcluded:
                    if (Version < 103 && player.Has(TreacheryCardType.Amal)) result.Add(typeof(AmalPlayed));
                    break;

                case Phase.BattlePhase:
                    {
                        if (CurrentBattle == null && player == NextPlayerToBattle)
                        {
                            result.Add(typeof(BattleInitiated));
                        }

                        if (CurrentBattle != null && SwitchedSkilledLeader.CanBePlayed(this, player))
                        {
                            result.Add(typeof(SwitchedSkilledLeader));
                        }

                        if (HMSAdvantageChosen.CanBePlayed(this, player))
                        {
                            result.Add(typeof(HMSAdvantageChosen));
                        }

                        if (Voice.MayUseVoice(this, player))
                        {
                            result.Add(typeof(Voice));
                        }

                        if (Thought.MayBeUsed(this, player))
                        {
                            result.Add(typeof(Thought));
                        }

                        if (Prescience.MayUsePrescience(this, player))
                        {
                            result.Add(typeof(Prescience));
                        }

                        if (CurrentBattle != null && faction == CurrentBattle.Aggressor && AggressorBattleAction == null)
                        {
                            result.Add(typeof(Battle));
                        }
                        else if (CurrentBattle != null && faction == CurrentBattle.Defender && DefenderBattleAction == null)
                        {
                            result.Add(typeof(Battle));
                        }

                        if (CurrentBattle != null && faction == CurrentBattle.Aggressor && AggressorBattleAction != null)
                        {
                            result.Add(typeof(BattleRevision));
                        }
                        else if (CurrentBattle != null && faction == CurrentBattle.Defender && DefenderBattleAction != null)
                        {
                            result.Add(typeof(BattleRevision));
                        }

                        if (CurrentBattle != null && ResidualPlayed.MayPlay(this, player))
                        {
                            result.Add(typeof(ResidualPlayed));
                        }

                        if (Version < 103 && player.Has(TreacheryCardType.Amal) && NrOfBattlesFought == 0) result.Add(typeof(AmalPlayed));
                    }
                    break;

                case Phase.ClaimingBattle:
                    if (HasLowThreshold(Faction.Pink) && BattleAboutToStart.OpponentOf(Faction.Pink) == player)
                    {
                        result.Add(typeof(BattleClaimed));
                    }
                    else if (faction == Faction.Pink)
                    {
                        result.Add(typeof(BattleClaimed));
                    }
                    break;

                case Phase.CallTraitorOrPass:

                    if (AggressorBattleAction != null && DefenderBattleAction != null &&
                            (AggressorTraitorAction == null && faction == CurrentBattle.Aggressor ||
                             AggressorTraitorAction == null && faction == Faction.Black && GetPlayer(AggressorBattleAction.Initiator).Ally == Faction.Black && player.Traitors.Contains(DefenderBattleAction.Hero) ||
                             DefenderTraitorAction == null && faction == CurrentBattle.Defender ||
                             DefenderTraitorAction == null && faction == Faction.Black && GetPlayer(DefenderBattleAction.Initiator).Ally == Faction.Black && player.Traitors.Contains(AggressorBattleAction.Hero)))
                    {
                        result.Add(typeof(TreacheryCalled));
                    }

                    if (faction == AggressorBattleAction.Initiator && AggressorBattleAction.Weapon != null && AggressorBattleAction.Weapon.Type == TreacheryCardType.PoisonTooth && !PoisonToothCancelled) result.Add(typeof(PoisonToothCancelled));

                    if (faction == DefenderBattleAction.Initiator && DefenderBattleAction.Weapon != null && DefenderBattleAction.Weapon.Type == TreacheryCardType.PoisonTooth && !PoisonToothCancelled) result.Add(typeof(PoisonToothCancelled));

                    if (PortableAntidoteUsed.CanBePlayed(this, player)) result.Add(typeof(PortableAntidoteUsed));

                    if (Diplomacy.CanBePlayed(this, player)) result.Add(typeof(Diplomacy));

                    break;

                case Phase.Retreating:
                    if (faction == BattleLoser) result.Add(typeof(Retreat));
                    break;

                case Phase.MeltingRock:
                    if (RockWasMelted.CanBePlayed(this, player)) result.Add(typeof(RockWasMelted));
                    break;

                case Phase.CaptureDecision:
                    if (faction == Faction.Black) result.Add(typeof(Captured));
                    break;

                case Phase.BattleConclusion:
                    if (faction == BattleWinner && !BattleWasConcludedByWinner) result.Add(typeof(BattleConcluded));
                    if (LoserConcluded.IsApplicable(this, player)) result.Add(typeof(LoserConcluded));
                    break;

                case Phase.AvoidingAudit:
                    if (player == Auditee) result.Add(typeof(AuditCancelled));
                    break;

                case Phase.Auditing:
                    if (faction == Faction.Brown) result.Add(typeof(Audited));
                    break;

                case Phase.RevealingFacedancer:
                    if (faction == Faction.Purple) result.Add(typeof(FaceDancerRevealed));
                    break;

                case Phase.Facedancing:
                    if (faction == Faction.Purple) result.Add(typeof(FaceDanced));
                    break;

                case Phase.BattleReport:
                    if (Version < 103 && player.Has(TreacheryCardType.Amal) && NextPlayerToBattle == null) result.Add(typeof(AmalPlayed));
                    break;

                case Phase.DividingCollectedResources:
                    if (DivideResources.IsApplicable(this, player)) result.Add(typeof(DivideResources));
                    break;

                case Phase.AcceptingResourceDivision:
                    if (DivideResourcesAccepted.IsApplicable(this, player)) result.Add(typeof(DivideResourcesAccepted));
                    break;

                case Phase.ReplacingFaceDancer:
                    if (faction == Faction.Purple) result.Add(typeof(FaceDancerReplaced));
                    break;

                case Phase.Extortion:
                    if (ExtortionPrevented.CanBePlayed(this, player)) result.Add(typeof(ExtortionPrevented));
                    break;

                case Phase.Contemplate:
                    if (Version < 103 && player.Has(TreacheryCardType.Amal)) result.Add(typeof(AmalPlayed));
                    if (faction == Faction.Brown && !Prevented(FactionAdvantage.BrownEconomics) && EconomicsStatus == BrownEconomicsStatus.None) result.Add(typeof(BrownEconomics));
                    if (faction == Faction.Cyan && TerrorPlanted.IsApplicable(this, player)) result.Add(typeof(TerrorPlanted));
                    break;

                case Phase.PerformingKarmaHandSwap:
                    if (faction == Faction.Black && !KarmaPrevented(faction)) result.Add(typeof(KarmaHandSwap));
                    break;

                case Phase.TradingCards:
                    if (faction == CurrentCardTradeOffer.Target) result.Add(typeof(CardTraded));
                    break;

                case Phase.Clairvoyance:
                    if (faction == LatestClairvoyance.Target) result.Add(typeof(ClairVoyanceAnswered));
                    break;

                case Phase.Thought:
                    if (player == CurrentBattle.OpponentOf(CurrentThought.Initiator)) result.Add(typeof(ThoughtAnswered));
                    break;

                case Phase.SearchingDiscarded:
                    if (DiscardedSearched.CanBePlayed(player)) result.Add(typeof(DiscardedSearched));
                    break;
            }

            //Events that are (amost) always valid
            if (CurrentMainPhase > MainPhase.Setup)
            {
                if (faction == Faction.Brown && player.HasLowThreshold() && ResourcesAudited.ValidFactions(this, player).Any()) result.Add(typeof(ResourcesAudited));
            }

            if (CurrentMainPhase == MainPhase.ShipmentAndMove && SetShipmentPermission.IsApplicable(this, player)) result.Add(typeof(SetShipmentPermission));

            if (Version <= 123)
            {
                result.Add(typeof(HideSecrets));
            }

            if (NexusPlayed.IsApplicable(this, player)) result.Add(typeof(NexusPlayed));

            if (player.Ally != Faction.None) result.Add(typeof(AllyPermission));

            bool isAfterSetup = Version < 107 ? CurrentPhase > Phase.TradingFactions : CurrentMainPhase > MainPhase.Setup;
            bool hasFinalizedBattlePlanWaitingToBeResolved = (CurrentPhase == Phase.BattlePhase || CurrentPhase == Phase.MeltingRock || CurrentPhase == Phase.CallTraitorOrPass) && CurrentBattle != null && CurrentBattle.PlanOf(player) != null;

            if (JuicePlayed.CanBePlayedBy(this, player))
            {
                result.Add(typeof(JuicePlayed));
            }

            if (CardGiven.IsApplicable(this, player)) result.Add(typeof(CardGiven));

            if (faction == Faction.Pink &&
                CurrentPhase == Phase.CallTraitorOrPass &&
                CurrentBattle != null &&
                CurrentBattle.IsAggressorOrDefender(player) &&
                !KarmaPrevented(faction) &&
                !player.SpecialKarmaPowerUsed &&
                player.Has(TreacheryCardType.Karma) &&
                Applicable(Rule.AdvancedKarama))
            {
                var planOfPink = CurrentBattle.PlanOf(Faction.Pink);
                if (planOfPink != null && planOfPink.Defense == null && planOfPink.Weapon == null)
                {
                    result.Add(typeof(KarmaPinkDial));
                }
            }

            if (isAfterSetup && (Version < 100 || !hasFinalizedBattlePlanWaitingToBeResolved) && !CurrentPhaseIsUnInterruptable)
            {
                if (CurrentMainPhase < MainPhase.Battle && player.NoFieldIsActive)
                {
                    result.Add(typeof(WhiteRevealedNoField));
                }

                if (faction == Faction.Brown && !Prevented(FactionAdvantage.BrownDiscarding) && ConsiderAsEndOfPhase && BrownDiscarded.ValidCards(player).Any())
                {
                    result.Add(typeof(BrownDiscarded));
                }

                if (!result.Contains(typeof(AmalPlayed)) && player.Has(TreacheryCardType.Amal) && ConsiderAsStartOfPhase)
                {
                    result.Add(typeof(AmalPlayed));
                }

                if (DiscardedTaken.CanBePlayed(this, player))
                {
                    result.Add(typeof(DiscardedTaken));
                }

                if (CurrentPhase != Phase.SearchingDiscarded && DiscardedSearchedAnnounced.CanBePlayed(this, player))
                {
                    result.Add(typeof(DiscardedSearchedAnnounced));
                }

                if (CurrentMainPhase == MainPhase.ShipmentAndMove && BrownMovePrevention.CanBePlayedBy(this, player))
                {
                    result.Add(typeof(BrownMovePrevention));
                }

                if (BrownKarmaPrevention.CanBePlayedBy(this, player))
                {
                    result.Add(typeof(BrownKarmaPrevention));
                }

                if (CurrentMainPhase == MainPhase.ShipmentAndMove && BrownExtraMove.CanBePlayedBy(this, player))
                {
                    result.Add(typeof(BrownExtraMove));
                }

                if (CurrentMainPhase == MainPhase.Resurrection && BrownFreeRevivalPrevention.CanBePlayedBy(this, player))
                {
                    result.Add(typeof(BrownFreeRevivalPrevention));
                }

                if (CurrentMainPhase == MainPhase.Contemplate && BrownRemoveForce.CanBePlayedBy(this, player))
                {
                    result.Add(typeof(BrownRemoveForce));
                }

                if (
                    (faction == Faction.Brown && player.Ally != Faction.None || player.Ally == Faction.Brown) &&
                    ConsiderAsEndOfPhase &&
                    player.AlliedPlayer.TreacheryCards.Count > 0 &&
                    player.TreacheryCards.Count > 0 &&
                    LastTurnCardWasTraded < CurrentTurn)
                {
                    result.Add(typeof(CardTraded));
                }

                if (player.Has(TreacheryCardType.RaiseDead))
                {
                    result.Add(typeof(RaiseDeadPlayed));
                }

                if (player.Has(TreacheryCardType.Clairvoyance))
                {
                    result.Add(typeof(ClairVoyancePlayed));
                }

                if (player.HasKarma && !KarmaPrevented(faction))
                {
                    result.Add(typeof(Karma));
                }

                if (Players.Count > 1 &&
                    faction == Faction.Black &&
                    !KarmaPrevented(faction) &&
                    !player.SpecialKarmaPowerUsed &&
                    player.Has(TreacheryCardType.Karma) &&
                    CurrentMainPhase == MainPhase.Bidding &&
                    Applicable(Rule.AdvancedKarama))
                {
                    result.Add(typeof(KarmaHandSwapInitiated));
                }

                if (faction == Faction.Orange &&
                    !KarmaPrevented(faction) &&
                    !player.SpecialKarmaPowerUsed &&
                    player.Has(TreacheryCardType.Karma) &&
                    CurrentMainPhase == MainPhase.ShipmentAndMove &&
                    Applicable(Rule.AdvancedKarama))
                {
                    result.Add(typeof(KarmaShipmentPrevention));
                }

                if (faction == Faction.Purple &&
                    !KarmaPrevented(faction) &&
                    !player.SpecialKarmaPowerUsed &&
                    player.Has(TreacheryCardType.Karma) &&
                    CurrentMainPhase == MainPhase.Resurrection &&
                    Applicable(Rule.AdvancedKarama))
                {
                    result.Add(typeof(KarmaRevivalPrevention));
                }


                if (faction == Faction.Brown &&
                    !KarmaPrevented(faction) &&
                    !player.SpecialKarmaPowerUsed &&
                    player.Has(TreacheryCardType.Karma) &&
                    Applicable(Rule.AdvancedKarama))
                {
                    result.Add(typeof(KarmaBrownDiscard));
                }

                if (faction == Faction.White &&
                    !KarmaPrevented(faction) &&
                    !player.SpecialKarmaPowerUsed &&
                    player.Has(TreacheryCardType.Karma) &&
                    Applicable(Rule.AdvancedKarama) &&
                    !HasBidToPay(player))
                {
                    result.Add(typeof(KarmaWhiteBuy));
                }

                if (faction == Faction.Red &&
                    !KarmaPrevented(faction) &&
                    !player.SpecialKarmaPowerUsed &&
                    player.Has(TreacheryCardType.Karma) &&
                    CurrentMainPhase == MainPhase.Resurrection &&
                    Applicable(Rule.AdvancedKarama))
                {
                    result.Add(typeof(KarmaFreeRevival));
                }

                if (faction == Faction.Grey &&
                    (!KarmaPrevented(faction) && !player.SpecialKarmaPowerUsed && player.Has(TreacheryCardType.Karma) || KarmaHmsMovesLeft == 1) &&
                    CurrentMainPhase == MainPhase.ShipmentAndMove &&
                    player == ShipmentAndMoveSequence.CurrentPlayer &&
                    player.AnyForcesIn(Map.HiddenMobileStronghold) > 0 &&
                    Applicable(Rule.AdvancedKarama))
                {
                    result.Add(typeof(KarmaHmsMovement));
                }

                if (faction == Faction.Yellow && CurrentMainPhase == MainPhase.Blow && CurrentTurn > 1 &&
                    !KarmaPrevented(faction) &&
                    !player.SpecialKarmaPowerUsed &&
                    player.Has(TreacheryCardType.Karma) &&
                    Applicable(Rule.AdvancedKarama))
                {
                    result.Add(typeof(KarmaMonster));
                }

                if (faction == Faction.Green && CurrentMainPhase == MainPhase.Battle &&
                    CurrentBattle != null &&
                    CurrentBattle.IsInvolved(player) &&
                    !KarmaPrevented(faction) &&
                    !player.SpecialKarmaPowerUsed &&
                    player.Has(TreacheryCardType.Karma) &&
                    Applicable(Rule.AdvancedKarama))
                {
                    result.Add(typeof(KarmaPrescience));
                }

                if (faction == Faction.Blue && CurrentPhase > Phase.AllianceB &&
                    (Version <= 150 && CurrentPhase < Phase.OrangeShip || CurrentMainPhase < MainPhase.ShipmentAndMove || player.Initiated(CurrentBlueNexus) && CurrentMainPhase == MainPhase.ShipmentAndMove) &&
                    BlueBattleAnnouncement.ValidTerritories(this, player).Any())
                {
                    result.Add(typeof(BlueBattleAnnouncement));
                }

                if (Players.Count > 1 &&
                    Donated.ValidTargets(this, player).Any() &&
                    (isHost || player.Resources > 0) &&
                    Donated.MayDonate(this, player) &&
                    (AggressorBattleAction == null || faction != AggressorBattleAction.Initiator) &&
                    (DefenderBattleAction == null || faction != DefenderBattleAction.Initiator))
                {
                    result.Add(typeof(Donated));
                }

                if (faction == Faction.White &&
                    CurrentPhase != Phase.BlackMarketBidding &&
                    CurrentPhase != Phase.Bidding &&
                    !hasFinalizedBattlePlanWaitingToBeResolved &&
                    player.Ally != Faction.None &&
                    WhiteGaveCard.ValidCards(player).Any() &&
                    player.AlliedPlayer.HasRoomForCards)
                {
                    result.Add(typeof(WhiteGaveCard));
                }

                if (CurrentPhase != Phase.BlackMarketBidding &&
                    (CurrentPhase != Phase.Bidding || CurrentBid == null) &&
                    !hasFinalizedBattlePlanWaitingToBeResolved &&
                    DistransUsed.CanBePlayed(this, player))
                {
                    result.Add(typeof(DistransUsed));
                }

                if (Version >= 110)
                {
                    result.Add(typeof(DealOffered));
                    result.Add(typeof(DealAccepted));
                }
            }

            if (Version < 110)
            {
                if (player.Ally != Faction.None) result.Add(typeof(AllyPermission));

                result.Add(typeof(DealOffered));
                result.Add(typeof(DealAccepted));
            }

        }

        public bool CurrentPhaseIsUnInterruptable =>
                CurrentMainPhase == MainPhase.Ended ||
                CurrentPhase == Phase.AssigningSkill ||
                CurrentPhase == Phase.Clairvoyance ||
                CurrentPhase == Phase.Thought ||
                CurrentPhase == Phase.TradingCards ||
                CurrentPhase == Phase.Bureaucracy ||
                CurrentPhase == Phase.SearchingDiscarded ||
                CurrentPhase == Phase.PerformingKarmaHandSwap ||
                CurrentPhase == Phase.ReplacingCardJustWon ||
                CurrentPhase == Phase.DividingCollectedResources ||
                CurrentPhase == Phase.AcceptingResourceDivision;

        public static IEnumerable<Type> GetGameEventTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(ass => ass.GetTypes().Where(t => t.IsSubclassOf(typeof(GameEvent))).Distinct());
        }

        private bool ConsiderAsEndOfPhase =>
            CurrentMoment == MainPhaseMoment.End ||
            CurrentMoment == MainPhaseMoment.Start &&
            (CurrentMainPhase == MainPhase.Bidding && CurrentMainPhase == MainPhase.ShipmentAndMove);

        private bool ConsiderAsStartOfPhase =>
            CurrentMoment == MainPhaseMoment.Start ||
            (CurrentMainPhase == MainPhase.Blow && CurrentMoment == MainPhaseMoment.End) ||
            Version < 109 && CurrentMoment == MainPhaseMoment.End;
    }
}
