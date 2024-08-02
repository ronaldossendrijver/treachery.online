/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;

namespace Treachery.Shared;

public class GameInfo
{
    public string GameId { get; init; }
    public bool CreatorParticipates { get; set; }
    public string GameName { get; set; }
    public bool HasPassword { get; set; }
    public int ExpansionLevel { get; set; }
    public Phase CurrentPhase { get; set; }
    public MainPhase CurrentMainPhase { get; set; }
    public int CurrentTurn { get; set; }
    public int MaximumPlayers { get; set; }
    public int MaximumNumberOfTurns { get; set; }
    public List<Faction> FactionsInPlay { get; set; }
    public string[] Players { get; set; }
    public string[] Observers { get; set; }
    public int NumberOfBots { get; set; }
    public List<Rule> Rules { get; set; }
    public bool InviteOthers { get; set; }
    public DateTime? LastAction { get; set; }
    
    public override bool Equals(object obj) => obj is GameInfo info && info.GameId == GameId;

    public override int GetHashCode() => GameId.GetHashCode();
    
    public static GameInfo Extract(ManagedGame managedGame) => new()
    {
        GameId = managedGame.GameId,
        Players = managedGame.Game.PlayerNames.ToArray(),
        Observers = managedGame.Game.ObserverNames.ToArray(),
        FactionsInPlay = managedGame.Game.CurrentPhase <= Phase.AwaitingPlayers ? managedGame.Game.Settings.AllowedFactionsInPlay : managedGame.Game.FactionsInPlay,
        NumberOfBots = managedGame.Game.NumberOfBots,
        Rules = managedGame.Game.CurrentPhase <= Phase.AwaitingPlayers ? managedGame.Game.Settings.InitialRules : managedGame.Game.Rules.ToList(),
        LastAction = managedGame.Game.CurrentPhase > Phase.AwaitingPlayers ? managedGame.Game.History.Last().Time : DateTime.Now,
        CurrentMainPhase = managedGame.Game.CurrentMainPhase,
        CurrentPhase = managedGame.Game.CurrentPhase,
        CurrentTurn = managedGame.Game.CurrentTurn,
        ExpansionLevel = Game.ExpansionLevel,
        GameName = managedGame.Game.Name,
        HasPassword = managedGame.HashedPassword != null,
        CreatorParticipates = true,
        InviteOthers = true,
        MaximumPlayers = managedGame.Game.Settings.MaximumPlayers,
        MaximumNumberOfTurns = managedGame.Game.Settings.MaximumTurns,
    };
}