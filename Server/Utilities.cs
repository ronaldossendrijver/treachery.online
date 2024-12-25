namespace Treachery.Server;

public static class Utilities
{
    public static GameInfo ExtractGameInfo(ManagedGame managedGame, int userId) => new()
    {
        GameId = managedGame.GameId,
        CreatorUserId = managedGame.CreatorUserId,
        YourCurrentSeat = managedGame.Game.Participation.SeatedPlayers.GetValueOrDefault(userId, -1),
        Players = managedGame.Game.PlayerNames.ToArray(),
        Observers = managedGame.Game.ObserverNames.ToArray(),
        FactionsInPlay = managedGame.Game.CurrentPhase <= Phase.AwaitingPlayers ? 
            managedGame.Game.Settings.AllowedFactionsInPlay : 
            managedGame.Game.Players.Select(p => p.Faction).ToList(),
        NumberOfBots = managedGame.Game.NumberOfBots,
        Rules = managedGame.Game.CurrentPhase <= Phase.AwaitingPlayers ? 
            managedGame.Game.Settings.InitialRules : 
            managedGame.Game.Rules.ToList(),
        LastAction = managedGame.Game.CurrentPhase > Phase.AwaitingPlayers ? 
            managedGame.Game.History.Last().Time : 
            managedGame.CreationDate,
        CurrentMainPhase = managedGame.Game.CurrentMainPhase,
        CurrentPhase = managedGame.Game.CurrentPhase,
        CurrentTurn = managedGame.Game.CurrentTurn,
        ExpansionLevel = Game.ExpansionLevel,
        GameName = managedGame.Name,
        HasPassword = managedGame.HashedPassword != null,
        NumberOfPlayers = managedGame.Game.Settings.NumberOfPlayers,
        MaximumTurns = managedGame.Game.Settings.MaximumTurns,
        AvailableSeats = managedGame.Game.Participation.AvailableSeats.Select(seat => new AvailableSeatInfo
        {
            Seat = seat, 
            Faction = managedGame.Game.GetFactionInSeat(seat), 
            IsBot = managedGame.Game.IsBot(seat)
        }).ToList()
    };
}