using Treachery.Bots;
using Treachery.Shared.Model;

namespace Treachery.Server;

public partial class GameHub
{
    public async Task<VoidResult> RequestEstablishPlayers(string userToken, string gameId, EstablishPlayers e)
    {
        if (!AreValid(userToken, gameId, out _, out var game, out var error))
            return error!;
        
        await ProcessGameEvent(userToken, gameId, e);

        var participation = game!.Game.Participation;

        var userIds = participation.SeatedPlayers.Keys.ToList();
        var participantIndex = 0;
        foreach (var player in game.Game.Players)
        {
            if (participantIndex >= userIds.Count)
                break;
            
            var userId =  userIds[participantIndex++]; 
            participation.SeatedPlayers[userId] = player.Seat;
        }
        
        await Clients.Group(gameId).HandleAssignSeats(participation.SeatedPlayers);
        
        game.LastActivity = DateTimeOffset.Now;
        await PersistGameIfNeeded(game);
        return Success();
    }

    public async Task<VoidResult> RequestEndPhase(string userToken, string gameId, EndPhase e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestDonated(string userToken, string gameId, Donated e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestResourcesTransferred(string userToken, string gameId, ResourcesTransferred e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestFactionSelected(string userToken, string gameId, FactionSelected e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestFactionTradeOffered(string userToken, string gameId, FactionTradeOffered e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestPerformSetup(string userToken, string gameId, PerformSetup e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestCardsDetermined(string userToken, string gameId, CardsDetermined e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestPerformYellowSetup(string userToken, string gameId, PerformYellowSetup e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestBluePrediction(string userToken, string gameId, BluePrediction e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestCharityClaimed(string userToken, string gameId, CharityClaimed e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestPerformBluePlacement(string userToken, string gameId, PerformBluePlacement e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestTraitorsSelected(string userToken, string gameId, TraitorsSelected e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestStormSpellPlayed(string userToken, string gameId, StormSpellPlayed e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestTestingStationUsed(string userToken, string gameId, TestingStationUsed e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestTakeLosses(string userToken, string gameId, TakeLosses e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestMetheorPlayed(string userToken, string gameId, MetheorPlayed e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestYellowSentMonster(string userToken, string gameId, YellowSentMonster e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestYellowRidesMonster(string userToken, string gameId, YellowRidesMonster e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestAllianceOffered(string userToken, string gameId, AllianceOffered e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestAllianceBroken(string userToken, string gameId, AllianceBroken e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestBid(string userToken, string gameId, Bid e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestRevival(string userToken, string gameId, Revival e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestBlueBattleAnnouncement(string userToken, string gameId, BlueBattleAnnouncement e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestShipment(string userToken, string gameId, Shipment e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestOrangeDelay(string userToken, string gameId, OrangeDelay e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestBlueAccompanies(string userToken, string gameId, BlueAccompanies e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestBlueFlip(string userToken, string gameId, BlueFlip e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestMove(string userToken, string gameId, Move e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestCaravan(string userToken, string gameId, Caravan e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestBattleInitiated(string userToken, string gameId, BattleInitiated e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestBattle(string userToken, string gameId, Battle e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestBattleRevision(string userToken, string gameId, BattleRevision e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestTreacheryCalled(string userToken, string gameId, TreacheryCalled e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestBattleConcluded(string userToken, string gameId, BattleConcluded e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestClairvoyancePlayed(string userToken, string gameId, ClairVoyancePlayed e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestClairvoyanceAnswered(string userToken, string gameId, ClairVoyanceAnswered e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestRaiseDeadPlayed(string userToken, string gameId, RaiseDeadPlayed e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestKarma(string userToken, string gameId, Karma e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestKarmaFreeRevival(string userToken, string gameId, KarmaFreeRevival e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestKarmaShipmentPrevention(string userToken, string gameId, KarmaShipmentPrevention e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestKarmaRevivalPrevention(string userToken, string gameId, KarmaRevivalPrevention e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestKarmaHandSwapInitiated(string userToken, string gameId, KarmaHandSwapInitiated e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestKarmaHandSwap(string userToken, string gameId, KarmaHandSwap e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestKarmaMonster(string userToken, string gameId, KarmaMonster e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestKarmaWhiteBuy(string userToken, string gameId, KarmaWhiteBuy e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestAllyPermission(string userToken, string gameId, AllyPermission e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestMulliganPerformed(string userToken, string gameId, MulliganPerformed e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestFaceDancerRevealed(string userToken, string gameId, FaceDancerRevealed e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestFaceDanced(string userToken, string gameId, FaceDanced e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestFaceDancerReplaced(string userToken, string gameId, FaceDancerReplaced e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestSetIncreasedRevivalLimits(string userToken, string gameId, SetIncreasedRevivalLimits e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestSetShipmentPermission(string userToken, string gameId, SetShipmentPermission e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestRequestPurpleRevival(string userToken, string gameId, RequestPurpleRevival e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestAcceptOrCancelPurpleRevival(string userToken, string gameId, AcceptOrCancelPurpleRevival e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestPerformHmsPlacement(string userToken, string gameId, PerformHmsPlacement e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestPerformHmsMovement(string userToken, string gameId, PerformHmsMovement e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestKarmaHmsMovement(string userToken, string gameId, KarmaHmsMovement e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestAmalPlayed(string userToken, string gameId, AmalPlayed e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestGreyRemovedCardFromAuction(string userToken, string gameId, GreyRemovedCardFromAuction e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestGreySelectedStartingCard(string userToken, string gameId, GreySelectedStartingCard e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestGreySwappedCardOnBid(string userToken, string gameId, GreySwappedCardOnBid e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestHarvesterPlayed(string userToken, string gameId, HarvesterPlayed e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestPoisonToothCancelled(string userToken, string gameId, PoisonToothCancelled e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestReplacedCardWon(string userToken, string gameId, ReplacedCardWon e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestThumperPlayed(string userToken, string gameId, ThumperPlayed e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestVoice(string userToken, string gameId, Voice e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestPrescience(string userToken, string gameId, Prescience e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestKarmaPrescience(string userToken, string gameId, KarmaPrescience e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestRedBidSupport(string userToken, string gameId, RedBidSupport e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestDealOffered(string userToken, string gameId, DealOffered e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestDealAccepted(string userToken, string gameId, DealAccepted e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestDiscoveryEntered(string userToken, string gameId, DiscoveryEntered e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestStormDialled(string userToken, string gameId, StormDialled e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestHideSecrets(string userToken, string gameId, HideSecrets e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestPlayerReplaced(string userToken, string gameId, PlayerReplaced e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestBrownDiscarded(string userToken, string gameId, BrownDiscarded e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestRedDiscarded(string userToken, string gameId, RedDiscarded e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestBrownEconomics(string userToken, string gameId, BrownEconomics e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestCardTraded(string userToken, string gameId, CardTraded e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestKarmaBrownDiscard(string userToken, string gameId, KarmaBrownDiscard e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestAuditCancelled(string userToken, string gameId, AuditCancelled e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestAudited(string userToken, string gameId, Audited e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestBrownMovePrevention(string userToken, string gameId, BrownMovePrevention e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestBrownKarmaPrevention(string userToken, string gameId, BrownKarmaPrevention e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestBrownExtraMove(string userToken, string gameId, BrownExtraMove e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestBrownFreeRevivalPrevention(string userToken, string gameId, BrownFreeRevivalPrevention e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestBrownRemoveForce(string userToken, string gameId, BrownRemoveForce e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestWhiteAnnouncesBlackMarket(string userToken, string gameId, WhiteAnnouncesBlackMarket e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestBlackMarketBid(string userToken, string gameId, BlackMarketBid e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestWhiteAnnouncesAuction(string userToken, string gameId, WhiteAnnouncesAuction e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestWhiteSpecifiesAuction(string userToken, string gameId, WhiteSpecifiesAuction e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestWhiteKeepsUnsoldCard(string userToken, string gameId, WhiteKeepsUnsoldCard e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestWhiteRevealedNoField(string userToken, string gameId, WhiteRevealedNoField e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestWhiteGaveCard(string userToken, string gameId, WhiteGaveCard e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestCardGiven(string userToken, string gameId, CardGiven e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestRockWasMelted(string userToken, string gameId, RockWasMelted e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestResidualPlayed(string userToken, string gameId, ResidualPlayed e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestFlightUsed(string userToken, string gameId, FlightUsed e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestFlightDiscoveryUsed(string userToken, string gameId, FlightDiscoveryUsed e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestDistransUsed(string userToken, string gameId, DistransUsed e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestDiscardedTaken(string userToken, string gameId, DiscardedTaken e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestDiscardedSearchedAnnounced(string userToken, string gameId, DiscardedSearchedAnnounced e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestDiscardedSearched(string userToken, string gameId, DiscardedSearched e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestJuicePlayed(string userToken, string gameId, JuicePlayed e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestPortableAntidoteUsed(string userToken, string gameId, PortableAntidoteUsed e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestBureaucracy(string userToken, string gameId, Bureaucracy e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestDiplomacy(string userToken, string gameId, Diplomacy e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestSkillAssigned(string userToken, string gameId, SkillAssigned e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestSwitchedSkilledLeader(string userToken, string gameId, SwitchedSkilledLeader e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestThought(string userToken, string gameId, Thought e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestThoughtAnswered(string userToken, string gameId, ThoughtAnswered e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestHmsAdvantageChosen(string userToken, string gameId, HMSAdvantageChosen e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestRetreat(string userToken, string gameId, Retreat e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestPlanetology(string userToken, string gameId, Planetology e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestCaptured(string userToken, string gameId, Captured e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestNexusCardDrawn(string userToken, string gameId, NexusCardDrawn e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestTerrorPlanted(string userToken, string gameId, TerrorPlanted e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestTerrorRevealed(string userToken, string gameId, TerrorRevealed e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestDiscoveryRevealed(string userToken, string gameId, DiscoveryRevealed e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestAmbassadorPlaced(string userToken, string gameId, AmbassadorPlaced e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestAmbassadorActivated(string userToken, string gameId, AmbassadorActivated e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestExtortionPrevented(string userToken, string gameId, ExtortionPrevented e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestDiscarded(string userToken, string gameId, Discarded e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestAllianceByTerror(string userToken, string gameId, AllianceByTerror e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestNexusVoted(string userToken, string gameId, NexusVoted e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestAllianceByAmbassador(string userToken, string gameId, AllianceByAmbassador e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestLoserConcluded(string userToken, string gameId, LoserConcluded e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestPerformCyanSetup(string userToken, string gameId, PerformCyanSetup e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestDivideResources(string userToken, string gameId, DivideResources e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestDivideResourcesAccepted(string userToken, string gameId, DivideResourcesAccepted e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestBattleClaimed(string userToken, string gameId, BattleClaimed e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestKarmaPinkDial(string userToken, string gameId, KarmaPinkDial e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestTraitorDiscarded(string userToken, string gameId, TraitorDiscarded e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestNexusPlayed(string userToken, string gameId, NexusPlayed e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestResourcesAudited(string userToken, string gameId, ResourcesAudited e) => await ProcessGameEvent(userToken, gameId, e);
    public async Task<VoidResult> RequestRecruitsPlayed(string userToken, string gameId, RecruitsPlayed e) => await ProcessGameEvent(userToken, gameId, e);
    
    public async Task<VoidResult> SetTimer(string userToken, string gameId, int value)
    {
        if (!AreValid(userToken, gameId, out var user, out var game, out var error))
            return error!;

        if (!game!.Game.IsHost(user!.Id))
            return Error(ErrorType.NoHost);
        
        await Clients.Group(gameId).HandleSetTimer(value);
        return Success();
    }
    
    private async Task<VoidResult> ProcessGameEvent<TEvent>(string userToken, string gameId, TEvent e) where TEvent : GameEvent
    {
        if (!AreValid(userToken, gameId, out var user, out var game, out var error))
            return error!;
        
        e.Initialize(game!.Game);
        e.Time = DateTimeOffset.Now;
        
        return await ValidateAndExecute(e, game, game.Game.IsHost(user!.Id));
    }

    private async Task<VoidResult> ValidateAndExecute<TEvent>(TEvent e, ManagedGame game, bool isHost)
        where TEvent : GameEvent
    {
        var validationResult = e.Execute(true, isHost);
        e.Time = DateTimeOffset.Now;
        
        if (validationResult != null)
        {
            Log("Invalid bot decision: " + validationResult);
            return Error(ErrorType.InvalidGameEvent, validationResult.ToString());
        }

        if (game.Game.CurrentMainPhase is MainPhase.Ended && !game.StatisticsSent && game.Game.NumberOfBots < 0.5f * game.Game.NumberOfSeatedPlayers)
        {
            await SendMailAndStatistics(game);
            game.StatisticsSent = true;
        }

        game.LastActivity = DateTimeOffset.Now;
        
        await Clients.Group(game.GameId).HandleGameEvent(e, game.Game.History.Count);
        await SendAsyncPlayMessagesIfApplicable(game.GameId);
        await PersistGameIfNeeded(game);
        ScheduleBotEvent(game);
        return Success();
    }

    private async Task SendAsyncPlayMessagesIfApplicable(string gameId)
    {
        if (!RunningGamesByGameId.TryGetValue(gameId, out var game))
            return;
        
        if (game.Game.Settings.AsyncPlay)
        {
            var now = DateTimeOffset.Now;
            var elapsedMinutes = (int)now.Subtract(game.LastAsyncPlayMessageSent).TotalMinutes;
            if (elapsedMinutes < game.Game.Settings.AsyncPlayMessageIntervalMinutes)
            {
                int delayInSeconds = 1 + 60 * (game.Game.Settings.AsyncPlayMessageIntervalMinutes - elapsedMinutes);
                _ = Task.Delay(delayInSeconds * 1000)
                    .ContinueWith(_ => SendAsyncPlayMessagesIfApplicable(gameId));
                
                return;
            }
            var whatHappened = game.Game.History
                .Where(e => e.Time > game.LastAsyncPlayMessageSent && e is not (AllyPermission))
                .ToList();
            
            if (whatHappened.Count == 0)
                return;

            var history = $"The game was just started. Have fun!";
            if (whatHappened.Count > 1)
            {
                history = "The following happened:";
                history += "<ul>";
                foreach (var evt in whatHappened.Where(e => e is not EstablishPlayers))
                {
                    history += $"<li>{evt.GetShortMessage().ToString(DefaultSkin.Default)}</li>";
                }
                history += "</ul>";
            }
            
            var turnInfo = game.Game.CurrentTurn == 0 ? 
                "Setting up a new game" : 
                DefaultSkin.Default.Format("Turn: {0}, phase: {1}", game.Game.CurrentTurn, game.Game.CurrentPhase);
            
            await using var context = GetDbContext();

            var nl = Environment.NewLine + Environment.NewLine;
            foreach (var userId in game.Game.Participation.SeatedPlayers.Keys)
            {
                var user = await context.Users.FindAsync(userId);
                var mail = user?.Email;
                if (mail == null) continue;

                var player = game.Game.GetPlayerByUserId(userId);
                if (player == null) continue;

                var status = GameStatus.DetermineStatus(game.Game, player, true);
                
                var statusInfo = status.GetMessage(player, game.Game.IsHost(userId)).ToString(DefaultSkin.Default);

                var waitingFor = status.WaitingForHost ? "Players are waiting for the host to continue the game " + nl :
                    status.WaitingForPlayers.Any() ? "Waiting for: " + string.Join(", ", status.WaitingForPlayers.Select(p => p.Name)) + nl : string.Empty;

                var userToken = UsersByUserToken.FirstOrDefault(tokenAndUser => tokenAndUser.Value.Id == userId).Key;

                var link = userToken == null
                    ? "Join game at: <a href=\"https://treachery.online/\">https://treachery.online/</a>"
                    : $"Jump to game: <a href=\"https://treachery.online/{userToken}/{game.GameId}\">https://treachery.online/</a>";

                var asyncMessage = 
                    $"<p>{turnInfo}</p><p>{statusInfo}</p><p>{waitingFor}</p><p>{link}</p><p>{history}</p>";

                MailMessage mailMessage = new()
                {
                    From = new MailAddress("noreply@treachery.online"),
                    Subject = $"Update for {game.Name}",
                    IsBodyHtml = true,
                    Body = asyncMessage
                };

                mailMessage.To.Add(new MailAddress(mail));
                await SendMail(mailMessage);
            }
            
            game.LastAsyncPlayMessageSent = now;
        }
    }

    private void ScheduleBotEvent(ManagedGame managedGame, bool immediate = false)
    {
        if (immediate)
        {
            _ = PerformBotEvent(managedGame);
        }
        else
        {
            var delay = DetermineBotDelay(managedGame.Game.CurrentMainPhase, managedGame.Game.LatestEvent());
            _ = Task.Delay(delay).ContinueWith(_ => PerformBotEvent(managedGame));    
        }
    }

    private async Task PerformBotEvent(ManagedGame managedGame)
    {
        var game = managedGame.Game;
        var botsActAsHosts = game.Participation.Hosts.Count == 0 && game.NumberOfObservers > 0;
        
        if (!game.Participation.BotsArePaused && game.CurrentPhase > Phase.AwaitingPlayers)
        {
            var bots = Deck<Player>.Randomize(game.Players.Where(p => p.IsBot));
            var eventsPerBot = bots.ToDictionary(x => x.Seat, x => game.GetApplicableEvents(x, botsActAsHosts));

            foreach (var bot in bots)
            {
                var classicBot = GetOrInitializeBot(managedGame, bot);
                var evt = classicBot.DetermineHighestPriorityInPhaseAction(eventsPerBot[bot.Seat]);
                if (evt == null) continue;
                await ValidateAndExecute(evt, managedGame, false);
                return;
            }
            
            foreach (var bot in bots)
            {
                var classicBot = GetOrInitializeBot(managedGame, bot);
                var evt = classicBot.DetermineHighPriorityInPhaseAction(eventsPerBot[bot.Seat]);
                if (evt == null) continue;
                await ValidateAndExecute(evt, managedGame, false);
                return;
            }
            
            foreach (var bot in bots)
            {
                var classicBot = GetOrInitializeBot(managedGame, bot);
                var evt = classicBot.DetermineMiddlePriorityInPhaseAction(eventsPerBot[bot.Seat]);
                if (evt == null) continue;
                await ValidateAndExecute(evt, managedGame, false);
                return;
            }
            
            foreach (var bot in bots)
            {
                var classicBot = GetOrInitializeBot(managedGame, bot);
                var evt = classicBot.DetermineLowPriorityInPhaseAction(eventsPerBot[bot.Seat]);
                if (evt == null) continue;
                await ValidateAndExecute(evt, managedGame, false);
                return;
            }
            
            if (botsActAsHosts)
                foreach (var bot in bots)
                {
                    var classicBot = GetOrInitializeBot(managedGame, bot);
                    var evt = classicBot.DetermineEndPhaseAction(eventsPerBot[bot.Seat]);
                    if (evt == null) continue;
                    await ValidateAndExecute(evt, managedGame, true);
                    return;
                }
            
            
        }
    }

    private static IBot GetOrInitializeBot(ManagedGame game, Player player)
    {
        if (game.Bots.TryGetValue(player.Faction, out var bot))
        {
            bot.SetGameAndPlayer(game.Game, player);
            return bot;
        }
        
        bot = new ClassicBot(game.Game, player, BotParameters.GetDefaultParameters(player.Faction));
        game.Bots.Add(player.Faction, bot);
        return bot;
    }
    
    private static int DetermineBotDelay(MainPhase phase, GameEvent e)
    {
        if (phase is MainPhase.Resurrection or MainPhase.Charity || e is AllyPermission or DealOffered or SetShipmentPermission)
            return 800;
        
        if (e is Bid)
            return 1200;
        
        if (phase is MainPhase.ShipmentAndMove or MainPhase.Battle)
            return 4800;
        
        return 2000;
    }

    private async Task SendMailAndStatistics(ManagedGame game)
    {
        await SendEndOfGameMail(game);
        await SendGameStatistics(game.Game);
    }
}

