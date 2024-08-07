/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Treachery.Client;

public class Client : IGameService, IGameClient, IAsyncDisposable
{
    private const int HeartbeatDelay = 3000;

    //General info
    public ServerSettings ServerSettings { get; private set; }
    public bool IsConnected => _connection.State == HubConnectionState.Connected;
    
    //Logged in player
    private LoginInfo LoginInfo { get; set; }
    
    public bool LoggedIn => LoginInfo != null;
    private string UserToken => LoginInfo?.Token;
    public int UserId => LoginInfo?.UserId ?? -1;
    
    //Game in progress
    public Game Game { get; private set; }
    private string GameToken { get; set; } = string.Empty;
    public GameStatus Status { get; private set; }
    public List<Type> Actions { get; private set; } = []; 
    
    public bool InGame => Game != null;
    public Player Player => Game.GetPlayerByUserId(UserId);
    public string PlayerName => LoginInfo.PlayerName;
    public Faction Faction => Player?.Faction ?? Faction.None;
    public bool IsObserver => Game.IsObserver(UserId);
    public bool IsHost => Game.IsHost(UserId);
    public Phase CurrentPhase => Game.CurrentPhase;
    
    //Settings
    public float CurrentEffectVolume { get; set; } = -1;
    public float CurrentChatVolume { get; set; } = -1;
    public Battle BattleUnderConstruction { get; set; } = null;
    public int BidAutoPassThreshold { get; set; } = 0;
    public bool AutoPass { get; set; }
    public bool KeepAutoPassSetting { get; set; } = false;
    public int Timer { get; set; } = -1;
    public bool MuteGlobalChat { get; set; } = false;

    public event Action RefreshHandler;
    public event Action RefreshPopoverHandler;

    private readonly HubConnection _connection;
    private Browser Browser { get; }
    
