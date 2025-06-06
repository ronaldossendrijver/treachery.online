﻿namespace Treachery.Shared;

public class Participation
{
    /// <summary>
    /// For each User (player or observer) in the game, hold the player name
    /// </summary>
    public Dictionary<int, string> PlayerNames { get; init; } = [];
    
    /// <summary>
    /// For all Users (UserIds) that are players, hold their Seat. Seat is -1 when player is not yet seated.
    /// </summary>
    public Dictionary<int, int> SeatedPlayers { get; set; } = [];

    /// <summary>
    /// All joined users that are Observers (UserIds)
    /// </summary>
    public HashSet<int> Observers { get; init; } = [];

    /// <summary>
    /// All joined users (UserIds) that are Hosts
    /// </summary>
    public HashSet<int> Hosts { get; init; } = [];
    
    /// <summary>
    /// Seats that may be taken by other players
    /// </summary>
    public HashSet<int> AvailableSeats { get; init; } = [];
    
    /// <summary>
    /// Kicked users (UserIds)
    /// </summary>
    public HashSet<int> Kicked { get; init; } = [];
    
    public bool BotsArePaused { get; set; }
}