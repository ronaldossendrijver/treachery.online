namespace Treachery.Server;

public class ErrorLogSnapshot
{
    public int ErrorLogEntryId { get; set; }
    public string GameState { get; set; } = string.Empty;
    public ErrorLogEntry ErrorLogEntry { get; set; } = null!;
}
