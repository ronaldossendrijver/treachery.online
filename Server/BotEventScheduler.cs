using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Treachery.Server;

public interface IBotEventScheduler
{
    void ScheduleBotEvent(ManagedGame game);
    void ScheduleAsyncPlayMessage(string gameId, TimeSpan delay);
}

public sealed class BotEventScheduler(
    IServiceScopeFactory scopeFactory,
    ILogger<BotEventScheduler> logger) : BackgroundService, IBotEventScheduler
{
    private readonly ConcurrentDictionary<(string GameId, WorkType Type), ScheduledWork> _scheduled = new();
    private readonly ConcurrentDictionary<string, byte> _processing = new();
    private readonly ConcurrentDictionary<Task, byte> _inFlight = new();

    public void ScheduleBotEvent(ManagedGame game)
    {
        var delay = GameHub.DetermineBotDelay(
            game.Game.CurrentMainPhase,
            game.Game.LatestEvent(),
            game.Game.Participation.BotsSpeed);
        Schedule(game.GameId, WorkType.BotEvent, TimeSpan.FromMilliseconds(delay));
    }

    public void ScheduleAsyncPlayMessage(string gameId, TimeSpan delay)
        => Schedule(gameId, WorkType.AsyncPlayMessage, delay);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        try
        {
            do
            {
                RunScheduledWork(stoppingToken);
                await CheckForStalledGames(stoppingToken);
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        finally
        {
            await Task.WhenAll(_inFlight.Keys);
        }
    }

    private void Schedule(string gameId, WorkType type, TimeSpan delay)
    {
        var dueAt = DateTimeOffset.UtcNow + delay;
        _scheduled[(gameId, type)] = new ScheduledWork(gameId, type, dueAt);
    }

    private void RunScheduledWork(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var (key, work) in _scheduled.ToArray())
        {
            if (work.DueAt > now ||
                !((ICollection<KeyValuePair<(string GameId, WorkType Type), ScheduledWork>>)_scheduled)
                    .Remove(new KeyValuePair<(string GameId, WorkType Type), ScheduledWork>(key, work)))
                continue;

            if (!_processing.TryAdd(work.GameId, 0))
            {
                Schedule(work.GameId, work.Type, TimeSpan.FromSeconds(1));
                continue;
            }

            var task = ExecuteWorkAsync(work, cancellationToken);
            _inFlight.TryAdd(task, 0);
            _ = task.ContinueWith(
                completed => _inFlight.TryRemove(completed, out _),
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }
    }

    private async Task ExecuteWorkAsync(ScheduledWork work, CancellationToken cancellationToken)
    {
        try
        {
            if (!GameHub.TryGetRunningGame(work.GameId, out var game))
                return;

            await using var scope = scopeFactory.CreateAsyncScope();
            // This hub is only an operation-scoped dispatcher; broadcasts use IHubContext, not Hub.Context.
            var hub = ActivatorUtilities.CreateInstance<GameHub>(scope.ServiceProvider);
            switch (work.Type)
            {
                case WorkType.BotEvent:
                    await hub.PerformBotEvent(game);
                    break;
                case WorkType.AsyncPlayMessage:
                    await hub.SendAsyncPlayMessagesIfApplicable(work.GameId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(work.Type), work.Type, "Unknown bot work type.");
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Scheduled {WorkType} failed for game {GameId}.", work.Type, work.GameId);
        }
        finally
        {
            _processing.TryRemove(work.GameId, out _);
        }
    }

    private async Task CheckForStalledGames(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var game in GameHub.GetRunningGamesSnapshot())
        {
            if (!_processing.TryAdd(game.GameId, 0))
                continue;

            try
            {
                await game.NudgeBotsIfStalledAsync(now, async () =>
                {
                    await using var scope = scopeFactory.CreateAsyncScope();
                    var hub = ActivatorUtilities.CreateInstance<GameHub>(scope.ServiceProvider);
                    await hub.PerformBotEventInLock(game);
                });
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Stalled bot recovery failed for game {GameId}.", game.GameId);
            }
            finally
            {
                _processing.TryRemove(game.GameId, out _);
            }
        }
    }

    private enum WorkType
    {
        BotEvent,
        AsyncPlayMessage
    }

    private sealed record ScheduledWork(string GameId, WorkType Type, DateTimeOffset DueAt);
}
