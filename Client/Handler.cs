/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Treachery.Shared;

namespace Treachery.Client
{
    public partial class Handler
    {
        #region FieldsAndConstructor

        public const int HEARTBEAT_DELAY = 10000;
        private const int MAX_HEARTBEATS = 17280;  //17280 heartbeats of 10 seconds each = 48 hours
        private const int DISCONNECT_TIMEOUT = 25000;

        //Game in progress
        public Game Game { get; private set; }
        public GameStatus Status { get; private set; }
        public int GameInProgressHostId { get; private set; }
        
        //Player and Host
        public string PlayerName { get; private set; } = "";
        public HostProxy HostProxy { get; private set; } = null;
        public Host Host { get; private set; } = null;
        public bool IsObserver { get; private set; } = false;
        public ServerSettings ServerSettings { get; private set; }
        public Dictionary<int, string> JoinErrors { get; private set; } = new();
        public DateTime Disconnected { get; private set; } = default;
        

        //Sound and camera
        public float CurrentEffectVolume { get; set; } = -1;
        public float CurrentChatVolume { get; set; } = -1;
        public float CurrentVideoVolume { get; set; } = -1;
        public CaptureDevice AudioDevice { get; set; }
        public CaptureDevice VideoDevice { get; set; }

        //Info displayed on the map
        public bool ShowWheelsAndHMS { get; set; } = true;
        public Battle BattleUnderConstruction { get; set; } = null;

        //Other settings
        public bool StatisticsSent { get; set; } = false;
        public bool BotsArePaused { get; set; } = false;

        public event Action RefreshHandler;
        public event Action RefreshPopoverHandler;

        private readonly HubConnection _connection;

        public Handler(Uri uri)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(uri)
                .WithAutomaticReconnect(new RetryPolicy())
                .AddNewtonsoftJsonProtocol(configuration =>
                {
                    configuration.PayloadSerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All;
                    configuration.PayloadSerializerSettings.Error += LogSerializationError;
                })
                .Build();

            Game = new Game();
            UpdateStatus(Game, Player, IsPlayer);
            RegisterHandlers();
            Browser.OnVideoData += ProcessVideoData;
        }

        private void ProcessVideoData(byte[] data)
        {
            if (HostProxy != null)
            {
                _ = HostProxy.SendVideo(Player.PositionAtTable, data);
            }
        }

        public bool IsDisconnected
        {
            get
            {
                return Disconnected != default;
            }

            set
            {
                Disconnected = default;
            }
        }

        public void Refresh()
        {
            RefreshHandler?.Invoke();
        }

        public void RefreshPopovers()
        {
            RefreshPopoverHandler?.Invoke();
        }

