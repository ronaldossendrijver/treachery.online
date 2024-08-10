/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public partial class Game
{
    public List<Player> Players { get; private set; } = [];

    public Dictionary<Player, string> LegacyNames { get; } = [];
    
    public GameParticipation Participation { get; } = new();
    
    public bool IsPlaying(Faction faction) => Players.Any(p => p.Faction == faction);

    public Player GetPlayer(Faction? f) => Players.FirstOrDefault(p => p.Faction == f);
    
    public Player GetPlayerInSeat(int seat) => Players.FirstOrDefault(p => p.Seat == seat);
    
    public Faction GetFactionInSeat(int seat) => GetPlayerInSeat(seat)?.Faction ?? Faction.None;
    
    public Faction GetAlly(Faction f) => GetPlayer(f)?.Ally ?? Faction.None;

    public IEnumerable<Faction> PlayersOtherThan(Player p) => Players.Where(x => x.Faction != p.Faction).Select(x => x.Faction);

    public int GetUserIdOfPlayer(Player player) => UserIdInSeat(player.Seat);
    
    public string GetPlayerName(int userId) => Participation.Users.GetValueOrDefault(userId);

    public string GetPlayerName(Player player) => Version >= 170
        ? player.IsBot ? "Bot" : Participation.Users.GetValueOrDefault(UserIdInSeat(player.Seat))
        : LegacyNames.GetValueOrDefault(player);
    
    public Player GetPlayerByUserId(int userId) => GetPlayerBySeat(SeatOf(userId));

    public Player GetPlayerByName(string name) => Players.FirstOrDefault(p => GetPlayerName(p) == name);

    public int GetUserIdByName(string name) => Participation.Users.FirstOrDefault(u => u.Value == name).Key;
    
    public Player GetPlayerBySeat(int seatNr) => Players.FirstOrDefault(p => p.Seat == seatNr);
    
    private int UserIdInSeat(int seat) => Participation.SeatedPlayers.FirstOrDefault(seatedUser => seatedUser.Value == seat).Key;
    
    private int SeatOf(int userId) => Participation.SeatedPlayers.GetValueOrDefault(userId);

    public bool IsOpen(int seat) => 
        CurrentPhase is Phase.AwaitingPlayers && Participation.StandingPlayers.Count < MaximumPlayers || 
        Participation.AvailableSeats.Contains(seat);
    
    public bool IsBot(Player p) => IsBot(p.Seat);
    public bool IsBot(int seat) => !Participation.SeatedPlayers.ContainsValue(seat);
    
    public bool SeatIsAvailable(int seat) => 
        Participation.AvailableSeats.Contains(seat) || 
        Participation.BotPositionsAreAvailable && IsBot(seat);
    
    public int NumberOfHosts => Participation.Hosts.Count;

    public int NumberOfBots => Players.Count(IsBot);
    
    public int NumberOfObservers => Participation.Observers.Count;
    
    public int NumberOfPlayers => CurrentPhase is Phase.AwaitingPlayers ? Participation.StandingPlayers.Count : Participation.SeatedPlayers.Count;
    
    public IEnumerable<string> PlayerNames =>
        Participation.Users
            .Where(idAndName => !Participation.Observers.Contains(idAndName.Key))
            .Select(idAndName => idAndName.Value);
    
    public IEnumerable<string> ObserverNames => 
        Participation.Users
            .Where(idAndName => Participation.Observers.Contains(idAndName.Key))
            .Select(idAndName => idAndName.Value);
    
    public bool IsPlayer(int userId) => CurrentPhase is Phase.AwaitingPlayers && Participation.SeatedPlayers.ContainsKey(userId) || 
                                        Participation.StandingPlayers.Contains(userId);
    
    public bool IsHost(int userId) => Participation.Hosts.Contains(userId);
    
    public bool IsObserver(int userId) => Participation.Observers.Contains(userId);
    
    public bool IsParticipant(int userId) => Participation.Users.ContainsKey(userId);

    public bool WasKicked(int userId) => Participation.Kicked.Contains(userId);
    
    public void AddPlayer(int userId, string playerName, int seat = -1)
    {
        if (CurrentPhase is Phase.AwaitingPlayers)
        {
            Participation.StandingPlayers.Add(userId);            
        }
        else
        {
            var replacedPlayer = GetPlayerInSeat(seat);
            if (Participation.SeatedPlayers.ContainsValue(seat))
            {
                var currentUserId = Participation.SeatedPlayers.First(keyValue => keyValue.Value == seat).Key;
                Participation.SeatedPlayers.Remove(currentUserId);
                AddObserver(userId, Participation.Users.GetValueOrDefault(userId));
            }
            
            Log(playerName, " now controls ", replacedPlayer.Faction);
            Participation.SeatedPlayers[userId] = seat;
            Participation.AvailableSeats.Remove(seat);
        }

        Participation.Users[userId] = playerName;
    }
    
    public void AddObserver(int userId, string observerName)
    {
        if (CurrentReport != null)
            Log(observerName, " is now observing this game");
        
        Participation.Observers.Add(userId);
        Participation.Users[userId] = observerName;
    }

    public void RemoveUser(int userId, bool kick)
    {
        if (CurrentReport != null)
            Log(Participation.Users.GetValueOrDefault(userId), kick ? " was kicked" : " was removed", " from the game");
        
        Participation.StandingPlayers.Remove(userId);
        Participation.SeatedPlayers.Remove(userId);
        Participation.Observers.Remove(userId);
        Participation.Users.Remove(userId);
        Participation.Hosts.Remove(userId);

        if (kick)
        {
            Participation.Kicked.Add(userId);
        }
    }

    public void OpenOrCloseSeat(int seat)
    {
        if (SeatIsAvailable(seat))
        {
            Participation.AvailableSeats.Remove(seat);
        }
        else
        {
            Participation.AvailableSeats.Add(seat);
        }
    }

    public void SetOrUnsetHost(int userId)
    {
        if (IsHost(userId))
        {
            Participation.Hosts.Remove(userId);
        }
        else
        {
            Participation.Hosts.Add(userId);
        }
    }
}