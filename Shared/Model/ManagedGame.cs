using System;

namespace Treachery.Shared;

public class ManagedGame
{
    public DateTimeOffset CreationDate { get; set; }
    
    public int CreatorUserId { get; set; }
    
    public string GameId { get; set; }
    
    public Game Game { get; set; }
    
    public string GameName { get; set; }
    
    public string HashedPassword { get; init; }

    public bool ObserversRequirePassword { get; init; }
    
    public bool BotsArePaused { get; set; }
    
    public bool StatisticsSent { get; set; }

    
    public bool AsyncPlay { get; set; }
    
    public int AsyncPlayMessageIntervalSeconds { get; set; }

    public DateTimeOffset LastAsyncPlayMessageSent { get; set; }
}