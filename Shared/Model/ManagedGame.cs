using System;
using System.Threading;
using System.Threading.Tasks;

namespace Treachery.Shared;

public class ManagedGame
{
    private readonly SemaphoreSlim _eventSemaphore = new(1, 1);
    private readonly SemaphoreSlim _loadSemaphore = new(1, 1);
    private Game? _game;

    public DateTimeOffset CreationDate { get; init; }
    
    public DateTimeOffset LastActivity { get; set; }
    
    public DateTimeOffset LastPersisted { get; set; }
    
    public int CreatorUserId { get; init; }

    public string GameId { get; init; } = string.Empty;

    public Game Game
    {
        get => _game ?? throw new InvalidOperationException("The game has not been loaded.");
        set => _game = value;
    }

    public bool IsLoaded => _game is not null;

    public Participation Participation { get; set; } = new();
    
    public string Name { get; init; } = string.Empty;
    
    public string HashedPassword { get; init; } = string.Empty;

    public bool ObserversRequirePassword { get; init; }
    
    public bool StatisticsSent { get; set; }

    public DateTimeOffset LastAsyncPlayMessageSent { get; set; }

    public Dictionary<Faction, IBot> Bots { get; } = [];

    public async Task<string?> LoadIfNeededAsync(Func<Task<(Game? game, string? error)>> load)
    {
        await _loadSemaphore.WaitAsync();
        try
        {
            if (IsLoaded)
                return null;

            var result = await load();
            if (result.game is not null)
            {
                Game = result.game;
                Participation = result.game.Participation;
                return null;
            }

            return result.error ?? "Unable to load the saved game.";
        }
        finally
        {
            _loadSemaphore.Release();
        }
    }

    public async Task<T> ProcessEventAsync<T>(Func<Task<T>> processEvent)
    {
        await _eventSemaphore.WaitAsync();
        try
        {
            return await processEvent();
        }
        finally
        {
            _eventSemaphore.Release();
        }
    }
}