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
    public int CreatorUserId { get; init; }
    public string GameId { get; init; }
    public bool HasPassword { get; init; }
    public string GameName { get; init; }
    public int ExpansionLevel { get; init; }
    public int MaximumNumberOfPlayers { get; init; }
    public int MaximumTurns { get; init; }
    public MainPhase CurrentMainPhase { get; init; }
    public Phase CurrentPhase { get; init; }
    public int CurrentTurn { get; init; }
    public int NumberOfBots { get; init; }
    public int ActualNumberOfPlayers { get; init; }
    public int NumberOfObservers { get; init; }
    public List<Faction> FactionsInPlay { get; init; }
    //public string[] Players { get; init; } = [];
    //public string[] Observers { get; init; } = [];
    public Ruleset Ruleset { get; init; }
    //public List<Rule> Rules { get; init; } = [];
    public DateTimeOffset? LastAction { get; init; }
    public int YourCurrentSeat { get; init; }
    public List<AvailableSeatInfo> AvailableSeats { get; init; } = [];
    
    [JsonIgnore]
    public bool YouAreIn => YourCurrentSeat >= 0;

    [JsonIgnore] 
    public bool CanBeJoined => CurrentPhase is Phase.AwaitingPlayers || AvailableSeats.Count > 0;

    public override bool Equals(object obj) => obj is GameInfo info && info.GameId == GameId;

    public override int GetHashCode() => GameId.GetHashCode();
}