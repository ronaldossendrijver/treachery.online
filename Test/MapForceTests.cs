namespace Treachery.Test;

[TestClass]
[DoNotParallelize]
public sealed class MapForceTests
{
    [TestMethod]
    [DataRow(Faction.Green, false, 3, true)]
    [DataRow(Faction.White, false, 3, true)]
    [DataRow(Faction.White, true, 3, false)]
    [DataRow(Faction.Green, false, 0, false)]
    public void CountIsPositionedWithinTheForceStack(Faction faction, bool special, int amount, bool hasCount)
    {
        using var test = new ComponentTestContext();
        var component = test.RenderContext.Render<MapForce>(parameters => parameters
            .Add(value => value.X, 100)
            .Add(value => value.Y, 150)
            .Add(value => value.Diameter, 20)
            .Add(value => value.Faction, faction)
            .Add(value => value.Special, special)
            .Add(value => value.Amount, amount)
            .Add(value => value.FxKey, "force-1-test-f"));

        var stack = component.Find("[data-fx-key='force-1-test-f']");
        StringAssert.Contains(stack.GetAttribute("style")!, "left:100px;top:150px;");
        Assert.AreEqual(amount, stack.QuerySelectorAll("img").Length);
        var count = stack.QuerySelector("[data-fx-count] > span");
        Assert.AreEqual(hasCount, count != null);
        if (count != null)
        {
            Assert.AreEqual(amount.ToString(), count.TextContent.Trim());
            StringAssert.Contains(count.GetAttribute("style")!, "left:10px;top:10px;");
        }
    }

    [TestMethod]
    public void ReserveForceCountsKeepTheirOriginalPosition()
    {
        using var test = new ComponentTestContext();
        var component = test.RenderContext.Render<MapForce>(parameters => parameters
            .Add(value => value.X, 40)
            .Add(value => value.Y, 70)
            .Add(value => value.Diameter, 10)
            .Add(value => value.Faction, Faction.Green)
            .Add(value => value.Amount, 2));

        var stack = component.Find("div");
        StringAssert.Contains(stack.GetAttribute("style")!, "left:40px;top:70px;");
        StringAssert.Contains(stack.QuerySelector("[data-fx-count] > span")!.GetAttribute("style")!, "left:5px;top:5px;");
    }
}
