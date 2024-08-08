using System;
using System.Threading.Tasks;

namespace Treachery.Shared;

public interface IGameHub
{
    //Authentication
    
    Task<Result<LoginInfo>> RequestCreateUser(string userName, string hashedPassword, string email, string playerName);
    Task<Result<LoginInfo>> RequestLogin(int version, string userName, string hashedPassword);
    Task<VoidResult> RequestPasswordReset(string email);
    Task<Result<LoginInfo>> RequestSetPassword(string userName, string passwordResetToken, string newHashedPassword);
    Task<Result<LoginInfo>> GetLoginInfo(string userToken);
    Task<Result<LoginInfo>> RequestUpdateUserInfo(string userToken, string hashedPassword, string playerName, string email);

    //Game Management
    
    Task<Result<GameInitInfo>> RequestCreateGame(string userToken, string hashedPassword, string stateData, string skinData);
    Task<VoidResult> RequestCloseGame(string userToken, string gameId);
    Task<Result<GameInitInfo>> RequestJoinGame(string userToken, string gameId, string hashedPassword, int seat);
    Task<Result<GameInitInfo>> RequestObserveGame(string userToken, string gameId, string hashedPassword);
    Task<Result<GameInitInfo>> RequestReconnectGame(string userToken, string gameToken);
    Task<VoidResult> RequestSetOrUnsetHost(string userToken, string gameToken, int userId);
    Task<VoidResult> RequestOpenOrCloseSeat(string userToken, string gameToken, int seat);
    Task<VoidResult> RequestLeaveGame(string userToken, string gameToken);
    Task<VoidResult> RequestKick(string userToken, string gameToken, int userId);
    Task<VoidResult> RequestLoadGame(string userToken, string hashedPassword, string state, string skin);
    Task<VoidResult> RequestSetSkin(string userToken, string gameToken, string skin);
    Task<VoidResult> RequestUndo(string userToken, string gameToken, int untilEventNr);
    Task<Result<GameInitInfo>> RequestGameState(string userToken, string gameToken);
    Task<VoidResult> RequestPauseBots(string userToken, string gameToken);
    Task<Result<List<GameInfo>>> RequestRunningGames(string userToken);
    Task<VoidResult> RequestRegisterHeartbeat(string userToken);
    
