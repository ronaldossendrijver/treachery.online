using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Treachery.Server;

public class ErrorLogCleanupService(IServiceScopeFactory scopeFactory, ILogger<ErrorLogCleanupService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var errorLog = scope.ServiceProvider.GetRequiredService<ErrorLogService>();
                var deletedCount = await errorLog.DeleteExpiredAsync(stoppingToken);
                logger.LogInformation("Deleted {DeletedCount} error-log entries older than 30 days.", deletedCount);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error-log retention cleanup failed.");
            }
        }
    }
}