        private void LogSerializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            Support.Log(e.ErrorContext.Error.ToString());
        }

        public string MyName
        {
            get
            {
                return Player != null ? Player.Name : "";
            }
        }

        public async Task Start()
        {
            await _connection.StartAsync();
            await GetServerSettings();
        }

        public async Task StartHost(string hostPWD, string loadedGameData, Game loadedGame)
        {
            Host = new Host(PlayerName, hostPWD, this, loadedGameData, loadedGame, _connection);
            await LetHostJoin();
        }

        private async Task LetHostJoin()
        {
            if (IsHost && !Host.JoinedPlayers.Any())
            {
                JoinErrors[Host.HostID] = "";
                await Host.JoinGame(PlayerName);
                await Task.Delay(5000).ContinueWith(e => LetHostJoin());
            }
        }

        #endregion FieldsAndConstructor

        #region HostMessageHandlers
        private void RegisterHandlers()
        {
            _connection.On<GameInfo>("GameAvailable", (info) => ReceiveGameAvailable(info));
            _connection.On<int, string>("HandleJoinAsPlayer", (hostID, denyMessage) => HandleJoinAsPlayer(hostID, denyMessage));
            _connection.On<int, string>("HandleJoinAsObserver", (hostID, denyMessage) => HandleJoinAsObserver(hostID, denyMessage));
            _connection.On<int, GameEvent>("HandleEvent", (nr, e) => HandleEvent(nr, e));
            _connection.On<ChatMessage>("HandleChatMessage", (e) => HandleChatMessage(e));
            _connection.On<int>("HandleUndo", (untilEventNr) => HandleUndo(untilEventNr));
            _connection.On<string>("HandleLoadSkin", (skin) => HandleLoadSkin(skin));
            _connection.On<int, byte[]>("ReceiveVideo", (playerposition, data) => ReceiveVideo(playerposition, data));
            _connection.On<string, string, string>("HandleLoadGame", (state, playerName, skin) => HandleLoadGame(state, playerName, skin));
        }

        private async Task ReceiveVideo(int playerPosition, byte[] data)
        {
            await Browser.PushVideoData("video" + playerPosition, data, 0.01f * CurrentVideoVolume);
        }

        //Process information about a currently running game on treachery.online
        public Dictionary<GameInfo, DateTime> AvailableGames = new();
        private void ReceiveGameAvailable(GameInfo info)
        {
            if (HostProxy != null && info.HostID == HostProxy.HostID)
            {
                hostLastSeen = DateTime.Now;
            }

            if (HostProxy == null || Game.CurrentPhase == Phase.AwaitingPlayers)
            {
                if (AvailableGames.ContainsKey(info))
                {
                    AvailableGames.Remove(info);
                }

                AvailableGames.Add(info, DateTime.Now);
                Refresh();
            }
        }

        PlayerJoined howThisPlayerJoined = null;
        public async Task Request(int hostID, PlayerJoined e)
        {
            howThisPlayerJoined = e;
            await _connection.SendAsync("RequestPlayerJoined", hostID, e);
        }

        ObserverJoined howThisObserverJoined = null;
        public async Task Request(int hostID, ObserverJoined e)
        {
            howThisObserverJoined = e;
            await _connection.SendAsync("RequestObserverJoined", hostID, e);
        }

        PlayerRejoined howThisPlayerRejoined = null;
        public async Task Request(int hostID, PlayerRejoined e)
        {
            howThisPlayerRejoined = e;
            await _connection.SendAsync("RequestPlayerRejoined", hostID, e);
        }

        ObserverRejoined howThisObserverRejoined = null;
        public async Task Request(int hostID, ObserverRejoined e)
        {
            howThisObserverRejoined = e;
            await _connection.SendAsync("RequestObserverRejoined", hostID, e);
        }

        private void HandleJoinAsPlayer(int hostID, string denyMessage)
        {
            if (denyMessage == "")
            {
                HostProxy = new HostProxy(hostID, _connection);
                IsObserver = false;
                hostLastSeen = DateTime.Now;

                var _ = Heartbeat(GameInProgressHostId);
            }
            else
            {
                if (JoinErrors.ContainsKey(hostID))
                {
                    JoinErrors[hostID] = denyMessage;
                }
            }

            Refresh();
        }

        private void HandleJoinAsObserver(int hostID, string denyMessage)
        {
            if (denyMessage == "")
            {
                HostProxy = new HostProxy(hostID, _connection);
                IsObserver = true;
                hostLastSeen = DateTime.Now;
                var _ = Heartbeat(GameInProgressHostId);
            }
            else
            {
                if (JoinErrors.ContainsKey(hostID))
                {
                    JoinErrors[hostID] = denyMessage;
                }
            }

            Refresh();
        }

        private async Task TryToReconnect()
        {
            if (howThisPlayerJoined != null) await Request(HostProxy.HostID, new PlayerRejoined() { Name = howThisPlayerJoined.Name, HashedPassword = howThisPlayerJoined.HashedPassword });
            else if (howThisPlayerRejoined != null) await Request(HostProxy.HostID, howThisPlayerRejoined);
            else if (howThisObserverJoined != null) await Request(HostProxy.HostID, new ObserverRejoined() { Name = howThisObserverJoined.Name, HashedPassword = howThisObserverJoined.HashedPassword });
            else if (howThisObserverRejoined != null) await Request(HostProxy.HostID, howThisObserverRejoined);
        }

        public DateTime hostLastSeen = DateTime.Now;
        private int nrOfHeartbeats = 0;
        private string oldConnectionId = "";

        public async Task Heartbeat(int gameInProgressHostId)
        {
            if (gameInProgressHostId == GameInProgressHostId && nrOfHeartbeats++ < MAX_HEARTBEATS && HostProxy != null)
            {
                try
                {
                    if (!CheckDisconnect())
                    {
                        if (oldConnectionId == "")
                        {
                            oldConnectionId = _connection.ConnectionId;
                        }

                        if (IsDisconnected || _connection.ConnectionId != oldConnectionId)
                        {
                            await TryToReconnect();
                            oldConnectionId = _connection.ConnectionId;
                        }

                        IsDisconnected = false;
                    }

                    await SaveGameInfo();

                    if (!IsDisconnected)
                    {
                        await HostProxy.SendHeartbeat(PlayerName);
                    }
                }
                catch (Exception e)
                {
                    Support.Log(e.ToString());
                }

                _ = Task.Delay(HEARTBEAT_DELAY).ContinueWith(e => Heartbeat(gameInProgressHostId));
            }
        }

        private bool CheckDisconnect()
        {
            if (_connection.State == HubConnectionState.Disconnected || DateTime.Now.Subtract(hostLastSeen).TotalMilliseconds > DISCONNECT_TIMEOUT)
            {
                Disconnected = DateTime.Now;
                Refresh();
                return true;
            }

            return false;
        }

        public Dictionary<int, GameEvent> _pending = new();
        private async Task HandleEvent(int newEventNumber, GameEvent e)
        {
            try
            {
                e.Game = Game;

                int expectedEventNumber = Game.EventCount + 1;
                if (newEventNumber == expectedEventNumber)
                {
                    //This is indeed the next expected event
                    Handle(e);
                    await PerformPostEventTasks(e);

                    //Handle any pending events directly following this event
                    while (_pending.ContainsKey(expectedEventNumber))
                    {
                        _pending[expectedEventNumber].Execute(true, IsHost);
                        _pending.Remove(expectedEventNumber);
                        expectedEventNumber++;
                    }
                }
                else if (!_pending.ContainsKey(newEventNumber))
                {
                    //This is not the expected event. Store this one for now.
                    _pending.Add(newEventNumber, e);
                }
            }
            catch (Exception ex)
            {
                Support.Log(ex.ToString());
            }
        }

        private async Task PerformBotEvent()
        {
            awaitingBotAction = false;

            if (!BotsArePaused && Game.CurrentPhase > Phase.AwaitingPlayers)
            {
                var bots = Deck<Player>.Randomize(Game.Players.Where(p => p.IsBot));

                foreach (var bot in bots)
                {
                    var evts = Game.GetApplicableEvents(bot, false);
                    var evt = bot.DetermineHighPrioInPhaseAction(evts);

                    if (evt != null && HostProxy != null)
                    {
                        await HostProxy.Request(evt);
                        return;
                    }
                }

                foreach (var bot in bots)
                {
                    var evts = Game.GetApplicableEvents(bot, false);
                    var evt = bot.DetermineMiddlePrioInPhaseAction(evts);

                    if (evt != null && HostProxy != null)
                    {
                        await HostProxy.Request(evt);
                        return;
                    }
                }

                foreach (var bot in bots)
                {
                    var evts = Game.GetApplicableEvents(bot, false);
                    var evt = bot.DetermineLowPrioInPhaseAction(evts);

                    if (evt != null && HostProxy != null)
                    {
                        await HostProxy.Request(evt);
                        return;
                    }
                }
            }
        }

        private static void Handle(GameEvent e)
        {
            var result = e.Execute(false, false);

            if (result != null)
            {
                Support.Log(result.ToString(Skin.Current));
            }
        }

        private async Task HandleLoadSkin(string skinData)
        {
            await Browser.SaveStringSetting("treachery.online;setting.skin", skinData);

            Skin.Current = Support.LoadSkin(skinData);
            //await Skin.Current.ValidateAndFix(Browser.UrlExists);
            Message.DefaultDescriber = Skin.Current;

            Refresh();
            RefreshPopovers();
        }

        public bool IsPlayer
        {
            get
            {
                return Player != null && Player.Faction != Faction.None;
            }
        }

        public Player Player
        {
            get
            {
                var result = Game.Players.SingleOrDefault(p => p.Name.ToLower().Trim() == PlayerName.ToLower().Trim());

                if (result == null)
                {
                    result = new Player(Game, PlayerName) { Faction = Faction.None };
                }

                return result;
            }
        }

        public Faction Faction
        {
            get
            {
                var player = Player;
                return player == null ? Faction.None : player.Faction;
            }
        }

        public bool IAm(Faction f)
        {
            var p = Player;
            return p != null && p.Faction == f;
        }

        public bool IAm(LeaderSkill skill)
        {
            return Game.SkilledAs(Player, skill);
        }

        public bool IAm(Player p)
        {
            return Player == p;
        }

        private async Task HandleLoadGame(string stateData, string targetPlayerName, string skinData)
        {
            if (targetPlayerName == "" || targetPlayerName.ToLower().Trim() == PlayerName.ToLower().Trim())
            {
                _pending.Clear();

                var state = GameState.Load(stateData);
                var errorMessage = Game.TryLoad(state, false, false, out Game loadedGame, true);

                if (errorMessage == null)
                {
                    Game = loadedGame;
                }
                else
                {
                    Support.Log(errorMessage.ToString(Skin.Current));
                }

                if (Player == null)
                {
                    IsObserver = true;
                }

                if (skinData != "")
                {
                    await HandleLoadSkin(skinData);
                }

                await PerformPostEventTasks(null);
                RefreshPopovers();
            }
        }

        private async Task HandleUndo(int untilEventNr)
        {
            try
            {
                Game = Game.Undo(untilEventNr);
                _pending.Clear();
                await PerformPostEventTasks(null);
            }
            catch (Exception ex)
            {
                Support.Log(ex.ToString());
            }
        }

        public LinkedList<ChatMessage> Messages = new();
        private async Task HandleChatMessage(ChatMessage m)
        {
            m.DateTimeReceived = DateTime.Now;

            var me = PlayerName.ToLower().Trim();
            if (m.TargetPlayerName == "" || m.SourcePlayerName == "" || m.SourcePlayerName.ToLower().Trim() == me || m.TargetPlayerName.ToLower().Trim() == me)
            {
                Messages.AddFirst(m);
                await Browser.PlaySound(Skin.Current.Sound_Chatmessage_URL, CurrentChatVolume);
                Refresh();

                await Browser.SendToChatPopup(PopupChatMessage.Construct(Game, MyName, m));
            }
        }

        public static async Task PopoutChatWindow()
        {
            await Browser.OpenChatPopup();
        }

        #endregion HostMessageHandlers

        #region ClientUpdates
        private async Task PerformPostEventTasks(GameEvent e)
        {
            if (!(e is AllyPermission || e is DealOffered))
            {
                UpdateStatus(Game, Player, IsPlayer);

                await TurnAlert();
                await PlaySoundsForMilestones();

                if (e == null || !(Game.CurrentPhase == Phase.Bidding || Game.CurrentPhase == Phase.BlackMarketBidding))
                {
                    if (IsHost)
                    {
                        await SaveGame();
                    }
                }
            }

            await Browser.RemoveFocusFromButtons();

            await PerformEndOfTurnTasks();

            if (Game.CurrentMainPhase == MainPhase.Ended)
            {
                await PerformEndOfGameTasks();
            }

            if (IsHost)
            {
                PerformBotAction(e);
            }

            Refresh();
        }

        private void UpdateStatus(Game game, Player player, bool isPlayer)
        {
            Status = GameStatus.DetermineStatus(game, player, isPlayer);
        }

        bool itAlreadyWasMyTurn = false;
        private async Task TurnAlert()
        {
            if (itAlreadyWasMyTurn)
            {
                itAlreadyWasMyTurn = !Status.WaitingForOthers(Player, IsHost);
            }
            else
            {
                if (!Status.WaitingForOthers(Player, IsHost) && Game.CurrentMainPhase != MainPhase.Battle)
                {
                    itAlreadyWasMyTurn = true;
                    await Browser.PlaySound(Skin.Current.Sound_YourTurn_URL, CurrentEffectVolume);
                }
            }
        }

        private async Task PlaySoundsForMilestones()
        {
            foreach (var m in Game.RecentMilestones)
            {
                await Browser.PlaySound(Skin.Current.GetSound(m), CurrentEffectVolume, false);
            }
        }

        private bool savegameSent = false;
        private async Task PerformEndOfGameTasks()
        {
            if (IsHost && Game.RecentMilestones.Contains(Milestone.GameWon) && !savegameSent && Game.Players.Count(p => p.IsBot) < Game.Players.Count(p => !p.IsBot))
            {
                savegameSent = true;
                await Host.HandleGameFinished(Game);
            }
        }

        private Phase previousPhase;
        private async Task PerformEndOfTurnTasks()
        {
            if (Game.CurrentPhase == Phase.TurnConcluded && Game.CurrentPhase != previousPhase)
            {
                Messages.Clear();
                await Browser.SendToChatPopup(new PopupChatClear());
            }

            previousPhase = Game.CurrentPhase;
        }

        private bool awaitingBotAction = false;
        private void PerformBotAction(GameEvent e)
        {
            if (!awaitingBotAction && Game.Players.Any(p => p.IsBot))
            {
                awaitingBotAction = true;
                int botDelay = DetermineBotDelay(Game.CurrentMainPhase, e, Status.FlashInfo.Count);
                _ = Task.Delay(botDelay).ContinueWith(e => PerformBotEvent());
            }
        }

        private static int DetermineBotDelay(MainPhase phase, GameEvent e, int nrOfFlashMessages)
        {
            if (nrOfFlashMessages > 0)
            {
                return 2500 + nrOfFlashMessages * 3000;
            }
            else if (phase == MainPhase.Resurrection || phase == MainPhase.Charity || e is AllyPermission || e is DealOffered)
            {
                return 400;
            }
            else if (e is Bid)
            {
                return 1500;
            }
            else if (phase == MainPhase.Battle || phase == MainPhase.ShipmentAndMove)
            {
                return 5000;
            }
            else
            {
                return 2000;
            }
        }

        private async Task SaveGameInfo()
        {
            if (!IsObserver)
            {
                await Browser.SaveSetting(string.Format("treachery.online;currentgame;{0};hostid", PlayerName.ToLower().Trim()), HostProxy.HostID);
                await Browser.SaveSetting(string.Format("treachery.online;currentgame;{0};time", PlayerName.ToLower().Trim()), DateTime.Now);
            }
        }

        bool _localStorageCleared = false;
        private async Task SaveGame()
        {
            try
            {
                if (Game.History.Count < 2500)
                {
                    bool mustClear = false;

                    try
                    {
                        await Browser.SaveSetting(string.Format("treachery.online;latestgame;{0}", PlayerName.ToLower().Trim()), GameState.GetStateAsString(Game));
                    }
                    catch (Exception)
                    {
                        if (!_localStorageCleared)
                        {
                            _localStorageCleared = true;
                            mustClear = true;
                        }
                    }

                    if (mustClear)
                    {
                        await Browser.ClearSettingsStartingWith("treachery.online;latestgame");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async Task ConfirmPlayername(string name)
        {
            if (PlayerJoined.ValidName(name) == "")
            {
                PlayerName = name;
                await CheckIfPlayerCanReconnect();
                Refresh();
            }
        }

        #endregion ClientUpdates

        #region SupportMethods

        public bool InScheduledMaintenance =>
            ServerSettings != null &&
            ServerSettings.ScheduledMaintenance.AddMinutes(15) > DateTime.UtcNow &&
           (ServerSettings.ScheduledMaintenance.Subtract(DateTime.UtcNow).TotalHours < 6 && CurrentPhase <= Phase.AwaitingPlayers ||
            ServerSettings.ScheduledMaintenance.Subtract(DateTime.UtcNow).TotalHours < 1);

        public IEnumerable<Type> Actions => Game.GetApplicableEvents(Player, IsHost);

        public bool IsConnected => _connection.State == HubConnectionState.Connected;

        public bool IsHost => Host != null;

        public Phase CurrentPhase => Game.CurrentPhase;

        public async Task CheckIfPlayerCanReconnect()
        {
            GameInProgressHostId = 0;

            int currentGameHostID = await Browser.LoadSetting<int>(string.Format("treachery.online;currentgame;{0};hostid", PlayerName.ToLower().Trim()));
            if (currentGameHostID == 0) return;

            DateTime currentGameDateTime = await Browser.LoadSetting<DateTime>(string.Format("treachery.online;currentgame;{0};time", PlayerName.ToLower().Trim()));
            if (DateTime.Now.Subtract(currentGameDateTime).TotalSeconds > 900) return;

            GameInProgressHostId = currentGameHostID;
        }

        public async Task ToggleBotPause()
        {
            if (BotsArePaused)
            {
                BotsArePaused = false;
                await PerformBotEvent();
            }
            else
            {
                BotsArePaused = true;
            }
        }

        private async Task GetServerSettings()
        {
            try
            {
                ServerSettings = await _connection.InvokeAsync<ServerSettings>("GetServerSettings");
            }
            catch (Exception ex)
            {
                Support.Log(ex.ToString());
            }
        }

        #endregion SupportMethods

        #region MapEvents

        public event EventHandler<Location> OnLocationSelected;
        public event EventHandler<Location> OnLocationSelectedWithCtrlOrAlt;
        public event EventHandler<Location> OnLocationSelectedWithShift;
        public event EventHandler<Location> OnLocationSelectedWithShiftAndWithCtrlOrAlt;

        public void LocationClick(LocationEventArgs e)
        {
            if (e.ShiftKey)
            {
                if (e.CtrlKey || e.AltKey)
                {
                    OnLocationSelectedWithShiftAndWithCtrlOrAlt?.Invoke(this, e.Location);
                }
                else
                {
                    OnLocationSelectedWithShift?.Invoke(this, e.Location);
                }
            }
            else
            {
                if (e.CtrlKey || e.AltKey)
                {
                    OnLocationSelectedWithCtrlOrAlt?.Invoke(this, e.Location);
                }
                else
                {
                    OnLocationSelected?.Invoke(this, e.Location);
                }
            }
        }

        #endregion
    }
}
