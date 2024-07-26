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
    /// Holds the seats occupies by Bots
    /// </summary>
    public HashSet<int> SeatedBots { get; set; } = [];
    
    /// <summary>
    /// For each User type player in the game, hold the player name
    /// </summary>
    public Dictionary<int, string> PlayerNames { get; set; } = [];
    
    /// <summary>
    /// For each Observer of the game, hold the player name
    /// </summary>
    public Dictionary<int, string> ObserverNames { get; set; }
    
    /// <summary>
    /// All Users that are Hosts
    /// </summary>
    public List<int> Hosts { get; set; }
    
    /// <summary>
    /// All Seats that may be taken by other players
    /// </summary>
    public IEnumerable<Faction> AvailableSeats { get; set; }
    
    public bool BotsArePaused { get; set; }
    public bool BotPositionsAreAvailable { get; set; }
    

    public object GetPlayerName(int userId) => PlayerNames.GetValueOrDefault(userId);

    public Player GetPlayer(int userId, Game game)
    {
        var seat = GetSeat(userId);
        return game.PlayerAtSeat(seat);
    }

    private int GetSeat(int userId) => SeatedUsers.GetValueOrDefault(userId);
}