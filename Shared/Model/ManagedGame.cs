using System;
using System.Threading;
using System.Threading.Tasks;

namespace Treachery.Shared;

public class ManagedGame
{
    private readonly SemaphoreSlim _eventSemaphore = new(1, 1);

    public DateTimeOffset CreationDate { get; init; }
    
    public DateTimeOffset LastActivity { get; set; }
    
    public DateTimeOffset LastPersisted { get; set; }
    
    public int CreatorUserId { get; init; }

    public string GameId { get; init; } = string.Empty;

    public Game Game { get; set; } = null!;
    
    public string Name { get; init; } = string.Empty;
    
    public string HashedPassword { get; init; } = string.Empty;

    public bool ObserversRequirePassword { get; init; }
    
    public bool StatisticsSent { get; set; }

    public DateTimeOffset LastAsyncPlayMessageSent { get; set; }

    public Dictionary<Faction, IBot> Bots { get; } = [];

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