using System;
using Treachery.Shared;

namespace Treachery.Server;

public class ManagedGame
{
    public Game Game { get; init; }
    
    public string HashedPassword { get; set; }

    public bool ObserversRequirePassword { get; set; }
    
    public bool BotsArePaused { get; set; }
    
    public GameInfo GetInfo => new()
    {
        Players = Game.PlayerNames.ToArray(),
        Observers = Game.ObserverNames.ToArray(),
        FactionsInPlay = Game.FactionsInPlay,
        NumberOfBots = Game.NumberOfBots,
        Rules = Game.Rules.ToList(),
        LastAction = Game.History.Last().Time
    };
}