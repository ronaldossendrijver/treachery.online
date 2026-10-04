using System.ComponentModel.DataAnnotations;

namespace Treachery.Server;

public class ErrorLogEntry
{
    public int Id { get; set; }
    public DateTime OccurredAt { get; set; }
    public int? GameVersion { get; set; }

    [MaxLength(64)]
    public string Source { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string Message { get; set; } = string.Empty;

    [MaxLength(16000)]
    public string Details { get; set; } = string.Empty;

    [MaxLength(2048)]
    public string Url { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string UserAgent { get; set; } = string.Empty;

    public int? UserId { get; set; }

    [MaxLength(4000)]
    public string? Username { get; set; }
}
