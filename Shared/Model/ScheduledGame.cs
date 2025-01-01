using System;

namespace Treachery.Shared;

public class ScheduledGame
{
    public DateTimeOffset DateTime { get; init; }
    
    public int CreatorUserId { get; init; }
    
    public int? NumberOfPlayers { get; set; } = 6;
    
    public int? MaximumTurns { get; set; } = 10;

    public Ruleset? Ruleset { get; set; }
    
    public bool AsyncPlay { get; set; }

    public List<Faction> AllowedFactionsInPlay { get; set; } = [];
    
    public List<int> SubscribedUsersCertain { get; set; } = [];
    
    public List<int> SubscribedUsersMaybe { get; set; } = [];
}