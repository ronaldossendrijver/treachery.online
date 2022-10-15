using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Treachery.Client;
using Treachery.Shared;

namespace Treachery.Server
{
    public class GameHub : Hub
    {
        private static readonly bool logging = false;

        private readonly IConfiguration Configuration;

        public GameHub(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        #region MessagesFromPlayersToHost

        /* 
         * Requesting Joining and Rejoining of games (by PLAYERS and OBSERVERS)
         */

        public async Task RequestPlayerJoined(int hostID, PlayerJoined e)
        {
            await Channel("host", hostID).SendAsync("ReceiveRequest_PlayerJoined", Context.ConnectionId, e);
        }

        public async Task RequestPlayerRejoined(int hostID, PlayerRejoined e)
        {
            await Channel("host", hostID).SendAsync("ReceiveRequest_PlayerRejoined", Context.ConnectionId, e);
        }

        public async Task RequestObserverJoined(int hostID, ObserverJoined e)
        {
            await Channel("host", hostID).SendAsync("ReceiveRequest_ObserverJoined", Context.ConnectionId, e);
        }

        public async Task RequestObserverRejoined(int hostID, ObserverRejoined e)
        {
            await Channel("host", hostID).SendAsync("ReceiveRequest_ObserverRejoined", Context.ConnectionId, e);
        }

        public async Task RequestEstablishPlayers(int hostID, EstablishPlayers e) { await Request(hostID, e); }
        public async Task RequestEndPhase(int hostID, EndPhase e) { await Request(hostID, e); }
        public async Task RequestDonated(int hostID, Donated e) { await Request(hostID, e); }
        public async Task RequestFactionSelected(int hostID, FactionSelected e) { await Request(hostID, e); }
        public async Task RequestFactionTradeOffered(int hostID, FactionTradeOffered e) { await Request(hostID, e); }
        public async Task RequestPerformSetup(int hostID, PerformSetup e) { await Request(hostID, e); }
        public async Task RequestCardsDetermined(int hostID, CardsDetermined e) { await Request(hostID, e); }
        public async Task RequestPerformYellowSetup(int hostID, PerformYellowSetup e) { await Request(hostID, e); }
        public async Task RequestBluePrediction(int hostID, BluePrediction e) { await Request(hostID, e); }
        public async Task RequestCharityClaimed(int hostID, CharityClaimed e) { await Request(hostID, e); }
        public async Task RequestPerformBluePlacement(int hostID, PerformBluePlacement e) { await Request(hostID, e); }
        public async Task RequestTraitorsSelected(int hostID, TraitorsSelected e) { await Request(hostID, e); }
        public async Task RequestStormSpellPlayed(int hostID, StormSpellPlayed e) { await Request(hostID, e); }
        public async Task RequestTakeLosses(int hostID, TakeLosses e) { await Request(hostID, e); }
        public async Task RequestMetheorPlayed(int hostID, MetheorPlayed e) { await Request(hostID, e); }
        public async Task RequestYellowSentMonster(int hostID, YellowSentMonster e) { await Request(hostID, e); }
        public async Task RequestYellowRidesMonster(int hostID, YellowRidesMonster e) { await Request(hostID, e); }
        public async Task RequestAllianceOffered(int hostID, AllianceOffered e) { await Request(hostID, e); }
        public async Task RequestAllianceBroken(int hostID, AllianceBroken e) { await Request(hostID, e); }
        public async Task RequestBid(int hostID, Bid e) { await Request(hostID, e); }
        public async Task RequestRevival(int hostID, Revival e) { await Request(hostID, e); }
        public async Task RequestBlueBattleAnnouncement(int hostID, BlueBattleAnnouncement e) { await Request(hostID, e); }
        public async Task RequestShipment(int hostID, Shipment e) { await Request(hostID, e); }
        public async Task RequestOrangeDelay(int hostID, OrangeDelay e) { await Request(hostID, e); }
        public async Task RequestBlueAccompanies(int hostID, BlueAccompanies e) { await Request(hostID, e); }
        public async Task RequestBlueFlip(int hostID, BlueFlip e) { await Request(hostID, e); }
        public async Task RequestMove(int hostID, Move e) { await Request(hostID, e); }
        public async Task RequestCaravan(int hostID, Caravan e) { await Request(hostID, e); }
        public async Task RequestBattleInitiated(int hostID, BattleInitiated e) { await Request(hostID, e); }
        public async Task RequestBattle(int hostID, Battle e) { await Request(hostID, e); }
        public async Task RequestBattleRevision(int hostID, BattleRevision e) { await Request(hostID, e); }
        public async Task RequestTreacheryCalled(int hostID, TreacheryCalled e) { await Request(hostID, e); }
        public async Task RequestBattleConcluded(int hostID, BattleConcluded e) { await Request(hostID, e); }
        public async Task RequestClairvoyancePlayed(int hostID, ClairVoyancePlayed e) { await Request(hostID, e); }
        public async Task RequestClairvoyanceAnswered(int hostID, ClairVoyanceAnswered e) { await Request(hostID, e); }
        public async Task RequestRaiseDeadPlayed(int hostID, RaiseDeadPlayed e) { await Request(hostID, e); }
        public async Task RequestKarma(int hostID, Karma e) { await Request(hostID, e); }
        public async Task RequestKarmaFreeRevival(int hostID, KarmaFreeRevival e) { await Request(hostID, e); }
        public async Task RequestKarmaShipmentPrevention(int hostID, KarmaShipmentPrevention e) { await Request(hostID, e); }
        public async Task RequestKarmaRevivalPrevention(int hostID, KarmaRevivalPrevention e) { await Request(hostID, e); }
        public async Task RequestKarmaHandSwapInitiated(int hostID, KarmaHandSwapInitiated e) { await Request(hostID, e); }
        public async Task RequestKarmaHandSwap(int hostID, KarmaHandSwap e) { await Request(hostID, e); }
        public async Task RequestKarmaMonster(int hostID, KarmaMonster e) { await Request(hostID, e); }
        public async Task RequestKarmaWhiteBuy(int hostID, KarmaWhiteBuy e) { await Request(hostID, e); }
        public async Task RequestAllyPermission(int hostID, AllyPermission e) { await Request(hostID, e); }
        public async Task RequestMulliganPerformed(int hostID, MulliganPerformed e) { await Request(hostID, e); }
        public async Task RequestFaceDanced(int hostID, FaceDanced e) { await Request(hostID, e); }
        public async Task RequestFaceDancerReplaced(int hostID, FaceDancerReplaced e) { await Request(hostID, e); }
        public async Task RequestSetIncreasedRevivalLimits(int hostID, SetIncreasedRevivalLimits e) { await Request(hostID, e); }
        public async Task RequestRequestPurpleRevival(int hostID, RequestPurpleRevival e) { await Request(hostID, e); }
        public async Task RequestAcceptOrCancelPurpleRevival(int hostID, AcceptOrCancelPurpleRevival e) { await Request(hostID, e); }
        public async Task RequestPerformHmsPlacement(int hostID, PerformHmsPlacement e) { await Request(hostID, e); }
        public async Task RequestPerformHmsMovement(int hostID, PerformHmsMovement e) { await Request(hostID, e); }
        public async Task RequestKarmaHmsMovement(int hostID, KarmaHmsMovement e) { await Request(hostID, e); }
        public async Task RequestAmalPlayed(int hostID, AmalPlayed e) { await Request(hostID, e); }
        public async Task RequestGreyRemovedCardFromAuction(int hostID, GreyRemovedCardFromAuction e) { await Request(hostID, e); }
        public async Task RequestGreySelectedStartingCard(int hostID, GreySelectedStartingCard e) { await Request(hostID, e); }
        public async Task RequestGreySwappedCardOnBid(int hostID, GreySwappedCardOnBid e) { await Request(hostID, e); }
        public async Task RequestHarvesterPlayed(int hostID, HarvesterPlayed e) { await Request(hostID, e); }
        public async Task RequestPoisonToothCancelled(int hostID, PoisonToothCancelled e) { await Request(hostID, e); }
        public async Task RequestReplacedCardWon(int hostID, ReplacedCardWon e) { await Request(hostID, e); }
        public async Task RequestThumperPlayed(int hostID, ThumperPlayed e) { await Request(hostID, e); }
        public async Task RequestVoice(int hostID, Voice e) { await Request(hostID, e); }
        public async Task RequestPrescience(int hostID, Prescience e) { await Request(hostID, e); }
        public async Task RequestKarmaPrescience(int hostID, KarmaPrescience e) { await Request(hostID, e); }
        public async Task RequestRedBidSupport(int hostID, RedBidSupport e) { await Request(hostID, e); }
        public async Task RequestDealOffered(int hostID, DealOffered e) { await Request(hostID, e); }
        public async Task RequestDealAccepted(int hostID, DealAccepted e) { await Request(hostID, e); }
        public async Task RequestStormDialled(int hostID, StormDialled e) { await Request(hostID, e); }
        public async Task RequestHideSecrets(int hostID, HideSecrets e) { await Request(hostID, e); }
        public async Task RequestPlayerReplaced(int hostID, PlayerReplaced e) { await Request(hostID, e); }
        public async Task RequestBrownDiscarded(int hostID, BrownDiscarded e) { await Request(hostID, e); }
        public async Task RequestBrownEconomics(int hostID, BrownEconomics e) { await Request(hostID, e); }
        public async Task RequestCardTraded(int hostID, CardTraded e) { await Request(hostID, e); }
        public async Task RequestKarmaBrownDiscard(int hostID, KarmaBrownDiscard e) { await Request(hostID, e); }
        public async Task RequestAuditCancelled(int hostID, AuditCancelled e) { await Request(hostID, e); }
        public async Task RequestAudited(int hostID, Audited e) { await Request(hostID, e); }
        public async Task RequestBrownMovePrevention(int hostID, BrownMovePrevention e) { await Request(hostID, e); }
        public async Task RequestBrownKarmaPrevention(int hostID, BrownKarmaPrevention e) { await Request(hostID, e); }
        public async Task RequestBrownExtraMove(int hostID, BrownExtraMove e) { await Request(hostID, e); }
        public async Task RequestBrownFreeRevivalPrevention(int hostID, BrownFreeRevivalPrevention e) { await Request(hostID, e); }
        public async Task RequestBrownRemoveForce(int hostID, BrownRemoveForce e) { await Request(hostID, e); }
        public async Task RequestWhiteAnnouncesBlackMarket(int hostID, WhiteAnnouncesBlackMarket e) { await Request(hostID, e); }
        public async Task RequestBlackMarketBid(int hostID, BlackMarketBid e) { await Request(hostID, e); }
        public async Task RequestWhiteAnnouncesAuction(int hostID, WhiteAnnouncesAuction e) { await Request(hostID, e); }
        public async Task RequestWhiteSpecifiesAuction(int hostID, WhiteSpecifiesAuction e) { await Request(hostID, e); }
        public async Task RequestWhiteKeepsUnsoldCard(int hostID, WhiteKeepsUnsoldCard e) { await Request(hostID, e); }
        public async Task RequestWhiteRevealedNoField(int hostID, WhiteRevealedNoField e) { await Request(hostID, e); }
        public async Task RequestWhiteGaveCard(int hostID, WhiteGaveCard e) { await Request(hostID, e); }
        public async Task RequestRockWasMelted(int hostID, RockWasMelted e) { await Request(hostID, e); }
        public async Task RequestResidualPlayed(int hostID, ResidualPlayed e) { await Request(hostID, e); }
        public async Task RequestFlightUsed(int hostID, FlightUsed e) { await Request(hostID, e); }
        public async Task RequestDistransUsed(int hostID, DistransUsed e) { await Request(hostID, e); }
        public async Task RequestDiscardedTaken(int hostID, DiscardedTaken e) { await Request(hostID, e); }
        public async Task RequestDiscardedSearchedAnnounced(int hostID, DiscardedSearchedAnnounced e) { await Request(hostID, e); }
        public async Task RequestDiscardedSearched(int hostID, DiscardedSearched e) { await Request(hostID, e); }
        public async Task RequestJuicePlayed(int hostID, JuicePlayed e) { await Request(hostID, e); }
        public async Task RequestPortableAntidoteUsed(int hostID, PortableAntidoteUsed e) { await Request(hostID, e); }
        public async Task RequestBureaucracy(int hostID, Bureaucracy e) { await Request(hostID, e); }
        public async Task RequestDiplomacy(int hostID, Diplomacy e) { await Request(hostID, e); }
        public async Task RequestSkillAssigned(int hostID, SkillAssigned e) { await Request(hostID, e); }
        public async Task RequestSwitchedSkilledLeader(int hostID, SwitchedSkilledLeader e) { await Request(hostID, e); }
        public async Task RequestThought(int hostID, Thought e) { await Request(hostID, e); }
        public async Task RequestThoughtAnswered(int hostID, ThoughtAnswered e) { await Request(hostID, e); }
        public async Task RequestHMSAdvantageChosen(int hostID, HMSAdvantageChosen e) { await Request(hostID, e); }
        public async Task RequestRetreat(int hostID, Retreat e) { await Request(hostID, e); }
        public async Task RequestPlanetology(int hostID, Planetology e) { await Request(hostID, e); }
        public async Task RequestCaptured(int hostID, Captured e) { await Request(hostID, e); }
        public async Task RequestNexusCardDrawn(int hostID, NexusCardDrawn e) { await Request(hostID, e); }

        private async Task Request<GameEventType>(int hostID, GameEventType e) where GameEventType : GameEvent
        {
            Log("Request<" + e.GetType().Name + ">", hostID, e);
            await Channel("host", hostID).SendAsync("ReceiveRequest_" + typeof(GameEventType).Name, e);
        }

        public async Task ProcessHeartbeat(int hostID, string playerName)
        {
            await Channel("host", hostID).SendAsync("ProcessHeartbeat", playerName);
        }

        public async Task RequestChatMessage(int hostID, GameChatMessage e)
        {
            await Channel("host", hostID).SendAsync("RequestChatMessage", e);
        }

        public async Task SendVideo(int hostID, int playerPosition, byte[] data)
        {
            await Channel("viewers", hostID).SendAsync("ReceiveVideo", playerPosition, data);
        }

        #endregion MessagesFromPlayersToHost

        #region MessagesFromHost

        /* 
         * Approving Joining and Rejoining of games (by HOST)
         */

        public async Task AnnounceGame(GameInfo info)
        {
            await AddToChannel("host", info.HostID, Context.ConnectionId);
            await Clients.All.SendAsync("GameAvailable", info);
        }

        public async Task RespondPlayerJoined(int gameID, string playerConnectionID, int hostID, string deniedMessage)
        {
            if (deniedMessage == "")
            {
                await AddToChannel("players", gameID, playerConnectionID);
                await AddToChannel("viewers", hostID, playerConnectionID);
            }

            await Clients.Client(playerConnectionID).SendAsync("HandleJoinAsPlayer", hostID, deniedMessage);
        }

        public async Task RespondPlayerRejoined(int gameID, string playerConnectionID, int hostID, string deniedMessage)
        {
            Log("RespondPlayerRejoined", gameID, playerConnectionID, hostID, deniedMessage);
            if (deniedMessage == "")
            {
                await AddToChannel("players", gameID, playerConnectionID);
                await AddToChannel("viewers", hostID, playerConnectionID);
            }

            await Clients.Client(playerConnectionID).SendAsync("HandleJoinAsPlayer", hostID, deniedMessage);
        }

        public async Task RespondObserverJoined(int gameID, string playerConnectionID, int hostID, string deniedMessage)
        {
            if (deniedMessage == "")
            {
                await AddToChannel("observers", gameID, playerConnectionID);
            }

            await Clients.Client(playerConnectionID).SendAsync("HandleJoinAsObserver", hostID, deniedMessage);
        }

        public async Task RespondObserverRejoined(int gameID, string playerConnectionID, int hostID, string deniedMessage)
        {
            if (deniedMessage == "")
            {
                await AddToChannel("observers", gameID, playerConnectionID);
            }

            await Clients.Client(playerConnectionID).SendAsync("HandleJoinAsObserver", hostID, deniedMessage);
        }

        public async Task NotifyUpdate(int gameID, int eventNumber, GameEvent e)
        {
            e.Time = DateTime.Now;
            await Channel("players", gameID).SendAsync("HandleEvent", eventNumber, e);
            await Channel("observers", gameID).SendAsync("HandleEvent", eventNumber, e);
        }

        public async Task LoadGame(int gameID, string state, string playerName, string skin)
        {
            await Channel("players", gameID).SendAsync("HandleLoadGame", state, playerName, skin);
            await Channel("observers", gameID).SendAsync("HandleLoadGame", state, playerName, skin);
        }

        public async Task LoadSkin(int gameID, string skin)
        {
            await Channel("players", gameID).SendAsync("HandleLoadSkin", skin);
            await Channel("observers", gameID).SendAsync("HandleLoadSkin", skin);
        }

        public async Task Undo(int gameID, int untilEventNr)
        {
            await Channel("players", gameID).SendAsync("HandleUndo", untilEventNr);
            await Channel("observers", gameID).SendAsync("HandleUndo", untilEventNr);
        }

        public async Task ApproveChatMessage(int gameID, GameChatMessage message)
        {
            await Channel("players", gameID).SendAsync("HandleChatMessage", message);
            await Channel("observers", gameID).SendAsync("HandleChatMessage", message);
        }

        public async Task SendGlobalChatMessage(GlobalChatMessage message)
        {
            await Clients.All.SendAsync("ReceiveGlobalChatMessage", message);
        }

        public async Task SetTimer(int gameID, int value)
        {
            await Channel("players", gameID).SendAsync("UpdateTimer", value);
            await Channel("observers", gameID).SendAsync("UpdateTimer", value);
        }

        public void GameFinished(string state, GameInfo info)
        {
            SendMail(state, info);
        }

        public async Task UploadStatistics(string state)
        {
            var gameState = GameState.Load(state);
            if (Game.TryLoad(gameState, false, true, out Game game, false) == null)
            {
                await SendGameStatistics(game);
            }
        }

        public ServerSettings GetServerSettings()
        {
            var maintenanceDateTime = Configuration["GameMaintenanceDateTime"] ?? DateTime.MinValue.ToString();

            var result = new ServerSettings()
            {
                ScheduledMaintenance = DateTime.Parse(maintenanceDateTime),
                AdminName = Configuration["GameAdminUsername"]
            };

            return result;
        }

        #endregion MessagesFromHost

        #region Support

        private void Log(string method, params object[] prs)
        {
            if (logging)
            {
                Console.WriteLine("ConnectionId: " + Context.ConnectionId + ", " + method + "(" + string.Join(",", prs) + ")");
            }
        }

        private IClientProxy Channel(string type, int id)
        {
            return Clients.Group(type + id);
        }

        private async Task AddToChannel(string channelType, int channelID, string connectionId)
        {
            await Groups.AddToGroupAsync(connectionId, channelType + channelID);
        }

        private void SendMail(string content, GameInfo info)
        {
            var ruleset = Game.DetermineApproximateRuleset(info.FactionsInPlay, info.Rules, info.ExpansionLevel);
            var subject = string.Format("{0} ({1} Players, {2} Bots, Turn {3} - {4})", info.GameName, info.Players.Length, info.NumberOfBots, info.CurrentTurn, ruleset);

            try
            {
                var username = Configuration["GameEndEmailUsername"];

                if (username != "")
                {
                    var password = Configuration["GameEndEmailPassword"];
                    var from = Configuration["GameEndEmailFrom"];
                    var to = Configuration["GameEndEmailTo"];

                    MailMessage mailMessage = new()
                    {
                        From = new MailAddress(from),
                        Subject = subject,
                        IsBodyHtml = true,
                        Body = "Game finished!",
                        Priority = info.NumberOfBots < 0.5f * info.Players.Length ? MailPriority.Normal : MailPriority.Low
                    };

                    mailMessage.To.Add(new MailAddress(to));

                    var savegameToAttach = new Attachment(GenerateStreamFromString(content), "savegame" + DateTime.Now.ToString("yyyyMMdd.HHmm") + ".json");
                    mailMessage.Attachments.Add(savegameToAttach);

                    SmtpClient client = new()
                    {
                        Credentials = new System.Net.NetworkCredential(username, password),
                        Host = "smtp.strato.com",
                        EnableSsl = true
                    };

                    client.Send(mailMessage);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error sending mail: {0}", e.Message);
            }
        }

        private static async Task SendGameStatistics(Game game)
        {
            try
            {
                var statistics = GameStatistics.GetStatistics(game);
                var httpClient = new HttpClient();
                var data = GetStatisticsAsString(statistics);
                var json = new StringContent(data, Encoding.UTF8, "application/json");
                var result = await httpClient.PostAsync("https://dune.games/.netlify/functions/plays-add", json);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error sending statistics: {0}", e.Message);
            }
        }

        private static string GetStatisticsAsString(GameStatistics g)
        {
            var serializer = JsonSerializer.CreateDefault();
            serializer.TypeNameHandling = TypeNameHandling.None;
            var writer = new StringWriter();
            serializer.Serialize(writer, g);
            writer.Close();
            return writer.ToString();
        }


        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        #endregion Support
    }
}