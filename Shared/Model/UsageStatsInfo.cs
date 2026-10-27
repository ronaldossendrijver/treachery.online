namespace Treachery.Shared.Model;

public enum UsageEventType
{
    UserRegistered,
    Login,
    UserActive,
    PasswordReset,
    GameCreated,
    GameLoaded,
    GameScheduled,
    ScheduledGameCancelled,
    GameJoined,
    GameObserved,
    GameLeft,
    GameStarted,
    GameFinished,
    GameClosed,
    MembershipCheckoutStarted,
    MembershipActivated,
    MembershipRenewed,
    MembershipEnded
}

public enum UsageGranularity
{
    Day,
    Week,
    Month
}

public class UsageStatsInfo
{
    public UsagePeriodStats[] Periods { get; set; } = [];
    public UsagePeriodStats Total { get; set; } = new();
}

public class UsagePeriodStats
{
    /// <summary>
    /// Start of the period (UTC).
    /// </summary>
    public DateTimeOffset PeriodStart { get; set; }

    /// <summary>
    /// Number of events per <see cref="UsageEventType"/> name.
    /// </summary>
    public Dictionary<string, int> Counts { get; set; } = [];

    /// <summary>
    /// Distinct users that caused at least one event.
    /// </summary>
    public int ActiveUsers { get; set; }

    public int GamesWithBots { get; set; }
    public int GamesWithAdvancedBots { get; set; }
    public int AsyncGames { get; set; }

    /// <summary>
    /// Average number of human players in started games.
    /// </summary>
    public double AveragePlayers { get; set; }

    /// <summary>
    /// Average number of turns played in finished games.
    /// </summary>
    public double AverageTurns { get; set; }

    /// <summary>
    /// Average time between creation and end of finished games.
    /// </summary>
    public double AverageDurationHours { get; set; }

    public int Count(UsageEventType type) => Counts.GetValueOrDefault(type.ToString());
}
