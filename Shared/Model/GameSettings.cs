namespace Treachery.Shared;

public class GameSettings
{
    public int NumberOfPlayers { get; set; } = 6;
    
    public int MaximumTurns { get; set; } = 10;

    public bool CreatorParticipates { get; set; }

    public List<Rule> InitialRules { get; set; } = [];

    public List<Faction> AllowedFactionsInPlay { get; set; } = [];
}