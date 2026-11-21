namespace Treachery.Shared;

public class BotBuildInfo
{
    public string Name { get; set; } = string.Empty;

    public string Build { get; set; } = string.Empty;

    public DateTimeOffset? BuildDate { get; set; }
}
