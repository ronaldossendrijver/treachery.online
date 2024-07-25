namespace Treachery.Shared;

public class GameParticipation
{
    public Dictionary<int, string> PlayerNames { get; set; } = [];
    
    public Dictionary<int, string> ObserverNames { get; set; }
    
    public List<int> Hosts { get; set; }
    
    public bool BotsArePaused { get; set; }
}