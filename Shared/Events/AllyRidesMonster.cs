namespace Treachery.Shared;

public class AllyRidesMonster : PlacementEvent
{
    public AllyRidesMonster() { }
    public AllyRidesMonster(Game game, Faction initiator) : base(game, initiator) { }

    public static IEnumerable<Location> ValidSources(Game game, Player player)
        => game.SecondEditionRideTerritories.SelectMany(t => t.Locations).Where(l => player.AnyForcesIn(l) > 0);

    public static IEnumerable<Location> ValidTargets(Game game, Player player)
        => YellowRidesMonster.ValidTargets(game, player);

    public new static IEnumerable<Location> ValidTargets(Game game, Player player, Dictionary<Location, Battalion> forces)
        => ValidTargets(game, player).Where(target => CanRideTo(game, player, target, forces));

    private static bool CanRideTo(Game game, Player player, Location target, Dictionary<Location, Battalion> forces)
    {
        if (game.Version < 197 || !game.Applicable(Rule.SE_BlueAdvisors) || !player.Is(Faction.Blue))
            return true;

        var advisors = forces.Values.Sum(b => b.AmountOfSpecialForces);
        var fighters = forces.Values.Sum(b => b.AmountOfForces);
        return advisors == 0 ||
               (player.ForcesIn(target.Territory) == 0 &&
                (fighters == 0 || player.SpecialForcesIn(target.Territory) > 0));
    }

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
        if (!CanRideTo(Game, Player, To, ForceLocations))
            return Message.Express("Advisors cannot ride together with fighters to that territory");
        return null;
    }

    protected override void ExecuteConcreteEvent()
    {
        Game.SecondEditionAllyRideDone = true;
        if (!Passed)
        {
            Game.LastShipmentOrMovement = this;
            var joinAdvisors = Game.Version >= 197 && Game.Applicable(Rule.SE_BlueAdvisors) &&
                               Player.Is(Faction.Blue) && Player.SpecialForcesIn(To!.Territory) > 0;
            foreach (var (source, amount) in ForceLocations)
            {
                Player.MoveForces(source, To!, amount.AmountOfForces);
                Player.MoveSpecialForces(source, To!, amount.AmountOfSpecialForces);
            }
            if (joinAdvisors) Player.FlipForces(To!, true);
            Log(Initiator, " ride the worm to ", To);
            Game.CheckIntrusion(this);
        }
        else Log(Initiator, " pass a worm ride");
        Game.DetermineNextShipmentAndMoveSubPhase();
    }
}