    //Game Events
    Task<VoidResult> RequestChangeSettings(string userToken, string gameToken, ChangeSettings e);
    Task<VoidResult> SetTimer(string userToken, string gameToken, int value);
    Task<VoidResult> RequestEstablishPlayers(string userToken, string gameToken, EstablishPlayers e);
    Task<VoidResult> RequestEndPhase(string userToken, string gameToken, EndPhase e);
    Task<VoidResult> RequestDonated(string userToken, string gameToken, Donated e);
    Task<VoidResult> RequestResourcesTransferred(string userToken, string gameToken, ResourcesTransferred e);
    Task<VoidResult> RequestFactionSelected(string userToken, string gameToken, FactionSelected e);
    Task<VoidResult> RequestFactionTradeOffered(string userToken, string gameToken, FactionTradeOffered e);
    Task<VoidResult> RequestPerformSetup(string userToken, string gameToken, PerformSetup e);
    Task<VoidResult> RequestCardsDetermined(string userToken, string gameToken, CardsDetermined e);
    Task<VoidResult> RequestPerformYellowSetup(string userToken, string gameToken, PerformYellowSetup e);
    Task<VoidResult> RequestBluePrediction(string userToken, string gameToken, BluePrediction e);
    Task<VoidResult> RequestCharityClaimed(string userToken, string gameToken, CharityClaimed e);
    Task<VoidResult> RequestPerformBluePlacement(string userToken, string gameToken, PerformBluePlacement e);
    Task<VoidResult> RequestTraitorsSelected(string userToken, string gameToken, TraitorsSelected e);
    Task<VoidResult> RequestStormSpellPlayed(string userToken, string gameToken, StormSpellPlayed e);
    Task<VoidResult> RequestTestingStationUsed(string userToken, string gameToken, TestingStationUsed e);
    Task<VoidResult> RequestTakeLosses(string userToken, string gameToken, TakeLosses e);
    Task<VoidResult> RequestMetheorPlayed(string userToken, string gameToken, MetheorPlayed e);
    Task<VoidResult> RequestYellowSentMonster(string userToken, string gameToken, YellowSentMonster e);
    Task<VoidResult> RequestYellowRidesMonster(string userToken, string gameToken, YellowRidesMonster e);
    Task<VoidResult> RequestAllianceOffered(string userToken, string gameToken, AllianceOffered e);
    Task<VoidResult> RequestAllianceBroken(string userToken, string gameToken, AllianceBroken e);
    Task<VoidResult> RequestBid(string userToken, string gameToken, Bid e);
    Task<VoidResult> RequestRevival(string userToken, string gameToken, Revival e);
    Task<VoidResult> RequestBlueBattleAnnouncement(string userToken, string gameToken, BlueBattleAnnouncement e);
    Task<VoidResult> RequestShipment(string userToken, string gameToken, Shipment e);
    Task<VoidResult> RequestOrangeDelay(string userToken, string gameToken, OrangeDelay e);
    Task<VoidResult> RequestBlueAccompanies(string userToken, string gameToken, BlueAccompanies e);
    Task<VoidResult> RequestBlueFlip(string userToken, string gameToken, BlueFlip e);
    Task<VoidResult> RequestMove(string userToken, string gameToken, Move e);
    Task<VoidResult> RequestCaravan(string userToken, string gameToken, Caravan e);
    Task<VoidResult> RequestBattleInitiated(string userToken, string gameToken, BattleInitiated e);
    Task<VoidResult> RequestBattle(string userToken, string gameToken, Battle e);
    Task<VoidResult> RequestBattleRevision(string userToken, string gameToken, BattleRevision e);
    Task<VoidResult> RequestTreacheryCalled(string userToken, string gameToken, TreacheryCalled e);
    Task<VoidResult> RequestBattleConcluded(string userToken, string gameToken, BattleConcluded e);
    Task<VoidResult> RequestClairvoyancePlayed(string userToken, string gameToken, ClairVoyancePlayed e);
    Task<VoidResult> RequestClairvoyanceAnswered(string userToken, string gameToken, ClairVoyanceAnswered e);
    Task<VoidResult> RequestRaiseDeadPlayed(string userToken, string gameToken, RaiseDeadPlayed e);
    Task<VoidResult> RequestKarma(string userToken, string gameToken, Karma e);
    Task<VoidResult> RequestKarmaFreeRevival(string userToken, string gameToken, KarmaFreeRevival e);
    Task<VoidResult> RequestKarmaShipmentPrevention(string userToken, string gameToken, KarmaShipmentPrevention e);
    Task<VoidResult> RequestKarmaRevivalPrevention(string userToken, string gameToken, KarmaRevivalPrevention e);
    Task<VoidResult> RequestKarmaHandSwapInitiated(string userToken, string gameToken, KarmaHandSwapInitiated e);
    Task<VoidResult> RequestKarmaHandSwap(string userToken, string gameToken, KarmaHandSwap e);
    Task<VoidResult> RequestKarmaMonster(string userToken, string gameToken, KarmaMonster e);
    Task<VoidResult> RequestKarmaWhiteBuy(string userToken, string gameToken, KarmaWhiteBuy e);
    Task<VoidResult> RequestAllyPermission(string userToken, string gameToken, AllyPermission e);
    Task<VoidResult> RequestMulliganPerformed(string userToken, string gameToken, MulliganPerformed e);
    Task<VoidResult> RequestFaceDancerRevealed(string userToken, string gameToken, FaceDancerRevealed e);
    Task<VoidResult> RequestFaceDanced(string userToken, string gameToken, FaceDanced e);
    Task<VoidResult> RequestFaceDancerReplaced(string userToken, string gameToken, FaceDancerReplaced e);
    Task<VoidResult> RequestSetIncreasedRevivalLimits(string userToken, string gameToken, SetIncreasedRevivalLimits e);
    Task<VoidResult> RequestSetShipmentPermission(string userToken, string gameToken, SetShipmentPermission e);
    Task<VoidResult> RequestRequestPurpleRevival(string userToken, string gameToken, RequestPurpleRevival e);
    Task<VoidResult> RequestAcceptOrCancelPurpleRevival(string userToken, string gameToken, AcceptOrCancelPurpleRevival e);
    Task<VoidResult> RequestPerformHmsPlacement(string userToken, string gameToken, PerformHmsPlacement e);
    Task<VoidResult> RequestPerformHmsMovement(string userToken, string gameToken, PerformHmsMovement e);
    Task<VoidResult> RequestKarmaHmsMovement(string userToken, string gameToken, KarmaHmsMovement e);
    Task<VoidResult> RequestAmalPlayed(string userToken, string gameToken, AmalPlayed e);
    Task<VoidResult> RequestGreyRemovedCardFromAuction(string userToken, string gameToken, GreyRemovedCardFromAuction e);
    Task<VoidResult> RequestGreySelectedStartingCard(string userToken, string gameToken, GreySelectedStartingCard e);
    Task<VoidResult> RequestGreySwappedCardOnBid(string userToken, string gameToken, GreySwappedCardOnBid e);
    Task<VoidResult> RequestHarvesterPlayed(string userToken, string gameToken, HarvesterPlayed e);
    Task<VoidResult> RequestPoisonToothCancelled(string userToken, string gameToken, PoisonToothCancelled e);
    Task<VoidResult> RequestReplacedCardWon(string userToken, string gameToken, ReplacedCardWon e);
    Task<VoidResult> RequestThumperPlayed(string userToken, string gameToken, ThumperPlayed e);
    Task<VoidResult> RequestVoice(string userToken, string gameToken, Voice e);
    Task<VoidResult> RequestPrescience(string userToken, string gameToken, Prescience e);
    Task<VoidResult> RequestKarmaPrescience(string userToken, string gameToken, KarmaPrescience e);
    Task<VoidResult> RequestRedBidSupport(string userToken, string gameToken, RedBidSupport e);
    Task<VoidResult> RequestDealOffered(string userToken, string gameToken, DealOffered e);
    Task<VoidResult> RequestDealAccepted(string userToken, string gameToken, DealAccepted e);
    Task<VoidResult> RequestDiscoveryEntered(string userToken, string gameToken, DiscoveryEntered e);
    Task<VoidResult> RequestStormDialled(string userToken, string gameToken, StormDialled e);
    Task<VoidResult> RequestHideSecrets(string userToken, string gameToken, HideSecrets e);
    Task<VoidResult> RequestPlayerReplaced(string userToken, string gameToken, PlayerReplaced e);
    Task<VoidResult> RequestBrownDiscarded(string userToken, string gameToken, BrownDiscarded e);
    Task<VoidResult> RequestRedDiscarded(string userToken, string gameToken, RedDiscarded e);
    Task<VoidResult> RequestBrownEconomics(string userToken, string gameToken, BrownEconomics e);
    Task<VoidResult> RequestCardTraded(string userToken, string gameToken, CardTraded e);
    Task<VoidResult> RequestKarmaBrownDiscard(string userToken, string gameToken, KarmaBrownDiscard e);
    Task<VoidResult> RequestAuditCancelled(string userToken, string gameToken, AuditCancelled e);
    Task<VoidResult> RequestAudited(string userToken, string gameToken, Audited e);
    Task<VoidResult> RequestBrownMovePrevention(string userToken, string gameToken, BrownMovePrevention e);
    Task<VoidResult> RequestBrownKarmaPrevention(string userToken, string gameToken, BrownKarmaPrevention e);
    Task<VoidResult> RequestBrownExtraMove(string userToken, string gameToken, BrownExtraMove e);
    Task<VoidResult> RequestBrownFreeRevivalPrevention(string userToken, string gameToken, BrownFreeRevivalPrevention e);
    Task<VoidResult> RequestBrownRemoveForce(string userToken, string gameToken, BrownRemoveForce e);
    Task<VoidResult> RequestWhiteAnnouncesBlackMarket(string userToken, string gameToken, WhiteAnnouncesBlackMarket e);
    Task<VoidResult> RequestBlackMarketBid(string userToken, string gameToken, BlackMarketBid e);
    Task<VoidResult> RequestWhiteAnnouncesAuction(string userToken, string gameToken, WhiteAnnouncesAuction e);
    Task<VoidResult> RequestWhiteSpecifiesAuction(string userToken, string gameToken, WhiteSpecifiesAuction e);
    Task<VoidResult> RequestWhiteKeepsUnsoldCard(string userToken, string gameToken, WhiteKeepsUnsoldCard e);
    Task<VoidResult> RequestWhiteRevealedNoField(string userToken, string gameToken, WhiteRevealedNoField e);
    Task<VoidResult> RequestWhiteGaveCard(string userToken, string gameToken, WhiteGaveCard e);
    Task<VoidResult> RequestCardGiven(string userToken, string gameToken, CardGiven e);
    Task<VoidResult> RequestRockWasMelted(string userToken, string gameToken, RockWasMelted e);
    Task<VoidResult> RequestResidualPlayed(string userToken, string gameToken, ResidualPlayed e);
    Task<VoidResult> RequestFlightUsed(string userToken, string gameToken, FlightUsed e);
    Task<VoidResult> RequestFlightDiscoveryUsed(string userToken, string gameToken, FlightDiscoveryUsed e);
    Task<VoidResult> RequestDistransUsed(string userToken, string gameToken, DistransUsed e);
    Task<VoidResult> RequestDiscardedTaken(string userToken, string gameToken, DiscardedTaken e);
    Task<VoidResult> RequestDiscardedSearchedAnnounced(string userToken, string gameToken, DiscardedSearchedAnnounced e);
    Task<VoidResult> RequestDiscardedSearched(string userToken, string gameToken, DiscardedSearched e);
    Task<VoidResult> RequestJuicePlayed(string userToken, string gameToken, JuicePlayed e);
    Task<VoidResult> RequestPortableAntidoteUsed(string userToken, string gameToken, PortableAntidoteUsed e);
    Task<VoidResult> RequestBureaucracy(string userToken, string gameToken, Bureaucracy e);
    Task<VoidResult> RequestDiplomacy(string userToken, string gameToken, Diplomacy e);
    Task<VoidResult> RequestSkillAssigned(string userToken, string gameToken, SkillAssigned e);
    Task<VoidResult> RequestSwitchedSkilledLeader(string userToken, string gameToken, SwitchedSkilledLeader e);
    Task<VoidResult> RequestThought(string userToken, string gameToken, Thought e);
    Task<VoidResult> RequestThoughtAnswered(string userToken, string gameToken, ThoughtAnswered e);
    Task<VoidResult> RequestHmsAdvantageChosen(string userToken, string gameToken, HMSAdvantageChosen e);
    Task<VoidResult> RequestRetreat(string userToken, string gameToken, Retreat e);
    Task<VoidResult> RequestPlanetology(string userToken, string gameToken, Planetology e);
    Task<VoidResult> RequestCaptured(string userToken, string gameToken, Captured e);
    Task<VoidResult> RequestNexusCardDrawn(string userToken, string gameToken, NexusCardDrawn e);
    Task<VoidResult> RequestTerrorPlanted(string userToken, string gameToken, TerrorPlanted e);
    Task<VoidResult> RequestTerrorRevealed(string userToken, string gameToken, TerrorRevealed e);
    Task<VoidResult> RequestDiscoveryRevealed(string userToken, string gameToken, DiscoveryRevealed e);
    Task<VoidResult> RequestAmbassadorPlaced(string userToken, string gameToken, AmbassadorPlaced e);
    Task<VoidResult> RequestAmbassadorActivated(string userToken, string gameToken, AmbassadorActivated e);
    Task<VoidResult> RequestExtortionPrevented(string userToken, string gameToken, ExtortionPrevented e);
    Task<VoidResult> RequestDiscarded(string userToken, string gameToken, Discarded e);
    Task<VoidResult> RequestAllianceByTerror(string userToken, string gameToken, AllianceByTerror e);
    Task<VoidResult> RequestNexusVoted(string userToken, string gameToken, NexusVoted e);
    Task<VoidResult> RequestAllianceByAmbassador(string userToken, string gameToken, AllianceByAmbassador e);
    Task<VoidResult> RequestLoserConcluded(string userToken, string gameToken, LoserConcluded e);
    Task<VoidResult> RequestPerformCyanSetup(string userToken, string gameToken, PerformCyanSetup e);
    Task<VoidResult> RequestDivideResources(string userToken, string gameToken, DivideResources e);
    Task<VoidResult> RequestDivideResourcesAccepted(string userToken, string gameToken, DivideResourcesAccepted e);
    Task<VoidResult> RequestBattleClaimed(string userToken, string gameToken, BattleClaimed e);
    Task<VoidResult> RequestKarmaPinkDial(string userToken, string gameToken, KarmaPinkDial e);
    Task<VoidResult> RequestTraitorDiscarded(string userToken, string gameToken, TraitorDiscarded e);
    Task<VoidResult> RequestNexusPlayed(string userToken, string gameToken, NexusPlayed e);
    Task<VoidResult> RequestResourcesAudited(string userToken, string gameToken, ResourcesAudited e);
    Task<VoidResult> RequestRecruitsPlayed(string userToken, string gameToken, RecruitsPlayed e);
    
    //Chat
    Task<VoidResult> SendChatMessage(string userToken, string gameToken, GameChatMessage e);
    Task<VoidResult> SendGlobalChatMessage(string userToken, GlobalChatMessage message);

    //Other
    Result<ServerInfo> Connect();
    
    //Admin
    Task<Result<string>> AdminUpdateMaintenance(string hashedPassword, DateTimeOffset maintenanceDate);
    Task<Result<string>> AdminPersistState(string hashedPassword);
    Task<Result<string>> AdminRestoreState(string hashedPassword);
    Task<Result<string>> AdminCloseGame(string hashedPassword, string gameId);
}