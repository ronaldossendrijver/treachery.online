using System.Reflection;
using Bunit;
using Treachery.Client.GameEventComponents;
using Treachery.Shared;

namespace Treachery.Test;

[TestClass]
[DoNotParallelize]
public sealed class HarkonnenTraitorsRedrawnComponentTests
{
    [TestMethod]
    public void SelectingLeaderEnablesConfirmAndRendersNameOnce()
    {
        var game = new Game();
        var player = new Player(game, Faction.Black);
        game.Players.Add(player);
        var leader = LeaderManager.Leaders.Single(leader => leader.Id == 1027);
        player.Traitors.Add(leader);
        typeof(Game).GetProperty("TraitorDeck", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(game, new Deck<IHero>([LeaderManager.Leaders[0]], new LoggedRandom()));

        using var test = new ComponentTestContext(game, Faction.Black);
        var component = test.RenderContext.Render<HarkonnenTraitorsRedrawnComponent>();

        var label = component.Find("label.ui-form-check-label");
        Assert.AreEqual("Beast Rabban", label.TextContent.Trim());
        var confirm = component.Find("button.ui-btn-confirm");
        Assert.IsTrue(confirm.HasAttribute("disabled"));

        component.Find("input[type=checkbox]").Change(true);

        Assert.IsFalse(component.Find("button.ui-btn-confirm").HasAttribute("disabled"));
    }
}
