/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Text.Json.Serialization;

namespace Treachery.Shared;

public class GameInfo
{
    public int CreatorId { get; init; }
    public string GameId { get; init; }
    public bool HasPassword { get; init; }
    public string Name { get; init; }
    public int MaxPlayers { get; init; }
    public int MaxTurns { get; init; }
    public MainPhase MainPhase { get; init; }
    public Phase Phase { get; init; }
    public int Turn { get; init; }
    public int NrOfBots { get; init; }
    public int NrOfPlayers { get; init; }
    public Faction[] FactionsInPlay { get; init; }
    public Ruleset Ruleset { get; init; }
    public DateTimeOffset? LastAction { get; init; }
    public Dictionary<int, int> SeatedPlayers { get; set; }
    public AvailableSeatInfo[] AvailableSeats { get; init; } = [];
    
    [JsonIgnore] 
    public bool CanBeJoined => Phase is Phase.AwaitingPlayers || AvailableSeats.Length > 0;

    public int YourCurrentSeat(int userId) => SeatedPlayers.GetValueOrDefault(userId, -1);

    public bool YouAreIn(int userId) => SeatedPlayers.ContainsKey(userId);

    public override bool Equals(object obj) => obj is GameInfo info && info.GameId == GameId;

    public override int GetHashCode() => GameId.GetHashCode();
}