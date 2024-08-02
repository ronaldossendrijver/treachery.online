using System.Linq;
using System.Threading.Tasks;
using Treachery.Shared;
using Treachery.Shared.Model;

namespace Treachery.Server;

public partial class GameHub
{
    public async Task<VoidResult> RequestChangeSettings(string playerToken, string gameToken, ChangeSettings e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestEstablishPlayers(string playerToken, string gameToken, EstablishPlayers e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestEndPhase(string playerToken, string gameToken, EndPhase e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestDonated(string playerToken, string gameToken, Donated e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestResourcesTransferred(string playerToken, string gameToken, ResourcesTransferred e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestFactionSelected(string playerToken, string gameToken, FactionSelected e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestFactionTradeOffered(string playerToken, string gameToken, FactionTradeOffered e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestPerformSetup(string playerToken, string gameToken, PerformSetup e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestCardsDetermined(string playerToken, string gameToken, CardsDetermined e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestPerformYellowSetup(string playerToken, string gameToken, PerformYellowSetup e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestBluePrediction(string playerToken, string gameToken, BluePrediction e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestCharityClaimed(string playerToken, string gameToken, CharityClaimed e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestPerformBluePlacement(string playerToken, string gameToken, PerformBluePlacement e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestTraitorsSelected(string playerToken, string gameToken, TraitorsSelected e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestStormSpellPlayed(string playerToken, string gameToken, StormSpellPlayed e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestTestingStationUsed(string playerToken, string gameToken, TestingStationUsed e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestTakeLosses(string playerToken, string gameToken, TakeLosses e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestMetheorPlayed(string playerToken, string gameToken, MetheorPlayed e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestYellowSentMonster(string playerToken, string gameToken, YellowSentMonster e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestYellowRidesMonster(string playerToken, string gameToken, YellowRidesMonster e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestAllianceOffered(string playerToken, string gameToken, AllianceOffered e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestAllianceBroken(string playerToken, string gameToken, AllianceBroken e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestBid(string playerToken, string gameToken, Bid e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestRevival(string playerToken, string gameToken, Revival e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestBlueBattleAnnouncement(string playerToken, string gameToken, BlueBattleAnnouncement e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestShipment(string playerToken, string gameToken, Shipment e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestOrangeDelay(string playerToken, string gameToken, OrangeDelay e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestBlueAccompanies(string playerToken, string gameToken, BlueAccompanies e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestBlueFlip(string playerToken, string gameToken, BlueFlip e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestMove(string playerToken, string gameToken, Move e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestCaravan(string playerToken, string gameToken, Caravan e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestBattleInitiated(string playerToken, string gameToken, BattleInitiated e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestBattle(string playerToken, string gameToken, Battle e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestBattleRevision(string playerToken, string gameToken, BattleRevision e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestTreacheryCalled(string playerToken, string gameToken, TreacheryCalled e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestBattleConcluded(string playerToken, string gameToken, BattleConcluded e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestClairvoyancePlayed(string playerToken, string gameToken, ClairVoyancePlayed e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestClairvoyanceAnswered(string playerToken, string gameToken, ClairVoyanceAnswered e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestRaiseDeadPlayed(string playerToken, string gameToken, RaiseDeadPlayed e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestKarma(string playerToken, string gameToken, Karma e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestKarmaFreeRevival(string playerToken, string gameToken, KarmaFreeRevival e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestKarmaShipmentPrevention(string playerToken, string gameToken, KarmaShipmentPrevention e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestKarmaRevivalPrevention(string playerToken, string gameToken, KarmaRevivalPrevention e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestKarmaHandSwapInitiated(string playerToken, string gameToken, KarmaHandSwapInitiated e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestKarmaHandSwap(string playerToken, string gameToken, KarmaHandSwap e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestKarmaMonster(string playerToken, string gameToken, KarmaMonster e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestKarmaWhiteBuy(string playerToken, string gameToken, KarmaWhiteBuy e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestAllyPermission(string playerToken, string gameToken, AllyPermission e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestMulliganPerformed(string playerToken, string gameToken, MulliganPerformed e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestFaceDancerRevealed(string playerToken, string gameToken, FaceDancerRevealed e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestFaceDanced(string playerToken, string gameToken, FaceDanced e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestFaceDancerReplaced(string playerToken, string gameToken, FaceDancerReplaced e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestSetIncreasedRevivalLimits(string playerToken, string gameToken, SetIncreasedRevivalLimits e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestSetShipmentPermission(string playerToken, string gameToken, SetShipmentPermission e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestRequestPurpleRevival(string playerToken, string gameToken, RequestPurpleRevival e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestAcceptOrCancelPurpleRevival(string playerToken, string gameToken, AcceptOrCancelPurpleRevival e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestPerformHmsPlacement(string playerToken, string gameToken, PerformHmsPlacement e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestPerformHmsMovement(string playerToken, string gameToken, PerformHmsMovement e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestKarmaHmsMovement(string playerToken, string gameToken, KarmaHmsMovement e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestAmalPlayed(string playerToken, string gameToken, AmalPlayed e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestGreyRemovedCardFromAuction(string playerToken, string gameToken, GreyRemovedCardFromAuction e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestGreySelectedStartingCard(string playerToken, string gameToken, GreySelectedStartingCard e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestGreySwappedCardOnBid(string playerToken, string gameToken, GreySwappedCardOnBid e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestHarvesterPlayed(string playerToken, string gameToken, HarvesterPlayed e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestPoisonToothCancelled(string playerToken, string gameToken, PoisonToothCancelled e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestReplacedCardWon(string playerToken, string gameToken, ReplacedCardWon e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestThumperPlayed(string playerToken, string gameToken, ThumperPlayed e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestVoice(string playerToken, string gameToken, Voice e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestPrescience(string playerToken, string gameToken, Prescience e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestKarmaPrescience(string playerToken, string gameToken, KarmaPrescience e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestRedBidSupport(string playerToken, string gameToken, RedBidSupport e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestDealOffered(string playerToken, string gameToken, DealOffered e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestDealAccepted(string playerToken, string gameToken, DealAccepted e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestDiscoveryEntered(string playerToken, string gameToken, DiscoveryEntered e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestStormDialled(string playerToken, string gameToken, StormDialled e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestHideSecrets(string playerToken, string gameToken, HideSecrets e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestPlayerReplaced(string playerToken, string gameToken, PlayerReplaced e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestBrownDiscarded(string playerToken, string gameToken, BrownDiscarded e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestRedDiscarded(string playerToken, string gameToken, RedDiscarded e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestBrownEconomics(string playerToken, string gameToken, BrownEconomics e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestCardTraded(string playerToken, string gameToken, CardTraded e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestKarmaBrownDiscard(string playerToken, string gameToken, KarmaBrownDiscard e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestAuditCancelled(string playerToken, string gameToken, AuditCancelled e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestAudited(string playerToken, string gameToken, Audited e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestBrownMovePrevention(string playerToken, string gameToken, BrownMovePrevention e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestBrownKarmaPrevention(string playerToken, string gameToken, BrownKarmaPrevention e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestBrownExtraMove(string playerToken, string gameToken, BrownExtraMove e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestBrownFreeRevivalPrevention(string playerToken, string gameToken, BrownFreeRevivalPrevention e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestBrownRemoveForce(string playerToken, string gameToken, BrownRemoveForce e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestWhiteAnnouncesBlackMarket(string playerToken, string gameToken, WhiteAnnouncesBlackMarket e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestBlackMarketBid(string playerToken, string gameToken, BlackMarketBid e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestWhiteAnnouncesAuction(string playerToken, string gameToken, WhiteAnnouncesAuction e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestWhiteSpecifiesAuction(string playerToken, string gameToken, WhiteSpecifiesAuction e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestWhiteKeepsUnsoldCard(string playerToken, string gameToken, WhiteKeepsUnsoldCard e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestWhiteRevealedNoField(string playerToken, string gameToken, WhiteRevealedNoField e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestWhiteGaveCard(string playerToken, string gameToken, WhiteGaveCard e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestCardGiven(string playerToken, string gameToken, CardGiven e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestRockWasMelted(string playerToken, string gameToken, RockWasMelted e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestResidualPlayed(string playerToken, string gameToken, ResidualPlayed e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestFlightUsed(string playerToken, string gameToken, FlightUsed e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestFlightDiscoveryUsed(string playerToken, string gameToken, FlightDiscoveryUsed e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestDistransUsed(string playerToken, string gameToken, DistransUsed e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestDiscardedTaken(string playerToken, string gameToken, DiscardedTaken e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestDiscardedSearchedAnnounced(string playerToken, string gameToken, DiscardedSearchedAnnounced e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestDiscardedSearched(string playerToken, string gameToken, DiscardedSearched e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestJuicePlayed(string playerToken, string gameToken, JuicePlayed e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestPortableAntidoteUsed(string playerToken, string gameToken, PortableAntidoteUsed e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestBureaucracy(string playerToken, string gameToken, Bureaucracy e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestDiplomacy(string playerToken, string gameToken, Diplomacy e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestSkillAssigned(string playerToken, string gameToken, SkillAssigned e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestSwitchedSkilledLeader(string playerToken, string gameToken, SwitchedSkilledLeader e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestThought(string playerToken, string gameToken, Thought e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestThoughtAnswered(string playerToken, string gameToken, ThoughtAnswered e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestHmsAdvantageChosen(string playerToken, string gameToken, HMSAdvantageChosen e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestRetreat(string playerToken, string gameToken, Retreat e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestPlanetology(string playerToken, string gameToken, Planetology e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestCaptured(string playerToken, string gameToken, Captured e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestNexusCardDrawn(string playerToken, string gameToken, NexusCardDrawn e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestTerrorPlanted(string playerToken, string gameToken, TerrorPlanted e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestTerrorRevealed(string playerToken, string gameToken, TerrorRevealed e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestDiscoveryRevealed(string playerToken, string gameToken, DiscoveryRevealed e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestAmbassadorPlaced(string playerToken, string gameToken, AmbassadorPlaced e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestAmbassadorActivated(string playerToken, string gameToken, AmbassadorActivated e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestExtortionPrevented(string playerToken, string gameToken, ExtortionPrevented e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestDiscarded(string playerToken, string gameToken, Discarded e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestAllianceByTerror(string playerToken, string gameToken, AllianceByTerror e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestNexusVoted(string playerToken, string gameToken, NexusVoted e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestAllianceByAmbassador(string playerToken, string gameToken, AllianceByAmbassador e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestLoserConcluded(string playerToken, string gameToken, LoserConcluded e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestPerformCyanSetup(string playerToken, string gameToken, PerformCyanSetup e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestDivideResources(string playerToken, string gameToken, DivideResources e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestDivideResourcesAccepted(string playerToken, string gameToken, DivideResourcesAccepted e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestBattleClaimed(string playerToken, string gameToken, BattleClaimed e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestKarmaPinkDial(string playerToken, string gameToken, KarmaPinkDial e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestTraitorDiscarded(string playerToken, string gameToken, TraitorDiscarded e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestNexusPlayed(string playerToken, string gameToken, NexusPlayed e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestResourcesAudited(string playerToken, string gameToken, ResourcesAudited e) => await ProcessGameEvent(playerToken, gameToken, e);
    public async Task<VoidResult> RequestRecruitsPlayed(string playerToken, string gameToken, RecruitsPlayed e) => await ProcessGameEvent(playerToken, gameToken, e);
    
    public async Task<VoidResult> SetTimer(string playerToken, string gameToken, int value)
    {
        if (!AreValid(playerToken, gameToken, out var user, out var game, out var error))
            return error;

        if (!game.Game.IsHost(user.Id))
            return Error("You are not a host");
        
        await Clients.Group(gameToken).HandleSetTimer(value);
        return Success();
    }

    
    private async Task<VoidResult> ProcessGameEvent<TEvent>(string playerToken, string gameToken, TEvent e) where TEvent : GameEvent
    {
        if (!AreValid(playerToken, gameToken, out var user, out var game, out var error))
            return error;
        
        e.Initialize(game.Game);
        return await ValidateAndExecute(gameToken, e, game, game.Game.IsHost(user.Id));
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
        SendEndOfGameMail(state, GameInfo.Extract(game));
        await SendGameStatistics(game.Game);
    }
    
    
}