    public Client(NavigationManager navigationManager, Browser browser)
    {
        Browser = browser;
        
        _connection = new HubConnectionBuilder()
            .WithUrl(navigationManager.ToAbsoluteUri("/gameHub"))
            .WithAutomaticReconnect(new RetryPolicy())
            .AddNewtonsoftJsonProtocol(configuration =>
            {
                configuration.PayloadSerializerSettings.TypeNameHandling = TypeNameHandling.All;
                configuration.PayloadSerializerSettings.Error += LogSerializationError;
            })
            .Build();
        
        _connection.Reconnected += OnReconnected;

        RegisterHandlers();
    }
    
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        
        if (_connection != null)
        {
            _connection.Reconnected -= OnReconnected;
            await _connection.DisposeAsync();
        }
    }

    public async Task Start(string userToken = null, string gameToken = null)
    {
        await _connection.StartAsync();
        await Connect();
        
        if (!string.IsNullOrEmpty(userToken) && !string.IsNullOrEmpty(gameToken))
        {
            var loginInfo = await _connection.InvokeAsync<Result<LoginInfo>>(nameof(IGameHub.GetLoginInfo), userToken);
            if (loginInfo.Success)
            {
                LoginInfo = loginInfo.Contents;
                GameToken = gameToken;
                await RequestReconnectGame();
            }
        }
        
        _ = Task.Delay(HeartbeatDelay).ContinueWith(_ => Heartbeat());
    }
    
    private void RegisterHandlers()
    {
        _connection.On<GameEvent,int>(nameof(HandleGameEvent), HandleGameEvent);
        _connection.On<GameChatMessage>(nameof(HandleChatMessage), HandleChatMessage);
        _connection.On<GlobalChatMessage>(nameof(HandleGlobalChatMessage), HandleGlobalChatMessage);
        _connection.On<int>(nameof(HandleSetTimer), HandleSetTimer);
        _connection.On<string>(nameof(HandleSetSkin), HandleSetSkin);
        _connection.On<int>(nameof(HandleUndo), HandleUndo);
        _connection.On<int,string,int>(nameof(HandleJoinGame), HandleJoinGame);
        _connection.On<int>(nameof(HandleSetOrUnsetHost), HandleSetOrUnsetHost);
        _connection.On<int,string>(nameof(HandleObserveGame), HandleObserveGame);
        _connection.On<int>(nameof(HandleOpenOrCloseSeat), HandleOpenOrCloseSeat);
        _connection.On<int,bool>(nameof(HandleRemoveUser), HandleRemoveUser);
        _connection.On<bool>(nameof(HandleBotStatus), HandleBotStatus);
        _connection.On<GameInitInfo>(nameof(HandleLoadGame), HandleLoadGame);
    }
    
    public void Refresh() => RefreshHandler?.Invoke();

    private void RefreshPopovers() => RefreshPopoverHandler?.Invoke();

    public void Reset()
    {
        GameToken = null;
        Game = null;
        Status = null;
        _ = Browser.StopSounds();
        Refresh();
    }
    
    //IGameClient methods

    public Task HandleSetTimer(int value)
    {
        Timer = value;
        return Task.CompletedTask;
    }

    public Task HandleJoinGame(int userId, string userName, int seat)
    {
        Game.AddPlayer(userId, userName, seat);
        Refresh();
        return Task.CompletedTask;
    }

    public Task HandleSetOrUnsetHost(int userId)
    {
        Game.SetOrUnsetHost(userId);
        Refresh();
        return Task.CompletedTask;
    }

    public Task HandleObserveGame(int userId, string userName)
    {
        Game.AddObserver(userId, userName);
        Refresh();
        return Task.CompletedTask;
    }

    public Task HandleOpenOrCloseSeat(int seat)
    {
        Game.OpenOrCloseSeat(seat);
        Refresh();
        return Task.CompletedTask;
    }

    public Task HandleRemoveUser(int userId, bool kick)
    {
        if (userId == UserId)
        {
            Reset();
        }
        else
        {
            Game.RemoveUser(userId, kick);
            Refresh();
        }
        
        return Task.CompletedTask;
    }

    public Task HandleBotStatus(bool paused)
    {
        Game.Participation.BotsArePaused = paused;
        Refresh();
        return Task.CompletedTask;
    }

    public async Task HandleGameEvent<TEvent>(TEvent e, int newEventNumber) where TEvent : GameEvent
    {
        Message resultMessage;
        e.Initialize(Game);
        var expectedEventNumber = Game.EventCount + 1;
        
        if (newEventNumber == expectedEventNumber)
        {
            //This is indeed the next expected event
            resultMessage = e.Execute(false, false);
        }
        else
        {
            //This is not the expected event. Request game state from the server.
            var gameInitInfo = await _connection.InvokeAsync<GameInitInfo>(nameof(IGameHub.RequestGameState), UserToken, GameToken);
            resultMessage = await LoadGame(gameInitInfo);
        }
        
        if (resultMessage == null)
        {
            await PerformPostEventTasks();
        }
        else
        {
            Support.Log(resultMessage.ToString(Skin.Current));
        }
    }

    public async Task HandleUndo(int untilEventNr)
    {
        try
        {
            Game = Game.Undo(untilEventNr);
            await PerformPostEventTasks();
        }
        catch (Exception ex)
        {
            Support.Log(ex.ToString());
        }
    }
    
    public async Task HandleSetSkin(string skinData)
    {
        await Browser.SaveStringSetting("treachery.online;setting.skin", skinData);

        Skin.Current = Skin.Load(skinData);

        Message.DefaultDescriber = Skin.Current;

        Refresh();
        RefreshPopovers();
    }

    public async Task HandleLoadGame(GameInitInfo initInfo) => 
        await LoadGame(initInfo);

    public LinkedList<ChatMessage> Messages { get; } = [];
    
    public async Task HandleGlobalChatMessage(GlobalChatMessage m)
    {
        Messages.AddFirst(m);

        if (!MuteGlobalChat)
        {
            await Browser.PlaySound(Skin.Current.Sound_Chatmessage_URL, CurrentChatVolume);
            Refresh();
        }
    }

    public async Task HandleChatMessage(GameChatMessage m)
    {
        if (m.TargetUserId == -1 || m.SourceUserId == -1 || m.SourceUserId == UserId || m.TargetUserId == UserId)
        {
            Messages.AddFirst(m);
            await Browser.PlaySound(Skin.Current.Sound_Chatmessage_URL, CurrentChatVolume);
            Refresh();
        }
    }

    public bool InScheduledMaintenance =>
        ServerSettings != null &&
        ServerSettings.ScheduledMaintenance.AddMinutes(15) > DateTime.UtcNow &&
        ((ServerSettings.ScheduledMaintenance.Subtract(DateTime.UtcNow).TotalHours < 6 && CurrentPhase <= Phase.AwaitingPlayers) ||
         ServerSettings.ScheduledMaintenance.Subtract(DateTime.UtcNow).TotalHours < 1);

  
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

    //Authentication
    
    public async Task<string> RequestCreateUser(string userName, string hashedPassword, string email, string playerName)
    {
        var result = await _connection.InvokeAsync<Result<LoginInfo>>(nameof(IGameHub.RequestCreateUser), userName, hashedPassword, email, playerName);

        if (result.Success)
        {
            LoginInfo = result.Contents;
            Refresh();
            return null;
        }

        return result.Message;
    }

    public async Task<string> RequestLogin(string userName, string hashedPassword)
    {
        var result = await _connection.InvokeAsync<Result<LoginInfo>>(nameof(IGameHub.RequestLogin), Game.LatestVersion, userName, hashedPassword);

        if (result.Success)
        {
            LoginInfo = result.Contents;
            Refresh();
            return null;
        }

        return result.Message;
    }
    
    public async Task<string> RequestPasswordReset(string email)
    {
        var result = await _connection.InvokeAsync<VoidResult>(nameof(IGameHub.RequestPasswordReset), email);
        return result.Message;
    }

    public async Task<string> RequestSetPassword(string userName, string passwordResetToken, string newHashedPassword)
    {
        var result = await _connection.InvokeAsync<Result<LoginInfo>>(nameof(IGameHub.RequestSetPassword), userName, passwordResetToken, newHashedPassword);

        if (result.Success)
        {
            LoginInfo = result.Contents;
            Refresh();
            return null;
        }

        return result.Message;
    }
    
    //Game Management

    public async Task<string> RequestCreateGame(string hashedPassword, string stateData = null, string skinData = null)
    {
        var result = await _connection.InvokeAsync<Result<GameInitInfo>>(nameof(IGameHub.RequestCreateGame), UserToken, hashedPassword, stateData, skinData);
        
        if (result.Success)
        {
            var loadMessage = await LoadGame(result.Contents);
            if (loadMessage != null)
                return loadMessage.ToString();

            GameToken = result.Contents.GameToken;
        }
        
        return result.Message;
    }
    
    public async Task<string> RequestCloseGame(string gameId)
    {
        var result = await _connection.InvokeAsync<Result<string>>(nameof(IGameHub.RequestCloseGame), UserToken, gameId);
        return result.Success ? result.Contents : result.Message;
    }

    public async Task<string> RequestJoinGame(string gameId, string hashedPassword, int seat = -1)
    {
        var result = await _connection.InvokeAsync<Result<GameInitInfo>>(nameof(IGameHub.RequestJoinGame), UserToken, gameId, hashedPassword, seat);
        if (result.Success)
        {
            var loadMessage = await LoadGame(result.Contents);
            if (loadMessage != null)
                return loadMessage.ToString();

            GameToken = result.Contents.GameToken;
        }

        return result.Message;
    }

    public async Task<string> RequestObserveGame(string gameId, string hashedPassword)
    {
        var result = await _connection.InvokeAsync<Result<GameInitInfo>>(nameof(IGameHub.RequestObserveGame), UserToken, gameId, hashedPassword);
        if (result.Success)
        {
            var loadMessage = await LoadGame(result.Contents);
            if (loadMessage != null)
                return loadMessage.ToString();

            GameToken = result.Contents.GameToken;
        }
        
        return result.Message;
    }

    public async Task<string> RequestReconnectGame()
    {
        var result = await _connection.InvokeAsync<Result<GameInitInfo>>(nameof(IGameHub.RequestReconnectGame), UserToken, GameToken);
        if (result.Success)
        {
            var loadMessage = await LoadGame(result.Contents);
            if (loadMessage != null)
                return loadMessage.ToString();

            GameToken = result.Contents.GameToken;
        }
        
        return result.Message;
    }

    public async Task<string> RequestSetOrUnsetHost(int userId) =>
        (await _connection.InvokeAsync<VoidResult>(nameof(IGameHub.RequestSetOrUnsetHost), UserToken, GameToken, userId)).Message;

    public async Task<string> RequestOpenOrCloseSeat(int seat) =>
        (await _connection.InvokeAsync<VoidResult>(nameof(IGameHub.RequestOpenOrCloseSeat), UserToken, GameToken, seat)).Message;

    public async Task RequestLeaveGame() =>
        await _connection.SendAsync(nameof(IGameHub.RequestLeaveGame), UserToken, GameToken);
    
    public async Task<string> RequestKick(int userId) => 
        (await _connection.InvokeAsync<VoidResult>(nameof(IGameHub.RequestKick), UserToken, GameToken, userId)).Message;

    public async Task<string> RequestLoadGame(string state, string skin = null) =>
        (await _connection.InvokeAsync<VoidResult>(nameof(IGameHub.RequestLoadGame), UserToken, GameToken, state, skin)).Message;

    public async Task<string> RequestSetSkin(string skin) =>
        (await _connection.InvokeAsync<VoidResult>(nameof(IGameHub.RequestSetSkin), UserToken, GameToken, skin)).Message;

    public async Task<string> RequestUndo(int untilEventNr) =>
        (await _connection.InvokeAsync<VoidResult>(nameof(IGameHub.RequestUndo), UserToken, GameToken, untilEventNr)).Message;

    public async Task<string> SetTimer(int value) =>
        (await _connection.InvokeAsync<VoidResult>(nameof(IGameHub.SetTimer), value)).Message;

    public async Task<string> RequestGameEvent<T>(T gameEvent) where T : GameEvent =>
        (await _connection.InvokeAsync<VoidResult>($"Request{typeof(T).Name}", UserToken, GameToken, gameEvent)).Message;

    public async Task<string> RequestPauseBots() =>
        (await _connection.InvokeAsync<VoidResult>(nameof(IGameHub.RequestPauseBots), UserToken, GameToken)).Message;

    public async Task SendChatMessage(GameChatMessage message) =>
        await _connection.SendAsync(nameof(IGameHub.SendChatMessage), UserToken, GameToken, message);

    public async Task SendGlobalChatMessage(GlobalChatMessage message) =>
        await _connection.SendAsync(nameof(IGameHub.SendGlobalChatMessage), UserToken, message);

    public async Task<string> AdminUpdateMaintenance(DateTime maintenanceDate)
    {
        var result = await _connection.InvokeAsync<Result<string>>(nameof(IGameHub.AdminUpdateMaintenance), UserToken, maintenanceDate);
        return result.Success ? result.Contents : result.Message;
    }

    public async Task<string> AdminPersistState()
    {
        var result = await _connection.InvokeAsync<Result<string>>(nameof(IGameHub.AdminPersistState), UserToken);
        return result.Success ? result.Contents : result.Message;
    }


    public async Task<string> AdminRestoreState()
    {
        var result = await _connection.InvokeAsync<Result<string>>(nameof(IGameHub.AdminRestoreState), UserToken);
        return result.Success ? result.Contents : result.Message;
    }

    public async Task<string> AdminCloseGame(string gameId)
    {
        var result = await _connection.InvokeAsync<Result<string>>(nameof(IGameHub.AdminCloseGame), UserToken, gameId);
        return result.Success ? result.Contents : result.Message;
    }

    public List<GameInfo> RunningGames { get; private set; } = [];
    private async Task Heartbeat()
    {
        try
        {
            if (IsConnected)
            {
                await _connection.SendAsync(nameof(IGameHub.RequestRegisterHeartbeat), UserToken, GameToken);

                if (LoggedIn && !InGame)
                {
                    var runningGamesResult = await _connection.InvokeAsync<Result<List<GameInfo>>>(nameof(IGameHub.RequestRunningGames), UserToken);
                    if (runningGamesResult.Success)
                    {
                        RunningGames = runningGamesResult.Contents;
                        Refresh();
                    }
                    else
                    {
                        Support.Log("Server error: " + runningGamesResult.Message);                        
                    }
                }
            }
        }
        catch (Exception e)
        {
            Support.Log(e.ToString());
        }

        _ = Task.Delay(HeartbeatDelay).ContinueWith(_ => Heartbeat());
    }

    private async Task<Message> LoadGame(GameInitInfo initInfo)
    {
        var currentState = GameState.Load(initInfo.GameState);
        var resultMessage = Game.TryLoad(currentState, initInfo.Participation, false, false, out var result);
        if (resultMessage == null)
        {
            Game = result;
            await PerformPostEventTasks();
            RefreshPopovers();
        }
        else
        {
            Support.Log(resultMessage.ToString(Skin.Current));
        }

        return resultMessage;
    }
    
    private async Task Connect()
    {
        var result = await _connection.InvokeAsync<Result<ServerSettings>>(nameof(IGameHub.Connect));
        if (result.Success)
        {
            ServerSettings = result.Contents;
        }
        else
        {
            Support.Log(result.Message);
        }
    }
    
    private async Task PerformPostEventTasks()
    {
        Status = GameStatus.DetermineStatus(Game, Player, !IsObserver);
        Actions = Game.GetApplicableEvents(Player, IsHost);

        await TurnAlert();
        await PlaySoundsForMilestones();
        await Browser.RemoveFocusFromButtons();

        if (Game.CurrentMainPhase == MainPhase.Bidding) 
            ResetAutoPassThreshold();

        PerformEndOfTurnTasks();
        Refresh();
    }

    private void ResetAutoPassThreshold()
    {
        if (Game.RecentMilestones.Contains(Milestone.AuctionWon) && (!KeepAutoPassSetting || Game.CurrentPhase == Phase.BiddingReport)) AutoPass = false;
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
        foreach (var m in Game.RecentMilestones) 
            await Browser.PlaySound(Skin.Current.GetSound(m), CurrentEffectVolume);
    }

    private Phase previousPhase;
    private void PerformEndOfTurnTasks()
    {
        if (Game.CurrentPhase == Phase.TurnConcluded && Game.CurrentPhase != previousPhase) 
            Messages.Clear();

        previousPhase = Game.CurrentPhase;
    }
    
    private async Task OnReconnected(string _) => await RequestReconnectGame();
    
    private static void LogSerializationError(object sender, ErrorEventArgs e) => Support.Log(e.ErrorContext.Error.ToString());
}