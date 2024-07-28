using System;
using Treachery.Shared;

namespace Treachery.Server;

public class ManagedGame
{
    public Game Game { get; set; }
    
    public GameSettings Settings { get; set; }
    
    public string HashedPassword { get; init; }

    public bool ObserversRequirePassword { get; init; }
    
    public bool BotsArePaused { get; set; }
}