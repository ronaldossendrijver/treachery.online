// /*
//  * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
//  * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
//  * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
//  * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//  * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
//  * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
// */

using System.Threading.Tasks;

namespace Treachery.Shared.Services;

public interface IGameClient
{
    public Task ReceiveEstablishPlayers(int hostId, EstablishPlayers e);
    public Task ReceiveEndPhase(int hostId, EndPhase e);
    public Task ReceiveDonated(int hostId, Donated e);
    public Task ReceiveResourcesTransferred(int hostId, ResourcesTransferred e);
    public Task ReceiveFactionSelected(int hostId, FactionSelected e);
    public Task ReceiveFactionTradeOffered(int hostId, FactionTradeOffered e);
    public Task ReceivePerformSetup(int hostId, PerformSetup e);
    public Task ReceiveCardsDetermined(int hostId, CardsDetermined e);
    public Task ReceivePerformYellowSetup(int hostId, PerformYellowSetup e);
    public Task ReceiveBluePrediction(int hostId, BluePrediction e);
    public Task ReceiveCharityClaimed(int hostId, CharityClaimed e);
    public Task ReceivePerformBluePlacement(int hostId, PerformBluePlacement e);
    public Task ReceiveTraitorsSelected(int hostId, TraitorsSelected e);
    public Task ReceiveStormSpellPlayed(int hostId, StormSpellPlayed e);
    public Task ReceiveTestingStationUsed(int hostId, TestingStationUsed e);
    public Task ReceiveTakeLosses(int hostId, TakeLosses e);
    public Task ReceiveMetheorPlayed(int hostId, MetheorPlayed e);
    public Task ReceiveYellowSentMonster(int hostId, YellowSentMonster e);
    public Task ReceiveYellowRidesMonster(int hostId, YellowRidesMonster e);
    public Task ReceiveAllianceOffered(int hostId, AllianceOffered e);
    public Task ReceiveAllianceBroken(int hostId, AllianceBroken e);
    public Task ReceiveBid(int hostId, Bid e);
    public Task ReceiveRevival(int hostId, Revival e);
    public Task ReceiveBlueBattleAnnouncement(int hostId, BlueBattleAnnouncement e);
    public Task ReceiveShipment(int hostId, Shipment e);
    public Task ReceiveOrangeDelay(int hostId, OrangeDelay e);
    public Task ReceiveBlueAccompanies(int hostId, BlueAccompanies e);
    public Task ReceiveBlueFlip(int hostId, BlueFlip e);
    public Task ReceiveMove(int hostId, Move e);
    public Task ReceiveCaravan(int hostId, Caravan e);
    public Task ReceiveBattleInitiated(int hostId, BattleInitiated e);
    public Task ReceiveBattle(int hostId, Battle e);
    public Task ReceiveBattleRevision(int hostId, BattleRevision e);
    public Task ReceiveTreacheryCalled(int hostId, TreacheryCalled e);
    public Task ReceiveBattleConcluded(int hostId, BattleConcluded e);
    public Task ReceiveClairvoyancePlayed(int hostId, ClairVoyancePlayed e);
    public Task ReceiveClairvoyanceAnswered(int hostId, ClairVoyanceAnswered e);
    public Task ReceiveRaiseDeadPlayed(int hostId, RaiseDeadPlayed e);
    public Task ReceiveKarma(int hostId, Karma e);
    public Task ReceiveKarmaFreeRevival(int hostId, KarmaFreeRevival e);
    public Task ReceiveKarmaShipmentPrevention(int hostId, KarmaShipmentPrevention e);
    public Task ReceiveKarmaRevivalPrevention(int hostId, KarmaRevivalPrevention e);
    public Task ReceiveKarmaHandSwapInitiated(int hostId, KarmaHandSwapInitiated e);
    public Task ReceiveKarmaHandSwap(int hostId, KarmaHandSwap e);
    public Task ReceiveKarmaMonster(int hostId, KarmaMonster e);
    public Task ReceiveKarmaWhiteBuy(int hostId, KarmaWhiteBuy e);
    public Task ReceiveAllyPermission(int hostId, AllyPermission e);
    public Task ReceiveMulliganPerformed(int hostId, MulliganPerformed e);
    public Task ReceiveFaceDancerRevealed(int hostId, FaceDancerRevealed e);
    public Task ReceiveFaceDanced(int hostId, FaceDanced e);
    public Task ReceiveFaceDancerReplaced(int hostId, FaceDancerReplaced e);
    public Task ReceiveSetIncreasedRevivalLimits(int hostId, SetIncreasedRevivalLimits e);
    public Task ReceiveSetShipmentPermission(int hostId, SetShipmentPermission e);
    public Task ReceiveReceivePurpleRevival(int hostId, RequestPurpleRevival e);
    public Task ReceiveAcceptOrCancelPurpleRevival(int hostId, AcceptOrCancelPurpleRevival e);
    public Task ReceivePerformHmsPlacement(int hostId, PerformHmsPlacement e);
    public Task ReceivePerformHmsMovement(int hostId, PerformHmsMovement e);
    public Task ReceiveKarmaHmsMovement(int hostId, KarmaHmsMovement e);
    public Task ReceiveAmalPlayed(int hostId, AmalPlayed e);
    public Task ReceiveGreyRemovedCardFromAuction(int hostId, GreyRemovedCardFromAuction e);
    public Task ReceiveGreySelectedStartingCard(int hostId, GreySelectedStartingCard e);
    public Task ReceiveGreySwappedCardOnBid(int hostId, GreySwappedCardOnBid e);
    public Task ReceiveHarvesterPlayed(int hostId, HarvesterPlayed e);
    public Task ReceivePoisonToothCancelled(int hostId, PoisonToothCancelled e);
    public Task ReceiveReplacedCardWon(int hostId, ReplacedCardWon e);
    public Task ReceiveThumperPlayed(int hostId, ThumperPlayed e);
    public Task ReceiveVoice(int hostId, Voice e);
    public Task ReceivePrescience(int hostId, Prescience e);
    public Task ReceiveKarmaPrescience(int hostId, KarmaPrescience e);
    public Task ReceiveRedBidSupport(int hostId, RedBidSupport e);
    public Task ReceiveDealOffered(int hostId, DealOffered e);
    public Task ReceiveDealAccepted(int hostId, DealAccepted e);
    public Task ReceiveDiscoveryEntered(int hostId, DiscoveryEntered e);
    public Task ReceiveStormDialled(int hostId, StormDialled e);
    public Task ReceiveHideSecrets(int hostId, HideSecrets e);
    public Task ReceivePlayerReplaced(int hostId, PlayerReplaced e);
    public Task ReceiveBrownDiscarded(int hostId, BrownDiscarded e);
    public Task ReceiveRedDiscarded(int hostId, RedDiscarded e);
    public Task ReceiveBrownEconomics(int hostId, BrownEconomics e);
    public Task ReceiveCardTraded(int hostId, CardTraded e);
    public Task ReceiveKarmaBrownDiscard(int hostId, KarmaBrownDiscard e);
    public Task ReceiveAuditCancelled(int hostId, AuditCancelled e);
    public Task ReceiveAudited(int hostId, Audited e);
    public Task ReceiveBrownMovePrevention(int hostId, BrownMovePrevention e);
    public Task ReceiveBrownKarmaPrevention(int hostId, BrownKarmaPrevention e);
    public Task ReceiveBrownExtraMove(int hostId, BrownExtraMove e);
    public Task ReceiveBrownFreeRevivalPrevention(int hostId, BrownFreeRevivalPrevention e);
    public Task ReceiveBrownRemoveForce(int hostId, BrownRemoveForce e);
    public Task ReceiveWhiteAnnouncesBlackMarket(int hostId, WhiteAnnouncesBlackMarket e);
    public Task ReceiveBlackMarketBid(int hostId, BlackMarketBid e);
    public Task ReceiveWhiteAnnouncesAuction(int hostId, WhiteAnnouncesAuction e);
    public Task ReceiveWhiteSpecifiesAuction(int hostId, WhiteSpecifiesAuction e);
    public Task ReceiveWhiteKeepsUnsoldCard(int hostId, WhiteKeepsUnsoldCard e);
    public Task ReceiveWhiteRevealedNoField(int hostId, WhiteRevealedNoField e);
    public Task ReceiveWhiteGaveCard(int hostId, WhiteGaveCard e);
    public Task ReceiveCardGiven(int hostId, CardGiven e);
    public Task ReceiveRockWasMelted(int hostId, RockWasMelted e);
    public Task ReceiveResidualPlayed(int hostId, ResidualPlayed e);
    public Task ReceiveFlightUsed(int hostId, FlightUsed e);
    public Task ReceiveFlightDiscoveryUsed(int hostId, FlightDiscoveryUsed e);
    public Task ReceiveDistransUsed(int hostId, DistransUsed e);
    public Task ReceiveDiscardedTaken(int hostId, DiscardedTaken e);
    public Task ReceiveDiscardedSearchedAnnounced(int hostId, DiscardedSearchedAnnounced e);
    public Task ReceiveDiscardedSearched(int hostId, DiscardedSearched e);
    public Task ReceiveJuicePlayed(int hostId, JuicePlayed e);
    public Task ReceivePortableAntidoteUsed(int hostId, PortableAntidoteUsed e);
    public Task ReceiveBureaucracy(int hostId, Bureaucracy e);
    public Task ReceiveDiplomacy(int hostId, Diplomacy e);
    public Task ReceiveSkillAssigned(int hostId, SkillAssigned e);
    public Task ReceiveSwitchedSkilledLeader(int hostId, SwitchedSkilledLeader e);
    public Task ReceiveThought(int hostId, Thought e);
    public Task ReceiveThoughtAnswered(int hostId, ThoughtAnswered e);
    public Task ReceiveHMSAdvantageChosen(int hostId, HMSAdvantageChosen e);
    public Task ReceiveRetreat(int hostId, Retreat e);
    public Task ReceivePlanetology(int hostId, Planetology e);
    public Task ReceiveCaptured(int hostId, Captured e);
    public Task ReceiveNexusCardDrawn(int hostId, NexusCardDrawn e);
    public Task ReceiveTerrorPlanted(int hostId, TerrorPlanted e);
    public Task ReceiveTerrorRevealed(int hostId, TerrorRevealed e);
    public Task ReceiveDiscoveryRevealed(int hostId, DiscoveryRevealed e);
    public Task ReceiveAmbassadorPlaced(int hostId, AmbassadorPlaced e);
    public Task ReceiveAmbassadorActivated(int hostId, AmbassadorActivated e);
    public Task ReceiveExtortionPrevented(int hostId, ExtortionPrevented e);
    public Task ReceiveDiscarded(int hostId, Discarded e);
    public Task ReceiveAllianceByTerror(int hostId, AllianceByTerror e);
    public Task ReceiveNexusVoted(int hostId, NexusVoted e);
    public Task ReceiveAllianceByAmbassador(int hostId, AllianceByAmbassador e);
    public Task ReceiveLoserConcluded(int hostId, LoserConcluded e);
    public Task ReceivePerformCyanSetup(int hostId, PerformCyanSetup e);
    public Task ReceiveDivideResources(int hostId, DivideResources e);
    public Task ReceiveDivideResourcesAccepted(int hostId, DivideResourcesAccepted e);
    public Task ReceiveBattleClaimed(int hostId, BattleClaimed e);
    public Task ReceiveKarmaPinkDial(int hostId, KarmaPinkDial e);
    public Task ReceiveTraitorDiscarded(int hostId, TraitorDiscarded e);
    public Task ReceiveNexusPlayed(int hostId, NexusPlayed e);
    public Task ReceiveResourcesAudited(int hostId, ResourcesAudited e);
    public Task ReceiveRecruitsPlayed(int hostId, RecruitsPlayed e);

}