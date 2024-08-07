using System.Linq;
using System.Threading.Tasks;
using Treachery.Shared;
using Treachery.Shared.Model;

namespace Treachery.Server;

public partial class GameHub
{
    public async Task<VoidResult> RequestChangeSettings(string userToken, string gameToken, ChangeSettings e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestEstablishPlayers(string userToken, string gameToken, EstablishPlayers e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestEndPhase(string userToken, string gameToken, EndPhase e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestDonated(string userToken, string gameToken, Donated e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestResourcesTransferred(string userToken, string gameToken, ResourcesTransferred e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestFactionSelected(string userToken, string gameToken, FactionSelected e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestFactionTradeOffered(string userToken, string gameToken, FactionTradeOffered e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestPerformSetup(string userToken, string gameToken, PerformSetup e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestCardsDetermined(string userToken, string gameToken, CardsDetermined e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestPerformYellowSetup(string userToken, string gameToken, PerformYellowSetup e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestBluePrediction(string userToken, string gameToken, BluePrediction e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestCharityClaimed(string userToken, string gameToken, CharityClaimed e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestPerformBluePlacement(string userToken, string gameToken, PerformBluePlacement e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestTraitorsSelected(string userToken, string gameToken, TraitorsSelected e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestStormSpellPlayed(string userToken, string gameToken, StormSpellPlayed e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestTestingStationUsed(string userToken, string gameToken, TestingStationUsed e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestTakeLosses(string userToken, string gameToken, TakeLosses e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestMetheorPlayed(string userToken, string gameToken, MetheorPlayed e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestYellowSentMonster(string userToken, string gameToken, YellowSentMonster e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestYellowRidesMonster(string userToken, string gameToken, YellowRidesMonster e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestAllianceOffered(string userToken, string gameToken, AllianceOffered e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestAllianceBroken(string userToken, string gameToken, AllianceBroken e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestBid(string userToken, string gameToken, Bid e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestRevival(string userToken, string gameToken, Revival e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestBlueBattleAnnouncement(string userToken, string gameToken, BlueBattleAnnouncement e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestShipment(string userToken, string gameToken, Shipment e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestOrangeDelay(string userToken, string gameToken, OrangeDelay e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestBlueAccompanies(string userToken, string gameToken, BlueAccompanies e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestBlueFlip(string userToken, string gameToken, BlueFlip e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestMove(string userToken, string gameToken, Move e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestCaravan(string userToken, string gameToken, Caravan e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestBattleInitiated(string userToken, string gameToken, BattleInitiated e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestBattle(string userToken, string gameToken, Battle e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestBattleRevision(string userToken, string gameToken, BattleRevision e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestTreacheryCalled(string userToken, string gameToken, TreacheryCalled e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestBattleConcluded(string userToken, string gameToken, BattleConcluded e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestClairvoyancePlayed(string userToken, string gameToken, ClairVoyancePlayed e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestClairvoyanceAnswered(string userToken, string gameToken, ClairVoyanceAnswered e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestRaiseDeadPlayed(string userToken, string gameToken, RaiseDeadPlayed e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestKarma(string userToken, string gameToken, Karma e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestKarmaFreeRevival(string userToken, string gameToken, KarmaFreeRevival e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestKarmaShipmentPrevention(string userToken, string gameToken, KarmaShipmentPrevention e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestKarmaRevivalPrevention(string userToken, string gameToken, KarmaRevivalPrevention e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestKarmaHandSwapInitiated(string userToken, string gameToken, KarmaHandSwapInitiated e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestKarmaHandSwap(string userToken, string gameToken, KarmaHandSwap e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestKarmaMonster(string userToken, string gameToken, KarmaMonster e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestKarmaWhiteBuy(string userToken, string gameToken, KarmaWhiteBuy e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestAllyPermission(string userToken, string gameToken, AllyPermission e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestMulliganPerformed(string userToken, string gameToken, MulliganPerformed e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestFaceDancerRevealed(string userToken, string gameToken, FaceDancerRevealed e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestFaceDanced(string userToken, string gameToken, FaceDanced e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestFaceDancerReplaced(string userToken, string gameToken, FaceDancerReplaced e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestSetIncreasedRevivalLimits(string userToken, string gameToken, SetIncreasedRevivalLimits e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestSetShipmentPermission(string userToken, string gameToken, SetShipmentPermission e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestRequestPurpleRevival(string userToken, string gameToken, RequestPurpleRevival e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestAcceptOrCancelPurpleRevival(string userToken, string gameToken, AcceptOrCancelPurpleRevival e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestPerformHmsPlacement(string userToken, string gameToken, PerformHmsPlacement e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestPerformHmsMovement(string userToken, string gameToken, PerformHmsMovement e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestKarmaHmsMovement(string userToken, string gameToken, KarmaHmsMovement e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestAmalPlayed(string userToken, string gameToken, AmalPlayed e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestGreyRemovedCardFromAuction(string userToken, string gameToken, GreyRemovedCardFromAuction e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestGreySelectedStartingCard(string userToken, string gameToken, GreySelectedStartingCard e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestGreySwappedCardOnBid(string userToken, string gameToken, GreySwappedCardOnBid e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestHarvesterPlayed(string userToken, string gameToken, HarvesterPlayed e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestPoisonToothCancelled(string userToken, string gameToken, PoisonToothCancelled e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestReplacedCardWon(string userToken, string gameToken, ReplacedCardWon e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestThumperPlayed(string userToken, string gameToken, ThumperPlayed e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestVoice(string userToken, string gameToken, Voice e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestPrescience(string userToken, string gameToken, Prescience e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestKarmaPrescience(string userToken, string gameToken, KarmaPrescience e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestRedBidSupport(string userToken, string gameToken, RedBidSupport e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestDealOffered(string userToken, string gameToken, DealOffered e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestDealAccepted(string userToken, string gameToken, DealAccepted e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestDiscoveryEntered(string userToken, string gameToken, DiscoveryEntered e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestStormDialled(string userToken, string gameToken, StormDialled e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestHideSecrets(string userToken, string gameToken, HideSecrets e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestPlayerReplaced(string userToken, string gameToken, PlayerReplaced e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestBrownDiscarded(string userToken, string gameToken, BrownDiscarded e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestRedDiscarded(string userToken, string gameToken, RedDiscarded e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestBrownEconomics(string userToken, string gameToken, BrownEconomics e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestCardTraded(string userToken, string gameToken, CardTraded e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestKarmaBrownDiscard(string userToken, string gameToken, KarmaBrownDiscard e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestAuditCancelled(string userToken, string gameToken, AuditCancelled e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestAudited(string userToken, string gameToken, Audited e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestBrownMovePrevention(string userToken, string gameToken, BrownMovePrevention e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestBrownKarmaPrevention(string userToken, string gameToken, BrownKarmaPrevention e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestBrownExtraMove(string userToken, string gameToken, BrownExtraMove e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestBrownFreeRevivalPrevention(string userToken, string gameToken, BrownFreeRevivalPrevention e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestBrownRemoveForce(string userToken, string gameToken, BrownRemoveForce e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestWhiteAnnouncesBlackMarket(string userToken, string gameToken, WhiteAnnouncesBlackMarket e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestBlackMarketBid(string userToken, string gameToken, BlackMarketBid e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestWhiteAnnouncesAuction(string userToken, string gameToken, WhiteAnnouncesAuction e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestWhiteSpecifiesAuction(string userToken, string gameToken, WhiteSpecifiesAuction e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestWhiteKeepsUnsoldCard(string userToken, string gameToken, WhiteKeepsUnsoldCard e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestWhiteRevealedNoField(string userToken, string gameToken, WhiteRevealedNoField e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestWhiteGaveCard(string userToken, string gameToken, WhiteGaveCard e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestCardGiven(string userToken, string gameToken, CardGiven e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestRockWasMelted(string userToken, string gameToken, RockWasMelted e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestResidualPlayed(string userToken, string gameToken, ResidualPlayed e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestFlightUsed(string userToken, string gameToken, FlightUsed e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestFlightDiscoveryUsed(string userToken, string gameToken, FlightDiscoveryUsed e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestDistransUsed(string userToken, string gameToken, DistransUsed e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestDiscardedTaken(string userToken, string gameToken, DiscardedTaken e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestDiscardedSearchedAnnounced(string userToken, string gameToken, DiscardedSearchedAnnounced e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestDiscardedSearched(string userToken, string gameToken, DiscardedSearched e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestJuicePlayed(string userToken, string gameToken, JuicePlayed e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestPortableAntidoteUsed(string userToken, string gameToken, PortableAntidoteUsed e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestBureaucracy(string userToken, string gameToken, Bureaucracy e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestDiplomacy(string userToken, string gameToken, Diplomacy e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestSkillAssigned(string userToken, string gameToken, SkillAssigned e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestSwitchedSkilledLeader(string userToken, string gameToken, SwitchedSkilledLeader e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestThought(string userToken, string gameToken, Thought e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestThoughtAnswered(string userToken, string gameToken, ThoughtAnswered e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestHmsAdvantageChosen(string userToken, string gameToken, HMSAdvantageChosen e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestRetreat(string userToken, string gameToken, Retreat e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestPlanetology(string userToken, string gameToken, Planetology e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestCaptured(string userToken, string gameToken, Captured e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestNexusCardDrawn(string userToken, string gameToken, NexusCardDrawn e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestTerrorPlanted(string userToken, string gameToken, TerrorPlanted e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestTerrorRevealed(string userToken, string gameToken, TerrorRevealed e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestDiscoveryRevealed(string userToken, string gameToken, DiscoveryRevealed e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestAmbassadorPlaced(string userToken, string gameToken, AmbassadorPlaced e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestAmbassadorActivated(string userToken, string gameToken, AmbassadorActivated e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestExtortionPrevented(string userToken, string gameToken, ExtortionPrevented e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestDiscarded(string userToken, string gameToken, Discarded e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestAllianceByTerror(string userToken, string gameToken, AllianceByTerror e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestNexusVoted(string userToken, string gameToken, NexusVoted e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestAllianceByAmbassador(string userToken, string gameToken, AllianceByAmbassador e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestLoserConcluded(string userToken, string gameToken, LoserConcluded e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestPerformCyanSetup(string userToken, string gameToken, PerformCyanSetup e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestDivideResources(string userToken, string gameToken, DivideResources e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestDivideResourcesAccepted(string userToken, string gameToken, DivideResourcesAccepted e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestBattleClaimed(string userToken, string gameToken, BattleClaimed e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestKarmaPinkDial(string userToken, string gameToken, KarmaPinkDial e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestTraitorDiscarded(string userToken, string gameToken, TraitorDiscarded e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestNexusPlayed(string userToken, string gameToken, NexusPlayed e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestResourcesAudited(string userToken, string gameToken, ResourcesAudited e) => await ProcessGameEvent(userToken, gameToken, e);
    public async Task<VoidResult> RequestRecruitsPlayed(string userToken, string gameToken, RecruitsPlayed e) => await ProcessGameEvent(userToken, gameToken, e);
    
    public async Task<VoidResult> SetTimer(string userToken, string gameToken, int value)
    {
        if (!AreValid(userToken, gameToken, out var user, out var game, out var error))
            return error;

        if (!game.Game.IsHost(user.Id))
            return Error("You are not a host");
        
        await Clients.Group(gameToken).HandleSetTimer(value);
        return Success();
    }

    
    private async Task<VoidResult> ProcessGameEvent<TEvent>(string userToken, string gameToken, TEvent e) where TEvent : GameEvent
    {
        if (!AreValid(userToken, gameToken, out var user, out var game, out var error))
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

        if (game.Game.CurrentMainPhase is MainPhase.Ended && !game.StatisticsSent)
        {
            await SendMailAndStatistics(game);
            game.StatisticsSent = true;
        }

        await Clients.Group(gameToken).HandleGameEvent(e, game.Game.History.Count);
        
        var botDelay = DetermineBotDelay(game.Game.CurrentMainPhase, e);
        _ = Task.Delay(botDelay).ContinueWith(_ => PerformBotEvent(gameToken, game));
        
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
        SendEndOfGameMail(state, GameInfo.Extract(game, -1));
        await SendGameStatistics(game.Game);
    }
}

