using System.Linq;
using System.Threading.Tasks;
using Treachery.Shared;
using Treachery.Shared.Model;

namespace Treachery.Server;

public partial class GameHub
{
    public async Task<VoidResult> RequestEstablishPlayers(string playerToken, string gameToken, EstablishPlayers e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestEndPhase(string playerToken, string gameToken, EndPhase e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestDonated(string playerToken, string gameToken, Donated e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestResourcesTransferred(string playerToken, string gameToken, ResourcesTransferred e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestFactionSelected(string playerToken, string gameToken, FactionSelected e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestFactionTradeOffered(string playerToken, string gameToken, FactionTradeOffered e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestPerformSetup(string playerToken, string gameToken, PerformSetup e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestCardsDetermined(string playerToken, string gameToken, CardsDetermined e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestPerformYellowSetup(string playerToken, string gameToken, PerformYellowSetup e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestBluePrediction(string playerToken, string gameToken, BluePrediction e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestCharityClaimed(string playerToken, string gameToken, CharityClaimed e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestPerformBluePlacement(string playerToken, string gameToken, PerformBluePlacement e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestTraitorsSelected(string playerToken, string gameToken, TraitorsSelected e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestStormSpellPlayed(string playerToken, string gameToken, StormSpellPlayed e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestTestingStationUsed(string playerToken, string gameToken, TestingStationUsed e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestTakeLosses(string playerToken, string gameToken, TakeLosses e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestMetheorPlayed(string playerToken, string gameToken, MetheorPlayed e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestYellowSentMonster(string playerToken, string gameToken, YellowSentMonster e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestYellowRidesMonster(string playerToken, string gameToken, YellowRidesMonster e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestAllianceOffered(string playerToken, string gameToken, AllianceOffered e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestAllianceBroken(string playerToken, string gameToken, AllianceBroken e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestBid(string playerToken, string gameToken, Bid e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestRevival(string playerToken, string gameToken, Revival e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestBlueBattleAnnouncement(string playerToken, string gameToken, BlueBattleAnnouncement e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestShipment(string playerToken, string gameToken, Shipment e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestOrangeDelay(string playerToken, string gameToken, OrangeDelay e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestBlueAccompanies(string playerToken, string gameToken, BlueAccompanies e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestBlueFlip(string playerToken, string gameToken, BlueFlip e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestMove(string playerToken, string gameToken, Move e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestCaravan(string playerToken, string gameToken, Caravan e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestBattleInitiated(string playerToken, string gameToken, BattleInitiated e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestBattle(string playerToken, string gameToken, Battle e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestBattleRevision(string playerToken, string gameToken, BattleRevision e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestTreacheryCalled(string playerToken, string gameToken, TreacheryCalled e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestBattleConcluded(string playerToken, string gameToken, BattleConcluded e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestClairvoyancePlayed(string playerToken, string gameToken, ClairVoyancePlayed e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestClairvoyanceAnswered(string playerToken, string gameToken, ClairVoyanceAnswered e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestRaiseDeadPlayed(string playerToken, string gameToken, RaiseDeadPlayed e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestKarma(string playerToken, string gameToken, Karma e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestKarmaFreeRevival(string playerToken, string gameToken, KarmaFreeRevival e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestKarmaShipmentPrevention(string playerToken, string gameToken, KarmaShipmentPrevention e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestKarmaRevivalPrevention(string playerToken, string gameToken, KarmaRevivalPrevention e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestKarmaHandSwapInitiated(string playerToken, string gameToken, KarmaHandSwapInitiated e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestKarmaHandSwap(string playerToken, string gameToken, KarmaHandSwap e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestKarmaMonster(string playerToken, string gameToken, KarmaMonster e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestKarmaWhiteBuy(string playerToken, string gameToken, KarmaWhiteBuy e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestAllyPermission(string playerToken, string gameToken, AllyPermission e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestMulliganPerformed(string playerToken, string gameToken, MulliganPerformed e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestFaceDancerRevealed(string playerToken, string gameToken, FaceDancerRevealed e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestFaceDanced(string playerToken, string gameToken, FaceDanced e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestFaceDancerReplaced(string playerToken, string gameToken, FaceDancerReplaced e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestSetIncreasedRevivalLimits(string playerToken, string gameToken, SetIncreasedRevivalLimits e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestSetShipmentPermission(string playerToken, string gameToken, SetShipmentPermission e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestRequestPurpleRevival(string playerToken, string gameToken, RequestPurpleRevival e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestAcceptOrCancelPurpleRevival(string playerToken, string gameToken, AcceptOrCancelPurpleRevival e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestPerformHmsPlacement(string playerToken, string gameToken, PerformHmsPlacement e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestPerformHmsMovement(string playerToken, string gameToken, PerformHmsMovement e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestKarmaHmsMovement(string playerToken, string gameToken, KarmaHmsMovement e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestAmalPlayed(string playerToken, string gameToken, AmalPlayed e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestGreyRemovedCardFromAuction(string playerToken, string gameToken, GreyRemovedCardFromAuction e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestGreySelectedStartingCard(string playerToken, string gameToken, GreySelectedStartingCard e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestGreySwappedCardOnBid(string playerToken, string gameToken, GreySwappedCardOnBid e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestHarvesterPlayed(string playerToken, string gameToken, HarvesterPlayed e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestPoisonToothCancelled(string playerToken, string gameToken, PoisonToothCancelled e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestReplacedCardWon(string playerToken, string gameToken, ReplacedCardWon e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestThumperPlayed(string playerToken, string gameToken, ThumperPlayed e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestVoice(string playerToken, string gameToken, Voice e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestPrescience(string playerToken, string gameToken, Prescience e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestKarmaPrescience(string playerToken, string gameToken, KarmaPrescience e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestRedBidSupport(string playerToken, string gameToken, RedBidSupport e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestDealOffered(string playerToken, string gameToken, DealOffered e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestDealAccepted(string playerToken, string gameToken, DealAccepted e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestDiscoveryEntered(string playerToken, string gameToken, DiscoveryEntered e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestStormDialled(string playerToken, string gameToken, StormDialled e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestHideSecrets(string playerToken, string gameToken, HideSecrets e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestPlayerReplaced(string playerToken, string gameToken, PlayerReplaced e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestBrownDiscarded(string playerToken, string gameToken, BrownDiscarded e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestRedDiscarded(string playerToken, string gameToken, RedDiscarded e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestBrownEconomics(string playerToken, string gameToken, BrownEconomics e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestCardTraded(string playerToken, string gameToken, CardTraded e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestKarmaBrownDiscard(string playerToken, string gameToken, KarmaBrownDiscard e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestAuditCancelled(string playerToken, string gameToken, AuditCancelled e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestAudited(string playerToken, string gameToken, Audited e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestBrownMovePrevention(string playerToken, string gameToken, BrownMovePrevention e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestBrownKarmaPrevention(string playerToken, string gameToken, BrownKarmaPrevention e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestBrownExtraMove(string playerToken, string gameToken, BrownExtraMove e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestBrownFreeRevivalPrevention(string playerToken, string gameToken, BrownFreeRevivalPrevention e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestBrownRemoveForce(string playerToken, string gameToken, BrownRemoveForce e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestWhiteAnnouncesBlackMarket(string playerToken, string gameToken, WhiteAnnouncesBlackMarket e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestBlackMarketBid(string playerToken, string gameToken, BlackMarketBid e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestWhiteAnnouncesAuction(string playerToken, string gameToken, WhiteAnnouncesAuction e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestWhiteSpecifiesAuction(string playerToken, string gameToken, WhiteSpecifiesAuction e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestWhiteKeepsUnsoldCard(string playerToken, string gameToken, WhiteKeepsUnsoldCard e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestWhiteRevealedNoField(string playerToken, string gameToken, WhiteRevealedNoField e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestWhiteGaveCard(string playerToken, string gameToken, WhiteGaveCard e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestCardGiven(string playerToken, string gameToken, CardGiven e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestRockWasMelted(string playerToken, string gameToken, RockWasMelted e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestResidualPlayed(string playerToken, string gameToken, ResidualPlayed e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestFlightUsed(string playerToken, string gameToken, FlightUsed e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestFlightDiscoveryUsed(string playerToken, string gameToken, FlightDiscoveryUsed e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestDistransUsed(string playerToken, string gameToken, DistransUsed e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestDiscardedTaken(string playerToken, string gameToken, DiscardedTaken e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestDiscardedSearchedAnnounced(string playerToken, string gameToken, DiscardedSearchedAnnounced e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestDiscardedSearched(string playerToken, string gameToken, DiscardedSearched e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestJuicePlayed(string playerToken, string gameToken, JuicePlayed e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestPortableAntidoteUsed(string playerToken, string gameToken, PortableAntidoteUsed e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestBureaucracy(string playerToken, string gameToken, Bureaucracy e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestDiplomacy(string playerToken, string gameToken, Diplomacy e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestSkillAssigned(string playerToken, string gameToken, SkillAssigned e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestSwitchedSkilledLeader(string playerToken, string gameToken, SwitchedSkilledLeader e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestThought(string playerToken, string gameToken, Thought e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestThoughtAnswered(string playerToken, string gameToken, ThoughtAnswered e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestHmsAdvantageChosen(string playerToken, string gameToken, HMSAdvantageChosen e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestRetreat(string playerToken, string gameToken, Retreat e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestPlanetology(string playerToken, string gameToken, Planetology e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestCaptured(string playerToken, string gameToken, Captured e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestNexusCardDrawn(string playerToken, string gameToken, NexusCardDrawn e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestTerrorPlanted(string playerToken, string gameToken, TerrorPlanted e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestTerrorRevealed(string playerToken, string gameToken, TerrorRevealed e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestDiscoveryRevealed(string playerToken, string gameToken, DiscoveryRevealed e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestAmbassadorPlaced(string playerToken, string gameToken, AmbassadorPlaced e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestAmbassadorActivated(string playerToken, string gameToken, AmbassadorActivated e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestExtortionPrevented(string playerToken, string gameToken, ExtortionPrevented e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestDiscarded(string playerToken, string gameToken, Discarded e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestAllianceByTerror(string playerToken, string gameToken, AllianceByTerror e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestNexusVoted(string playerToken, string gameToken, NexusVoted e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestAllianceByAmbassador(string playerToken, string gameToken, AllianceByAmbassador e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestLoserConcluded(string playerToken, string gameToken, LoserConcluded e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestPerformCyanSetup(string playerToken, string gameToken, PerformCyanSetup e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestDivideResources(string playerToken, string gameToken, DivideResources e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestDivideResourcesAccepted(string playerToken, string gameToken, DivideResourcesAccepted e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestBattleClaimed(string playerToken, string gameToken, BattleClaimed e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestKarmaPinkDial(string playerToken, string gameToken, KarmaPinkDial e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestTraitorDiscarded(string playerToken, string gameToken, TraitorDiscarded e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestNexusPlayed(string playerToken, string gameToken, NexusPlayed e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestResourcesAudited(string playerToken, string gameToken, ResourcesAudited e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    public async Task<VoidResult> RequestRecruitsPlayed(string playerToken, string gameToken, RecruitsPlayed e) { return await ProcessGameEvent(playerToken, gameToken, e); }
    
    public async Task<VoidResult> SetTimer(string playerToken, string gameToken, int value)
    {
        if (!AreValid(playerToken, gameToken, out var playerId, out var game, out var error))
            return error;

        if (!game.IsHost(playerId))
            return Error("You are not the host");
        
        await Clients.Group(gameToken).HandleSetTimer(value);
        return Success();
    }

    
    private async Task<VoidResult> ProcessGameEvent<TEvent>(string playerToken, string gameToken, TEvent e) where TEvent : GameEvent
    {
        if (!AreValid(playerToken, gameToken, out var playerId, out var game, out var error))
            return error;
        
        e.Initialize(game.Game);
        return await ValidateAndExecute(gameToken, e, game, game.Hosts.Contains(playerId));
    }

    private async Task<VoidResult> ValidateAndExecute<TEvent>(string gameToken, TEvent e, ManagedGame game, bool isHost)
        where TEvent : GameEvent
    {
        var validationResult = e.Execute(true, isHost);
        
        if (validationResult != null)
        {
            return Error(validationResult.ToString());
        }

        if (game.Game.CurrentMainPhase is MainPhase.Ended && !FinishedGames.ContainsKey(gameToken))
        {
            FinishedGames.TryAdd(gameToken, game.Game);
            await SendMailAndStatistics(game);
        }

        await Clients.Group(gameToken).HandleGameEvent(e, game.Game.History.Count);
        
        var botDelay = DetermineBotDelay(game.Game.CurrentMainPhase, e);
        _ = Task.Delay(botDelay).ContinueWith(e => PerformBotEvent(gameToken, game));
        
        return Success();
    }

    private async Task PerformBotEvent(string gameToken, ManagedGame managedGame)
    {
        var game = managedGame.Game;
        
        if (!managedGame.BotsArePaused && game.CurrentPhase > Phase.AwaitingPlayers)
        {
            var bots = Deck<Player>.Randomize(game.Players.Where(p => p.IsBot));

            foreach (var bot in bots)
            {
                var events = game.GetApplicableEvents(bot, false);
                var evt = bot.DetermineHighestPrioInPhaseAction(events);

                if (evt != null)
                {
                    await ValidateAndExecute(gameToken, evt, managedGame, false);
                    return;
                }
            }

            foreach (var bot in bots)
            {
                var evts = game.GetApplicableEvents(bot, false);
                var evt = bot.DetermineHighPrioInPhaseAction(evts);

                if (evt != null)
                {
                    await ValidateAndExecute(gameToken, evt, managedGame, false);
                    return;
                }
            }

            foreach (var bot in bots)
            {
                var evts = game.GetApplicableEvents(bot, false);
                var evt = bot.DetermineMiddlePrioInPhaseAction(evts);

                if (evt != null)
                {
                    await ValidateAndExecute(gameToken, evt, managedGame, false);
                    return;
                }
            }

            foreach (var bot in bots)
            {
                var evts = game.GetApplicableEvents(bot, false);
                var evt = bot.DetermineLowPrioInPhaseAction(evts);

                if (evt != null)
                {
                    await ValidateAndExecute(gameToken, evt, managedGame, false);
                    return;
                }
            }
        }
    }
    
    private static int DetermineBotDelay(MainPhase phase, GameEvent e)
    {
        if (phase == MainPhase.Resurrection || phase == MainPhase.Charity || e is AllyPermission || e is DealOffered || e is SetShipmentPermission)
            return 300;
        if (e is Bid)
            return 800;
        if (phase == MainPhase.ShipmentAndMove)
            return 3200;
        if (phase == MainPhase.Battle)
            return 3200;
        return 1600;
    }

    private async Task SendMailAndStatistics(ManagedGame game)
    {
        var state = GameState.GetStateAsString(game.Game);
        SendEndOfGameMail(state, game.Info);
        await SendGameStatistics(game.Game);
    }
    
    
}

