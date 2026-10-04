namespace Treachery.Test;

[TestClass]
public class CharityTests
{
    [TestMethod]
    public void LowThresholdHomeworldBonusIsCancelledDuringRecession()
    {
        var (game, beneGesserit) = CreateLowThresholdBeneGesseritGame();

        new BrownEconomics(game, Faction.Brown) { Status = BrownEconomicsStatus.Cancel }
            .Execute(false, true);
        new CharityClaimed(game, Faction.Pink).Execute(false, true);

        Assert.AreEqual(0, beneGesserit.Resources);
    }

    [TestMethod]
    public void LowThresholdHomeworldBonusIsGivenOutsideRecession()
    {
        var (game, beneGesserit) = CreateLowThresholdBeneGesseritGame();

        new CharityClaimed(game, Faction.Pink).Execute(false, true);

        Assert.AreEqual(3, beneGesserit.Resources);
    }

    private static (Game Game, Player BeneGesserit) CreateLowThresholdBeneGesseritGame()
    {
        var game = new Game(Game.LatestVersion, new Participation());
        game.Rules.Add(Rule.Homeworlds);

        var beneGesserit = new Player(game, Faction.Pink);
        game.Players.Add(beneGesserit);
        beneGesserit.InitializeHomeworld(game.Map.Homeworlds.Single(world => world.Faction == Faction.Pink), 0, 0);

        return (game, beneGesserit);
    }
}
