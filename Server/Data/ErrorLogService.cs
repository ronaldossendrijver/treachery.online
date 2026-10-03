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

    public async Task<int> DeleteExpiredAsync(CancellationToken cancellationToken = default)
    {
        var retentionDate = DateTime.UtcNow.AddDays(-30);
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
