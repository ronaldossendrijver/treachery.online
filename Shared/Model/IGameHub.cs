using System;
using System.Threading.Tasks;

namespace Treachery.Shared;

public interface IGameHub
{
    //Authentication
    
    Task<Result<string>> RequestCreateUser(string userName, string hashedPassword, string email, string playerName);
    Task<Result<string>> RequestLogin(int version, string userName, string hashedPassword);
    Task<VoidResult> RequestPasswordReset(string email);
    Task<Result<string>> RequestSetPassword(string userName, string passwordResetToken, string newHashedPassword);
    
    //Game Management
    
    Task<Result<JoinInfo>> RequestCreateGame(string playerToken, string hashedPassword, string settings);
    //Task<VoidResult> RequestAdmission(int version, string gameToken, GameAdmission admission);
    Task<Result<JoinInfo>> RequestJoinGame(string playerToken, string gameId, string hashedPassword, Faction faction);
    Task<Result<JoinInfo>> RequestObserveGame(string playerToken, string gameId, string hashedPassword);
    Task<Result<JoinInfo>> RequestReconnectGame(string playerToken, string gameToken);
    
   // Task RequestPlayerJoined(int hostID, PlayerJoined e);
    //Task RequestPlayerRejoined(int hostID, PlayerRejoined e);
    //Task RequestObserverJoined(int hostID, ObserverJoined e);
    //Task RequestObserverRejoined(string playerToken, string gameToken, ObserverRejoined e);
    Task<Result<JoinInfo>> RequestLoadGame(string playerToken, string hashedPassword, string state, string skin);
    Task<VoidResult> RequestSetSkin(string playerToken, string gameToken, string skin);
    Task<VoidResult> RequestUndo(string playerToken, string gameToken, int untilEventNr);
    Task<Result<string>> RequestGameState(string playerToken, string gameToken);
    Task<VoidResult> RequestPauseBots(string playerToken, string gameToken);
    
    //Game Events
    
