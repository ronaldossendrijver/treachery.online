using Treachery.Bots;

namespace Treachery.Server;

public class ErrorLogService(TreacheryContext context)
{
    public async Task RecordAsync(
        string source,
        string message,
        string details,
        string url,
        string userAgent,
        int? userId = null,
        string? username = null)
    {
        context.ErrorLogs.Add(new ErrorLogEntry
        {
            OccurredAt = DateTime.UtcNow,
            GameVersion = Game.LatestVersion,
            Source = Limit(source, 64),
            Message = Limit(message, 4000),
            Details = Limit(details, 16000),
            Url = Limit(url, 2048),
            UserAgent = Limit(userAgent, 1024),
            UserId = userId,
            Username = username is null ? null : Limit(username, 4000)
        });

        await context.SaveChangesAsync();
    }

    public async Task RecordBotDecisionAsync(string gameId, InvalidBotDecision decision)
    {
        var entry = new ErrorLogEntry
        {
            OccurredAt = decision.OccurredAt,
            GameVersion = Game.LatestVersion,
            GameId = Limit(gameId, 36),
            Source = "Server/BotDecision",
            Message = Limit($"Invalid bot decision: {decision.ActionType}", 4000),
            Details = Limit(
                $"Faction: {decision.Faction}, seat: {decision.Seat}{Environment.NewLine}" +
                $"Decision: {decision.Message}{Environment.NewLine}" +
                $"Validation error: {decision.ValidationError}",
                16000),
            Url = string.Empty,
            UserAgent = string.Empty,
            GameSnapshot = new ErrorLogSnapshot { GameState = decision.GameState }
        };

        context.ErrorLogs.Add(entry);
        await context.SaveChangesAsync();
    }

    public async Task<int> DeleteExpiredAsync(CancellationToken cancellationToken = default)
    {
        var retentionDate = DateTime.UtcNow.AddDays(-30);
        var expiredEntryIds = context.ErrorLogs
            .Where(entry => entry.OccurredAt < retentionDate)
            .Select(entry => entry.Id);
        await context.ErrorLogSnapshots
            .Where(snapshot => expiredEntryIds.Contains(snapshot.ErrorLogEntryId))
            .ExecuteDeleteAsync(cancellationToken);

        return await context.ErrorLogs
            .Where(entry => entry.OccurredAt < retentionDate)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private static string Limit(string? value, int maximumLength)
    {
        var text = value ?? string.Empty;
        return text.Length <= maximumLength ? text : text[..maximumLength];
    }
}
