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
    public ServerInfo ServerInfo { get; private set; }
    public Skin CurrentSkin { get; set; } = DefaultSkin.Default;
    public bool IsConnected => _connection.State == HubConnectionState.Connected;
    
    //Admin info
    public AdminInfo AdminInfo { get; private set; }
    
    //Logged in player
    private LoginInfo LoginInfo { get; set; }
    private string StoredPassword { get; set; }
    
    public bool LoggedIn => LoginInfo != null;
    private string UserToken => LoginInfo?.Token;
    public int UserId => LoginInfo?.UserId ?? -1;
    public string UserName => LoginInfo.UserName;
    public string UserEmail => LoginInfo.Email;
    
    //Game in progress
    public Game Game { get; private set; }
    public string GameName { get; private set; }
    private string GameId { get; set; } = string.Empty;
    public GameStatus Status { get; private set; }
    public List<Type> Actions { get; private set; } = []; 
    
    public bool InGame => Game != null;
    public bool PlayerNeedsSeating => InGame && Game.Participation.SeatedPlayers.ContainsValue(-1);
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
        _connection.Closed += OnDisconnected;
        

        RegisterHandlers();
    }
    
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        
        if (_connection != null)
        {
            _connection.Reconnected -= OnReconnected;
            _connection.Closed -= OnDisconnected;
            await _connection.DisposeAsync();
        }
    }

    public async Task Start(string userToken = null, string gameId = null)
    {
        await _connection.StartAsync();
        await Connect();
        
        if (!string.IsNullOrEmpty(userToken) && !string.IsNullOrEmpty(gameId))
        {
            var loginInfo = await _connection.InvokeAsync<Result<LoginInfo>>(nameof(IGameHub.GetLoginInfo), userToken);
            if (loginInfo.Success)
            {
                StoredPassword = null;
                LoginInfo = loginInfo.Contents;
                GameId = gameId;
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
        _connection.On<Dictionary<int, int>>(nameof(HandleAssignSeats), HandleAssignSeats);
    }

    public void Refresh(string source = null)
    {
        //if (source != null)
        //    Console.WriteLine(source);
        
        RefreshHandler?.Invoke();  
    } 

    private void RefreshPopovers() => RefreshPopoverHandler?.Invoke();

    public void ExitGame()
    {
        Game = null;
        GameName = null;
        GameId = null;
        Status = null;
        Actions = [];
        _ = Browser.StopSounds();
        Refresh(nameof(ExitGame));
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
        Refresh(nameof(HandleJoinGame));
        return Task.CompletedTask;
    }

    public async Task HandleSetOrUnsetHost(int userId)
    {
        Game.SetOrUnsetHost(userId);
        await PerformPostEventTasks();
    }

    public Task HandleObserveGame(int userId, string userName)
    {
        Game.AddObserver(userId, userName);
        Refresh(nameof(HandleObserveGame));
        return Task.CompletedTask;
    }

    public Task HandleOpenOrCloseSeat(int seat)
    {
        Game.OpenOrCloseSeat(seat);
        Refresh(nameof(HandleOpenOrCloseSeat));
        return Task.CompletedTask;
    }

    public async Task HandleRemoveUser(int userId, bool kick)
    {
        if (userId == UserId)
        {
            ExitGame();
        }
        else
        {
            Game.RemoveUser(userId, kick);
            await PerformPostEventTasks();
        }
    }

    public Task HandleBotStatus(bool paused)
    {
        Game.Participation.BotsArePaused = paused;
        Refresh(nameof(HandleBotStatus));
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
            var result = await Invoke<GameInitInfo>(nameof(IGameHub.RequestGameState), UserToken, GameId);
            resultMessage = result.Success ? await LoadGame(result.Contents) : Message.Express("Connection error");
        }
        
        if (resultMessage == null)
        {
            await PerformPostEventTasks();
        }
        else
        {
            Support.Log(resultMessage.ToString(CurrentSkin));
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

        CurrentSkin = Skin.Load(skinData, DefaultSkin.Default);

        Message.DefaultDescriber = CurrentSkin;

        Refresh(nameof(HandleSetSkin));
        RefreshPopovers();
    }

    public async Task HandleLoadGame(GameInitInfo initInfo) => await LoadGame(initInfo);

    public async Task HandleAssignSeats(Dictionary<int, int> assignment)
    {
        Game.Participation.SeatedPlayers = assignment;
        await PerformPostEventTasks();
    }

    public LinkedList<ChatMessage> Messages { get; } = [];
    
    public async Task HandleGlobalChatMessage(GlobalChatMessage m)
    {
        Messages.AddFirst(m);

        if (!MuteGlobalChat)
        {
            await Browser.PlaySound(CurrentSkin.Sound_Chatmessage_URL, CurrentChatVolume);
            Refresh(nameof(HandleGlobalChatMessage));
        }
    }

    public async Task HandleChatMessage(GameChatMessage m)
    {
        if (m.TargetUserId == -1 || m.SourceUserId == -1 || m.SourceUserId == UserId || m.TargetUserId == UserId)
        {
            Messages.AddFirst(m);
            await Browser.PlaySound(CurrentSkin.Sound_Chatmessage_URL, CurrentChatVolume);
            Refresh(nameof(HandleChatMessage));
        }
    }

    public bool InScheduledMaintenance =>
        ServerInfo != null &&
        ServerInfo.ScheduledMaintenance.AddMinutes(15) > DateTime.UtcNow &&
        ((InGame && ServerInfo.ScheduledMaintenance.Subtract(DateTime.UtcNow).TotalHours < 6 && CurrentPhase <= Phase.AwaitingPlayers) ||
         ServerInfo.ScheduledMaintenance.Subtract(DateTime.UtcNow).TotalHours < 1);

  
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
    
    public async Task<Result<LoginInfo>> RequestCreateUser(string userName, string hashedPassword, string email, string playerName)
    {
        var result = await _connection.InvokeAsync<Result<LoginInfo>>(nameof(IGameHub.RequestCreateUser), userName, hashedPassword, email, playerName);

        if (result.Success)
        {
            StoredPassword = hashedPassword;
            LoginInfo = result.Contents;
            Refresh(nameof(RequestCreateUser));
            return result;
        }

        return result;
    }

    public async Task<Result<LoginInfo>> RequestLogin(string userName, string hashedPassword)
    {
        var result = await _connection.InvokeAsync<Result<LoginInfo>>(nameof(IGameHub.RequestLogin), Game.LatestVersion, userName, hashedPassword);

        if (result.Success)
        {
            StoredPassword = hashedPassword;
            LoginInfo = result.Contents;
            Refresh(nameof(RequestLogin));
            return result;
        }

        StoredPassword = null;
        LoginInfo = null;
        Refresh(nameof(RequestLogin));
        
        return result;
    }

    public async Task<VoidResult> RequestPasswordReset(string usernameOrEmail) 
        => await _connection.InvokeAsync<VoidResult>(nameof(IGameHub.RequestPasswordReset), usernameOrEmail);

    public async Task<Result<LoginInfo>> RequestSetPassword(string userName, string passwordResetToken, string newHashedPassword)
    {
        var result = await _connection.InvokeAsync<Result<LoginInfo>>(nameof(IGameHub.RequestSetPassword), userName, passwordResetToken, newHashedPassword);

        if (result.Success)
        {
            StoredPassword = newHashedPassword;
            LoginInfo = result.Contents;
            Refresh(nameof(RequestSetPassword));
            return result;
        }

        return result;
    }
    
    public async Task<string> RequestUpdateUserInfo(string hashedPassword, string email, string playerName)
    {
        var result = await Invoke<LoginInfo>(nameof(IGameHub.RequestUpdateUserInfo), UserToken,
            hashedPassword, playerName, email);

        if (!result.Success) 
            return CurrentSkin.Describe(result.Error);
        
        StoredPassword = hashedPassword;
        LoginInfo = result.Contents;
        Refresh(nameof(RequestUpdateUserInfo));
        return null;
    }
    
    //Game Management

    public async Task<string> RequestCreateGame(string hashedPassword, string stateData = null, string skinData = null)
    {
        var result = await Invoke<GameInitInfo>(nameof(IGameHub.RequestCreateGame), UserToken, hashedPassword, stateData, skinData);
        if (result.Success)
        {
            var loadMessage = await LoadGame(result.Contents);
            if (loadMessage != null)
                return loadMessage.ToString();

            GameId = result.Contents.GameId;
        }
        
        return result.Success ? null :
            result.Error is ErrorType.InvalidGameEvent ? result.ErrorDetails : CurrentSkin.Describe(result.Error);
    }
    
    public async Task<string> RequestCloseGame(string gameId)
    {
        var result = await Invoke(nameof(IGameHub.RequestCloseGame), UserToken, gameId);
        return result.Success ? string.Empty : CurrentSkin.Describe(result.Error);
    }

    public async Task<string> RequestJoinGame(string gameId, string hashedPassword, int seat)
    {
        var result = await Invoke<GameInitInfo>(nameof(IGameHub.RequestJoinGame), UserToken, gameId, hashedPassword, seat);
        if (!result.Success) 
            return CurrentSkin.Describe(result.Error);
        
        var loadMessage = await LoadGame(result.Contents);
        if (loadMessage != null)
            return loadMessage.ToString();

        GameId = result.Contents.GameId;
        return null;
    }

    public async Task<string> RequestObserveGame(string gameId, string hashedPassword)
    {
        var result = await Invoke<GameInitInfo>(nameof(IGameHub.RequestObserveGame), UserToken, gameId, hashedPassword);
        if (!result.Success) 
            return CurrentSkin.Describe(result.Error);
        
        var loadMessage = await LoadGame(result.Contents);
        if (loadMessage != null)
            return loadMessage.ToString();

        GameId = result.Contents.GameId;
        return null;
    }

    public async Task<string> RequestSetOrUnsetHost(int userId) =>
        CurrentSkin.Describe((await Invoke(nameof(IGameHub.RequestSetOrUnsetHost), UserToken, GameId, userId)).Error);

    public async Task<string> RequestOpenOrCloseSeat(int seat) =>
        CurrentSkin.Describe((await Invoke(nameof(IGameHub.RequestOpenOrCloseSeat), UserToken, GameId, seat)).Error);

    public async Task<string> RequestLeaveGame() =>
        CurrentSkin.Describe((await Invoke(nameof(IGameHub.RequestLeaveGame), UserToken, GameId)).Error);
    
    public async Task<string> RequestKick(int userId) => 
        CurrentSkin.Describe((await Invoke(nameof(IGameHub.RequestKick), UserToken, GameId, userId)).Error);

    public async Task<string> RequestLoadGame(string state, string skin = null)
    {
        var result = await Invoke(nameof(IGameHub.RequestLoadGame), UserToken, GameId, state, skin);
        return result.Success ? null :
            result.Error is ErrorType.InvalidGameEvent ? result.ErrorDetails : CurrentSkin.Describe(result.Error);
    }
    public async Task<string> RequestAssignSeats(Dictionary<int, int> assignment) =>
        CurrentSkin.Describe((await Invoke(nameof(IGameHub.RequestAssignSeats), UserToken, GameId, assignment)).Error);

    public async Task<string> RequestSetSkin(string skin)
    {
        var result = await Invoke(nameof(IGameHub.RequestSetSkin), UserToken, GameId, skin);
        return result.Success ? null : CurrentSkin.Describe(result.Error);
    }
        

    public async Task<string> RequestUndo(int untilEventNr) =>
        CurrentSkin.Describe((await Invoke(nameof(IGameHub.RequestUndo), UserToken, GameId, untilEventNr)).Error);

    public async Task<string> SetTimer(int value) =>
        CurrentSkin.Describe((await Invoke(nameof(IGameHub.SetTimer), value)).Error);

    public async Task<string> RequestGameEvent<T>(T gameEvent) where T : GameEvent
    {
        var result = await Invoke($"Request{typeof(T).Name}", UserToken, GameId, gameEvent);
        return result.Success ? null :
            result.Error is ErrorType.InvalidGameEvent ? result.ErrorDetails : CurrentSkin.Describe(result.Error);
    }
        
    public async Task<string> RequestPauseBots() =>
        CurrentSkin.Describe((await Invoke(nameof(IGameHub.RequestPauseBots), UserToken, GameId)).Error);

    public async Task SendChatMessage(GameChatMessage message) =>
        await Invoke(nameof(IGameHub.SendChatMessage), UserToken, GameId, message);

    public async Task SendGlobalChatMessage(GlobalChatMessage message) =>
        await Invoke(nameof(IGameHub.SendGlobalChatMessage), UserToken, message);

    public async Task<string> AdminUpdateMaintenance(DateTimeOffset maintenanceDate)
    {
        var result = await Invoke<string>(nameof(IGameHub.AdminUpdateMaintenance), UserToken, maintenanceDate);
        return result.Success ? result.Contents : CurrentSkin.Describe(result.Error);
    }

    public async Task<string> AdminPersistState()
    {
        var result = await Invoke<string>(nameof(IGameHub.AdminPersistState), UserToken);
        return result.Success ? result.Contents : CurrentSkin.Describe(result.Error);
    }

    public async Task<string> AdminRestoreState()
    {
        var result = await Invoke<string>(nameof(IGameHub.AdminRestoreState), UserToken);
        return result.Success ? result.Contents : CurrentSkin.Describe(result.Error);
    }

    public async Task<string> AdminCloseGame(string gameId)
    {
        var result = await Invoke<string>(nameof(IGameHub.AdminCloseGame), UserToken, gameId);
        return result.Success ? result.Contents : CurrentSkin.Describe(result.Error);
    }
    
    public async Task<string> AdminDeleteUser(int userId)
    {
        var result = await Invoke<string>(nameof(IGameHub.AdminDeleteUser), UserToken, userId);
        return result.Success ? result.Contents : CurrentSkin.Describe(result.Error);
    }

    public async Task<string> GetAdminInfo()
    {
        var result = await Invoke<AdminInfo>(nameof(IGameHub.GetAdminInfo), UserToken);
        if (result.Success)
        {
            AdminInfo = result.Contents;
            return "AdminInfo retrieved";
        }
        else
        {
            return CurrentSkin.Describe(result.Error);            
        }
    }

    public List<GameInfo> RunningGames { get; private set; } = [];
    private async Task Heartbeat()
    {
        try
        {
            if (IsConnected)
            {
                await _connection.SendAsync(nameof(IGameHub.RequestRegisterHeartbeat), UserToken, GameId);

                if (LoggedIn && !InGame)
                {
                    var runningGamesResult = await _connection.InvokeAsync<Result<List<GameInfo>>>(nameof(IGameHub.RequestRunningGames), UserToken);
                    if (runningGamesResult.Success)
                    {
                        RunningGames = runningGamesResult.Contents;
                        Refresh(nameof(Heartbeat));
                    }
                    else
                    {
                        Support.Log("Server error: " + runningGamesResult.Error);                        
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
            GameName = initInfo.GameName;
            await PerformPostEventTasks();
            RefreshPopovers();
        }
        else
        {
            Support.Log(resultMessage.ToString(CurrentSkin));
        }

        return resultMessage;
    }
    
    private async Task Connect()
    {
        var result = await _connection.InvokeAsync<Result<ServerInfo>>(nameof(IGameHub.Connect));
        if (result.Success)
        {
            ServerInfo = result.Contents;
        }
        else
        {
            Support.Log(result.Error);
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
        Refresh(nameof(PerformPostEventTasks));
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
                await Browser.PlaySound(CurrentSkin.Sound_YourTurn_URL, CurrentEffectVolume);
            }
        }
    }

    private async Task PlaySoundsForMilestones()
    {
        foreach (var m in Game.RecentMilestones) 
            await Browser.PlaySound(CurrentSkin.GetSound(m), CurrentEffectVolume);
    }

    private Phase previousPhase;
    private void PerformEndOfTurnTasks()
    {
        if (Game.CurrentPhase == Phase.TurnConcluded && Game.CurrentPhase != previousPhase) 
            Messages.Clear();

        previousPhase = Game.CurrentPhase;
    }
    
    private async Task OnReconnected(string _) => await RequestReconnectGame();

    private Task OnDisconnected(Exception arg)
    {
        Refresh(nameof(OnDisconnected));
        return Task.CompletedTask;
    }
    
    private async Task RequestReconnectGame()
    {
        var result = await _connection.InvokeAsync<Result<GameInitInfo>>(nameof(IGameHub.RequestReconnectGame), UserToken, GameId);
        if (result.Success)
        {
            var loadMessage = await LoadGame(result.Contents);
            if (loadMessage != null)
                return;

            GameId = result.Contents.GameId;
            Refresh(nameof(RequestReconnectGame));
        }
    }

    private async Task<Result<VoidContents>> Invoke(string hubMethod, params object[] args) =>
        await Invoke<VoidContents>(hubMethod, args);
 
    private async Task<Result<T>> Invoke<T>(string hubMethod, params object[] args)
    {
        var result = args.Length switch
        {
            0 => await _connection.InvokeAsync<Result<T>>(hubMethod),
            1 => await _connection.InvokeAsync<Result<T>>(hubMethod, args[0]),
            2 => await _connection.InvokeAsync<Result<T>>(hubMethod, args[0], args[1]),
            3 => await _connection.InvokeAsync<Result<T>>(hubMethod, args[0], args[1], args[2]),
            4 => await _connection.InvokeAsync<Result<T>>(hubMethod, args[0], args[1], args[2], args[3]),
            5 => await _connection.InvokeAsync<Result<T>>(hubMethod, args[0], args[1], args[2], args[3], args[5]),
            _ => throw new ArgumentException("Too many arguments")
        };
        
        if (!result.Success && result.Error is ErrorType.UserNotFound && StoredPassword != null)
        {
            if ((await RequestLogin(UserName, StoredPassword)).Success)
            {
                result = args.Length switch
                {
                    0 => await _connection.InvokeAsync<Result<T>>(hubMethod),
                    1 => await _connection.InvokeAsync<Result<T>>(hubMethod, args[0]),
                    2 => await _connection.InvokeAsync<Result<T>>(hubMethod, args[0], args[1]),
                    3 => await _connection.InvokeAsync<Result<T>>(hubMethod, args[0], args[1], args[2]),
                    4 => await _connection.InvokeAsync<Result<T>>(hubMethod, args[0], args[1], args[2], args[3]),
                    5 => await _connection.InvokeAsync<Result<T>>(hubMethod, args[0], args[1], args[2], args[3], args[5]),
                    _ => throw new ArgumentException("Too many arguments")
                };
            }
        }

        return result;
    }
   
    private static void LogSerializationError(object sender, ErrorEventArgs e) => Support.Log(e.ErrorContext.Error.ToString());
}

