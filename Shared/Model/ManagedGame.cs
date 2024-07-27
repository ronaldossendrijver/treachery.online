using System;
using Treachery.Shared;

namespace Treachery.Server;

public class ManagedGame
{
    public Game Game { get; set; }
    
    public string HashedPassword { get; init; }

    public bool ObserversRequirePassword { get; init; }
    
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