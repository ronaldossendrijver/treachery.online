namespace Treachery.Shared;

public class GameInitInfo
{
    public string GameId { get; init; } = string.Empty;
    public string GameState { get; init; } = string.Empty;
    public string GameName { get; init; } = string.Empty;
    public Participation Participation { get; init; } = new();
}