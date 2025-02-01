using System;
using Treachery.Shared.Model;

namespace Treachery.Shared;

public class ScheduledGameInfo
{
    public string ScheduledGameId { get; init; }
    
    public DateTimeOffset DateTime { get; init; }
 
    public int CreatorUserId { get; init; }
    
    public string CreatorName { get; init; }
    
    public int? NumberOfPlayers { get; init; } = 6;
    
    public int? MaximumTurns { get; init; } = 10;

    public Ruleset? Ruleset { get; init; }
    
    public bool AsyncPlay { get; init; }

    public List<Faction> AllowedFactionsInPlay { get; init; } = [];
    
    public Dictionary<int,SubscriptionType> Subscribers { get; init; } = [];
    
    public Dictionary<int,string> SubscriberNames { get; init; } = [];
}