﻿// /*
//  * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
//  * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
//  * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
//  * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//  * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
//  * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
// */
namespace Treachery.Client;

public interface IGameService
{
    //Logged in?
    public bool LoggedIn { get; }
    public int UserId { get; }
    public string UserName { get; }
    public string UserEmail { get; }
    public string PlayerName { get; }
    public UserStatus UserStatus { get; }
    
    //Server info
    public ServerInfo ServerInfo { get; }
    public AdminInfo AdminInfo { get; }
    public List<GameInfo> RunningGames { get; }
    public List<GameInfo> RunningGamesWithOpenSeats { get; }
    public List<GameInfo> RunningGamesWithoutOpenSeats { get; }
    public List<ScheduledGame> ScheduledGames { get; }
    public Dictionary<int,LoggedInUserInfo> RecentlySeenUsers { get; }
    public LoggedInUserInfo GetUserInfo(int userId);
    public UserStatus GetUserStatus(int userId);

    //Game info
    public Game Game { get; }
    public string GameName { get; }
    public GameStatus Status { get; }
    public bool InGame { get; }
    public bool PlayerNeedsSeating { get; }
    public bool IsObserver { get; }
    
    //Client State
    public float CurrentEffectVolume { get; set;  }
    public float CurrentChatVolume { get; set; }
    public Battle BattleUnderConstruction { get; set; }
    public int BidAutoPassThreshold { get; set; }
    public bool AutoPass { get; set; }
    public bool KeepAutoPassSetting { get; set; }
    public int Timer { get; set; }
    public bool MuteGlobalChat { get; set; }

    public event Action RefreshHandler;
    public event Action RefreshPopoverHandler;
    
    public void Refresh(string source = null);
    public Task Start(string userToken = null, string gameId = null);
    public void ExitGame();
    public Player Player { get; }

    public Faction Faction { get; }
    
    public LinkedList<ChatMessage> Messages { get; }

    public bool InScheduledMaintenance { get; }

    public List<Type> Actions { get; }

    public bool IsConnected { get; }

    public bool IsHost { get; }

    public Phase CurrentPhase { get; }
    public Skin CurrentSkin { get; set; }

    public event EventHandler<Location> OnLocationSelected;
    public event EventHandler<Location> OnLocationSelectedWithCtrlOrAlt;
    public event EventHandler<Location> OnLocationSelectedWithShift;
    public event EventHandler<Location> OnLocationSelectedWithShiftAndWithCtrlOrAlt;

    public void LocationClick(LocationEventArgs e);
    
    //Authentication
    
    Task<Result<LoginInfo>> RequestCreateUser(string userName, string hashedPassword, string email, string playerName);
    Task<Result<LoginInfo>> RequestLogin(string userName, string hashedPassword);
    Task<VoidResult> RequestPasswordReset(string usernameOrEmail);
    Task<Result<LoginInfo>> RequestSetPassword(string userName, string passwordResetToken, string newHashedPassword);
    Task<string> RequestUpdateUserInfo(string hashedPassword, string email, string playerName);
    Task<VoidResult> RequestSetUserStatus(UserStatus status);
    
    //Game Management
    
    Task<string> RequestCreateGame(string hashedPassword, string stateData = null, string skinData = null);
    Task<string> RequestCloseGame(string gameId);
    Task<string> RequestJoinGame(string gameId, string hashedPassword, int seat);
    Task<string> RequestObserveGame(string gameId, string hashedPassword);
    Task<string> RequestSetOrUnsetHost(int userId);
    Task<string> RequestOpenOrCloseSeat(int seat);
    Task<string> RequestLeaveGame();
    Task<string> RequestKick(int userId);
    Task<string> RequestScheduleGame(DateTimeOffset dateTime, Ruleset? ruleset, int? numberOfPlayers, int? maximumTurns,
        List<Faction> allowedFactionsInPlay, bool asyncPlay);
    Task<string> RequestCancelGame(string scheduledGameId);
    Task<string> RequestSubscribeGame(string scheduledGameId, SubscriptionType subscription);
    
    Task<string> RequestLoadGame(string state, string skin = null);
    Task<string> RequestAssignSeats(Dictionary<int, int> seatedPlayers);
    Task<string> RequestSetSkin(string skin);
    Task<string> RequestUndo(int untilEventNr);
    Task<string> RequestPauseBots();
    
    //Game Events
    Task<string> SetTimer(int value);
    Task<string> RequestGameEvent<T>(T e) where T : GameEvent;
    
    //Chat
    Task SendChatMessage(GameChatMessage e);
    Task SendGlobalChatMessage(GlobalChatMessage message);
    
    //Admin
    Task<string> AdminUpdateMaintenance(DateTimeOffset maintenanceDate);
    Task<string> AdminPersistState();
    Task<string> AdminRestoreState();
    Task<string> AdminCloseGame(string gameId);
    Task<string> AdminDeleteUser(int userId);
    Task<string> GetAdminInfo();
    
}