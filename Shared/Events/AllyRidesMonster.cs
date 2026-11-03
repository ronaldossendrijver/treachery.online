namespace Treachery.Shared;

public class AllyRidesMonster : PlacementEvent
{
    public AllyRidesMonster() { }
    public AllyRidesMonster(Game game, Faction initiator) : base(game, initiator) { }

    public static IEnumerable<Location> ValidSources(Game game, Player player)
        => game.SecondEditionRideTerritories.SelectMany(t => t.Locations).Where(l => player.AnyForcesIn(l) > 0);

    public static IEnumerable<Location> ValidTargets(Game game, Player player)
        => YellowRidesMonster.ValidTargets(game, player);

    public override Message? Validate()
    {
        if (Player.Ally != Faction.Yellow || !Game.YellowAllowsWormRide)
            return Message.Express("Fremen have not permitted you to ride");
        if (Passed) return null;
        if (To == null || !ValidTargets(Game, Player).Contains(To)) return Message.Express("Invalid worm destination");
        if (TotalAmountOfForcesAddedToLocation <= 0) return Message.Express("Select forces to ride");
        if (ForceLocations.Any(kv => !ValidSources(Game, Player).Contains(kv.Key) ||
            kv.Value.AmountOfForces < 0 || kv.Value.AmountOfSpecialForces < 0 ||
            kv.Value.AmountOfForces > Player.ForcesIn(kv.Key) ||
            kv.Value.AmountOfSpecialForces > Player.SpecialForcesIn(kv.Key)))
            return Message.Express("Only surviving forces in the worm territories may ride");
        return null;
    }

    protected override void ExecuteConcreteEvent()
    {
        Game.SecondEditionAllyRideDone = true;
        if (!Passed)
        {
            Game.LastShipmentOrMovement = this;
            foreach (var (source, amount) in ForceLocations)
            {
                Player.MoveForces(source, To!, amount.AmountOfForces);
                Player.MoveSpecialForces(source, To!, amount.AmountOfSpecialForces);
            }
            Log(Initiator, " ride the worm to ", To);
            Game.CheckIntrusion(this);
        }
        else Log(Initiator, " pass a worm ride");
        Game.DetermineNextShipmentAndMoveSubPhase();
    }
}
