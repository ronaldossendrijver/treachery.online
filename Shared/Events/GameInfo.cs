/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

// ReSharper disable MemberCanBePrivate.Global

namespace Treachery.Shared;

public class GameInfo
{
    public int CreatorId { get; init; }
    public string CreatorUsername { get; init; } = string.Empty;
    public DateTimeOffset CreationDate { get; init; }
    public string GameId { get; init; } = string.Empty;
    public bool HasPassword { get; init; }
    public bool RequiresLoad { get; init; }
    public bool HasDetails { get; init; }
    public string Name { get; init; } = string.Empty;
    public int MaxPlayers { get; init; }
    public int MaxTurns { get; init; }
    public MainPhase MainPhase { get; init; }
    public Phase Phase { get; init; }
    public int Turn { get; init; }
    public int NrOfBots { get; init; }
    public int NrOfPlayers { get; init; }
    public Faction[] FactionsInPlay { get; init; } = [];
    public Ruleset Ruleset { get; init; }
    public DateTimeOffset? LastActivity { get; init; }
    public Dictionary<int, int> SeatedPlayers { get; set; } = [];
    public AvailableSeatInfo[] AvailableSeats { get; init; } = [];
    
    [JsonIgnore] 
    public bool CanBeJoined => RequiresLoad || Phase is Phase.AwaitingPlayers && SeatedPlayers.Count < MaxPlayers || AvailableSeats.Length > 0;

    public static GameInfo FromGame(Game game) => new()
    {
        HasDetails = true,
        FactionsInPlay = game.CurrentPhase <= Phase.AwaitingPlayers
            ? game.Settings.AllowedFactionsInPlay.ToArray()
            : game.Players.Where(p => p.Faction != Faction.None).Select(p => p.Faction).ToArray(),
        NrOfBots = game.NumberOfBots,
        Ruleset = game.CurrentPhase <= Phase.AwaitingPlayers
            ? Game.DetermineApproximateRuleset(game.Settings.AllowedFactionsInPlay, game.Settings.InitialRules, Game.ExpansionLevel)
            : Game.DetermineApproximateRuleset(game.Players.Select(p => p.Faction).ToList(), game.Rules, Game.ExpansionLevel),
        MainPhase = game.CurrentMainPhase,
        Phase = game.CurrentPhase,
        Turn = game.CurrentTurn,
        MaxPlayers = game.Settings.NumberOfPlayers,
        MaxTurns = game.Settings.MaximumTurns,
        NrOfPlayers = game.Participation.SeatedPlayers.Count,
        SeatedPlayers = game.Participation.SeatedPlayers,
        AvailableSeats = game.Players
            .Where(p => game.SeatIsAvailable(p.Seat))
            .Select(p => new AvailableSeatInfo
            {
                Seat = p.Seat,
                Faction = p.Faction,
                IsBot = p.IsBot
            }).ToArray()
    };

    public int YourCurrentSeat(int userId) => SeatedPlayers.GetValueOrDefault(userId, -1);

    public bool YouAreIn(int userId) => SeatedPlayers.ContainsKey(userId);

    public override bool Equals(object? obj) => obj is GameInfo info && info.GameId == GameId;

    public override int GetHashCode() => GameId.GetHashCode();
}