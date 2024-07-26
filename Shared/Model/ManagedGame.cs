using System;
using Treachery.Shared;

namespace Treachery.Server;

public class ManagedGame
{
    public Game Game { get; init; }
    
    public Dictionary<int, ISeatable> Seats { get; set; } = [];

    public List<int> AvailableSeats { get; set; } = [];

    public IEnumerable<User> Players => Seats.Values.OfType<User>();

    public List<User> Observers { get; set; } = [];

    public List<User> Hosts { get; set; } = [];

    public string HashedPassword { get; set; }
    
    public bool BotsArePaused { get; set; }

    public bool BotPositionsAreAvailable { get; set; }

    public bool IsHost(User user) => Hosts.Contains(user);

    public bool IsBot(int seat) => Seats.TryGetValue(seat, out var player) && player is Bot;

    public bool HasRoomFor(Faction faction) => AvailableSeats.Contains(Game.SeatOf(faction));
    
    public GameParticipation GetParticipation() => new()
    {
        Seats = Seats.Where(seat => seat.Value is User).ToDictionary(seat => seat.Key, seat => ((User)seat.Value).Id),
        SeatedUsers = Seats.Where(seat => seat.Value is User).ToDictionary(seat => ((User)seat.Value).Id, seat => seat.Key),
        SeatedBots = Seats.Where(seat => seat.Value is Bot).Select(seat => seat.Key).ToHashSet(),
        PlayerNames = Players.ToDictionary(user => user.Id, user => user.PlayerName),
        ObserverNames = Observers.ToDictionary(user => user.Id, user => user.PlayerName),
        Hosts = Hosts.Select(user => user.Id).ToList(),
        BotPositionsAreAvailable = BotPositionsAreAvailable,
        AvailableSeats = Game.CurrentPhase is Phase.AwaitingPlayers ? Enumerable.Repeat(Faction.None, Game.MaximumNumberOfPlayers) : AvailableSeats.Select(seat => Game.PlayerAtSeat(seat).Faction),
        BotsArePaused = BotsArePaused
    };
    
    public GameInfo GetInfo => new()
    {
        Players = Players.Select(p => p.Name).ToArray(),
        Observers = Observers.Select(p => p.Name).ToArray(),
        FactionsInPlay = Game.FactionsInPlay,
        NumberOfBots = Seats.Values.OfType<Bot>().Count(),
        Rules = Game.Rules.ToList(),
        LastAction = Game.History.Last().Time
    };
}