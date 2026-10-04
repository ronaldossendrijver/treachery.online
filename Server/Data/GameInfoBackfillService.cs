using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Treachery.Server;

public sealed class GameInfoBackfillService(
    IServiceScopeFactory scopeFactory,
    IHostApplicationLifetime applicationLifetime,
    GameInfoSummaryCache summaries,
    ILogger<GameInfoBackfillService> logger) : BackgroundService
{
    private const int ProgressInterval = 25;
    private const int BatchSize = 25;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await WaitForApplicationStartAsync(stoppingToken);

        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TreacheryContext>();
        var total = await context.PersistedGames.CountAsync(game => game.GameInfo == null, stoppingToken);
        if (total == 0)
        {
            logger.LogInformation("Game info backfill: no games need processing.");
            return;
        }

        logger.LogInformation("Game info backfill started: {Total} games need processing.", total);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var processed = 0;
        var updated = 0;
        var failed = 0;
        var lastId = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            var gameIds = await context.PersistedGames.AsNoTracking()
                .Where(game => game.GameInfo == null && game.Id > lastId)
                .OrderBy(game => game.Id)
                .Select(game => game.Id)
                .Take(BatchSize)
                .ToListAsync(stoppingToken);
            if (gameIds.Count == 0)
                break;

            foreach (var id in gameIds)
            {
                lastId = id;
                try
                {
                    var savedGame = await context.PersistedGames.AsNoTracking()
                        .Where(game => game.Id == id)
                        .Select(game => new { game.GameId, game.GameState, game.GameParticipation })
                        .SingleAsync(stoppingToken);

                    var participation = Utilities.Deserialize<Participation>(savedGame.GameParticipation) ?? new Participation();
                    var loadMessage = Game.TryLoad(GameState.Load(savedGame.GameState), participation, false, true, out var game);
                    if (loadMessage != null || game is null)
                    {
                        failed++;
                        logger.LogWarning("Game info backfill could not load game {GameId}: {LoadMessage}",
                            savedGame.GameId, loadMessage?.ToString() ?? "Unknown error");
                    }
                    else
                    {
                        var summary = GameInfo.FromGame(game);
                        var serializedSummary = Utilities.Serialize(summary);
                        var rowsUpdated = await context.PersistedGames
                            .Where(row => row.Id == id && row.GameInfo == null)
                            .ExecuteUpdateAsync(update => update.SetProperty(row => row.GameInfo, serializedSummary), stoppingToken);
                        if (rowsUpdated == 1)
                        {
                            summaries.Set(savedGame.GameId, summary);
                            updated++;
                        }
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex) when (ex is ArgumentException
                                           or InvalidDataException
                                           or InvalidOperationException
                                           or System.Text.Json.JsonException)
                {
                    failed++;
                    logger.LogError(ex, "Game info backfill failed for persisted game {PersistedGameId}.", id);
                }

                processed++;
                if (processed % ProgressInterval == 0 || processed == total)
                {
                    logger.LogInformation(
                        "Game info backfill progress: {Processed}/{Total} processed, {Updated} updated, {Failed} failed; elapsed {Elapsed}.",
                        processed, total, updated, failed, stopwatch.Elapsed);
                }
            }
        }

        logger.LogInformation(
            "Game info backfill finished: {Processed}/{Total} processed, {Updated} updated, {Failed} failed; elapsed {Elapsed}.",
            processed, total, updated, failed, stopwatch.Elapsed);
    }

    private async Task WaitForApplicationStartAsync(CancellationToken stoppingToken)
    {
        if (applicationLifetime.ApplicationStarted.IsCancellationRequested)
            return;

        var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var startedRegistration = applicationLifetime.ApplicationStarted.Register(() => started.TrySetResult(true));
        using var stoppingRegistration = stoppingToken.Register(() => started.TrySetCanceled(stoppingToken));
        await started.Task;
    }
}
