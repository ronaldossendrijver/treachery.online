namespace Treachery.Shared;

public class GameParticipation
{
    /// <summary>
    /// For all seats taken by Users (not Bots), hold the User info
    /// </summary>
    public Dictionary<int, int> Seats { get; set; } = [];
    
    /// <summary>
    /// For all Users, hold their Seat
    /// </summary>
    public Dictionary<int, int> SeatedUsers { get; set; } = [];
    
    /// <summary>
    /// For each User in the game, hold the player name
    /// </summary>
    public Dictionary<int, string> PlayerNames { get; set; } = [];
    
    public Dictionary<int, string> ObserverNames { get; set; }
    
    public List<int> Hosts { get; set; }
    
    public bool BotsArePaused { get; set; }
    public bool BotPositionsAreAvailable { get; set; }
    public IEnumerable<Faction> AvailableSeats { get; set; }
}