using System.Reflection;
using Treachery.Shared;

namespace Treachery.Test;

[TestClass]
public sealed class GameStatusTests
{
    [TestMethod]
    public void DetermineStatusSupportsObserverWithoutPlayer()
    {
        var status = GameStatus.DetermineStatus(new Game(), null, false);

        Assert.IsNotNull(status);
        Assert.IsNotNull(status.HighlightedTerritories);
    }

    [TestMethod]
    public void DetermineStatusSupportsDiscardingTraitorPhase()
    {
        var game = new Game();
        var player = new Player(game, Faction.Black);
        game.Players.Add(player);
        typeof(Game).GetProperty(
                nameof(Game.CurrentPhase),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(game, Phase.DiscardingTraitor);
        typeof(Game).GetProperty(
                "FactionThatMustDiscardTraitor",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(game, Faction.Black);

        var status = GameStatus.DetermineStatus(game, player, true);

        Assert.AreEqual(player, status.WaitingForPlayers.Single());
        Assert.IsTrue(status.WaitingForMe(player, false));
    }
}