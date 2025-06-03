/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Treachery.Client;

public class Client : IGameService, IGameClient, IAsyncDisposable
{
    private const int NudgeBotsDelay = 7000;
    
    //General info
    public ServerInfo ServerInfo { get; private set; }
    public Skin CurrentSkin { get; set; } = DefaultSkin.Default;
    public bool IsConnected => _connection.State == HubConnectionState.Connected;
    
    //Admin info
    public AdminInfo AdminInfo { get; private set; }
    
    //Server info
    public bool IsAdmin { get; set; } = false;
    public GameInfo[] OwnGames { get; private set; } = [];
    public GameInfo[] ActiveGames { get; private set; } = [];
    public GameInfo[] ActiveGamesWithOpenSeats { get; private set; } = [];
    public GameInfo[] ActiveGamesWithoutOpenSeats { get; private set; } = [];
    public ScheduledGameInfo[] ScheduledGames { get; private set; } = [];
    public Dictionary<int,LoggedInUserInfo> RecentlySeenUsers { get; private set; } = [];
    
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
    public string GameId { get; private set; } = string.Empty;
    public GameStatus Status { get; private set; }
    public List<Type> Actions { get; private set; } = []; 
    
    public bool InGame => Game != null;
    private bool ReseatRequested { get; set; }
    public bool PlayersNeedSeating => InGame && (ReseatRequested || Game.CurrentPhase >= Phase.TradingFactions && Game.Participation.SeatedPlayers.ContainsValue(-1));
    public Player Player => Game.GetPlayerByUserId(UserId) ?? new Player(Game, Faction.None);
    public string PlayerName => LoginInfo.PlayerName;
    public UserStatus UserStatus => LoginInfo != null ? GetUserStatus(LoginInfo.UserId) : UserStatus.None;
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
            .AddJsonProtocol(jsonOptions => jsonOptions.PayloadSerializerOptions.IncludeFields = true)
            .WithUrl(navigationManager.ToAbsoluteUri("/gameHub"))
            .WithAutomaticReconnect(new RetryPolicy())
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
        
