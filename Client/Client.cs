/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
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
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Treachery.Client;

public class Client : IGameService, IGameClient
{
    public const int HeartbeatDelay = 10000;
    private const int MaxHeartbeats = 17280;
    private const int DisconnectTimeout = 25000;

    //Server info
    public ServerSettings ServerSettings { get; private set; }
    
    //Logged in player
    public Dictionary<Guid, string> JoinErrors { get; } = new();
    public bool LoggedIn => LoginInfo != null;
    public LoginInfo LoginInfo { get; set; }
    public string PlayerToken => LoginInfo.PlayerToken;
    public int UserId => LoginInfo.UserId;
    private DateTime LastPing { get; set; }
    
    //Game in progress
    public string GameToken { get; set; }
    public Game Game { get; private set; }
    public GameParticipation Participation { get; private set; }
    public string PlayerName => Participation.PlayerNames.GetValueOrDefault(UserId, "?");
    public GameStatus Status { get; private set; }
    public bool IsObserver { get; private set; }
    public DateTime Disconnected { get; private set; }
    public bool BotsArePaused => Participation.BotsArePaused;
    
    //Sound and camera
    public float CurrentEffectVolume { get; set; } = -1;
    public float CurrentChatVolume { get; set; } = -1;

    //Settings
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
    }
    
    public async Task Start()
    {
        await _connection.StartAsync();
        await GetServerSettings();
    }
    
    public bool IsDisconnected => Disconnected != default;

    public void Refresh() => RefreshHandler?.Invoke();

    public void RefreshPopovers() => RefreshPopoverHandler?.Invoke();

    private void LogSerializationError(object sender, ErrorEventArgs e)
    {
        Support.Log(e.ErrorContext.Error.ToString());
    }

    public string MyName => Participation.PlayerNames.GetValueOrDefault(UserId);
    
    public void Reset()
    {
        GameToken = null;
        Game = null;
        Status = null;
        JoinErrors.Clear();
        IsObserver = false;
        _ = Browser.StopSounds();
        Refresh();
    }
    
    //IGameClient methods

    public async Task HandleSetTimer(int value)
    {
        Timer = value;
        await Task.CompletedTask;
    }

    //Process information about a currently running game on treachery.online
    public List<GameInfo> RunningGames { get; private set; } = [];

    //public IEnumerable<GameInfo> JoinableAdvertisedGames => _advertisedGames.Where(gameAndDate => gameAndDate.Key.CurrentPhase == Phase.AwaitingPlayers && DateTime.Now.Subtract(gameAndDate.Value).TotalSeconds < 15).Select(gameAndDate => gameAndDate.Key);
    //private readonly Dictionary<GameInfo, DateTime> _advertisedGames = new();
    public async Task HandleListOfGames(List<GameInfo> games)
    {
        RunningGames = games;
        if (Game == null)
        {
            Refresh();
        }
        await Task.CompletedTask;
    }
    
    //IGameService methods

    public Task UpdateParticipation(GameParticipation participation)
    {
        Participation = participation;
        return Task.CompletedTask;
    }

    public async Task RequestJoinGame(Guid gameId, string hashedPassword, Faction faction)
    {
        var result = await _connection.InvokeAsync<Result<string>>(nameof(IGameHub.RequestJoinGame), PlayerToken, gameId, hashedPassword, faction);
        if (!result.Success)
        {
            JoinErrors[gameId] = result.Message;
        }
        else
        {
            IsObserver = false;
            GameToken = result.Contents;
        }
    }

    public async Task RequestObserveGame(Guid gameId, string hashedPassword)
    {
        var result = await _connection.InvokeAsync<Result<string>>(nameof(IGameHub.RequestObserveGame), PlayerToken, gameId, hashedPassword);
        if (!result.Success)
        {
            JoinErrors[gameId] = result.Message;
        }
        else
        {
            IsObserver = false;
            GameToken = result.Contents;
        }
    }
    
    private async Task TryToReconnect() =>
        await _connection.InvokeAsync<VoidResult>(nameof(IGameHub.RequestReconnectGame), PlayerToken, GameToken);
    
    public async Task RequestSendGlobalChatMessage(GlobalChatMessage message) =>
        await _connection.SendAsync(nameof(IGameHub.SendGlobalChatMessage), PlayerToken, message);


    //public DateTime HostLastSeen { get; private set; } = DateTime.Now;
    //private int nrOfHeartbeats;
    private string oldConnectionId = "";

    /*public async Task Heartbeat(int gameInProgressHostId)
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

                Disconnected = default;
            }

            await SaveGameInfo();

            //if (!IsDisconnected) await HostProxy.SendHeartbeat(PlayerName);
        }
        catch (Exception e)
        {
            Support.Log(e.ToString());
        }

        _ = Task.Delay(HeartbeatDelay).ContinueWith(e => Heartbeat(gameInProgressHostId));
    }*/

    private bool CheckDisconnect()
    {
        if (_connection.State is not HubConnectionState.Disconnected || 
            DateTime.Now.Subtract(LastPing).TotalMilliseconds > DisconnectTimeout)
        {
            Disconnected = DateTime.Now;
            Refresh();
            return true;
        }

        return false;
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
            var currentStateData = await _connection.InvokeAsync<string>(nameof(IGameHub.RequestGameState), PlayerToken, GameToken);
            resultMessage = LoadGame(currentStateData);
        }
        
        if (resultMessage == null)
        {
            await PerformPostEventTasks(e);
        }
        else
        {
            Support.Log(resultMessage.ToString(Skin.Current));
        }
    }

    private Message LoadGame(string stateData)
    {
        var currentState = GameState.Load(stateData);
        var resultMessage = Game.TryLoad(currentState, true, IsHost, out var result);
        if (resultMessage == null)
        {
            Game = result;
        }

        return resultMessage;
    }

    public async Task HandlePing()
    {
        LastPing = DateTime.Now;
        await Task.CompletedTask;
    }
    
    public async Task HandleSetSkin(string skinData)
    {
        await Browser.SaveStringSetting("treachery.online;setting.skin", skinData);

        Skin.Current = Skin.Load(skinData);

        Message.DefaultDescriber = Skin.Current;

        Refresh();
        RefreshPopovers();
    }

    public bool IsPlayer => Player != null && Player.Faction != Faction.None;

    public Player Player => Game.PlayerAtSeat(Participation.SeatedUsers.GetValueOrDefault(UserId));

    public Faction Faction => Player?.Faction ?? Faction.None;

    public async Task HandleLoadGame(string stateData, string targetPlayerName, string skinData)
    {
        var resultMessage = LoadGame(stateData);
        if (resultMessage == null)
        {
            await PerformPostEventTasks(Game.LatestEvent());
            RefreshPopovers();
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
            await PerformPostEventTasks(null);
        }
        catch (Exception ex)
        {
            Support.Log(ex.ToString());
        }
    }

    public LinkedList<ChatMessage> Messages { get; } = [];
    
    public async Task HandleGlobalChatMessage(GlobalChatMessage m)
    {
        m.DateTimeReceived = DateTime.Now;
        Messages.AddFirst(m);

        if (!MuteGlobalChat)
        {
            await Browser.PlaySound(Skin.Current.Sound_Chatmessage_URL, CurrentChatVolume);
            Refresh();
        }
    }

    public async Task HandleChatMessage(GameChatMessage m)
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
        Refresh();
    }

    private void ResetAutopassThreshold()
    {
        if (Game.RecentMilestones.Contains(Milestone.AuctionWon) && (!KeepAutoPassSetting || Game.CurrentPhase == Phase.BiddingReport)) AutoPass = false;
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
            await Browser.SaveSetting($"treachery.online;currentgame;{PlayerName.ToLower().Trim()};time", DateTime.Now);
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
                    await Browser.SaveSetting($"treachery.online;latestgame;{PlayerName.ToLower().Trim()}", GameState.GetStateAsString(Game));
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


    public bool InScheduledMaintenance =>
        ServerSettings != null &&
        ServerSettings.ScheduledMaintenance.AddMinutes(15) > DateTime.UtcNow &&
        ((ServerSettings.ScheduledMaintenance.Subtract(DateTime.UtcNow).TotalHours < 6 && CurrentPhase <= Phase.AwaitingPlayers) ||
         ServerSettings.ScheduledMaintenance.Subtract(DateTime.UtcNow).TotalHours < 1);

    public IEnumerable<Type> Actions => Game.GetApplicableEvents(Player, IsHost);

    public bool IsConnected => _connection.State == HubConnectionState.Connected;
    
    public bool IsAuthenticated => PlayerToken != null;

    public bool IsHost => Participation.Hosts.Contains(UserId);

    public Phase CurrentPhase => Game.CurrentPhase;

    /*
    private async Task CheckIfPlayerCanReconnect()
    {
        GameInProgressHostId = 0;

        var currentGameHostID = await Browser.LoadSetting<int>(string.Format("treachery.online;currentgame;{0};hostid", PlayerName.ToLower().Trim()));
        if (currentGameHostID == 0) return;

        var currentGameDateTime = await Browser.LoadSetting<DateTime>(string.Format("treachery.online;currentgame;{0};time", PlayerName.ToLower().Trim()));
        if (DateTime.Now.Subtract(currentGameDateTime).TotalSeconds > 900) return;

        GameInProgressHostId = currentGameHostID;
    }
    */

    public async Task ToggleBotPause() =>
        await _connection.SendAsync(nameof(IGameHub.RequestPauseBots), PlayerToken, GameToken);

    private async Task GetServerSettings()
    {
        var result = await _connection.InvokeAsync<Result<ServerSettings>>(nameof(IGameHub.GetServerSettings));
        if (result.Success)
        {
            ServerSettings = result.Contents;
        }
        else
        {
            Support.Log(result.Message);
        }
    }
    

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

    public async Task<string> RequestLogin(string userName, string hashedPassword)
    {
        var result = await _connection.InvokeAsync<Result<LoginInfo>>(nameof(IGameHub.RequestLogin), userName, hashedPassword);

        if (result.Success)
        {
            LoginInfo = result.Contents;
            //TODO: offer this service
            //await CheckIfPlayerCanReconnect();
            Refresh();
            return null;
        }

        return result.Message;
    }
    
    public async Task<string> RequestCreateUser(string userName, string hashedPassword, string email, string playerName)
    {
        var result = await _connection.InvokeAsync<Result<LoginInfo>>(nameof(IGameHub.RequestCreateUser), userName, email, hashedPassword, playerName);

        if (result.Success)
        {
            LoginInfo = result.Contents;
            Refresh();
            return null;
        }

        return result.Message;
    }

    public async Task<string> RequestPasswordReset(string email) => 
        (await _connection.InvokeAsync<VoidResult>(nameof(IGameHub.RequestPasswordReset), email)).Message;
    
    public async Task<string> RequestSetPassword(string userName, string passwordResetToken, string hashedPassword)
    {
        var result = await _connection.InvokeAsync<Result<LoginInfo>>(nameof(IGameHub.RequestSetPassword), userName, passwordResetToken, hashedPassword);

        if (result.Success)
        {
            LoginInfo = result.Contents;
            Refresh();
            return null;
        }

        return result.Message;
    }

}