    Task<VoidResult> SetTimer(string playerToken, string gameToken, int value);
    Task<VoidResult> RequestEstablishPlayers(string playerToken, string gameToken, EstablishPlayers e);
    Task<VoidResult> RequestEndPhase(string playerToken, string gameToken, EndPhase e);
    Task<VoidResult> RequestDonated(string playerToken, string gameToken, Donated e);
    Task<VoidResult> RequestResourcesTransferred(string playerToken, string gameToken, ResourcesTransferred e);
    Task<VoidResult> RequestFactionSelected(string playerToken, string gameToken, FactionSelected e);
    Task<VoidResult> RequestFactionTradeOffered(string playerToken, string gameToken, FactionTradeOffered e);
    Task<VoidResult> RequestPerformSetup(string playerToken, string gameToken, PerformSetup e);
    Task<VoidResult> RequestCardsDetermined(string playerToken, string gameToken, CardsDetermined e);
    Task<VoidResult> RequestPerformYellowSetup(string playerToken, string gameToken, PerformYellowSetup e);
    Task<VoidResult> RequestBluePrediction(string playerToken, string gameToken, BluePrediction e);
    Task<VoidResult> RequestCharityClaimed(string playerToken, string gameToken, CharityClaimed e);
    Task<VoidResult> RequestPerformBluePlacement(string playerToken, string gameToken, PerformBluePlacement e);
    Task<VoidResult> RequestTraitorsSelected(string playerToken, string gameToken, TraitorsSelected e);
    Task<VoidResult> RequestStormSpellPlayed(string playerToken, string gameToken, StormSpellPlayed e);
    Task<VoidResult> RequestTestingStationUsed(string playerToken, string gameToken, TestingStationUsed e);
    Task<VoidResult> RequestTakeLosses(string playerToken, string gameToken, TakeLosses e);
    Task<VoidResult> RequestMetheorPlayed(string playerToken, string gameToken, MetheorPlayed e);
    Task<VoidResult> RequestYellowSentMonster(string playerToken, string gameToken, YellowSentMonster e);
    Task<VoidResult> RequestYellowRidesMonster(string playerToken, string gameToken, YellowRidesMonster e);
    Task<VoidResult> RequestAllianceOffered(string playerToken, string gameToken, AllianceOffered e);
    Task<VoidResult> RequestAllianceBroken(string playerToken, string gameToken, AllianceBroken e);
    Task<VoidResult> RequestBid(string playerToken, string gameToken, Bid e);
    Task<VoidResult> RequestRevival(string playerToken, string gameToken, Revival e);
    Task<VoidResult> RequestBlueBattleAnnouncement(string playerToken, string gameToken, BlueBattleAnnouncement e);
    Task<VoidResult> RequestShipment(string playerToken, string gameToken, Shipment e);
    Task<VoidResult> RequestOrangeDelay(string playerToken, string gameToken, OrangeDelay e);
    Task<VoidResult> RequestBlueAccompanies(string playerToken, string gameToken, BlueAccompanies e);
    Task<VoidResult> RequestBlueFlip(string playerToken, string gameToken, BlueFlip e);
    Task<VoidResult> RequestMove(string playerToken, string gameToken, Move e);
    Task<VoidResult> RequestCaravan(string playerToken, string gameToken, Caravan e);
    Task<VoidResult> RequestBattleInitiated(string playerToken, string gameToken, BattleInitiated e);
    Task<VoidResult> RequestBattle(string playerToken, string gameToken, Battle e);
    Task<VoidResult> RequestBattleRevision(string playerToken, string gameToken, BattleRevision e);
    Task<VoidResult> RequestTreacheryCalled(string playerToken, string gameToken, TreacheryCalled e);
    Task<VoidResult> RequestBattleConcluded(string playerToken, string gameToken, BattleConcluded e);
    Task<VoidResult> RequestClairvoyancePlayed(string playerToken, string gameToken, ClairVoyancePlayed e);
    Task<VoidResult> RequestClairvoyanceAnswered(string playerToken, string gameToken, ClairVoyanceAnswered e);
    Task<VoidResult> RequestRaiseDeadPlayed(string playerToken, string gameToken, RaiseDeadPlayed e);
    Task<VoidResult> RequestKarma(string playerToken, string gameToken, Karma e);
    Task<VoidResult> RequestKarmaFreeRevival(string playerToken, string gameToken, KarmaFreeRevival e);
    Task<VoidResult> RequestKarmaShipmentPrevention(string playerToken, string gameToken, KarmaShipmentPrevention e);
    Task<VoidResult> RequestKarmaRevivalPrevention(string playerToken, string gameToken, KarmaRevivalPrevention e);
    Task<VoidResult> RequestKarmaHandSwapInitiated(string playerToken, string gameToken, KarmaHandSwapInitiated e);
    Task<VoidResult> RequestKarmaHandSwap(string playerToken, string gameToken, KarmaHandSwap e);
    Task<VoidResult> RequestKarmaMonster(string playerToken, string gameToken, KarmaMonster e);
    Task<VoidResult> RequestKarmaWhiteBuy(string playerToken, string gameToken, KarmaWhiteBuy e);
    Task<VoidResult> RequestAllyPermission(string playerToken, string gameToken, AllyPermission e);
    Task<VoidResult> RequestMulliganPerformed(string playerToken, string gameToken, MulliganPerformed e);
    Task<VoidResult> RequestFaceDancerRevealed(string playerToken, string gameToken, FaceDancerRevealed e);
    Task<VoidResult> RequestFaceDanced(string playerToken, string gameToken, FaceDanced e);
    Task<VoidResult> RequestFaceDancerReplaced(string playerToken, string gameToken, FaceDancerReplaced e);
    Task<VoidResult> RequestSetIncreasedRevivalLimits(string playerToken, string gameToken, SetIncreasedRevivalLimits e);
    Task<VoidResult> RequestSetShipmentPermission(string playerToken, string gameToken, SetShipmentPermission e);
    Task<VoidResult> RequestRequestPurpleRevival(string playerToken, string gameToken, RequestPurpleRevival e);
    Task<VoidResult> RequestAcceptOrCancelPurpleRevival(string playerToken, string gameToken, AcceptOrCancelPurpleRevival e);
    Task<VoidResult> RequestPerformHmsPlacement(string playerToken, string gameToken, PerformHmsPlacement e);
    Task<VoidResult> RequestPerformHmsMovement(string playerToken, string gameToken, PerformHmsMovement e);
    Task<VoidResult> RequestKarmaHmsMovement(string playerToken, string gameToken, KarmaHmsMovement e);
    Task<VoidResult> RequestAmalPlayed(string playerToken, string gameToken, AmalPlayed e);
    Task<VoidResult> RequestGreyRemovedCardFromAuction(string playerToken, string gameToken, GreyRemovedCardFromAuction e);
    Task<VoidResult> RequestGreySelectedStartingCard(string playerToken, string gameToken, GreySelectedStartingCard e);
    Task<VoidResult> RequestGreySwappedCardOnBid(string playerToken, string gameToken, GreySwappedCardOnBid e);
    Task<VoidResult> RequestHarvesterPlayed(string playerToken, string gameToken, HarvesterPlayed e);
    Task<VoidResult> RequestPoisonToothCancelled(string playerToken, string gameToken, PoisonToothCancelled e);
    Task<VoidResult> RequestReplacedCardWon(string playerToken, string gameToken, ReplacedCardWon e);
    Task<VoidResult> RequestThumperPlayed(string playerToken, string gameToken, ThumperPlayed e);
    Task<VoidResult> RequestVoice(string playerToken, string gameToken, Voice e);
    Task<VoidResult> RequestPrescience(string playerToken, string gameToken, Prescience e);
    Task<VoidResult> RequestKarmaPrescience(string playerToken, string gameToken, KarmaPrescience e);
    Task<VoidResult> RequestRedBidSupport(string playerToken, string gameToken, RedBidSupport e);
    Task<VoidResult> RequestDealOffered(string playerToken, string gameToken, DealOffered e);
    Task<VoidResult> RequestDealAccepted(string playerToken, string gameToken, DealAccepted e);
    Task<VoidResult> RequestDiscoveryEntered(string playerToken, string gameToken, DiscoveryEntered e);
    Task<VoidResult> RequestStormDialled(string playerToken, string gameToken, StormDialled e);
    Task<VoidResult> RequestHideSecrets(string playerToken, string gameToken, HideSecrets e);
    Task<VoidResult> RequestPlayerReplaced(string playerToken, string gameToken, PlayerReplaced e);
    Task<VoidResult> RequestBrownDiscarded(string playerToken, string gameToken, BrownDiscarded e);
    Task<VoidResult> RequestRedDiscarded(string playerToken, string gameToken, RedDiscarded e);
    Task<VoidResult> RequestBrownEconomics(string playerToken, string gameToken, BrownEconomics e);
    Task<VoidResult> RequestCardTraded(string playerToken, string gameToken, CardTraded e);
    Task<VoidResult> RequestKarmaBrownDiscard(string playerToken, string gameToken, KarmaBrownDiscard e);
    Task<VoidResult> RequestAuditCancelled(string playerToken, string gameToken, AuditCancelled e);
    Task<VoidResult> RequestAudited(string playerToken, string gameToken, Audited e);
    Task<VoidResult> RequestBrownMovePrevention(string playerToken, string gameToken, BrownMovePrevention e);
    Task<VoidResult> RequestBrownKarmaPrevention(string playerToken, string gameToken, BrownKarmaPrevention e);
    Task<VoidResult> RequestBrownExtraMove(string playerToken, string gameToken, BrownExtraMove e);
    Task<VoidResult> RequestBrownFreeRevivalPrevention(string playerToken, string gameToken, BrownFreeRevivalPrevention e);
    Task<VoidResult> RequestBrownRemoveForce(string playerToken, string gameToken, BrownRemoveForce e);
    Task<VoidResult> RequestWhiteAnnouncesBlackMarket(string playerToken, string gameToken, WhiteAnnouncesBlackMarket e);
    Task<VoidResult> RequestBlackMarketBid(string playerToken, string gameToken, BlackMarketBid e);
    Task<VoidResult> RequestWhiteAnnouncesAuction(string playerToken, string gameToken, WhiteAnnouncesAuction e);
    Task<VoidResult> RequestWhiteSpecifiesAuction(string playerToken, string gameToken, WhiteSpecifiesAuction e);
    Task<VoidResult> RequestWhiteKeepsUnsoldCard(string playerToken, string gameToken, WhiteKeepsUnsoldCard e);
    Task<VoidResult> RequestWhiteRevealedNoField(string playerToken, string gameToken, WhiteRevealedNoField e);
    Task<VoidResult> RequestWhiteGaveCard(string playerToken, string gameToken, WhiteGaveCard e);
    Task<VoidResult> RequestCardGiven(string playerToken, string gameToken, CardGiven e);
    Task<VoidResult> RequestRockWasMelted(string playerToken, string gameToken, RockWasMelted e);
    Task<VoidResult> RequestResidualPlayed(string playerToken, string gameToken, ResidualPlayed e);
    Task<VoidResult> RequestFlightUsed(string playerToken, string gameToken, FlightUsed e);
    Task<VoidResult> RequestFlightDiscoveryUsed(string playerToken, string gameToken, FlightDiscoveryUsed e);
    Task<VoidResult> RequestDistransUsed(string playerToken, string gameToken, DistransUsed e);
    Task<VoidResult> RequestDiscardedTaken(string playerToken, string gameToken, DiscardedTaken e);
    Task<VoidResult> RequestDiscardedSearchedAnnounced(string playerToken, string gameToken, DiscardedSearchedAnnounced e);
    Task<VoidResult> RequestDiscardedSearched(string playerToken, string gameToken, DiscardedSearched e);
    Task<VoidResult> RequestJuicePlayed(string playerToken, string gameToken, JuicePlayed e);
    Task<VoidResult> RequestPortableAntidoteUsed(string playerToken, string gameToken, PortableAntidoteUsed e);
    Task<VoidResult> RequestBureaucracy(string playerToken, string gameToken, Bureaucracy e);
    Task<VoidResult> RequestDiplomacy(string playerToken, string gameToken, Diplomacy e);
    Task<VoidResult> RequestSkillAssigned(string playerToken, string gameToken, SkillAssigned e);
    Task<VoidResult> RequestSwitchedSkilledLeader(string playerToken, string gameToken, SwitchedSkilledLeader e);
    Task<VoidResult> RequestThought(string playerToken, string gameToken, Thought e);
    Task<VoidResult> RequestThoughtAnswered(string playerToken, string gameToken, ThoughtAnswered e);
    Task<VoidResult> RequestHmsAdvantageChosen(string playerToken, string gameToken, HMSAdvantageChosen e);
    Task<VoidResult> RequestRetreat(string playerToken, string gameToken, Retreat e);
    Task<VoidResult> RequestPlanetology(string playerToken, string gameToken, Planetology e);
    Task<VoidResult> RequestCaptured(string playerToken, string gameToken, Captured e);
    Task<VoidResult> RequestNexusCardDrawn(string playerToken, string gameToken, NexusCardDrawn e);
    Task<VoidResult> RequestTerrorPlanted(string playerToken, string gameToken, TerrorPlanted e);
    Task<VoidResult> RequestTerrorRevealed(string playerToken, string gameToken, TerrorRevealed e);
    Task<VoidResult> RequestDiscoveryRevealed(string playerToken, string gameToken, DiscoveryRevealed e);
    Task<VoidResult> RequestAmbassadorPlaced(string playerToken, string gameToken, AmbassadorPlaced e);
    Task<VoidResult> RequestAmbassadorActivated(string playerToken, string gameToken, AmbassadorActivated e);
    Task<VoidResult> RequestExtortionPrevented(string playerToken, string gameToken, ExtortionPrevented e);
    Task<VoidResult> RequestDiscarded(string playerToken, string gameToken, Discarded e);
    Task<VoidResult> RequestAllianceByTerror(string playerToken, string gameToken, AllianceByTerror e);
    Task<VoidResult> RequestNexusVoted(string playerToken, string gameToken, NexusVoted e);
    Task<VoidResult> RequestAllianceByAmbassador(string playerToken, string gameToken, AllianceByAmbassador e);
    Task<VoidResult> RequestLoserConcluded(string playerToken, string gameToken, LoserConcluded e);
    Task<VoidResult> RequestPerformCyanSetup(string playerToken, string gameToken, PerformCyanSetup e);
    Task<VoidResult> RequestDivideResources(string playerToken, string gameToken, DivideResources e);
    Task<VoidResult> RequestDivideResourcesAccepted(string playerToken, string gameToken, DivideResourcesAccepted e);
    Task<VoidResult> RequestBattleClaimed(string playerToken, string gameToken, BattleClaimed e);
    Task<VoidResult> RequestKarmaPinkDial(string playerToken, string gameToken, KarmaPinkDial e);
    Task<VoidResult> RequestTraitorDiscarded(string playerToken, string gameToken, TraitorDiscarded e);
    Task<VoidResult> RequestNexusPlayed(string playerToken, string gameToken, NexusPlayed e);
    Task<VoidResult> RequestResourcesAudited(string playerToken, string gameToken, ResourcesAudited e);
    Task<VoidResult> RequestRecruitsPlayed(string playerToken, string gameToken, RecruitsPlayed e);
    
    //Chat
    
    Task<VoidResult> RequestChatMessage(string playerToken, string gameToken, GameChatMessage e);
    Task<VoidResult> SendGlobalChatMessage(string playerToken, GlobalChatMessage message);

    //Other
    
    Result<ServerSettings> GetServerSettings();
    //void ProcessHeartbeat(string playerToken);
}

public class Result<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T Contents { get; set; }
}

public class VoidResult : Result<VoidContents>
{
    
}

public class VoidContents
{
    
}