        _ = Task.Delay(IGameService.HeartbeatDelay).ContinueWith(_ => Heartbeat());
    }
    
    private void RegisterHandlers()
    {
        _connection.On<GameEvent,int>(nameof(HandleGameEvent), HandleGameEvent);
        _connection.On<GameChatMessage>(nameof(HandleChatMessage), HandleChatMessage);
        _connection.On<GlobalChatMessage>(nameof(HandleGlobalChatMessage), HandleGlobalChatMessage);
        _connection.On<GameSettings>(nameof(HandleUpdateSettings), HandleUpdateSettings);
        _connection.On<int>(nameof(HandleSetTimer), HandleSetTimer);
        _connection.On<string>(nameof(HandleSetSkin), HandleSetSkin);
        _connection.On<int>(nameof(HandleUndo), HandleUndo);
        _connection.On<int,string,int>(nameof(HandleJoinGame), HandleJoinGame);
        _connection.On<int>(nameof(HandleSetOrUnsetHost), HandleSetOrUnsetHost);
        _connection.On<int,string>(nameof(HandleObserveGame), HandleObserveGame);
        _connection.On<int[]>(nameof(HandleOpenOrCloseSeats), HandleOpenOrCloseSeats);
        _connection.On<int,bool>(nameof(HandleRemoveUser), HandleRemoveUser);
        _connection.On<bool>(nameof(HandleBotStatus), HandleBotStatus);
        _connection.On<GameInitInfo>(nameof(HandleLoadGame), HandleLoadGame);
        _connection.On<Dictionary<int, int>>(nameof(HandleAssignSeats), HandleAssignSeats);
    }

    public void Refresh(string source = null)
    {
        RefreshHandler?.Invoke();  
    } 

    private void RefreshPopovers() => RefreshPopoverHandler?.Invoke();

    public async Task ExitGame()
    {
        await SendHeartbeatAndGetServerStatus();
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
    
    public Task HandleUpdateSettings(GameSettings settings)
    {
        Game.Settings = settings;
        Refresh(nameof(HandleUpdateSettings));
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
        Refresh(nameof(HandleObserveGame));
        return Task.CompletedTask;
    }

    public Task HandleOpenOrCloseSeats(int[] seats)
    {
        foreach (var seat in seats)
            Game.OpenOrCloseSeat(seat);
        
        Refresh(nameof(HandleOpenOrCloseSeats));
        return Task.CompletedTask;
    }

    public async Task HandleRemoveUser(int userId, bool kick)
    {
        if (userId == UserId)
        {
            await ExitGame();
        }
        else
        {
            Game.RemoveUser(userId, kick);
            Refresh();
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
        if (!InGame)
            return;
        
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

    public Task HandleAssignSeats(Dictionary<int, int> assignment)
    {
        Game.Participation.SeatedPlayers = assignment;
        Refresh();
        return Task.CompletedTask;
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
            await SendHeartbeatAndGetServerStatus();
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
    
    public async Task<string> RequestSetUserStatus(UserStatus status)
    {
        var result = await Invoke<ServerStatus>(nameof(IGameHub.RequestSetUserStatus), UserToken, status);
        return UpdateServerStatusOnSuccess(result);
    }
    
    //Game Management

    public async Task<string> RequestCreateGame(string password, string stateData = null, string skinData = null)
    {
        var result = await Invoke<GameInitInfo>(nameof(IGameHub.RequestCreateGame), UserToken, password, stateData, skinData);
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
    
    public async Task<string> RequestScheduleGame(DateTimeOffset dateTime, Ruleset? ruleset, int? numberOfPlayers, int? maximumTurns, List<Faction> allowedFactions, bool asyncPlay)
    {
        var result = await Invoke<ServerStatus>(nameof(IGameHub.RequestScheduleGame), UserToken, dateTime, ruleset, numberOfPlayers, maximumTurns, allowedFactions, asyncPlay);
        return UpdateServerStatusOnSuccess(result);
    }
    
    public async Task<string> RequestCancelGame(string scheduledGameId)
    {
        var result = await Invoke<ServerStatus>(nameof(IGameHub.RequestCancelGame), UserToken, scheduledGameId);
        return UpdateServerStatusOnSuccess(result);
    }

    public async Task<string> RequestCloseGame(string gameId)
    {
        var result = await Invoke<ServerStatus>(nameof(IGameHub.RequestCloseGame), UserToken, gameId);
        return UpdateServerStatusOnSuccess(result);
    }

    private string UpdateServerStatusOnSuccess(Result<ServerStatus> result)
    {
        if (!result.Success) 
            return CurrentSkin.Describe(result.Error);
        
        HandleUpdatedServerStatus(result.Contents);
        return string.Empty;
    }

    public async Task<string> RequestUpdateSettings(string gameId, GameSettings settings)
    {
        var result = await Invoke(nameof(IGameHub.RequestUpdateSettings), UserToken, gameId, settings);
        return result.Success ? string.Empty : CurrentSkin.Describe(result.Error);
    }

    public async Task<string> RequestJoinGame(string gameId, string password, int seat)
    {
        var result = await Invoke<GameInitInfo>(nameof(IGameHub.RequestJoinGame), UserToken, gameId, password, seat);
        if (!result.Success) 
            return CurrentSkin.Describe(result.Error);
        
        var loadMessage = await LoadGame(result.Contents);
        if (loadMessage != null)
            return loadMessage.ToString();

        GameId = result.Contents.GameId;
        return null;
    }
    
    public async Task<string> RequestSubscribeGame(string scheduledGameId, SubscriptionType subscription)
    {
        var result = await Invoke<ServerStatus>(nameof(IGameHub.RequestSubscribeGame), UserToken, scheduledGameId, subscription);
        return UpdateServerStatusOnSuccess(result);
    }

    public async Task<string> RequestObserveGame(string gameId, string password)
    {
        var result = await Invoke<GameInitInfo>(nameof(IGameHub.RequestObserveGame), UserToken, gameId, password);
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

    public void RequestReseat()
    {
        ReseatRequested = true;
        Refresh(nameof(RequestReseat));
    } 
    
    public async Task<string> RequestLeaveGame()
    {
        var result = await Invoke<ServerStatus>(nameof(IGameHub.RequestLeaveGame), UserToken, GameId);
        return UpdateServerStatusOnSuccess(result);
    }

    public async Task<string> RequestKick(int userId) => 
        CurrentSkin.Describe((await Invoke(nameof(IGameHub.RequestKick), UserToken, GameId, userId)).Error);

    public async Task<string> RequestLoadGame(string state, string skin = null)
    {
        var result = await Invoke(nameof(IGameHub.RequestLoadGame), UserToken, GameId, state, skin);
        return result.Success ? null :
            result.Error is ErrorType.InvalidGameEvent ? result.ErrorDetails : CurrentSkin.Describe(result.Error);
    }

    public async Task<string> RequestAssignSeats(Dictionary<int, int> assignment)
    {
        ReseatRequested = false;
        return CurrentSkin.Describe((await Invoke(nameof(IGameHub.RequestAssignSeats), UserToken, GameId, assignment)).Error);
    }

    public async Task<string> RequestSetSkin(string skin)
    {
        var result = await Invoke(nameof(IGameHub.RequestSetSkin), UserToken, GameId, skin);
        return result.Success ? null : CurrentSkin.Describe(result.Error);
    }
        

    public async Task<string> RequestUndo(int untilEventNr) =>
        CurrentSkin.Describe((await Invoke(nameof(IGameHub.RequestUndo), UserToken, GameId, untilEventNr)).Error);

    public async Task<string> SetTimer(int value) =>
        CurrentSkin.Describe((await Invoke(nameof(IGameHub.SetTimer), value)).Error);

    /*
    public async Task<string> RequestEstablishPlayers(EstablishPlayers e)
    {
        var result = await Invoke<Participation>(nameof(IGameHub.RequestEstablishPlayers), UserToken, GameId, e);
        if (!result.Success)
        {
            return result.Error is ErrorType.InvalidGameEvent ? result.ErrorDetails : CurrentSkin.Describe(result.Error);
        }

        Game.Participation = result.Contents;
        return null;
    }
    */
    
    //TODO: make strongly typed methods
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
    
    public async Task<string> AdminCancelGame(string scheduledGameId)
    {
        var result = await Invoke<string>(nameof(IGameHub.AdminCancelGame), UserToken, scheduledGameId);
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

    private async Task Heartbeat()
    {
        try
        {
            if (IsConnected && LoggedIn)
            {
                await SendHeartbeatAndGetServerStatus();
            }
        }
        catch (Exception e)
        {
            Support.Log(e.ToString());
        }

        _ = Task.Delay(IGameService.HeartbeatDelay).ContinueWith(_ => Heartbeat());
    }

    private async Task SendHeartbeatAndGetServerStatus()
    {
        var scope = InGame ? GameListScope.None :
            IsAdmin ? GameListScope.All : GameListScope.Active;
        
        var result = await Invoke<ServerStatus>(nameof(IGameHub.RequestHeartbeat), UserToken, scope);
        UpdateServerStatusOnSuccess(result);
    }

    private void HandleUpdatedServerStatus(ServerStatus status)
    {
        OwnGames = status.OwnGames;
        ActiveGames = status.RunningGames;
        ActiveGamesWithOpenSeats = ActiveGames.Where(g => g.CanBeJoined).ToArray();
        ActiveGamesWithoutOpenSeats = ActiveGames.Where(g => !g.CanBeJoined).ToArray();
        ScheduledGames = status.ScheduledGames;
        RecentlySeenUsers = status.RecentlySeenUsers.ToDictionary(x => x.Id, x => x);
        Refresh(nameof(HandleUpdatedServerStatus));
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

        var lastActionTime = Game.LastAction;
        if (!Status.WaitingForHost && IsHost && Status.WaitingForPlayers.Count > 0 && Status.WaitingForPlayers.All(p => p.IsBot))
            _ = Task.Delay(NudgeBotsDelay).ContinueWith(_ => RequestNudgeBots(lastActionTime));
        
        await TurnAlert();
        await PlaySoundsForMilestones();
        await Browser.RemoveFocusFromButtons();

        if (Game.CurrentMainPhase == MainPhase.Bidding) 
            ResetAutoPassThreshold();

        Refresh(nameof(PerformPostEventTasks));
    }

    private async Task RequestNudgeBots(DateTime gameLastAction)
    {
        if (Game.LastAction == gameLastAction)
            await _connection.InvokeAsync<VoidResult>(nameof(IGameHub.RequestNudgeBots));
    }

    private void ResetAutoPassThreshold()
    {
        if (Game.RecentMilestones.Contains(Milestone.AuctionWon) && (!KeepAutoPassSetting || Game.CurrentPhase == Phase.BiddingReport)) AutoPass = false;
    }

    private bool _itAlreadyWasMyTurn;
    private async Task TurnAlert()
    {
        if (_itAlreadyWasMyTurn)
        {
            _itAlreadyWasMyTurn = !Status.WaitingForOthers(Player, IsHost);
        }
        else
        {
            if (!Status.WaitingForOthers(Player, IsHost) && Game.CurrentMainPhase != MainPhase.Battle)
            {
                _itAlreadyWasMyTurn = true;
                await Browser.PlaySound(CurrentSkin.Sound_YourTurn_URL, CurrentEffectVolume);
            }
        }
    }

    private async Task PlaySoundsForMilestones()
    {
        foreach (var m in Game.RecentMilestones) 
            await Browser.PlaySound(CurrentSkin.GetSound(m), CurrentEffectVolume);
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
            5 => await _connection.InvokeAsync<Result<T>>(hubMethod, args[0], args[1], args[2], args[3], args[4]),
            6 => await _connection.InvokeAsync<Result<T>>(hubMethod, args[0], args[1], args[2], args[3], args[4], args[5]),
            7 => await _connection.InvokeAsync<Result<T>>(hubMethod, args[0], args[1], args[2], args[3], args[4], args[5], args[6]),
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
                    5 => await _connection.InvokeAsync<Result<T>>(hubMethod, args[0], args[1], args[2], args[3], args[4]),
                    6 => await _connection.InvokeAsync<Result<T>>(hubMethod, args[0], args[1], args[2], args[3], args[4], args[5]),
                    7 => await _connection.InvokeAsync<Result<T>>(hubMethod, args[0], args[1], args[2], args[3], args[4], args[5], args[6]),
                    _ => throw new ArgumentException("Too many arguments")
                };
            }
        }

        return result;
    }

    private UserStatus GetUserStatus(int userId) =>
        RecentlySeenUsers.TryGetValue(userId, out var user) ? user.Status : UserStatus.None;
}