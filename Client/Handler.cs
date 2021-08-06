/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Blazor.Extensions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Treachery.Shared;
using Treachery.Client.Pages;

namespace Treachery.Client
{
    public partial class Handler
    {
        #region FieldsAndConstructor

        public const int MAX_HEARTBEATS = 17280;  //17280 heartbeats of 10 seconds each = 48 hours
        public const int HEARTBEAT_DELAY = 10000;
        public const int DISCONNECT_TIMEOUT = 25000;
        public const int CHATMESSAGE_LIFETIME = 120;

        //public readonly Main _page;
        public readonly HubConnection _connection;
        public BECanvasComponent _canvas;
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
        public bool StatisticsSent = false;
        public bool BotsArePaused { get; private set; } = false;

        public bool IsGameMaster => !IsObserver && Player.Faction == Faction.None && CurrentPhase > Phase.TradingFactions;


        public DateTime Disconnected { get; private set; } = default;

        public bool IsDisconnected { 
            
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
            //var reconnectTimeouts = new int[] { 0, 2, 10, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20 };

            _connection = new HubConnectionBuilder()
            .WithUrl(uri)
            .WithAutomaticReconnect(new RetryPolicy())
            .AddNewtonsoftJsonProtocol(configuration =>
            {
                configuration.PayloadSerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All;
                configuration.PayloadSerializerSettings.Error += LogSerializationError;
            })
            .Build();
            
            //_page = page;
            _logger = logger;
            Game = new Game();
            Game.MessageHandler += Game_MessageHandler;
            RegisterHandlers();
        }

        public event Action RefreshPageControls;

        public event Action RefreshPageAll;

        public void RefreshControls()
        {
            RefreshPageControls.Invoke();
        }

        public void RefreshAll()
        {
            RefreshPageAll.Invoke();
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
                RefreshControls();
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
            RefreshControls();
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
            RefreshControls();
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
                    if (!await CheckDisconnect())
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

                    if ((nrOfHeartbeats % 3) == 0) await Browser.EnablePopovers();
                }
                catch (Exception e)
                {
                    Support.Log(e.ToString());
                }

