namespace Treachery.Shared;

public class GameParticipation
{
    /// <summary>
    /// All Users that wish to participate while awaiting players (before the game has actually started)
    /// </summary>
    public HashSet<int> StandingUsers { get; set; } = [];
    
    /// <summary>
    /// For all Users that are players, hold their Seat
    /// </summary>
    public Dictionary<int, int> SeatedUsers { get; set; } = [];

    /// <summary>
    /// For each User (player or observer) in the game, hold the name
    /// </summary>
    public Dictionary<int, string> UserNames { get; set; } = [];

    /// <summary>
    /// Holds the seats occupies by Bots
    /// </summary>
    public HashSet<int> SeatedBots { get; set; } = [];
    
    /// <summary>
    /// Observers
    /// </summary>
    public HashSet<int> Observers { get; set; }
    
    /// <summary>
    /// All Users that are Hosts
    /// </summary>
    public HashSet<int> Hosts { get; set; }
    
    /// <summary>
    /// All Seats that may be taken by other players
    /// </summary>
    public HashSet<int> AvailableSeats { get; set; }
    
    public bool BotsArePaused { get; set; }
    
    public bool BotPositionsAreAvailable { get; set; }
}