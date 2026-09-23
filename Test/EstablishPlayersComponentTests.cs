using Bunit;
using NSubstitute;
using Treachery.Client.GameEventComponents;
using Treachery.Shared;

namespace Treachery.Test;

[TestClass]
[DoNotParallelize]
public sealed class EstablishPlayersComponentTests
{
    [TestMethod]
    public void DefaultCurrentVersionSettingsProduceValidEvent()
    {
        using var test = new ComponentTestContext(faction: Faction.None);
        EstablishPlayers? submitted = null;
        test.Client.RequestGameEvent(Arg.Do<EstablishPlayers>(value => submitted = value))
            .Returns(Task.FromResult<string?>(null));

        var component = test.RenderContext.Render<EstablishPlayersComponent>();
        component.FindAll("button").Single(button => button.TextContent == "Start").Click();

        Assert.IsNotNull(submitted);
        Assert.AreEqual(Game.LatestVersion, submitted.Game.Version);
        Assert.AreEqual(Faction.None, submitted.Initiator);
        Assert.AreEqual(6, submitted.Settings.NumberOfPlayers);
        Assert.AreEqual(10, submitted.Settings.MaximumTurns);
        Assert.IsGreaterThanOrEqualTo(6, submitted.Settings.AllowedFactionsInPlay.Count);
        Assert.IsNull(submitted.Validate());
    }
}
