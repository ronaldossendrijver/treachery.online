namespace Treachery.Server;

public static class Utilities
{
    public static GameInfo ExtractGameInfo(ManagedGame managedGame, int userId) => new()
    {
        GameId = managedGame.GameId,
        CreatorUserId = managedGame.CreatorUserId,
        YourCurrentSeat = managedGame.Game.Participation.SeatedPlayers.GetValueOrDefault(userId, -1),
        NumberOfObservers = managedGame.Game.Participation.Observers.Count,
        FactionsInPlay = managedGame.Game.CurrentPhase <= Phase.AwaitingPlayers ? 
            managedGame.Game.Settings.AllowedFactionsInPlay : 
            managedGame.Game.Players.Select(p => p.Faction).ToList(),
        NumberOfBots = managedGame.Game.NumberOfBots,
        Ruleset = managedGame.Game.CurrentPhase <= Phase.AwaitingPlayers ? 
            Game.DetermineApproximateRuleset(managedGame.Game.Settings.AllowedFactionsInPlay, managedGame.Game.Settings.InitialRules, Game.ExpansionLevel)  : 
            Game.DetermineApproximateRuleset(managedGame.Game.Players.Select(p => p.Faction).ToList(), managedGame.Game.Rules, Game.ExpansionLevel),
        LastAction = managedGame.Game.CurrentPhase > Phase.AwaitingPlayers ? 
            managedGame.Game.History.Last().Time : 
            managedGame.CreationDate,
        CurrentMainPhase = managedGame.Game.CurrentMainPhase,
        CurrentPhase = managedGame.Game.CurrentPhase,
        CurrentTurn = managedGame.Game.CurrentTurn,
        ExpansionLevel = Game.ExpansionLevel,
        GameName = managedGame.Name,
        HasPassword = managedGame.HashedPassword != null,
        MaximumNumberOfPlayers = managedGame.Game.Settings.NumberOfPlayers,
        MaximumTurns = managedGame.Game.Settings.MaximumTurns,
        ActualNumberOfPlayers = managedGame.Game.CurrentPhase <= Phase.AwaitingPlayers ? 
            managedGame.Game.Participation.StandingPlayers.Count :
            managedGame.Game.Participation.SeatedPlayers.Count,
        AvailableSeats = managedGame.Game.Participation.AvailableSeats.Select(seat => new AvailableSeatInfo
        {
            Seat = seat, 
            Faction = managedGame.Game.GetFactionInSeat(seat), 
            IsBot = managedGame.Game.IsBot(seat)
        }).ToList()
    };
}