// /*
//  * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
//  * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
//  * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
//  * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//  * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
//  * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
// */

using System.Threading.Tasks;
using Treachery.Shared.Services;

namespace Treachery.Client;

public class GameClient : IGameClient
{
    private string ValidatedUsername { get; set; }
    private string ValidatedHashedPassword { get; set; }

    /*public Task<string> Login(string userName, string password)
    {
        
    }*/
    
    public Task ReceiveEstablishPlayers(int hostId, EstablishPlayers e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveEndPhase(int hostId, EndPhase e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveDonated(int hostId, Donated e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveResourcesTransferred(int hostId, ResourcesTransferred e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveFactionSelected(int hostId, FactionSelected e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveFactionTradeOffered(int hostId, FactionTradeOffered e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceivePerformSetup(int hostId, PerformSetup e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveCardsDetermined(int hostId, CardsDetermined e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceivePerformYellowSetup(int hostId, PerformYellowSetup e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveBluePrediction(int hostId, BluePrediction e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveCharityClaimed(int hostId, CharityClaimed e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceivePerformBluePlacement(int hostId, PerformBluePlacement e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveTraitorsSelected(int hostId, TraitorsSelected e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveStormSpellPlayed(int hostId, StormSpellPlayed e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveTestingStationUsed(int hostId, TestingStationUsed e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveTakeLosses(int hostId, TakeLosses e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveMetheorPlayed(int hostId, MetheorPlayed e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveYellowSentMonster(int hostId, YellowSentMonster e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveYellowRidesMonster(int hostId, YellowRidesMonster e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveAllianceOffered(int hostId, AllianceOffered e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveAllianceBroken(int hostId, AllianceBroken e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveBid(int hostId, Bid e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveRevival(int hostId, Revival e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveBlueBattleAnnouncement(int hostId, BlueBattleAnnouncement e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveShipment(int hostId, Shipment e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveOrangeDelay(int hostId, OrangeDelay e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveBlueAccompanies(int hostId, BlueAccompanies e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveBlueFlip(int hostId, BlueFlip e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveMove(int hostId, Move e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveCaravan(int hostId, Caravan e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveBattleInitiated(int hostId, BattleInitiated e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveBattle(int hostId, Battle e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveBattleRevision(int hostId, BattleRevision e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveTreacheryCalled(int hostId, TreacheryCalled e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveBattleConcluded(int hostId, BattleConcluded e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveClairvoyancePlayed(int hostId, ClairVoyancePlayed e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveClairvoyanceAnswered(int hostId, ClairVoyanceAnswered e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveRaiseDeadPlayed(int hostId, RaiseDeadPlayed e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveKarma(int hostId, Karma e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveKarmaFreeRevival(int hostId, KarmaFreeRevival e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveKarmaShipmentPrevention(int hostId, KarmaShipmentPrevention e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveKarmaRevivalPrevention(int hostId, KarmaRevivalPrevention e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveKarmaHandSwapInitiated(int hostId, KarmaHandSwapInitiated e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveKarmaHandSwap(int hostId, KarmaHandSwap e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveKarmaMonster(int hostId, KarmaMonster e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveKarmaWhiteBuy(int hostId, KarmaWhiteBuy e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveAllyPermission(int hostId, AllyPermission e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveMulliganPerformed(int hostId, MulliganPerformed e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveFaceDancerRevealed(int hostId, FaceDancerRevealed e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveFaceDanced(int hostId, FaceDanced e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveFaceDancerReplaced(int hostId, FaceDancerReplaced e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveSetIncreasedRevivalLimits(int hostId, SetIncreasedRevivalLimits e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveSetShipmentPermission(int hostId, SetShipmentPermission e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveReceivePurpleRevival(int hostId, RequestPurpleRevival e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveAcceptOrCancelPurpleRevival(int hostId, AcceptOrCancelPurpleRevival e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceivePerformHmsPlacement(int hostId, PerformHmsPlacement e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceivePerformHmsMovement(int hostId, PerformHmsMovement e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveKarmaHmsMovement(int hostId, KarmaHmsMovement e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveAmalPlayed(int hostId, AmalPlayed e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveGreyRemovedCardFromAuction(int hostId, GreyRemovedCardFromAuction e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveGreySelectedStartingCard(int hostId, GreySelectedStartingCard e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveGreySwappedCardOnBid(int hostId, GreySwappedCardOnBid e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveHarvesterPlayed(int hostId, HarvesterPlayed e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceivePoisonToothCancelled(int hostId, PoisonToothCancelled e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveReplacedCardWon(int hostId, ReplacedCardWon e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveThumperPlayed(int hostId, ThumperPlayed e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveVoice(int hostId, Voice e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceivePrescience(int hostId, Prescience e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveKarmaPrescience(int hostId, KarmaPrescience e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveRedBidSupport(int hostId, RedBidSupport e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveDealOffered(int hostId, DealOffered e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveDealAccepted(int hostId, DealAccepted e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveDiscoveryEntered(int hostId, DiscoveryEntered e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveStormDialled(int hostId, StormDialled e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveHideSecrets(int hostId, HideSecrets e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceivePlayerReplaced(int hostId, PlayerReplaced e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveBrownDiscarded(int hostId, BrownDiscarded e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveRedDiscarded(int hostId, RedDiscarded e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveBrownEconomics(int hostId, BrownEconomics e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveCardTraded(int hostId, CardTraded e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveKarmaBrownDiscard(int hostId, KarmaBrownDiscard e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveAuditCancelled(int hostId, AuditCancelled e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveAudited(int hostId, Audited e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveBrownMovePrevention(int hostId, BrownMovePrevention e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveBrownKarmaPrevention(int hostId, BrownKarmaPrevention e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveBrownExtraMove(int hostId, BrownExtraMove e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveBrownFreeRevivalPrevention(int hostId, BrownFreeRevivalPrevention e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveBrownRemoveForce(int hostId, BrownRemoveForce e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveWhiteAnnouncesBlackMarket(int hostId, WhiteAnnouncesBlackMarket e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveBlackMarketBid(int hostId, BlackMarketBid e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveWhiteAnnouncesAuction(int hostId, WhiteAnnouncesAuction e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveWhiteSpecifiesAuction(int hostId, WhiteSpecifiesAuction e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveWhiteKeepsUnsoldCard(int hostId, WhiteKeepsUnsoldCard e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveWhiteRevealedNoField(int hostId, WhiteRevealedNoField e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveWhiteGaveCard(int hostId, WhiteGaveCard e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveCardGiven(int hostId, CardGiven e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveRockWasMelted(int hostId, RockWasMelted e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveResidualPlayed(int hostId, ResidualPlayed e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveFlightUsed(int hostId, FlightUsed e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveFlightDiscoveryUsed(int hostId, FlightDiscoveryUsed e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveDistransUsed(int hostId, DistransUsed e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveDiscardedTaken(int hostId, DiscardedTaken e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveDiscardedSearchedAnnounced(int hostId, DiscardedSearchedAnnounced e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveDiscardedSearched(int hostId, DiscardedSearched e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveJuicePlayed(int hostId, JuicePlayed e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceivePortableAntidoteUsed(int hostId, PortableAntidoteUsed e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveBureaucracy(int hostId, Bureaucracy e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveDiplomacy(int hostId, Diplomacy e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveSkillAssigned(int hostId, SkillAssigned e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveSwitchedSkilledLeader(int hostId, SwitchedSkilledLeader e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveThought(int hostId, Thought e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveThoughtAnswered(int hostId, ThoughtAnswered e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveHMSAdvantageChosen(int hostId, HMSAdvantageChosen e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveRetreat(int hostId, Retreat e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceivePlanetology(int hostId, Planetology e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveCaptured(int hostId, Captured e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveNexusCardDrawn(int hostId, NexusCardDrawn e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveTerrorPlanted(int hostId, TerrorPlanted e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveTerrorRevealed(int hostId, TerrorRevealed e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveDiscoveryRevealed(int hostId, DiscoveryRevealed e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveAmbassadorPlaced(int hostId, AmbassadorPlaced e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveAmbassadorActivated(int hostId, AmbassadorActivated e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveExtortionPrevented(int hostId, ExtortionPrevented e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveDiscarded(int hostId, Discarded e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveAllianceByTerror(int hostId, AllianceByTerror e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveNexusVoted(int hostId, NexusVoted e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveAllianceByAmbassador(int hostId, AllianceByAmbassador e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveLoserConcluded(int hostId, LoserConcluded e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceivePerformCyanSetup(int hostId, PerformCyanSetup e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveDivideResources(int hostId, DivideResources e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveDivideResourcesAccepted(int hostId, DivideResourcesAccepted e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveBattleClaimed(int hostId, BattleClaimed e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveKarmaPinkDial(int hostId, KarmaPinkDial e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveTraitorDiscarded(int hostId, TraitorDiscarded e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveNexusPlayed(int hostId, NexusPlayed e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveResourcesAudited(int hostId, ResourcesAudited e)
    {
        throw new System.NotImplementedException();
    }

    public Task ReceiveRecruitsPlayed(int hostId, RecruitsPlayed e)
    {
        throw new System.NotImplementedException();
    }
}