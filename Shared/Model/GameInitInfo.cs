namespace Treachery.Shared;

public class GameInitInfo
{
    public string GameId { get; init; }
    public string GameState { get; init; }
    public string GameName { get; init; }
    public GameParticipation Participation { get; init; }
}