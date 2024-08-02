namespace Treachery.Shared;

public class ManagedGame
{
    public string GameId { get; set; }
    
    public Game Game { get; set; }
    
    public string HashedPassword { get; init; }

    public bool ObserversRequirePassword { get; init; }
    
    public bool BotsArePaused { get; set; }
}