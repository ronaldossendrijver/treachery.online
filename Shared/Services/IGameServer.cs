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

public interface IGameServer
{
    /*
     * Technical events
     */
    
    /// <summary>
    /// Creating a game returns the id of the game
    /// </summary>
    /// <param name="hostId"></param>
    /// <returns>id of the game</returns>
    public Task<int> RequestCreateGame(int hostId);
    
    /// <summary>
    /// Successfully joining a game returns the current state of the game 
    /// </summary>
    /// <param name="gameId"></param>
    /// <returns></returns>
    public Task<GameEvent[]> RequestJoinGame(int gameId);
    
    /*
     * In-game events
     */
    
    public Task<string> RequestEstablishPlayers(int hostId, EstablishPlayers e);
    public Task<string> RequestEndPhase(int hostId, EndPhase e);
    public Task<string> RequestDonated(int hostId, Donated e);
    public Task<string> RequestResourcesTransferred(int hostId, ResourcesTransferred e);
    public Task<string> RequestFactionSelected(int hostId, FactionSelected e);
    public Task<string> RequestFactionTradeOffered(int hostId, FactionTradeOffered e);
    public Task<string> RequestPerformSetup(int hostId, PerformSetup e);
    public Task<string> RequestCardsDetermined(int hostId, CardsDetermined e);
    public Task<string> RequestPerformYellowSetup(int hostId, PerformYellowSetup e);
    public Task<string> RequestBluePrediction(int hostId, BluePrediction e);
    public Task<string> RequestCharityClaimed(int hostId, CharityClaimed e);
    public Task<string> RequestPerformBluePlacement(int hostId, PerformBluePlacement e);
    public Task<string> RequestTraitorsSelected(int hostId, TraitorsSelected e);
    public Task<string> RequestStormSpellPlayed(int hostId, StormSpellPlayed e);
    public Task<string> RequestTestingStationUsed(int hostId, TestingStationUsed e);
    public Task<string> RequestTakeLosses(int hostId, TakeLosses e);
    public Task<string> RequestMetheorPlayed(int hostId, MetheorPlayed e);
    public Task<string> RequestYellowSentMonster(int hostId, YellowSentMonster e);
    public Task<string> RequestYellowRidesMonster(int hostId, YellowRidesMonster e);
    public Task<string> RequestAllianceOffered(int hostId, AllianceOffered e);
    public Task<string> RequestAllianceBroken(int hostId, AllianceBroken e);
    public Task<string> RequestBid(int hostId, Bid e);
    public Task<string> RequestRevival(int hostId, Revival e);
    public Task<string> RequestBlueBattleAnnouncement(int hostId, BlueBattleAnnouncement e);
    public Task<string> RequestShipment(int hostId, Shipment e);
    public Task<string> RequestOrangeDelay(int hostId, OrangeDelay e);
    public Task<string> RequestBlueAccompanies(int hostId, BlueAccompanies e);
    public Task<string> RequestBlueFlip(int hostId, BlueFlip e);
    public Task<string> RequestMove(int hostId, Move e);
    public Task<string> RequestCaravan(int hostId, Caravan e);
    public Task<string> RequestBattleInitiated(int hostId, BattleInitiated e);
    public Task<string> RequestBattle(int hostId, Battle e);
    public Task<string> RequestBattleRevision(int hostId, BattleRevision e);
    public Task<string> RequestTreacheryCalled(int hostId, TreacheryCalled e);
    public Task<string> RequestBattleConcluded(int hostId, BattleConcluded e);
    public Task<string> RequestClairvoyancePlayed(int hostId, ClairVoyancePlayed e);
    public Task<string> RequestClairvoyanceAnswered(int hostId, ClairVoyanceAnswered e);
    public Task<string> RequestRaiseDeadPlayed(int hostId, RaiseDeadPlayed e);
    public Task<string> RequestKarma(int hostId, Karma e);
    public Task<string> RequestKarmaFreeRevival(int hostId, KarmaFreeRevival e);
    public Task<string> RequestKarmaShipmentPrevention(int hostId, KarmaShipmentPrevention e);
    public Task<string> RequestKarmaRevivalPrevention(int hostId, KarmaRevivalPrevention e);
    public Task<string> RequestKarmaHandSwapInitiated(int hostId, KarmaHandSwapInitiated e);
    public Task<string> RequestKarmaHandSwap(int hostId, KarmaHandSwap e);
    public Task<string> RequestKarmaMonster(int hostId, KarmaMonster e);
    public Task<string> RequestKarmaWhiteBuy(int hostId, KarmaWhiteBuy e);
    public Task<string> RequestAllyPermission(int hostId, AllyPermission e);
    public Task<string> RequestMulliganPerformed(int hostId, MulliganPerformed e);
    public Task<string> RequestFaceDancerRevealed(int hostId, FaceDancerRevealed e);
    public Task<string> RequestFaceDanced(int hostId, FaceDanced e);
    public Task<string> RequestFaceDancerReplaced(int hostId, FaceDancerReplaced e);
    public Task<string> RequestSetIncreasedRevivalLimits(int hostId, SetIncreasedRevivalLimits e);
    public Task<string> RequestSetShipmentPermission(int hostId, SetShipmentPermission e);
    public Task<string> RequestReceivePurpleRevival(int hostId, RequestPurpleRevival e);
    public Task<string> RequestAcceptOrCancelPurpleRevival(int hostId, AcceptOrCancelPurpleRevival e);
    public Task<string> RequestPerformHmsPlacement(int hostId, PerformHmsPlacement e);
    public Task<string> RequestPerformHmsMovement(int hostId, PerformHmsMovement e);
    public Task<string> RequestKarmaHmsMovement(int hostId, KarmaHmsMovement e);
    public Task<string> RequestAmalPlayed(int hostId, AmalPlayed e);
    public Task<string> RequestGreyRemovedCardFromAuction(int hostId, GreyRemovedCardFromAuction e);
    public Task<string> RequestGreySelectedStartingCard(int hostId, GreySelectedStartingCard e);
    public Task<string> RequestGreySwappedCardOnBid(int hostId, GreySwappedCardOnBid e);
    public Task<string> RequestHarvesterPlayed(int hostId, HarvesterPlayed e);
    public Task<string> RequestPoisonToothCancelled(int hostId, PoisonToothCancelled e);
    public Task<string> RequestReplacedCardWon(int hostId, ReplacedCardWon e);
    public Task<string> RequestThumperPlayed(int hostId, ThumperPlayed e);
    public Task<string> RequestVoice(int hostId, Voice e);
    public Task<string> RequestPrescience(int hostId, Prescience e);
    public Task<string> RequestKarmaPrescience(int hostId, KarmaPrescience e);
    public Task<string> RequestRedBidSupport(int hostId, RedBidSupport e);
    public Task<string> RequestDealOffered(int hostId, DealOffered e);
    public Task<string> RequestDealAccepted(int hostId, DealAccepted e);
    public Task<string> RequestDiscoveryEntered(int hostId, DiscoveryEntered e);
    public Task<string> RequestStormDialled(int hostId, StormDialled e);
    public Task<string> RequestHideSecrets(int hostId, HideSecrets e);
    public Task<string> RequestPlayerReplaced(int hostId, PlayerReplaced e);
    public Task<string> RequestBrownDiscarded(int hostId, BrownDiscarded e);
    public Task<string> RequestRedDiscarded(int hostId, RedDiscarded e);
    public Task<string> RequestBrownEconomics(int hostId, BrownEconomics e);
    public Task<string> RequestCardTraded(int hostId, CardTraded e);
    public Task<string> RequestKarmaBrownDiscard(int hostId, KarmaBrownDiscard e);
    public Task<string> RequestAuditCancelled(int hostId, AuditCancelled e);
    public Task<string> RequestAudited(int hostId, Audited e);
    public Task<string> RequestBrownMovePrevention(int hostId, BrownMovePrevention e);
    public Task<string> RequestBrownKarmaPrevention(int hostId, BrownKarmaPrevention e);
    public Task<string> RequestBrownExtraMove(int hostId, BrownExtraMove e);
    public Task<string> RequestBrownFreeRevivalPrevention(int hostId, BrownFreeRevivalPrevention e);
    public Task<string> RequestBrownRemoveForce(int hostId, BrownRemoveForce e);
    public Task<string> RequestWhiteAnnouncesBlackMarket(int hostId, WhiteAnnouncesBlackMarket e);
    public Task<string> RequestBlackMarketBid(int hostId, BlackMarketBid e);
    public Task<string> RequestWhiteAnnouncesAuction(int hostId, WhiteAnnouncesAuction e);
    public Task<string> RequestWhiteSpecifiesAuction(int hostId, WhiteSpecifiesAuction e);
    public Task<string> RequestWhiteKeepsUnsoldCard(int hostId, WhiteKeepsUnsoldCard e);
    public Task<string> RequestWhiteRevealedNoField(int hostId, WhiteRevealedNoField e);
    public Task<string> RequestWhiteGaveCard(int hostId, WhiteGaveCard e);
    public Task<string> RequestCardGiven(int hostId, CardGiven e);
    public Task<string> RequestRockWasMelted(int hostId, RockWasMelted e);
    public Task<string> RequestResidualPlayed(int hostId, ResidualPlayed e);
    public Task<string> RequestFlightUsed(int hostId, FlightUsed e);
    public Task<string> RequestFlightDiscoveryUsed(int hostId, FlightDiscoveryUsed e);
    public Task<string> RequestDistransUsed(int hostId, DistransUsed e);
    public Task<string> RequestDiscardedTaken(int hostId, DiscardedTaken e);
    public Task<string> RequestDiscardedSearchedAnnounced(int hostId, DiscardedSearchedAnnounced e);
    public Task<string> RequestDiscardedSearched(int hostId, DiscardedSearched e);
    public Task<string> RequestJuicePlayed(int hostId, JuicePlayed e);
    public Task<string> RequestPortableAntidoteUsed(int hostId, PortableAntidoteUsed e);
    public Task<string> RequestBureaucracy(int hostId, Bureaucracy e);
    public Task<string> RequestDiplomacy(int hostId, Diplomacy e);
    public Task<string> RequestSkillAssigned(int hostId, SkillAssigned e);
    public Task<string> RequestSwitchedSkilledLeader(int hostId, SwitchedSkilledLeader e);
    public Task<string> RequestThought(int hostId, Thought e);
    public Task<string> RequestThoughtAnswered(int hostId, ThoughtAnswered e);
    public Task<string> RequestHMSAdvantageChosen(int hostId, HMSAdvantageChosen e);
    public Task<string> RequestRetreat(int hostId, Retreat e);
    public Task<string> RequestPlanetology(int hostId, Planetology e);
    public Task<string> RequestCaptured(int hostId, Captured e);
    public Task<string> RequestNexusCardDrawn(int hostId, NexusCardDrawn e);
    public Task<string> RequestTerrorPlanted(int hostId, TerrorPlanted e);
    public Task<string> RequestTerrorRevealed(int hostId, TerrorRevealed e);
    public Task<string> RequestDiscoveryRevealed(int hostId, DiscoveryRevealed e);
    public Task<string> RequestAmbassadorPlaced(int hostId, AmbassadorPlaced e);
    public Task<string> RequestAmbassadorActivated(int hostId, AmbassadorActivated e);
    public Task<string> RequestExtortionPrevented(int hostId, ExtortionPrevented e);
    public Task<string> RequestDiscarded(int hostId, Discarded e);
    public Task<string> RequestAllianceByTerror(int hostId, AllianceByTerror e);
    public Task<string> RequestNexusVoted(int hostId, NexusVoted e);
    public Task<string> RequestAllianceByAmbassador(int hostId, AllianceByAmbassador e);
    public Task<string> RequestLoserConcluded(int hostId, LoserConcluded e);
    public Task<string> RequestPerformCyanSetup(int hostId, PerformCyanSetup e);
    public Task<string> RequestDivideResources(int hostId, DivideResources e);
    public Task<string> RequestDivideResourcesAccepted(int hostId, DivideResourcesAccepted e);
    public Task<string> RequestBattleClaimed(int hostId, BattleClaimed e);
    public Task<string> RequestKarmaPinkDial(int hostId, KarmaPinkDial e);
    public Task<string> RequestTraitorDiscarded(int hostId, TraitorDiscarded e);
    public Task<string> RequestNexusPlayed(int hostId, NexusPlayed e);
    public Task<string> RequestResourcesAudited(int hostId, ResourcesAudited e);
    public Task<string> RequestRecruitsPlayed(int hostId, RecruitsPlayed e);

}