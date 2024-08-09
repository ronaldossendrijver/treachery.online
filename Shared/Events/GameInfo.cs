/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

// ReSharper disable MemberCanBePrivate.Global

using System;

namespace Treachery.Shared;

public class GameInfo
{
    public int CreatorUserId { get; init; }
    public bool YouAreIn { get; init; }
    public string GameId { get; init; }
    public bool HasPassword { get; init; }
    public string GameName { get; init; }
    public int ExpansionLevel { get; init; }
    public MainPhase CurrentMainPhase { get; init; }
    public Phase CurrentPhase { get; init; }
    public int CurrentTurn { get; init; }
    public int NumberOfPlayers { get; init; }
    public int MaximumTurns { get; init; }
    public List<Faction> FactionsInPlay { get; init; }
    public string[] Players { get; init; } = [];
    public string[] Observers { get; init; } = [];
    public int NumberOfBots { get; init; }
    public List<Rule> Rules { get; init; } = [];
    public DateTimeOffset? LastAction { get; init; }
    public List<AvailableSeatInfo> AvailableSeats { get; init; } = [];
    public override bool Equals(object obj) => obj is GameInfo info && info.GameId == GameId;

    public override int GetHashCode() => GameId.GetHashCode();
    
    public static GameInfo Extract(ManagedGame managedGame, int userId) => new()
    {
        GameId = managedGame.GameId,
        CreatorUserId = managedGame.CreatorUserId,
        YouAreIn = managedGame.Game.Participation.Users.ContainsKey(userId),
        Players = managedGame.Game.PlayerNames.ToArray(),
        Observers = managedGame.Game.ObserverNames.ToArray(),
        FactionsInPlay = managedGame.Game.CurrentPhase <= Phase.AwaitingPlayers ? managedGame.Game.Settings.AllowedFactionsInPlay : managedGame.Game.FactionsInPlay,
        NumberOfBots = managedGame.Game.NumberOfBots,
        Rules = managedGame.Game.CurrentPhase <= Phase.AwaitingPlayers ? managedGame.Game.Settings.InitialRules : managedGame.Game.Rules.ToList(),
        LastAction = managedGame.Game.CurrentPhase > Phase.AwaitingPlayers ? managedGame.Game.History.Last().Time : managedGame.CreationDate,
        CurrentMainPhase = managedGame.Game.CurrentMainPhase,
        CurrentPhase = managedGame.Game.CurrentPhase,
        CurrentTurn = managedGame.Game.CurrentTurn,
        ExpansionLevel = Game.ExpansionLevel,
        GameName = managedGame.GameName,
        HasPassword = managedGame.HashedPassword != null,
        NumberOfPlayers = managedGame.Game.Settings.NumberOfPlayers,
        MaximumTurns = managedGame.Game.Settings.MaximumTurns,
        AvailableSeats = managedGame.Game.Participation.AvailableSeats.Select(seat => new AvailableSeatInfo()
        {
            Seat = seat, 
            Faction = managedGame.Game.GetFactionInSeat(seat), 
            IsBot = managedGame.Game.IsBot(seat)
        }).ToList()
    };
}