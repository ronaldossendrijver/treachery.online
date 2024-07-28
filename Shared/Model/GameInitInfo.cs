namespace Treachery.Shared;

public class GameInitInfo
{
    public string GameToken { get; set; }
    
    public string GameState { get; set; }

    
    public GameSettings Settings { get; set; }

    public GameParticipation Participation { get; set; }
}