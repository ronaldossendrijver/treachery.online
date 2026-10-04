namespace Treachery.Shared.Model;

public class ErrorLogInfo
{
    public int Id { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public int? GameVersion { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string? Username { get; set; }
}
