using Treachery.Shared;

namespace Treachery.Server;

public class ManagedGame
{
    public Game Game { get; set; }

    public List<User> Players { get; set; } = [];

    public List<User> Observers { get; set; } = [];

    public List<User> Hosts { get; set; } = [];

    public string HashedPassword { get; set; }
    
    public GameInfo Info => new()
    {
        Players = Players.Select(p => p.Name).ToArray(),
        Observers = Observers.Select(p => p.Name).ToArray(),
        FactionsInPlay = Game.FactionsInPlay,
        NumberOfBots = Game.Players.Count(p => p.IsBot),
        Rules = Game.Rules.ToList(),
        LastAction = Game.History.Last().Time
    };

    public bool IsHost(User user) => Hosts.Contains(user);

    public bool HasRoomFor(Faction faction)
    {
        //TODO implement
        return true;
    }
}