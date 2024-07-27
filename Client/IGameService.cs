// /*
//  * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
//  * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
//  * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
//  * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//  * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
//  * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
// */
namespace Treachery.Client;

public interface IGameService
{
    public Game Game { get; }
    public GameStatus Status { get; }
    public bool InGame { get; }
    
    //Player and Host
    public bool LoggedIn { get; }
    public string PlayerName { get; }
    public bool IsObserver { get; }
    public ServerSettings ServerSettings { get; }
    public Dictionary<Guid, string> JoinErrors { get; }
    public DateTime Disconnected { get; }
    public bool InGame { get; }
    
    //Client State
    public float CurrentEffectVolume { get; set;  }
    public float CurrentChatVolume { get; set; }
    public Battle BattleUnderConstruction { get; set; }
    public int BidAutoPassThreshold { get; set; }
    public bool AutoPass { get; set; }
    public bool KeepAutoPassSetting { get; set; }
    public bool BotsArePaused { get; }
    public int Timer { get; set; }
    public bool MuteGlobalChat { get; set; }

    public event Action RefreshHandler;
    public event Action RefreshPopoverHandler;
    public bool IsDisconnected { get; }
    public void Refresh();
    public void RefreshPopovers();
    public Task Start();
    
    public List<GameInfo> RunningGames { get; }

    public Task RequestSendGlobalChatMessage(GlobalChatMessage message);

    public void Reset();

    public bool IsPlayer { get; }

    public Player Player { get; }

    public Faction Faction { get; }
    
    public LinkedList<ChatMessage> Messages { get; }

    public bool InScheduledMaintenance { get; }

    public IEnumerable<Type> Actions { get; }

    public bool IsConnected { get; }

    public bool IsAuthenticated { get; }

    public bool IsHost { get; }

    public Phase CurrentPhase { get; }
    
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
    
}