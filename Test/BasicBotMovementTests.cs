namespace Treachery.Test;

[TestClass]
public sealed class BasicBotMovementTests
{
    [TestMethod]
    public void BotDoesNotMoveIntoHiddenMobileStrongholdOccupiedByAlly()
    {
        var game = new Game();
        var guild = new Player(game, Faction.Orange);
        var ix = new Player(game, Faction.Purple);
        game.Players.AddRange([guild, ix]);
        guild.Ally = ix.Faction;
        ix.Ally = guild.Faction;

        var source = game.Map.Carthag;
        var destination = game.Map.HiddenMobileStronghold;
        destination.PointAt(game, source);
        guild.AddForces(source, 4, false);

        var bot = new TestableBasicBot(game, guild);
        var battalion = guild.ForcesInLocations[source];
        Assert.IsNotNull(bot.CreateMove(destination, source, battalion));

        ix.AddForces(destination, 2, false);

        Assert.IsNull(bot.CreateMove(destination, source, battalion));
    }

    private sealed class TestableBasicBot(Game game, Player player)
        : BasicBot(game, player, BotParameters.GetDefaultParameters(player.Faction))
    {
        public MovedBattalion? CreateMove(Location to, Location from, Battalion battalion)
            => ConstructMove(to, from, battalion);
    }
}
