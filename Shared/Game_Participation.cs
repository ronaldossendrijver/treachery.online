/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
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
    
    public Participation Participation { get; } = new();
    
    public bool IsPlaying(Faction faction) => Players.Any(p => p.Faction == faction);

    public Player GetPlayer(Faction? f) => Players.FirstOrDefault(p => p.Faction == f);

    private Player GetPlayerInSeat(int seat) => Players.FirstOrDefault(p => p.Seat == seat);
    
    public Faction GetFactionInSeat(int seat) => GetPlayerInSeat(seat)?.Faction ?? Faction.None;
    
    public Faction GetAlly(Faction f) => GetPlayer(f)?.Ally ?? Faction.None;

    public IEnumerable<Faction> PlayersOtherThan(Player p) => Players.Where(x => x.Faction != p.Faction).Select(x => x.Faction);

    public int GetUserIdOfPlayer(Player player) => UserIdInSeat(player.Seat);
    
    public string GetPlayerName(int userId) => Participation.PlayerNames.GetValueOrDefault(userId, "?");

    public string GetPlayerName(Player player) => Version >= 170
        ? player.IsBot ? "Bot" : Participation.PlayerNames.GetValueOrDefault(UserIdInSeat(player.Seat), "?")
        : LegacyNames.GetValueOrDefault(player, string.Empty);
    
    public Player GetPlayerByUserId(int userId) => GetPlayerBySeat(SeatOf(userId));

    public Player GetPlayerByName(string name) => Players.FirstOrDefault(p => GetPlayerName(p) == name);

    public int GetUserIdByName(string name) => Participation.PlayerNames.FirstOrDefault(u => u.Value == name).Key;
    
    public Player GetPlayerBySeat(int seatNr) => Players.FirstOrDefault(p => p.Seat == seatNr);

    public int UserIdInSeat(int seat)
    {
        foreach (var userIdAndSeat in Participation.SeatedPlayers)
            if (userIdAndSeat.Value == seat)
                return userIdAndSeat.Key;

        return -1;
    } 
    
    private int SeatOf(int userId) => Participation.SeatedPlayers.GetValueOrDefault(userId, -1);

    public bool IsOpen(int seat) => 
        CurrentPhase is Phase.AwaitingPlayers && Participation.SeatedPlayers.Count < MaximumPlayers || 
        SeatIsAvailable(seat);
    
    public bool IsBot(Player p) => !Participation.SeatedPlayers.ContainsValue(p.Seat);
    
    public bool SeatIsAvailable(int seat) => Participation.AvailableSeats.Contains(seat);
    
    public int NumberOfHosts => Participation.Hosts.Count;

    public int NumberOfBots => Players.Count(IsBot);
    
    public int NumberOfObservers => Participation.Observers.Count;
    
    public int NumberOfSeatedPlayers => Participation.SeatedPlayers.Count;
    
    public IEnumerable<string> PlayerNames =>
        Participation.PlayerNames
            .Where(idAndName => !Participation.Observers.Contains(idAndName.Key))
            .Select(idAndName => idAndName.Value);
    
    public IEnumerable<string> ObserverNames => 
        Participation.PlayerNames
            .Where(idAndName => Participation.Observers.Contains(idAndName.Key))
            .Select(idAndName => idAndName.Value);

    public bool IsPlayer(int userId) => Participation.SeatedPlayers.ContainsKey(userId);
    
    public bool IsHost(int userId) => Participation.Hosts.Contains(userId);
    
    public bool IsObserver(int userId) => Participation.Observers.Contains(userId);
    
    public bool IsParticipant(int userId) => Participation.PlayerNames.ContainsKey(userId);

    public bool WasKicked(int userId) => Participation.Kicked.Contains(userId);
    
    public void AddPlayer(int userId, string playerName, int seat = -1)
    {
        if (seat >= 0)
        {
            var player = GetPlayerInSeat(seat);
            Log(playerName, " now controls ", player.Faction);
        }

        Participation.SeatedPlayers[userId] = seat;
        Participation.PlayerNames[userId] = playerName;
        Participation.AvailableSeats.Remove(seat);
        Participation.Observers.Remove(userId);
    }
    
    public void AddObserver(int userId, string observerName)
    {
        if (CurrentReport != null)
            Log(observerName, " is now observing");
        
        Participation.Observers.Add(userId);
        Participation.PlayerNames[userId] = observerName;
    }

    public void RemoveUser(int userId, bool kick)
    {
        if (CurrentReport != null)
            Log(GetPlayerName(userId), kick ? " was kicked from" : " left", " the game");
        
        Participation.SeatedPlayers.Remove(userId);
        Participation.Observers.Remove(userId);
        Participation.PlayerNames.Remove(userId);
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

    public void ResetSeats()
    {
        foreach (var userId in Participation.SeatedPlayers.Keys)
            Participation.SeatedPlayers[userId] = -1;
    }
}