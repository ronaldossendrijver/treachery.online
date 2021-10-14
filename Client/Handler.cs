/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Treachery.Shared;

namespace Treachery.Client
{
    public partial class Handler
    {
        #region FieldsAndConstructor

        public const int MAX_HEARTBEATS = 17280;  //17280 heartbeats of 10 seconds each = 48 hours
        public const int HEARTBEAT_DELAY = 10000;
        public const int DISCONNECT_TIMEOUT = 25000;
        public const int CHATMESSAGE_LIFETIME = 120;

        public readonly HubConnection _connection;
        private readonly ILogger _logger;

        public string PlayerName = "";
        public Game Game;
        public HostProxy HostProxy = null;
        public Host Host = null;
        public bool IsObserver = false;

        public Dictionary<int, string> _joinError = new();
        public int _gameinprogressHostId;
        public Battle _battleUnderConstruction = null;
        public int BidAutoPassThreshold = 0;
        public bool Autopass = false;
        public bool KeepAutopassSetting = false;
        public float CurrentEffectVolume = -1;
        public float CurrentChatVolume = -1;
        public bool ShowWheelsAndHMS = true;
        public bool StatisticsSent = false;
        public bool IsFullScreen = false;
        public bool BotsArePaused { get; private set; } = false;

        public bool IsGameMaster => !IsObserver && Player.Faction == Faction.None && CurrentPhase > Phase.TradingFactions;


        public DateTime Disconnected { get; private set; } = default;

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

        public Handler(Uri uri, ILogger logger)
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

            _logger = logger;
            Game = new Game();
            UpdateStatus();
            Game.MessageHandler += Game_MessageHandler;
            RegisterHandlers();
        }

        public event Action RefreshHandler;

        public void Refresh()
        {
            RefreshHandler.Invoke();
        }

