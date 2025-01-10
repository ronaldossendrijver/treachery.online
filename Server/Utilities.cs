namespace Treachery.Server;

public static class Utilities
{
    public static GameInfo ExtractGameInfo(ManagedGame managedGame) => new()
    {
        GameId = managedGame.GameId,
        CreatorId = managedGame.CreatorUserId,
        FactionsInPlay = managedGame.Game.CurrentPhase <= Phase.AwaitingPlayers ? 
            managedGame.Game.Settings.AllowedFactionsInPlay.ToArray() : 
            managedGame.Game.Players.Where(p => p.Faction != Faction.None).Select(p => p.Faction).ToArray(),
        NrOfBots = managedGame.Game.NumberOfBots,
        Ruleset = managedGame.Game.CurrentPhase <= Phase.AwaitingPlayers ? 
            Game.DetermineApproximateRuleset(managedGame.Game.Settings.AllowedFactionsInPlay, managedGame.Game.Settings.InitialRules, Game.ExpansionLevel)  : 
            Game.DetermineApproximateRuleset(managedGame.Game.Players.Select(p => p.Faction).ToList(), managedGame.Game.Rules, Game.ExpansionLevel),
        LastAction = DetermineLastAction(managedGame),
        MainPhase = managedGame.Game.CurrentMainPhase,
        Phase = managedGame.Game.CurrentPhase,
        Turn = managedGame.Game.CurrentTurn,
        Name = managedGame.Name,
        HasPassword = !string.IsNullOrEmpty(managedGame.HashedPassword),
        MaxPlayers = managedGame.Game.Settings.NumberOfPlayers,
        MaxTurns = managedGame.Game.Settings.MaximumTurns,
        NrOfPlayers = managedGame.Game.Participation.SeatedPlayers.Count,
        SeatedPlayers = managedGame.Game.Participation.SeatedPlayers,
        AvailableSeats = managedGame.Game.Players
            .Where(p => managedGame.Game.SeatIsAvailable(p.Seat))
            .Select(p => new AvailableSeatInfo
        {
            Seat = p.Seat, 
            Faction = p.Faction, 
            IsBot = p.IsBot
        }).ToArray()
    };

    public static DateTimeOffset DetermineLastAction(ManagedGame managedGame)
    {
        return managedGame.Game.CurrentPhase > Phase.AwaitingPlayers ? 
            managedGame.Game.History.Last().Time : 
            managedGame.CreationDate;
    }
}