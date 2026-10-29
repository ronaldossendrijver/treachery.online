namespace Treachery.Server;

public partial class GameHub
{
    private static ConcurrentDictionary<int, DateOnly> LastActiveDayByUserId { get; } = [];

    private async Task LogUsage(UsageEventType type, int? userId, string? gameId = null, UsageData? data = null)
    {
        await using var db = GetDbContext();
        await UsageLog.RecordAsync(db, type, userId, gameId, data);
    }

    /// <summary>
    /// Records that a user was active, at most once per user per (UTC) day.
    /// </summary>
    private async Task LogUserActive(int userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (LastActiveDayByUserId.TryGetValue(userId, out var lastActiveDay) && lastActiveDay == today)
            return;

        LastActiveDayByUserId[userId] = today;
        await LogUsage(UsageEventType.UserActive, userId);
    }

    private static UsageData GameUsageData(ManagedGame game) => new()
    {
        Players = game.Game.NumberOfSeatedPlayers,
        Bots = game.Game.NumberOfBots,
        AdvancedBots = game.Game.Applicable(Rule.AdvancedBots),
        Ruleset = game.Game.Ruleset.ToString(),
        AsyncPlay = game.Game.Settings.AsyncPlay,
        MaximumPlayers = game.Game.MaximumPlayers,
        MaximumTurns = game.Game.MaximumTurns,
        BotFactions = JoinOrNull(game.Game.Players.Where(game.Game.IsBot).Select(p => p.Faction)),
        HumanFactions = JoinOrNull(game.Game.Players.Where(p => !game.Game.IsBot(p)).Select(p => p.Faction)),
        Rules = JoinOrNull(game.Game.Rules),
        BotRules = JoinOrNull(game.Game.RulesForBots),
        Turn = game.Game.CurrentTurn
    };

    private static string? JoinOrNull<T>(IEnumerable<T> values)
    {
        var joined = string.Join(";", values);
        return joined.Length == 0 ? null : joined;
    }

    private static UsageData FinishedGameUsageData(ManagedGame game)
    {
        var data = GameUsageData(game);
        data.DurationMinutes = (int)(DateTimeOffset.Now - game.CreationDate).TotalMinutes;
        data.Winners = string.Join(";", game.Game.Winners.Select(p => p.Faction));
        return data;
    }

    private static UsageData ClosedGameUsageData(ManagedGame game, bool byAdmin)
    {
        var data = game.IsLoaded ? GameUsageData(game) : new UsageData();
        data.ByAdmin = byAdmin ? true : null;
        return data;
    }

    public async Task<Result<UsageStatsInfo>> GetAdminUsageStats(string userToken, DateTimeOffset from, DateTimeOffset to, UsageGranularity granularity)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user) || !IsAdmin(user))
            return Error<UsageStatsInfo>(ErrorType.InvalidUserNameOrPassword);

        if (to <= from)
            return Error<UsageStatsInfo>(ErrorType.InvalidGameEvent, "End date must be after start date");

        await using var db = GetDbContext();
        var events = await UsageLog.LoadAsync(db, from, to);
        return Success(UsageLog.Compute(events, from, to, granularity));
    }

    public async Task<Result<string>> GetAdminUsageCsv(string userToken, DateTimeOffset from, DateTimeOffset to)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user) || !IsAdmin(user))
            return Error<string>(ErrorType.InvalidUserNameOrPassword);

        await using var db = GetDbContext();
        var events = await UsageLog.LoadAsync(db, from, to, UsageLog.MaximumCsvRows);
        return Success(UsageLog.ToCsv(events));
    }
}