        private void Game_MessageHandler(object sender, ChatMessage e)
        {
            _ = HandleChatMessage(e);
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
            try
            {
                await _connection.StartAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }

        private Battle RevisablePlan = null;
        private BattleInitiated RevisablePlanBattle = null;

        public void SetRevisablePlan(Battle plan)
        {
            RevisablePlan = plan;
            RevisablePlanBattle = Game.CurrentBattle;
        }
        public Battle GetRevisablePlan()
        {
            if (RevisablePlan != null && RevisablePlanBattle == Game.CurrentBattle)
            {
                return RevisablePlan;
            }
            else
            {
                return null;
            }
        }

        public void StartHost(string hostPWD, string loadedGameData, Game loadedGame)
        {
            Host = new Host(PlayerName, hostPWD, this, loadedGameData, loadedGame);
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
            _connection.On<string, string, string>("HandleLoadGame", (state, playerName, skin) => HandleLoadGame(state, playerName, skin));
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

                var _ = Heartbeat();
            }
            else
            {
                if (_joinError.ContainsKey(hostID))
                {
                    _joinError[hostID] = denyMessage;
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
                var _ = Heartbeat();
            }
            else
            {
                if (_joinError.ContainsKey(hostID))
                {
                    _joinError[hostID] = denyMessage;
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

        public async Task Heartbeat()
        {
            if (nrOfHeartbeats++ < MAX_HEARTBEATS)
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

                    if ((nrOfHeartbeats % 6) == 0) await Browser.RefreshPopovers();
                }
                catch (Exception e)
                {
                    Support.Log(e.ToString());
                }

                _ = Task.Delay(HEARTBEAT_DELAY).ContinueWith(e => Heartbeat());
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

        public async Task PerformBotEvent()
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

            if (result != "")
            {
                Support.Log(result);
            }
        }

        private async Task HandleLoadSkin(string skinData)
        {
            await Browser.SaveStringSetting("treachery.online;setting.skin", skinData);

            Skin.Current = Support.LoadSkin(skinData);
            await Skin.Current.ValidateAndFix(Browser.UrlExists);

            Refresh();
        }

        public bool IsPlayer
        {
            get
            {
                return Player != null && Player.Faction != Faction.None;
            }
        }

        Player _player = null;
        public Player Player
        {
            get
            {
                _player = Game.Players.SingleOrDefault(p => p.Name.ToLower().Trim() == PlayerName.ToLower().Trim());

                if (_player != null) return _player;

                _player = new Player(Game, PlayerName) { Faction = Faction.None };
                return _player;
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

                var result = Game.TryLoad(state, false, false, ref Game);
                Game.MessageHandler += Game_MessageHandler;

                if (result != "")
                {
                    Support.Log(result);
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
            }
        }

        private async Task HandleUndo(int untilEventNr)
        {
            try
            {
                Game = Game.Undo(untilEventNr);
                Game.MessageHandler += Game_MessageHandler;
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

                await Browser.SendToChatPopup(ConstructPopupChatMessage(m));
            }
        }

        public PopupChatMessage ConstructPopupChatMessage(ChatMessage m)
        {
            var sourcePlayer = Game.GetPlayer(m.SourcePlayerName);
            var sourceFaction = sourcePlayer != null ? sourcePlayer.Faction : Faction.None;
            return new PopupChatMessage() { body = Support.HTMLEncode(m.GetBodyIncludingPlayerInfo(MyName, Game)), style = Skin.Current.GetFactionColor(sourceFaction) };
        }

        public static async Task PopoutChatWindow()
        {
            await Browser.OpenChatPopup();
        }

        #endregion HostMessageHandlers

        #region Sounds

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
            if (IsHost && Game.RecentMilestones.Contains(Milestone.GameWon) && !savegameSent)
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

        #endregion Sounds

        #region ClientUpdates
        private async Task PerformPostEventTasks(GameEvent e)
        {
            if (!(e is AllyPermission || e is DealOffered || e is DealAccepted))
            {
                UpdateStatus();

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

            if (Game.CurrentMainPhase == MainPhase.Bidding)
            {
                ResetAutopassThreshold();
            }

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

        private bool awaitingBotAction;
        private void PerformBotAction(GameEvent e)
        {
            if (!awaitingBotAction && Game.Players.Any(p => p.IsBot))
            {
                awaitingBotAction = true;
                int botDelay = DetermineBotDelay(Game.CurrentMainPhase, e, Status.FlashInfo.Any());
                _ = Task.Delay(botDelay).ContinueWith(e => PerformBotEvent());
            }
        }

        private static int DetermineBotDelay(MainPhase phase, GameEvent e, bool hasFlashInfo)
        {
            if (hasFlashInfo)
            {
                return 5000;
            }
            else if (phase == MainPhase.Resurrection || phase == MainPhase.Charity || e is AllyPermission || e is DealOffered || e is DealAccepted)
            {
                return 200;
            }
            else if (e is Bid)
            {
                return 1000;
            }
            else if (phase == MainPhase.Battle || phase == MainPhase.ShipmentAndMove)
            {
                return 5000;
            }
            else
            {
                return 1500;
            }
        }

        private void ResetAutopassThreshold()
        {
            if (Game.RecentMilestones.Contains(Milestone.AuctionWon) && (!KeepAutopassSetting || Game.CurrentPhase == Phase.BiddingReport))
            {
                Autopass = false;
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

        public async Task ToggleFullScreen()
        {
            await Browser.ToggleFullScreen();
            IsFullScreen = !IsFullScreen;
            Refresh();
        }

        public IEnumerable<Type> Actions => Game.GetApplicableEvents(Player, IsHost);

        public bool IsConnected => _connection.State == HubConnectionState.Connected;

        public bool IsHost => Host != null;

        public Phase CurrentPhase => Game.CurrentPhase;


        public async Task CheckIfPlayerCanReconnect()
        {
            _gameinprogressHostId = 0;

            int currentGameHostID = await Browser.LoadSetting<int>(string.Format("treachery.online;currentgame;{0};hostid", PlayerName.ToLower().Trim()));
            if (currentGameHostID == 0) return;

            DateTime currentGameDateTime = await Browser.LoadSetting<DateTime>(string.Format("treachery.online;currentgame;{0};time", PlayerName.ToLower().Trim()));
            if (DateTime.Now.Subtract(currentGameDateTime).TotalSeconds > 900) return;

            _gameinprogressHostId = currentGameHostID;
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

        public async Task<DateTime> GetScheduledMaintenance()
        {
            try
            {
                return await _connection.InvokeAsync<DateTime>("GetScheduledMaintenance");
            }
            catch (Exception ex)
            {
                Support.Log(ex.ToString());
            }

            return default;
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
