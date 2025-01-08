using System;

namespace Treachery.Shared;

public class ManagedGame
{
    public DateTimeOffset CreationDate { get; init; }
    
    public int CreatorUserId { get; init; }
    
    public string GameId { get; init; }
    
    public Game Game { get; set; }
    
    public string Name { get; set; }
    
    public string HashedPassword { get; init; }

    public bool ObserversRequirePassword { get; init; }
    
    public bool StatisticsSent { get; set; }

    public DateTimeOffset LastAsyncPlayMessageSent { get; set; }
}