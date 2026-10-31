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
}
