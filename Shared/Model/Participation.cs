namespace Treachery.Shared;

public class Participation
{
    /// <summary>
    /// For each User (player or observer) in the game, hold the player name
    /// </summary>
    public Dictionary<int, string> PlayerNames { get; set; } = [];
    
    /// <summary>
    /// For all Users (UserIds) that are players, hold their Seat. Seat is -1 when player is not yet seated.
    /// </summary>
    public Dictionary<int, int> SeatedPlayers { get; set; } = [];

    /// <summary>
    /// All joined users that are Observers (UserIds)
    /// </summary>
    public HashSet<int> Observers { get; set; } = [];

    /// <summary>
    /// All joined users (UserIds) that are Hosts
    /// </summary>
    public HashSet<int> Hosts { get; set; } = [];
    
    /// <summary>
    /// Seats that may be taken by other players
    /// </summary>
    public HashSet<int> AvailableSeats { get; set; } = [];
    
    /// <summary>
    /// Kicked users (UserIds)
    /// </summary>
    public HashSet<int> Kicked { get; set; } = [];

    public int BotsSpeed { get; set; } = 1;
}