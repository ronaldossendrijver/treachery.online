namespace Treachery.Shared;

public class GameSettings
{
    public int MaximumPlayers { get; set; } = 6;
    
    public int MaximumTurns { get; set; } = 10;

    public bool CreatorParticipates { get; set; } = true;

    public List<Rule> InitialRules { get; set; } = Game.RulesetDefinition[Ruleset.BasicGame].ToList();

    public List<Faction> AllowedFactionsInPlay { get; set; } = Enumerations.GetValuesExceptDefault(Faction.None).ToList();
}