/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Treachery.Client;

public class Client
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
    public HostProxy HostProxy { get; private set; }
    public Host Host { get; private set; }
    public bool IsObserver { get; private set; }
    public ServerSettings ServerSettings { get; private set; }
    public Dictionary<int, string> JoinErrors { get; } = new();
    public DateTime Disconnected { get; private set; }
    
    private string ValidatedUsername { get; set; }
    private string ValidatedHashedPassword { get; set; }
    
    //Sound and camera
    public float CurrentEffectVolume { get; set; } = -1;
    public float CurrentChatVolume { get; set; } = -1;
    public CaptureDevice AudioDevice { get; set; }
    public CaptureDevice VideoDevice { get; set; }

    //Settings
    public Battle BattleUnderConstruction { get; set; } = null;
    public int BidAutoPassThreshold { get; set; } = 0;
    public bool Autopass { get; set; }
    public bool KeepAutopassSetting { get; set; } = false;
    public bool StatisticsSent { get; set; } = false;
    public bool BotsArePaused { get; set; }
    public int Timer { get; set; } = -1;
    public bool MuteGlobalChat { get; set; } = false;

    public event Action RefreshHandler;
    public event Action RefreshPopoverHandler;

    private readonly HubConnection _connection;

    public Client(Uri uri)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(uri)
            .WithAutomaticReconnect(new RetryPolicy())
            .AddNewtonsoftJsonProtocol(configuration =>
            {
                configuration.PayloadSerializerSettings.TypeNameHandling = TypeNameHandling.All;
                configuration.PayloadSerializerSettings.Error += LogSerializationError;
            })
            .Build();

        Game = new Game();
        UpdateStatus(Game, Player, IsPlayer);
        RegisterHandlers();
    }

    public bool IsDisconnected
    {
        get => Disconnected != default;
        set => Disconnected = default;
    }

    public void Refresh()
    {
        RefreshHandler?.Invoke();
    }

    public void RefreshPopovers()
    {
        RefreshPopoverHandler?.Invoke();
    }

    private void LogSerializationError(object sender, ErrorEventArgs e)
    {
        Support.Log(e.ErrorContext.Error.ToString());
    }

    public string MyName => Player != null ? Player.Name : "";

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
        _connection.On<GameInfo>("GameAvailable", info => ReceiveGameAvailable(info));
        _connection.On<int, string>("HandleJoinAsPlayer", (hostID, denyMessage) => HandleJoinAsPlayer(hostID, denyMessage));
        _connection.On<int, string>("HandleJoinAsObserver", (hostID, denyMessage) => HandleJoinAsObserver(hostID, denyMessage));
        _connection.On<int, GameEvent>("HandleEvent", (nr, e) => HandleEvent(nr, e));
        _connection.On<GameChatMessage>("HandleChatMessage", message => HandleChatMessage(message));
        _connection.On<int>("HandleUndo", untilEventNr => HandleUndo(untilEventNr));
        _connection.On<string>("HandleLoadSkin", skin => HandleLoadSkin(skin));
        _connection.On<string, string, string>("HandleLoadGame", (state, playerName, skin) => HandleLoadGame(state, playerName, skin));
        _connection.On<int>("UpdateTimer", value => UpdateTimer(value));
        _connection.On<GlobalChatMessage>("ReceiveGlobalChatMessage", message => HandleGlobalChatMessage(message));
    }

    private void UpdateTimer(int value)
    {
        Timer = value;
    }

    //Process information about a currently running game on treachery.online
    public IEnumerable<GameInfo> RunningGames => _availableGames.Where(gameAndDate => DateTime.Now.Subtract(gameAndDate.Value).TotalSeconds < 15).Select(gameAndDate => gameAndDate.Key);
    private readonly Dictionary<GameInfo, DateTime> _availableGames = new();

    public IEnumerable<GameInfo> JoinableAdvertisedGames => _advertisedGames.Where(gameAndDate => gameAndDate.Key.CurrentPhase == Phase.AwaitingPlayers && DateTime.Now.Subtract(gameAndDate.Value).TotalSeconds < 15).Select(gameAndDate => gameAndDate.Key);
    private readonly Dictionary<GameInfo, DateTime> _advertisedGames = new();
    private void ReceiveGameAvailable(GameInfo info)
    {
        if (HostProxy != null && info.HostID == HostProxy.HostID) hostLastSeen = DateTime.Now;

        if (_availableGames.ContainsKey(info)) _availableGames.Remove(info);

        if (_advertisedGames.ContainsKey(info)) _advertisedGames.Remove(info);

        if (HostProxy == null || Game.CurrentPhase == Phase.AwaitingPlayers)
        {
            _availableGames.Add(info, DateTime.Now);
            Refresh();
        }
        else if (info.InviteOthers && (IsObserver || (Game != null && Game.NumberOfHumanPlayers <= 1)))
        {
            _advertisedGames.Add(info, DateTime.Now);
            Refresh();
        }
    }

    private PlayerJoined howThisPlayerJoined;
    public async Task Request(int hostID, PlayerJoined e)
    {
        howThisPlayerJoined = e;
        await _connection.SendAsync("RequestPlayerJoined", hostID, e);
    }

    private ObserverJoined howThisObserverJoined;
    public async Task Request(int hostID, ObserverJoined e)
    {
        howThisObserverJoined = e;
        await _connection.SendAsync("RequestObserverJoined", hostID, e);
    }

    private PlayerRejoined howThisPlayerRejoined;
    public async Task Request(int hostID, PlayerRejoined e)
    {
        howThisPlayerRejoined = e;
        await _connection.SendAsync("RequestPlayerRejoined", hostID, e);
    }

    private ObserverRejoined howThisObserverRejoined;
    public async Task Request(int hostID, ObserverRejoined e)
    {
        howThisObserverRejoined = e;
        await _connection.SendAsync("RequestObserverRejoined", hostID, e);
    }

    public async Task Request(GlobalChatMessage message)
    {
        if (message.Body != null && message.Body.Length > 0)
            try
            {
                await _connection.SendAsync("SendGlobalChatMessage", message);
            }
            catch (Exception)
            {
                Support.Log("Disconnected...");
            }
    }

    public void Reset()
    {
        GameInProgressHostId = 0;
        Game = new Game();
        UpdateStatus(Game, Player, IsPlayer);
        JoinErrors.Clear();
        howThisPlayerJoined = null;
        howThisObserverJoined = null;
        howThisPlayerRejoined = null;
        howThisObserverRejoined = null;

        if (Host != null)
        {
            Host.Stop();
            Host = null;
        }

        HostProxy = null;
        IsObserver = false;
        _ = Browser.StopSounds();
        Refresh();
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
            if (JoinErrors.ContainsKey(hostID)) JoinErrors[hostID] = denyMessage;
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
            if (JoinErrors.ContainsKey(hostID)) JoinErrors[hostID] = denyMessage;
        }

        Refresh();
    }

    private async Task TryToReconnect()
    {
        if (howThisPlayerJoined != null) await Request(HostProxy.HostID, new PlayerRejoined { Name = howThisPlayerJoined.Name, HashedPassword = howThisPlayerJoined.HashedPassword });
        else if (howThisPlayerRejoined != null) await Request(HostProxy.HostID, howThisPlayerRejoined);
        else if (howThisObserverJoined != null) await Request(HostProxy.HostID, new ObserverRejoined { Name = howThisObserverJoined.Name, HashedPassword = howThisObserverJoined.HashedPassword });
        else if (howThisObserverRejoined != null) await Request(HostProxy.HostID, howThisObserverRejoined);
    }

    public DateTime hostLastSeen = DateTime.Now;
    private int nrOfHeartbeats;
    private string oldConnectionId = "";

    public async Task Heartbeat(int gameInProgressHostId)
    {
        if (gameInProgressHostId == GameInProgressHostId && nrOfHeartbeats++ < MAX_HEARTBEATS && HostProxy != null)
        {
            try
            {
                if (!CheckDisconnect())
                {
                    if (oldConnectionId == "") oldConnectionId = _connection.ConnectionId;

                    if (IsDisconnected || _connection.ConnectionId != oldConnectionId)
                    {
                        await TryToReconnect();
                        oldConnectionId = _connection.ConnectionId;
                    }

                    IsDisconnected = false;
                }

                await SaveGameInfo();

                if (!IsDisconnected) await HostProxy.SendHeartbeat(PlayerName);
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
            e.Initialize(Game);

            var expectedEventNumber = Game.EventCount + 1;
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
                var evt = bot.DetermineHighestPrioInPhaseAction(evts);

                if (evt != null && HostProxy != null)
                {
                    await HostProxy.Request(evt);
                    return;
                }
            }

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

        if (result != null) Support.Log(result.ToString(Skin.Current));
    }

    private async Task HandleLoadSkin(string skinData)
    {
        await Browser.SaveStringSetting("treachery.online;setting.skin", skinData);

        Skin.Current = Skin.Load(skinData);

        Message.DefaultDescriber = Skin.Current;

        Refresh();
        RefreshPopovers();
    }

    public bool IsPlayer => Player != null && Player.Faction != Faction.None;

    public Player Player
    {
        get
        {
            var result = Game.Players.SingleOrDefault(p => p.Name.ToLower().Trim() == PlayerName.ToLower().Trim());

            if (result == null) result = new Player(Game, PlayerName) { Faction = Faction.None };

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

    private async Task HandleLoadGame(string stateData, string targetPlayerName, string skinData)
    {
        if (targetPlayerName == "" || targetPlayerName.ToLower().Trim() == PlayerName.ToLower().Trim())
        {
            _pending.Clear();

            var state = GameState.Load(stateData);
            var errorMessage = Game.TryLoad(state, false, false, out var loadedGame);

            if (errorMessage == null)
                Game = loadedGame;
            else
                Support.Log(errorMessage.ToString(Skin.Current));

            if (Player == null) IsObserver = true;

            if (skinData != "") await HandleLoadSkin(skinData);

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

    private async Task HandleChatMessage(GameChatMessage m)
    {
        m.DateTimeReceived = DateTime.Now;

        var me = PlayerName.ToLower().Trim();
        if (m.TargetPlayerName == "" || m.SourcePlayerName == "" || m.SourcePlayerName.ToLower().Trim() == me || m.TargetPlayerName.ToLower().Trim() == me)
        {
            Messages.AddFirst(m);
            await Browser.PlaySound(Skin.Current.Sound_Chatmessage_URL, CurrentChatVolume);
            Refresh();
        }
    }

    private async Task HandleGlobalChatMessage(GlobalChatMessage m)
    {
        m.DateTimeReceived = DateTime.Now;
        Messages.AddFirst(m);

        if (!MuteGlobalChat)
        {
            await Browser.PlaySound(Skin.Current.Sound_Chatmessage_URL, CurrentChatVolume);
            Refresh();
        }
    }

    #endregion HostMessageHandlers

    #region ClientUpdates
    private async Task PerformPostEventTasks(GameEvent e)
    {
        UpdateStatus(Game, Player, IsPlayer);

        if (!(e is AllyPermission || e is DealOffered))
        {
            await TurnAlert();
            await PlaySoundsForMilestones();

            if (e == null || !(Game.CurrentPhase == Phase.Bidding || Game.CurrentPhase == Phase.BlackMarketBidding))
                if (IsHost) await SaveGame();
        }

        await Browser.RemoveFocusFromButtons();

        if (Game.CurrentMainPhase == MainPhase.Bidding) ResetAutopassThreshold();

        PerformEndOfTurnTasks();

        if (Game.CurrentMainPhase == MainPhase.Ended) await PerformEndOfGameTasks();

        if (IsHost) PerformBotAction(e);

        Refresh();
    }

    private void ResetAutopassThreshold()
    {
        if (Game.RecentMilestones.Contains(Milestone.AuctionWon) && (!KeepAutopassSetting || Game.CurrentPhase == Phase.BiddingReport)) Autopass = false;
    }

    private void UpdateStatus(Game game, Player player, bool isPlayer)
    {
        Status = GameStatus.DetermineStatus(game, player, isPlayer);
    }

    private bool itAlreadyWasMyTurn;
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
        foreach (var m in Game.RecentMilestones) await Browser.PlaySound(Skin.Current.GetSound(m), CurrentEffectVolume);
    }

    private bool savegameSent;
    private async Task PerformEndOfGameTasks()
    {
        if (IsHost && Game.RecentMilestones.Contains(Milestone.GameWon) && !savegameSent && Game.Players.Count(p => p.IsBot) < Game.Players.Count(p => !p.IsBot))
        {
            savegameSent = true;
            await Host.HandleGameFinished(Game);
        }
    }

    private Phase previousPhase;
    private void PerformEndOfTurnTasks()
    {
        if (Game.CurrentPhase == Phase.TurnConcluded && Game.CurrentPhase != previousPhase) Messages.Clear();

        previousPhase = Game.CurrentPhase;
    }

    private async Task SaveGameInfo()
    {
        if (!IsObserver)
        {
            await Browser.SaveSetting(string.Format("treachery.online;currentgame;{0};hostid", PlayerName.ToLower().Trim()), HostProxy.HostID);
            await Browser.SaveSetting(string.Format("treachery.online;currentgame;{0};time", PlayerName.ToLower().Trim()), DateTime.Now);
        }
    }

    private bool _localStorageCleared;
    private async Task SaveGame()
    {
        try
        {
            if (Game.History.Count < 2500)
            {
                var mustClear = false;

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

                if (mustClear) await Browser.ClearSettingsStartingWith("treachery.online;latestgame");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    #endregion ClientUpdates

    #region Bots

    private bool awaitingBotAction;
    private void PerformBotAction(GameEvent e)
    {
        if (!awaitingBotAction && Game.Players.Any(p => p.IsBot))
        {
            awaitingBotAction = true;
            var botDelay = DetermineBotDelay(Game.CurrentMainPhase, e, Status.FlashInfo.Count);
            _ = Task.Delay(botDelay).ContinueWith(e => PerformBotEvent());
        }
    }

    private static int DetermineBotDelay(MainPhase phase, GameEvent e, int nrOfFlashMessages)
    {
        if (nrOfFlashMessages > 0)
            return 2500 + nrOfFlashMessages * 3000;
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

    #endregion

    #region SupportMethods

    public bool InScheduledMaintenance =>
        ServerSettings != null &&
        ServerSettings.ScheduledMaintenance.AddMinutes(15) > DateTime.UtcNow &&
        ((ServerSettings.ScheduledMaintenance.Subtract(DateTime.UtcNow).TotalHours < 6 && CurrentPhase <= Phase.AwaitingPlayers) ||
         ServerSettings.ScheduledMaintenance.Subtract(DateTime.UtcNow).TotalHours < 1);

    public IEnumerable<Type> Actions => Game.GetApplicableEvents(Player, IsHost);

    public bool IsConnected => _connection.State == HubConnectionState.Connected;
    
    public bool IsAuthenticated => ValidatedUsername != null;

    public bool IsHost => Host != null;

    public Phase CurrentPhase => Game.CurrentPhase;

    public async Task SetPlayerName(string name)
    {
        PlayerName = name;
        await CheckIfPlayerCanReconnect();
        Refresh();
    }

    private async Task CheckIfPlayerCanReconnect()
    {
        GameInProgressHostId = 0;

        var currentGameHostID = await Browser.LoadSetting<int>(string.Format("treachery.online;currentgame;{0};hostid", PlayerName.ToLower().Trim()));
        if (currentGameHostID == 0) return;

        var currentGameDateTime = await Browser.LoadSetting<DateTime>(string.Format("treachery.online;currentgame;{0};time", PlayerName.ToLower().Trim()));
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
                OnLocationSelectedWithShiftAndWithCtrlOrAlt?.Invoke(this, e.Location);
            else
                OnLocationSelectedWithShift?.Invoke(this, e.Location);
        }
        else
        {
            if (e.CtrlKey || e.AltKey)
                OnLocationSelectedWithCtrlOrAlt?.Invoke(this, e.Location);
            else
                OnLocationSelected?.Invoke(this, e.Location);
        }
    }

    #endregion

    public async Task<string> RequestLogin(string userName, string hashedPassword)
    {
        var result = await _connection.InvokeAsync<string>("RequestLogin", userName, hashedPassword);

        if (result == null)
        {
            ValidatedUsername = userName;
            ValidatedHashedPassword = hashedPassword;
        }

        return result;
    }
    
    public async Task<string> RequestCreateUser(string userName, string hashedPassword, string email, string playerName)
    {
        var result = await _connection.InvokeAsync<string>("RequestCreateUser", userName, hashedPassword, email, playerName);

        if (result == null)
        {
            ValidatedUsername = userName;
            ValidatedHashedPassword = hashedPassword;
        }

        return result;
    }

    public async Task<string> RequestPasswordReset(string email) => await _connection.InvokeAsync<string>("RequestPasswordReset", email);
    
    public async Task<string> RequestSetPassword(string userName, string passwordResetToken, string hashedPassword)
    {
        var result = await _connection.InvokeAsync<string>("RequestSetPassword", userName, passwordResetToken, hashedPassword);

        if (result == null)
        {
            ValidatedUsername = userName;
            ValidatedHashedPassword = hashedPassword;
        }

        return result;
    }
    
}