                _ = Task.Delay(HEARTBEAT_DELAY).ContinueWith(e => Heartbeat());
            }
        }

        private async Task<bool> CheckDisconnect()
        {
            if (_connection.State == HubConnectionState.Disconnected || DateTime.Now.Subtract(hostLastSeen).TotalMilliseconds > DISCONNECT_TIMEOUT)
            {
                Disconnected = DateTime.Now;
                await MapDrawer.Draw();
                return true;
            }

            return false;
        }

        public Dictionary<int, GameEvent> _pending = new();
        private async Task HandleEvent(int newEventNumber, GameEvent e)
        {
            //Console.WriteLine("HandleEvent(" + newEventNumber + "," + e + ")");

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
            
            MapDrawer.Loading = true;
            RefreshAll();
            _ = Task.Delay(4000).ContinueWith(e => RedrawMapAfterSkinLoad());
        }

        private static async Task RedrawMapAfterSkinLoad()
        {
            MapDrawer.Loading = false;
            MapDrawer.UpdateIntelligence();
            await MapDrawer.Draw();
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
                //Console.WriteLine("Player:" + _player + ", playername: " + PlayerName + ", in game: " + string.Join(",", Game.Players.Select(p => p.Name)));

                //if (_player != null && Game.CurrentPhase > Phase.TradingFactions) return _player;

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
            Support.LogDuration("HandleLoadGame-Start");

            if (targetPlayerName == "" || targetPlayerName.ToLower().Trim() == PlayerName.ToLower().Trim())
            {
                _pending.Clear();

                MapDrawer.Loading = true;

                var state = GameState.Load(stateData);

                var result = Game.TryLoad(state, false, false, ref Game);
                Game.MessageHandler += Game_MessageHandler;

                MapDrawer.Loading = false;

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

            Support.LogDuration("HandleLoadGame-End");
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
                RefreshControls();

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

        #region Timers
        public Stopwatch Timer = null;
        public MainPhase TimerType;
        public List<Faction> TimedFactions = null;

        private readonly Dictionary<Faction, ThinkTimer> _timers = new Dictionary<Faction, ThinkTimer>();
        public ThinkTimer ThinkTimer(Faction f)
        {
            if (!_timers.ContainsKey(f))
            {
                _timers.Add(f, new ThinkTimer());
            }

            return _timers[f];
        }

        private void CheckTimers()
        {
            bool isBidding = (Game.CurrentPhase == Phase.Bidding);
            bool isShippingOrMoving = (Game.CurrentMainPhase == MainPhase.ShipmentAndMove && Game.CurrentPhase != Phase.ShipmentAndMoveConcluded);
            bool isBattling = (Game.CurrentPhase == Phase.BattlePhase && Game.CurrentBattle != null);

            var timedAction = isShippingOrMoving || isBidding || isBattling;
            var waitingFor = WaitingForFactions();

            //If the timer is running, check if it should be turned off if:
            //- The faction(s) we are waiting for are not the factions for which the timer was meant
            //- This phase has no timer
            if (Timer != null && (!timedAction || Different(TimedFactions, waitingFor)))
            {
                foreach (var f in TimedFactions)
                {
                    ThinkTimer(f).AddTime(TimerType, Timer.Elapsed);
                }

                Timer.Stop();
                Timer = null;
            }

            if (Timer == null && timedAction)
            {
                TimerType = Game.CurrentMainPhase;
                Timer = new Stopwatch();
                TimedFactions = waitingFor;
                Timer.Start();
            }
        }

        private static bool Different<T>(List<T> a, List<T> b)
        {
            if (a == null && b == null) return false;
            if (a == null || b == null) return true;
            return a.Count != b.Count || (a.Intersect(b).Count() != a.Count);
        }

        private List<Faction> WaitingForFactions()
        {
            var result = new List<Faction>();

            switch (Game.CurrentMainPhase)
            {
                case MainPhase.Bidding:
                    result.Add(Game.BidSequence.CurrentFaction);
                    break;

                case MainPhase.ShipmentAndMove:
                    {
                        switch (Game.CurrentPhase)
                        {
                            case Phase.NonOrangeShip:
                            case Phase.NonOrangeMove:
                                result.Add(Game.ShipmentAndMoveSequence.CurrentFaction);
                                break;

                            case Phase.OrangeShip:
                            case Phase.OrangeMove:
                                result.Add(Faction.Orange);
                                break;

                            case Phase.BlueAccompaniesNonOrange:
                            case Phase.BlueAccompaniesOrange:
                            case Phase.BlueIntrudedByNonOrangeShip:
                            case Phase.BlueIntrudedByOrangeShip:
                            case Phase.BlueIntrudedByNonOrangeMove:
                            case Phase.BlueIntrudedByOrangeMove:
                            case Phase.BlueIntrudedByCaravan:
                                result.Add(Faction.Blue);
                                break;
                        }
                    }
                    break;

                case MainPhase.Battle:
                    if (Game.CurrentBattle != null)
                    {
                        result.Add(Game.CurrentBattle.Initiator);
                        result.Add(Game.CurrentBattle.Target);
                    }
                    break;
            }

            return result;
        }
        #endregion Timers

        #region Sounds

        bool itAlreadyWasMyTurn = false;
        private async Task TurnAlert()
        {
            if (itAlreadyWasMyTurn)
            {
                itAlreadyWasMyTurn = !Status.WaitingForOthers;
            }
            else
            {
                if (!Status.WaitingForOthers && Game.CurrentMainPhase != MainPhase.Battle)
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
            if (!(e is AllyPermission))
            {
                await TurnAlert();
                await PlaySoundsForMilestones();
                CheckTimers();
                
                if (e == null || Game.CurrentMainPhase != MainPhase.Bidding)
                {
                    MapDrawer.UpdateIntelligence();
                }
            }

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
                await SaveGame();
                PerformBotAction();
            }

            RefreshAll();
            /*
            if (!(e is AllyPermission || e is Bid && !Game.RecentMilestones.Contains(Milestone.AuctionWon) || e is DealOffered))
            {
                await Browser.EnablePopovers();
            }
            */
        }

        private void PerformBotAction()
        {
            if (Game.Players.Any(p => p.IsBot))
            {
                int botDelay = 1000;
                if (Game.CurrentPhase == Phase.Clairvoyance || Game.CurrentPhase == Phase.OrangeMove || Game.CurrentPhase == Phase.NonOrangeMove || Game.CurrentPhase == Phase.OrangeShip || Game.CurrentPhase == Phase.NonOrangeShip) botDelay = 4000;
                else if (Game.CurrentPhase == Phase.Resurrection || Game.CurrentPhase == Phase.BluePredicting || Game.CurrentPhase == Phase.BlueAccompaniesNonOrange || Game.CurrentPhase == Phase.BlueAccompaniesOrange || Game.CurrentPhase == Phase.BlueIntrudedByNonOrangeShip || Game.CurrentPhase == Phase.BlueIntrudedByNonOrangeMove || Game.CurrentPhase == Phase.BlueIntrudedByCaravan || Game.CurrentPhase == Phase.BlueIntrudedByOrangeShip || Game.CurrentPhase == Phase.BlueIntrudedByOrangeMove || Game.CurrentPhase == Phase.HmsMovement || Game.CurrentPhase == Phase.HmsPlacement) botDelay = 2000;
                else if (Game.CurrentPhase == Phase.YellowRidingMonsterA || Game.CurrentPhase == Phase.YellowRidingMonsterB || Game.CurrentPhase == Phase.YellowSendingMonsterA || Game.CurrentPhase == Phase.YellowSendingMonsterB) botDelay = 4000;
                else if (Game.CurrentPhase == Phase.Clairvoyance) botDelay = 4000;
                else if (Game.CurrentPhase == Phase.HarvesterA || Game.CurrentPhase == Phase.HarvesterB || Game.CurrentPhase == Phase.Thumper) botDelay = 4000;
                else if (Game.CurrentPhase == Phase.PerformingKarmaHandSwap) botDelay = 10000;
                else if (Game.CurrentPhase == Phase.BattlePhase) botDelay = 4000;
                else if (Game.CurrentPhase == Phase.CallTraitorOrPass || Game.CurrentPhase == Phase.BattleConclusion || Game.CurrentPhase == Phase.Facedancing) botDelay = 10000;

                _ = Task.Delay(botDelay).ContinueWith(e => PerformBotEvent());
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
                if (Game.History.Count < 2000)
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
                RefreshControls();
            }
        }

        #endregion ClientUpdates

        #region SupportMethods

        public IEnumerable<Type> Actions
        {
            get
            {
                return Game.GetApplicableEvents(Player, IsHost);
            }
        }

        public bool IsConnected => _connection.State == HubConnectionState.Connected;

        public bool IsHost
        {
            get
            {
                return Host != null;
            }
        }

        public Phase CurrentPhase
        {
            get
            {
                return Game.CurrentPhase;
            }
        }

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

        #endregion SupportMethods
    }


}
