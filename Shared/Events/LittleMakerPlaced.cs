namespace Treachery.Shared;

public class LittleMakerPlaced : GameEvent
{
    public LittleMakerPlaced() { }
    public LittleMakerPlaced(Game game, Faction initiator) : base(game, initiator) { }
    public int LocationId { get; set; } = -1;

    public static IEnumerable<Location> ValidTargets(Game game)
        => game.Map.Locations(false).Where(l => l.SpiceBlowAmount > 0 && !game.IsInStorm(l) &&
            game.ResourcesIn(l) == 0 && game.Players.All(p => p.AnyForcesIn(l) == 0));

    public override Message? Validate()
    {
        if (Player != Game.LastPlayerInStormOrder) return Message.Express("Only the last player in Storm order may place Little Maker");
        if (!ValidTargets(Game).Any(l => l.Id == LocationId)) return Message.Express("Choose a vacant Spice Blow marker outside the storm");
        return null;
    }

    protected override void ExecuteConcreteEvent()
        => Game.PlaceLittleMaker(Game.Map.LocationLookup.Find(LocationId)!);
}
