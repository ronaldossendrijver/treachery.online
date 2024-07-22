// /*
//  * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
//  * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
//  * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
//  * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//  * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
//  * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
// */

using System.Threading.Tasks;
using Treachery.Shared;

namespace Treachery.Server;

public interface IGameClient
{
    
    public Task HandleGlobalChatMessage(GlobalChatMessage message);

    public Task HandleGameEvent<TEvent>(TEvent evt) where TEvent : GameEvent;
    
    /*
    public Game Game { get; }
    public GameStatus Status { get; }
    public int GameInProgressHostId { get; }

    //Player and Host
    public string PlayerName { get; }
    public HostProxy HostProxy { get; }
    public Host Host { get; }
    public bool IsObserver { get; }
    public ServerSettings ServerSettings { get; }
    public Dictionary<int, string> JoinErrors { get; }
    public DateTime Disconnected { get; }
    
    //Client State
    public float CurrentEffectVolume { get; set;  }
    public float CurrentChatVolume { get; set; }
    public Battle BattleUnderConstruction { get; set; }
    public int BidAutoPassThreshold { get; set; }
    public bool Autopass { get; set; }
    public bool KeepAutopassSetting { get; set; }
    public bool StatisticsSent { get; set; }
    public bool BotsArePaused { get; set; }
    public int Timer { get; set; }
    public bool MuteGlobalChat { get; set; }

    public event Action RefreshHandler;
    public event Action RefreshPopoverHandler;
    public bool IsDisconnected { get; }
    public void Refresh();
    public void RefreshPopovers();
    public string MyName { get; }
    public Task Start();
    public Task StartHost(string hostPWD, string loadedGameData, Game loadedGame);

    public IEnumerable<GameInfo> RunningGames { get; }
    public IEnumerable<GameInfo> JoinableAdvertisedGames { get; }

    public Task Request(int hostID, PlayerJoined e);
    public Task Request(int hostID, ObserverJoined e);
    public Task Request(int hostID, PlayerRejoined e);

    public Task Request(int hostID, ObserverRejoined e);

    public Task Request(GlobalChatMessage message);

    public void Reset();

    public DateTime HostLastSeen { get; }

    public Task Heartbeat(int gameInProgressHostId);
    public bool IsPlayer { get; }

    public Player Player { get; }

    public Faction Faction { get; }

    public bool IAm(Faction f);
    
    public LinkedList<ChatMessage> Messages { get; }

    public bool InScheduledMaintenance { get; }

    public IEnumerable<Type> Actions { get; }

    public bool IsConnected { get; }

    public bool IsAuthenticated { get; }

    public bool IsHost { get; }

    public Phase CurrentPhase { get; }

    public Task SetPlayerName(string name);

    public Task ToggleBotPause();

    public event EventHandler<Location> OnLocationSelected;
    public event EventHandler<Location> OnLocationSelectedWithCtrlOrAlt;
    public event EventHandler<Location> OnLocationSelectedWithShift;
    public event EventHandler<Location> OnLocationSelectedWithShiftAndWithCtrlOrAlt;

    public void LocationClick(LocationEventArgs e);

    public Task<string> RequestLogin(string userName, string hashedPassword);

    public Task<string> RequestCreateUser(string userName, string hashedPassword, string email, string playerName);

    public Task<string> RequestPasswordReset(string email);

    public Task<string> RequestSetPassword(string userName, string passwordResetToken, string hashedPassword);
    
    public Task<string> GetRunningGames(string userToken);
    */


    Task HandleChatMessage(GameChatMessage gameChatMessage);
    Task HandleSetTimer(int value);
    Task HandleSetSkin(string skin);
    Task HandleUndo(int untilEventNr);
}