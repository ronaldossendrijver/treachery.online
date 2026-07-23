using System;

namespace Treachery.Shared;

public class ManagedGame
{
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
}