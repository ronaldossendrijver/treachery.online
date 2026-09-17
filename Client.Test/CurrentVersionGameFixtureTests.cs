using Treachery.Shared;

namespace Treachery.Client.Test;

[TestClass]
public sealed class CurrentVersionGameFixtureTests
{
    [TestMethod]
    public void EmptyCurrentVersionStateCanBeLoaded()
    {
        var state = GameState.GetStateAsString(new Game());

        var game = CurrentVersionGameFixture.ReplayUntil(state, 0);

        Assert.AreEqual(Game.LatestVersion, game.Version);
        Assert.IsEmpty(game.History);
    }

    [TestMethod]
    public void OlderStateIsRejectedExplicitly()
    {
        var state = new GameState { Version = Game.LatestVersion - 1 };
        var stateData = Utilities.Serialize(state);

        var exception = Assert.ThrowsExactly<ArgumentException>(
            () => CurrentVersionGameFixture.ReplayUntil(stateData, 0));

        Assert.Contains("is not supported", exception.Message);
    }
}
