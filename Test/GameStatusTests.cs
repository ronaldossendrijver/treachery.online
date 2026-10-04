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
}