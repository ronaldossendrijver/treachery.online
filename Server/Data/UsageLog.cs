using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Treachery.Server;

/// <summary>
/// Records usage events and aggregates them into statistics. Recording never throws, so it cannot break gameplay.
/// </summary>
public static class UsageLog
{
    public const int MaximumCsvRows = 200_000;
    public const int MaximumDataLength = 4000;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static async Task RecordAsync(TreacheryContext db, UsageEventType type, int? userId, string? gameId = null, UsageData? data = null)
    {
        var entry = new UsageEvent
        {
            OccurredAt = DateTime.UtcNow,
            Type = type,
            UserId = userId,
            GameId = gameId is { Length: > 36 } ? gameId[..36] : gameId,
            Data = data == null ? null : Serialize(data)
        };

        try
        {
            db.UsageEvents.Add(entry);
            await db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            db.Entry(entry).State = EntityState.Detached;
            Console.WriteLine($"Failed to record usage event {type}: {e.Message}");
        }
    }

    public static async Task<List<UsageEvent>> LoadAsync(TreacheryContext db, DateTimeOffset from, DateTimeOffset to, int? maximum = null)
    {
        var query = db.UsageEvents.AsNoTracking()
            .Where(e => e.OccurredAt >= from.UtcDateTime && e.OccurredAt < to.UtcDateTime)
            .OrderBy(e => e.OccurredAt)
            .AsQueryable();

        if (maximum.HasValue)
            query = query.Take(maximum.Value);

        return await query.ToListAsync();
    }

    public static UsageStatsInfo Compute(IReadOnlyCollection<UsageEvent> events, DateTimeOffset from, DateTimeOffset to, UsageGranularity granularity)
    {
        var periods = new List<UsagePeriodStats>();
        var eventsByPeriod = events.ToLookup(e => PeriodStart(e.OccurredAt, granularity));

        var first = PeriodStart(from.UtcDateTime, granularity);
        var last = PeriodStart(to.UtcDateTime.AddTicks(-1), granularity);
        for (var period = first; period <= last && periods.Count < 5000; period = NextPeriod(period, granularity))
            periods.Add(Aggregate(eventsByPeriod[period], period));

        return new UsageStatsInfo
        {
            Periods = periods.ToArray(),
            Total = Aggregate(events, first)
        };
    }

    public static string ToCsv(IEnumerable<UsageEvent> events)
    {
        var csv = new StringBuilder("OccurredAtUtc,Type,UserId,GameId,Data\n");
        foreach (var e in events)
        {
            csv.Append(e.OccurredAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)).Append(',')
                .Append(e.Type).Append(',')
                .Append(e.UserId?.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(e.GameId).Append(',')
                .Append(e.Data == null ? string.Empty : "\"" + e.Data.Replace("\"", "\"\"") + "\"")
                .Append('\n');
        }

        return csv.ToString();
    }

    private static UsagePeriodStats Aggregate(IEnumerable<UsageEvent> periodEvents, DateTime periodStart)
    {
        var list = periodEvents as IReadOnlyCollection<UsageEvent> ?? periodEvents.ToList();
        var started = list.Where(e => e.Type == UsageEventType.GameStarted).Select(ParseData).ToList();
        var finished = list.Where(e => e.Type == UsageEventType.GameFinished).Select(ParseData).ToList();

        return new UsagePeriodStats
        {
            PeriodStart = new DateTimeOffset(periodStart, TimeSpan.Zero),
            Counts = list.GroupBy(e => e.Type).ToDictionary(g => g.Key.ToString(), g => g.Count()),
            ActiveUsers = list.Where(e => e.UserId.HasValue).Select(e => e.UserId).Distinct().Count(),
            GamesWithBots = started.Count(d => d.Bots > 0),
            GamesWithAdvancedBots = started.Count(d => d.AdvancedBots == true),
            AsyncGames = started.Count(d => d.AsyncPlay == true),
            AveragePlayers = Average(started.Select(d => d.Players)),
            AverageTurns = Average(finished.Select(d => d.Turn)),
            AverageDurationHours = Average(finished.Select(d => d.DurationMinutes)) / 60
        };
    }

    private static UsageData ParseData(UsageEvent e)
    {
        if (string.IsNullOrEmpty(e.Data))
            return new UsageData();

        try
        {
            return JsonSerializer.Deserialize<UsageData>(e.Data, JsonOptions) ?? new UsageData();
        }
        catch (JsonException)
        {
            return new UsageData();
        }
    }

    private static double Average(IEnumerable<int?> values)
    {
        var known = values.Where(v => v.HasValue).Select(v => v!.Value).ToList();
        return known.Count == 0 ? 0 : known.Average();
    }

    private static DateTime PeriodStart(DateTime utc, UsageGranularity granularity)
    {
        var date = DateTime.SpecifyKind(utc.Date, DateTimeKind.Utc);
        return granularity switch
        {
            UsageGranularity.Week => date.AddDays(-(((int)date.DayOfWeek + 6) % 7)),
            UsageGranularity.Month => new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc),
            _ => date
        };
    }

    private static DateTime NextPeriod(DateTime period, UsageGranularity granularity) => granularity switch
    {
        UsageGranularity.Week => period.AddDays(7),
        UsageGranularity.Month => period.AddMonths(1),
        _ => period.AddDays(1)
    };

    /// <summary>
    /// Serializes data, dropping the longest lists if needed so the result is always complete, valid JSON.
    /// </summary>
    public static string Serialize(UsageData data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        if (json.Length <= MaximumDataLength)
            return json;

        data.Rules = null;
        data.BotRules = null;
        json = JsonSerializer.Serialize(data, JsonOptions);
        return json.Length <= MaximumDataLength ? json : "{}";
    }
}
