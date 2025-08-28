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
    Task<Result<ServerStatus>> RequestSetUserStatus(string userToken, UserStatus status);

    //Game Management
    
    Task<Result<GameInitInfo>> RequestCreateGame(string name, string userToken, string hashedPassword, string stateData, string skinData);
    Task<VoidResult> RequestUpdateSettings(string userToken, string gameId, GameSettings settings);
    Task<Result<ServerStatus>> RequestCloseGame(string userToken, string gameId);
    Task<Result<GameInitInfo>> RequestJoinGame(string userToken, string gameId, string hashedPassword, int seat);
    Task<Result<GameInitInfo>> RequestObserveGame(string userToken, string gameId, string hashedPassword);
    Task<Result<GameInitInfo>> RequestReconnectGame(string userToken, string gameId);
    Task<VoidResult> RequestSetOrUnsetHost(string userToken, string gameId, int userId);
    Task<VoidResult> RequestOpenOrCloseSeat(string userToken, string gameId, int seat);
    Task<Result<ServerStatus>> RequestLeaveGame(string userToken, string gameId);
    Task<VoidResult> RequestKick(string userToken, string gameId, int userId);
    Task<VoidResult> RequestLoadGame(string userToken, string hashedPassword, string state, string skin);
    Task<VoidResult> RequestSetSkin(string userToken, string gameId, string skin);
    Task<VoidResult> RequestUndo(string userToken, string gameId, int untilEventNr);
    Task<Result<GameInitInfo>> RequestGameState(string userToken, string gameId);
    Task<VoidResult> RequestSetBotSpeed(string userToken, string gameId, int speed);
    Task<Result<ServerStatus>> RequestHeartbeat(string userToken, GameListScope scope);
    Task<VoidResult> RequestAssignSeats(string userToken, string gameId, Dictionary<int, int> assignment);
    Task<Result<ServerStatus>> RequestScheduleGame(string userToken,  
        DateTimeOffset dateTime, Ruleset? ruleset, int? numberOfPlayers, int? maximumTurns, 
        List<Faction> allowedFactionsInPlay, bool asyncPlay);
    Task<Result<ServerStatus>> RequestCancelGame(string userToken, string scheduledGameId);
    Task<Result<ServerStatus>> RequestSubscribeGame(string userToken, string scheduledGameId, SubscriptionType subscription);
    
    //Game Events
    Task<VoidResult> SetTimer(string userToken, string gameId, int value);
    Task<VoidResult> RequestEstablishPlayers(string userToken, string gameId, EstablishPlayers e);
    Task<VoidResult> RequestEndPhase(string userToken, string gameId, EndPhase e);
    Task<VoidResult> RequestDonated(string userToken, string gameId, Donated e);
    Task<VoidResult> RequestResourcesTransferred(string userToken, string gameId, ResourcesTransferred e);
    Task<VoidResult> RequestFactionSelected(string userToken, string gameId, FactionSelected e);
    Task<VoidResult> RequestFactionTradeOffered(string userToken, string gameId, FactionTradeOffered e);
    Task<VoidResult> RequestPerformSetup(string userToken, string gameId, PerformSetup e);
    Task<VoidResult> RequestCardsDetermined(string userToken, string gameId, CardsDetermined e);
    Task<VoidResult> RequestPerformYellowSetup(string userToken, string gameId, PerformYellowSetup e);
    Task<VoidResult> RequestBluePrediction(string userToken, string gameId, BluePrediction e);
    Task<VoidResult> RequestCharityClaimed(string userToken, string gameId, CharityClaimed e);
    Task<VoidResult> RequestPerformBluePlacement(string userToken, string gameId, PerformBluePlacement e);
    Task<VoidResult> RequestTraitorsSelected(string userToken, string gameId, TraitorsSelected e);
    Task<VoidResult> RequestStormSpellPlayed(string userToken, string gameId, StormSpellPlayed e);
    Task<VoidResult> RequestTestingStationUsed(string userToken, string gameId, TestingStationUsed e);
    Task<VoidResult> RequestTakeLosses(string userToken, string gameId, TakeLosses e);
    Task<VoidResult> RequestMetheorPlayed(string userToken, string gameId, MetheorPlayed e);
    Task<VoidResult> RequestYellowSentMonster(string userToken, string gameId, YellowSentMonster e);
    Task<VoidResult> RequestYellowRidesMonster(string userToken, string gameId, YellowRidesMonster e);
    Task<VoidResult> RequestAllianceOffered(string userToken, string gameId, AllianceOffered e);
    Task<VoidResult> RequestAllianceBroken(string userToken, string gameId, AllianceBroken e);
    Task<VoidResult> RequestBid(string userToken, string gameId, Bid e);
    Task<VoidResult> RequestRevival(string userToken, string gameId, Revival e);
    Task<VoidResult> RequestBlueBattleAnnouncement(string userToken, string gameId, BlueBattleAnnouncement e);
    Task<VoidResult> RequestShipment(string userToken, string gameId, Shipment e);
    Task<VoidResult> RequestOrangeDelay(string userToken, string gameId, OrangeDelay e);
    Task<VoidResult> RequestBlueAccompanies(string userToken, string gameId, BlueAccompanies e);
    Task<VoidResult> RequestBlueFlip(string userToken, string gameId, BlueFlip e);
    Task<VoidResult> RequestMove(string userToken, string gameId, Move e);
    Task<VoidResult> RequestCaravan(string userToken, string gameId, Caravan e);
    Task<VoidResult> RequestBattleInitiated(string userToken, string gameId, BattleInitiated e);
    Task<VoidResult> RequestBattle(string userToken, string gameId, Battle e);
    Task<VoidResult> RequestBattleRevision(string userToken, string gameId, BattleRevision e);
    Task<VoidResult> RequestTreacheryCalled(string userToken, string gameId, TreacheryCalled e);
    Task<VoidResult> RequestBattleConcluded(string userToken, string gameId, BattleConcluded e);
    Task<VoidResult> RequestClairvoyancePlayed(string userToken, string gameId, ClairVoyancePlayed e);
    Task<VoidResult> RequestClairvoyanceAnswered(string userToken, string gameId, ClairVoyanceAnswered e);
    Task<VoidResult> RequestRaiseDeadPlayed(string userToken, string gameId, RaiseDeadPlayed e);
    Task<VoidResult> RequestKarma(string userToken, string gameId, Karma e);
    Task<VoidResult> RequestKarmaFreeRevival(string userToken, string gameId, KarmaFreeRevival e);
    Task<VoidResult> RequestKarmaShipmentPrevention(string userToken, string gameId, KarmaShipmentPrevention e);
    Task<VoidResult> RequestKarmaRevivalPrevention(string userToken, string gameId, KarmaRevivalPrevention e);
    Task<VoidResult> RequestKarmaHandSwapInitiated(string userToken, string gameId, KarmaHandSwapInitiated e);
    Task<VoidResult> RequestKarmaHandSwap(string userToken, string gameId, KarmaHandSwap e);
    Task<VoidResult> RequestKarmaMonster(string userToken, string gameId, KarmaMonster e);
    Task<VoidResult> RequestKarmaWhiteBuy(string userToken, string gameId, KarmaWhiteBuy e);
    Task<VoidResult> RequestAllyPermission(string userToken, string gameId, AllyPermission e);
    Task<VoidResult> RequestMulliganPerformed(string userToken, string gameId, MulliganPerformed e);
    Task<VoidResult> RequestFaceDancerRevealed(string userToken, string gameId, FaceDancerRevealed e);
    Task<VoidResult> RequestFaceDanced(string userToken, string gameId, FaceDanced e);
    Task<VoidResult> RequestFaceDancerReplaced(string userToken, string gameId, FaceDancerReplaced e);
    Task<VoidResult> RequestSetIncreasedRevivalLimits(string userToken, string gameId, SetIncreasedRevivalLimits e);
    Task<VoidResult> RequestSetShipmentPermission(string userToken, string gameId, SetShipmentPermission e);
    Task<VoidResult> RequestRequestPurpleRevival(string userToken, string gameId, RequestPurpleRevival e);
    Task<VoidResult> RequestAcceptOrCancelPurpleRevival(string userToken, string gameId, AcceptOrCancelPurpleRevival e);
    Task<VoidResult> RequestPerformHmsPlacement(string userToken, string gameId, PerformHmsPlacement e);
    Task<VoidResult> RequestPerformHmsMovement(string userToken, string gameId, PerformHmsMovement e);
    Task<VoidResult> RequestKarmaHmsMovement(string userToken, string gameId, KarmaHmsMovement e);
    Task<VoidResult> RequestAmalPlayed(string userToken, string gameId, AmalPlayed e);
    Task<VoidResult> RequestGreyRemovedCardFromAuction(string userToken, string gameId, GreyRemovedCardFromAuction e);
    Task<VoidResult> RequestGreySelectedStartingCard(string userToken, string gameId, GreySelectedStartingCard e);
    Task<VoidResult> RequestGreySwappedCardOnBid(string userToken, string gameId, GreySwappedCardOnBid e);
    Task<VoidResult> RequestHarvesterPlayed(string userToken, string gameId, HarvesterPlayed e);
    Task<VoidResult> RequestPoisonToothCancelled(string userToken, string gameId, PoisonToothCancelled e);
    Task<VoidResult> RequestReplacedCardWon(string userToken, string gameId, ReplacedCardWon e);
    Task<VoidResult> RequestThumperPlayed(string userToken, string gameId, ThumperPlayed e);
    Task<VoidResult> RequestVoice(string userToken, string gameId, Voice e);
    Task<VoidResult> RequestPrescience(string userToken, string gameId, Prescience e);
    Task<VoidResult> RequestKarmaPrescience(string userToken, string gameId, KarmaPrescience e);
    Task<VoidResult> RequestRedBidSupport(string userToken, string gameId, RedBidSupport e);
    Task<VoidResult> RequestDealOffered(string userToken, string gameId, DealOffered e);
    Task<VoidResult> RequestDealAccepted(string userToken, string gameId, DealAccepted e);
    Task<VoidResult> RequestDiscoveryEntered(string userToken, string gameId, DiscoveryEntered e);
    Task<VoidResult> RequestStormDialled(string userToken, string gameId, StormDialled e);
    Task<VoidResult> RequestHideSecrets(string userToken, string gameId, HideSecrets e);
    Task<VoidResult> RequestPlayerReplaced(string userToken, string gameId, PlayerReplaced e);
    Task<VoidResult> RequestBrownDiscarded(string userToken, string gameId, BrownDiscarded e);
    Task<VoidResult> RequestRedDiscarded(string userToken, string gameId, RedDiscarded e);
    Task<VoidResult> RequestBrownEconomics(string userToken, string gameId, BrownEconomics e);
    Task<VoidResult> RequestCardTraded(string userToken, string gameId, CardTraded e);
    Task<VoidResult> RequestKarmaBrownDiscard(string userToken, string gameId, KarmaBrownDiscard e);
    Task<VoidResult> RequestAuditCancelled(string userToken, string gameId, AuditCancelled e);
    Task<VoidResult> RequestAudited(string userToken, string gameId, Audited e);
    Task<VoidResult> RequestBrownMovePrevention(string userToken, string gameId, BrownMovePrevention e);
    Task<VoidResult> RequestBrownKarmaPrevention(string userToken, string gameId, BrownKarmaPrevention e);
    Task<VoidResult> RequestBrownExtraMove(string userToken, string gameId, BrownExtraMove e);
    Task<VoidResult> RequestBrownFreeRevivalPrevention(string userToken, string gameId, BrownFreeRevivalPrevention e);
    Task<VoidResult> RequestBrownRemoveForce(string userToken, string gameId, BrownRemoveForce e);
    Task<VoidResult> RequestWhiteAnnouncesBlackMarket(string userToken, string gameId, WhiteAnnouncesBlackMarket e);
    Task<VoidResult> RequestBlackMarketBid(string userToken, string gameId, BlackMarketBid e);
    Task<VoidResult> RequestWhiteAnnouncesAuction(string userToken, string gameId, WhiteAnnouncesAuction e);
    Task<VoidResult> RequestWhiteSpecifiesAuction(string userToken, string gameId, WhiteSpecifiesAuction e);
    Task<VoidResult> RequestWhiteKeepsUnsoldCard(string userToken, string gameId, WhiteKeepsUnsoldCard e);
    Task<VoidResult> RequestWhiteRevealedNoField(string userToken, string gameId, WhiteRevealedNoField e);
    Task<VoidResult> RequestWhiteGaveCard(string userToken, string gameId, WhiteGaveCard e);
    Task<VoidResult> RequestCardGiven(string userToken, string gameId, CardGiven e);
    Task<VoidResult> RequestRockWasMelted(string userToken, string gameId, RockWasMelted e);
    Task<VoidResult> RequestResidualPlayed(string userToken, string gameId, ResidualPlayed e);
    Task<VoidResult> RequestFlightUsed(string userToken, string gameId, FlightUsed e);
    Task<VoidResult> RequestFlightDiscoveryUsed(string userToken, string gameId, FlightDiscoveryUsed e);
    Task<VoidResult> RequestDistransUsed(string userToken, string gameId, DistransUsed e);
    Task<VoidResult> RequestDiscardedTaken(string userToken, string gameId, DiscardedTaken e);
    Task<VoidResult> RequestDiscardedSearchedAnnounced(string userToken, string gameId, DiscardedSearchedAnnounced e);
    Task<VoidResult> RequestDiscardedSearched(string userToken, string gameId, DiscardedSearched e);
    Task<VoidResult> RequestJuicePlayed(string userToken, string gameId, JuicePlayed e);
    Task<VoidResult> RequestPortableAntidoteUsed(string userToken, string gameId, PortableAntidoteUsed e);
    Task<VoidResult> RequestBureaucracy(string userToken, string gameId, Bureaucracy e);
    Task<VoidResult> RequestDiplomacy(string userToken, string gameId, Diplomacy e);
    Task<VoidResult> RequestSkillAssigned(string userToken, string gameId, SkillAssigned e);
    Task<VoidResult> RequestSwitchedSkilledLeader(string userToken, string gameId, SwitchedSkilledLeader e);
    Task<VoidResult> RequestThought(string userToken, string gameId, Thought e);
    Task<VoidResult> RequestThoughtAnswered(string userToken, string gameId, ThoughtAnswered e);
    Task<VoidResult> RequestHmsAdvantageChosen(string userToken, string gameId, HMSAdvantageChosen e);
    Task<VoidResult> RequestRetreat(string userToken, string gameId, Retreat e);
    Task<VoidResult> RequestPlanetology(string userToken, string gameId, Planetology e);
    Task<VoidResult> RequestCaptured(string userToken, string gameId, Captured e);
    Task<VoidResult> RequestNexusCardDrawn(string userToken, string gameId, NexusCardDrawn e);
    Task<VoidResult> RequestTerrorPlanted(string userToken, string gameId, TerrorPlanted e);
    Task<VoidResult> RequestTerrorRevealed(string userToken, string gameId, TerrorRevealed e);
    Task<VoidResult> RequestDiscoveryRevealed(string userToken, string gameId, DiscoveryRevealed e);
    Task<VoidResult> RequestAmbassadorPlaced(string userToken, string gameId, AmbassadorPlaced e);
    Task<VoidResult> RequestAmbassadorActivated(string userToken, string gameId, AmbassadorActivated e);
    Task<VoidResult> RequestExtortionPrevented(string userToken, string gameId, ExtortionPrevented e);
    Task<VoidResult> RequestDiscarded(string userToken, string gameId, Discarded e);
    Task<VoidResult> RequestAllianceByTerror(string userToken, string gameId, AllianceByTerror e);
    Task<VoidResult> RequestNexusVoted(string userToken, string gameId, NexusVoted e);
    Task<VoidResult> RequestAllianceByAmbassador(string userToken, string gameId, AllianceByAmbassador e);
    Task<VoidResult> RequestLoserConcluded(string userToken, string gameId, LoserConcluded e);
    Task<VoidResult> RequestPerformCyanSetup(string userToken, string gameId, PerformCyanSetup e);
    Task<VoidResult> RequestDivideResources(string userToken, string gameId, DivideResources e);
    Task<VoidResult> RequestDivideResourcesAccepted(string userToken, string gameId, DivideResourcesAccepted e);
    Task<VoidResult> RequestBattleClaimed(string userToken, string gameId, BattleClaimed e);
    Task<VoidResult> RequestKarmaPinkDial(string userToken, string gameId, KarmaPinkDial e);
    Task<VoidResult> RequestTraitorDiscarded(string userToken, string gameId, TraitorDiscarded e);
    Task<VoidResult> RequestNexusPlayed(string userToken, string gameId, NexusPlayed e);
    Task<VoidResult> RequestResourcesAudited(string userToken, string gameId, ResourcesAudited e);
    Task<VoidResult> RequestRecruitsPlayed(string userToken, string gameId, RecruitsPlayed e);
    
    //Chat
    Task<VoidResult> SendChatMessage(string userToken, string gameId, GameChatMessage e);
    Task<VoidResult> SendGlobalChatMessage(string userToken, GlobalChatMessage message);

    //Other
    Task<Result<ServerInfo>> Connect();
    Task<VoidResult> RequestNudgeBots(string userToken, string gameId);
    
    //Admin
    Task<Result<string>> AdminUpdateMaintenance(string userToken, DateTimeOffset maintenanceDate);
    Task<Result<string>> AdminPersistState(string userToken);
    Task<Result<string>> AdminRestoreState(string userToken);
    Task<Result<string>> AdminCloseGame(string userToken, string gameId);
    Task<Result<string>> AdminCancelGame(string userToken, string scheduledGameId);
    Task<Result<string>> AdminDeleteUser(string userToken, int userId);
    Task<Result<AdminInfo>> GetAdminInfo(string userToken);
}