using System.ComponentModel.DataAnnotations;

namespace Treachery.Server;

public class UsageEvent
{
    public long Id { get; set; }
    public DateTime OccurredAt { get; set; }
    public UsageEventType Type { get; set; }
    public int? UserId { get; set; }

    [MaxLength(36)]
    public string? GameId { get; set; }

    /// <summary>
    /// Optional JSON-serialized <see cref="UsageData"/>.
    /// </summary>
    [MaxLength(1000)]
    public string? Data { get; set; }
}

/// <summary>
/// Details stored with game related usage events. Only the relevant fields are filled.
/// </summary>
public class UsageData
{
    public int? Players { get; set; }
    public int? Bots { get; set; }
    public bool? AdvancedBots { get; set; }
    public string? Ruleset { get; set; }
    public bool? AsyncPlay { get; set; }
    public int? Turn { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Winners { get; set; }
    public bool? FromSavegame { get; set; }
    public bool? ByAdmin { get; set; }
}
