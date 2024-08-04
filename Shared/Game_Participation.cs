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
    public List<Player> InitialBots { get; set; } = [];
    public GameParticipation Participation { get; private set; } = new();
    
    public bool IsPlaying(Faction faction)
    {
        return Players.Any(p => p.Faction == faction);
    }

    public Player GetPlayer(Faction? f)
    {
        return Players.FirstOrDefault(p => p.Faction == f);
    }
    
    public Player GetPlayerInSeat(int seat)
    {
        return Players.FirstOrDefault(p => p.Seat == seat);
    }
    
    public Faction GetFactionInSeat(int seat)
    {
        var p = GetPlayerInSeat(seat);
        return p?.Faction ?? Faction.None;
    }
    
    public Faction GetAlly(Faction f)
    {
        var player = GetPlayer(f);
        return player?.Ally ?? Faction.None;
    }

    public IEnumerable<Faction> PlayersOtherThan(Player p)
    {
        return Players.Where(x => x.Faction != p.Faction).Select(x => x.Faction);
    }

    public int GetUserIdOfPlayer(Player player) => Participation.SeatedPlayers.GetValueOrDefault(player.Seat);
    
    public string GetPlayerName(int userId) => Participation.UserNames.GetValueOrDefault(userId);
    
    public string GetPlayerName(Player player) => Participation.UserNames.GetValueOrDefault(UserIdInSeat(player.Seat));
    
    public bool IsHost(Player player) => IsHost(UserIdInSeat(player.Seat));

    public Player GetPlayerByUserId(int userId) => GetPlayerBySeat(SeatOf(userId));

    public Player GetPlayerByName(string name) => Players.FirstOrDefault(p => GetPlayerName(p) == name);

    public int GetUserIdByName(string name) => Participation.UserNames.FirstOrDefault(u => u.Value == name).Key;
    
    public Player GetPlayerBySeat(int seatNr) => Players.FirstOrDefault(p => p.Seat == seatNr);
    
    private int UserIdInSeat(int seat) => Participation.SeatedPlayers.FirstOrDefault(seatedUser => seatedUser.Value == seat).Key;
    
    private int SeatOf(int userId) => Participation.SeatedPlayers.GetValueOrDefault(userId);

    public bool IsOpen(int seat) => 
        CurrentPhase is Phase.AwaitingPlayers && Participation.StandingPlayers.Count < MaximumPlayers || 
        Participation.AvailableSeats.Contains(seat);
    
    public int SeatOf(Faction f) => GetPlayer(f)?.Seat ?? -1;

    public bool IsBot(Player p) => Participation.SeatedBots.Contains(p.Seat);
    public bool IsBot(int seat) => Participation.SeatedBots.Contains(seat);
    
    public void SeatOrUnseatBot(int seat)
    {
        if (IsBot(seat))
        {
            Participation.SeatedBots.Remove(seat);
        }
        else
        {
            Participation.SeatedBots.Add(seat);
        }
    }
    
    public void AddPlayer(int userId, string playerName, int seat = -1)
    {
        if (CurrentPhase is Phase.AwaitingPlayers)
        {
            Participation.StandingPlayers.Add(userId);            
        }
        else
        {
            if (Participation.SeatedPlayers.ContainsValue(seat))
            {
                var currentUserId = Participation.SeatedPlayers.First(keyValue => keyValue.Value == seat).Key;
                Participation.SeatedPlayers.Remove(currentUserId);
                AddObserver(userId, Participation.UserNames.GetValueOrDefault(userId));
            }
            
            Participation.SeatedPlayers[userId] = seat;
        }

        Participation.UserNames[userId] = playerName;
    }
    
    public void AddObserver(int userId, string observerName)
    {
        Participation.Observers.Add(userId);
        Participation.UserNames[userId] = observerName;
    }

    public void RemoveUser(int userId)
    {
        Participation.StandingPlayers.Remove(userId);
        Participation.SeatedPlayers.Remove(userId);
        Participation.Observers.Remove(userId);
        Participation.UserNames.Remove(userId);
    }

    public bool SeatIsAvailable(int seat) => 
        Participation.AvailableSeats.Contains(seat) || 
        Participation.BotPositionsAreAvailable && IsBot(seat);
    
    public void OpenOrCloseSeat(int seat)
    {
        if (SeatIsAvailable(seat))
        {
            Participation.AvailableSeats.Remove(seat);
            
        }
        else
        {
            Participation.AvailableSeats.Add(seat);
            Log();
        }
    }

    public int NumberOfPlayers => Participation.StandingPlayers.Count + Participation.SeatedPlayers.Count;

    public int NumberOfHosts => Participation.Hosts.Count;
    
    public int NumberOfBots => Participation.SeatedBots.Count;
    
    public int NumberOfObservers => Participation.Observers.Count;
    
    public IEnumerable<string> PlayerNames =>
        Participation.UserNames
            .Where(idAndName => !Participation.Observers.Contains(idAndName.Key))
            .Select(idAndName => idAndName.Value);
    
    public IEnumerable<string> ObserverNames => 
        Participation.UserNames
            .Where(idAndName => Participation.Observers.Contains(idAndName.Key))
            .Select(idAndName => idAndName.Value);

    
    public bool IsPlayer(int userId) => Participation.SeatedPlayers.ContainsKey(userId) || Participation.StandingPlayers.Contains(userId);
    
    public bool IsHost(int userId) => Participation.Hosts.Contains(userId);
    
    public bool IsObserver(int userId) => Participation.Observers.Contains(userId);
    
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