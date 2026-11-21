using System.Reflection;
using Treachery.Shared;
using Treachery.Shared.Model;

namespace Treachery.Test;

[TestClass]
public sealed class BeneGesseritMovementTests
{
    [TestMethod]
    [DataRow(176, true, true)]
    [DataRow(176, false, true)]
    [DataRow(177, true, false)]
    [DataRow(Game.LatestVersion, true, false)]
    [DataRow(Game.LatestVersion, false, true)]
    public void FighterMovingToOccupiedTerritoryWithoutBlueCanBecomeAdvisorOnlyInOlderGames(
        int version, bool asAdvisors, bool expectedValid)
    {
        var (game, blue, _, source, destination) = CreateGame(version);
        blue.AddForces(source, 1, false);

        var move = CreateMove(game, source, destination, 1, 0, asAdvisors);

        Assert.AreEqual(expectedValid, move.Validate() == null);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void AdvisorMovingToOccupiedTerritoryMayChooseEitherType(bool asAdvisors)
    {
        var (game, blue, green, source, destination) = CreateGame(Game.LatestVersion);
        blue.AddSpecialForces(source, 1, false);
        green.AddForces(source, 1, false);

        var move = CreateMove(game, source, destination, 0, 1, asAdvisors);

        Assert.IsNull(move.Validate());
    }

    [TestMethod]
    [DataRow(false, true, false)]
    [DataRow(false, true, true)]
    [DataRow(true, false, false)]
    [DataRow(true, false, true)]
    public void MovingForcesMustMatchExistingBlueForces(
        bool sourceIsAdvisor, bool destinationIsAdvisor, bool asAdvisors)
    {
        var (game, blue, green, source, destination) = CreateGame(Game.LatestVersion);
        if (sourceIsAdvisor)
        {
            blue.AddSpecialForces(source, 1, false);
            green.AddForces(source, 1, false);
        }
        else
        {
            blue.AddForces(source, 1, false);
        }

        if (destinationIsAdvisor)
            blue.AddSpecialForces(destination, 1, false);
        else
            blue.AddForces(destination, 1, false);

        var move = CreateMove(game, source, destination, sourceIsAdvisor ? 0 : 1, sourceIsAdvisor ? 1 : 0, asAdvisors);

        Assert.AreEqual(destinationIsAdvisor == asAdvisors, move.Validate() == null);
    }

    [TestMethod]
    public void AdvisorMovingToEmptyTerritoryMustBecomeFighter()
    {
        var (game, blue, green, source, destination) = CreateGame(Game.LatestVersion, occupiedDestination: false);
        blue.AddSpecialForces(source, 1, false);
        green.AddForces(source, 1, false);

        Assert.IsNull(CreateMove(game, source, destination, 0, 1, asAdvisors: false).Validate());
        Assert.IsNotNull(CreateMove(game, source, destination, 0, 1, asAdvisors: true).Validate());
    }

    [TestMethod]
    public void WormRideFightersJoinExistingAdvisorsAsAdvisors()
    {
        var (game, blue, _, source, destination) = CreateWormRideGame();
        blue.AddForces(source, 1, false);
        blue.AddSpecialForces(destination, 1, false);
        var ride = CreateRide(game, source, destination, 1, 0);

        Assert.IsNull(ride.Validate());
        Assert.IsNull(ride.Execute(false, true));
        Assert.AreEqual(0, blue.ForcesIn(destination.Territory));
        Assert.AreEqual(2, blue.SpecialForcesIn(destination.Territory));
    }

    [TestMethod]
    public void WormRideCannotMoveAdvisorsIntoExistingFighters()
    {
        var (game, blue, _, source, destination) = CreateWormRideGame();
        blue.AddSpecialForces(source, 1, false);
        blue.AddForces(destination, 1, false);

        var ride = CreateRide(game, source, destination, 0, 1);
        Assert.IsNotNull(ride.Validate());
        Assert.IsFalse(AllyRidesMonster.ValidTargets(game, blue, ride.ForceLocations).Contains(destination));
    }

    [TestMethod]
    public void WormRideCannotCombineDifferentTypesInAnEmptyTerritory()
    {
        var (game, blue, _, source, destination) = CreateWormRideGame();
        var otherSource = game.Map.Carthag;
        game.SecondEditionRideTerritories.Add(otherSource.Territory);
        blue.AddForces(source, 1, false);
        blue.AddSpecialForces(otherSource, 1, false);
        var forces = new Dictionary<Location, Battalion>
        {
            [source] = new(Faction.Blue, 1, 0, source),
            [otherSource] = new(Faction.Blue, 0, 1, otherSource)
        };
        var ride = new AllyRidesMonster(game, Faction.Blue) { To = destination, ForceLocations = forces };

        Assert.IsNotNull(ride.Validate());
        Assert.IsFalse(AllyRidesMonster.ValidTargets(game, blue, forces).Contains(destination));
    }

    [TestMethod]
    public void WormRideCanCombineDifferentTypesWithExistingAdvisors()
    {
        var (game, blue, _, source, destination) = CreateWormRideGame();
        var otherSource = game.Map.Carthag;
        game.SecondEditionRideTerritories.Add(otherSource.Territory);
        blue.AddForces(source, 1, false);
        blue.AddSpecialForces(otherSource, 1, false);
        blue.AddSpecialForces(destination, 1, false);
        var ride = new AllyRidesMonster(game, Faction.Blue)
        {
            To = destination,
            ForceLocations = new Dictionary<Location, Battalion>
            {
                [source] = new(Faction.Blue, 1, 0, source),
                [otherSource] = new(Faction.Blue, 0, 1, otherSource)
            }
        };

        Assert.IsNull(ride.Validate());
        Assert.IsNull(ride.Execute(false, true));
        Assert.AreEqual(0, blue.ForcesIn(destination.Territory));
        Assert.AreEqual(3, blue.SpecialForcesIn(destination.Territory));
    }

    [TestMethod]
    public void BotSelectsAValidDestinationForMixedWormRiders()
    {
        var (game, blue, _, source, _) = CreateWormRideGame();
        var otherSource = game.Map.Carthag;
        game.SecondEditionRideTerritories.Add(otherSource.Territory);
        blue.AddForces(source, 1, false);
        blue.AddSpecialForces(otherSource, 1, false);
        var bot = new BasicBot(game, blue, BotParameters.GetDefaultParameters(Faction.Blue));

        var ride = Assert.IsInstanceOfType<AllyRidesMonster>(
            bot.DetermineLowPriorityInPhaseAction([typeof(AllyRidesMonster)]));

        Assert.IsFalse(ride.Passed);
        Assert.IsNull(ride.Validate());
        Assert.AreEqual(1, ride.ForcesAddedToLocation);
        Assert.AreEqual(1, ride.SpecialForcesAddedToLocation);
        Assert.IsGreaterThan(0, blue.SpecialForcesIn(ride.To!.Territory));
    }

    [TestMethod]
    public void WormRidePreservesHistoricalBehavior()
    {
        var (game, blue, _, source, destination) = CreateWormRideGame(196);
        blue.AddForces(source, 1, false);
        blue.AddSpecialForces(destination, 1, false);
        var ride = CreateRide(game, source, destination, 1, 0);

        Assert.IsNull(ride.Validate());
        Assert.IsNull(ride.Execute(false, true));
        Assert.AreEqual(1, blue.ForcesIn(destination.Territory));
        Assert.AreEqual(1, blue.SpecialForcesIn(destination.Territory));
    }

    [TestMethod]
    [DataRow(176, false)]
    [DataRow(177, true)]
    [DataRow(Game.LatestVersion, true)]
    public void FactionInfoDescribesTheMovementRulesForTheGameVersion(int version, bool fighterRestriction)
    {
        var (game, _, _, _, _) = CreateGame(version);

        var info = DefaultSkin.Default.GetFactionInfo_HTML(game, Faction.Blue);

        if (fighterRestriction)
        {
            Assert.Contains("When you move advisors into an occupied territory where you have no forces", info);
            Assert.Contains("Fighters moving into such a territory must remain fighters.", info);
        }
        else
        {
            Assert.Contains("When you move forces into an occupied territory where you have no forces", info);
            Assert.DoesNotContain("Fighters moving into such a territory must remain fighters.", info);
        }

        Assert.Contains("the moving forces become the same type", info);
        Assert.Contains("Advisors moving into an unoccupied territory become fighters.", info);
        Assert.Contains("where you have fighters, you may keep them as fighters or flip them to advisors.", info);
    }

    private static (Game Game, Player Blue, Player Green, Location Source, Location Destination) CreateGame(
        int version, bool occupiedDestination = true)
    {
        var game = new Game(version, new Participation());
        game.Rules.Add(Rule.BlueAdvisors);
        var blue = new Player(game, Faction.Blue);
        var green = new Player(game, Faction.Green);
        game.Players.Add(blue);
        game.Players.Add(green);

        var source = game.Map.LocationLookup.Find(43)!;
        var destination = game.Map.Arrakeen;
        if (occupiedDestination)
            green.AddForces(destination, 10, false);

        return (game, blue, green, source, destination);
    }

    private static Move CreateMove(Game game, Location source, Location destination, int fighters, int advisors, bool asAdvisors)
    {
        return new Move(game, Faction.Blue)
        {
            To = destination,
            ForceLocations = new Dictionary<Location, Battalion>
            {
                [source] = new(Faction.Blue, fighters, advisors, source)
            },
            AsAdvisors = asAdvisors
        };
    }

    private static (Game Game, Player Blue, Player Green, Location Source, Location Destination) CreateWormRideGame(int version = Game.LatestVersion)
    {
        var result = CreateGame(version);
        var (game, blue, _, source, _) = result;
        game.Rules.Add(Rule.SE_BlueAdvisors);
        blue.Ally = Faction.Yellow;
        game.Players.Add(new Player(game, Faction.Yellow) { Ally = Faction.Blue });
        game.SecondEditionRideTerritories.Add(source.Territory);
        SetGameProperty(game, "YellowAllowsWormRide", true);
        SetGameProperty(game, "SecondEditionYellowRideDone", true);
        SetGameProperty(game, "SecondEditionPendingWorms", 1);
        SetGameProperty(game, "CurrentPhase", Phase.AllyRidingMonster);
        return result;
    }

    private static AllyRidesMonster CreateRide(Game game, Location source, Location destination, int fighters, int advisors)
        => new(game, Faction.Blue)
        {
            To = destination,
            ForceLocations = new Dictionary<Location, Battalion> { [source] = new(Faction.Blue, fighters, advisors, source) }
        };

    private static void SetGameProperty(Game game, string propertyName, object value)
        => typeof(Game).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(game, value);
}
