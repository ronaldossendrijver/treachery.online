namespace Treachery.Test;

[TestClass]
[DoNotParallelize]
public sealed class BattlePlanTests
{
    [TestMethod]
    [DataRow(false, false)]
    [DataRow(true, true)]
    public void ReinforcementsBonusIsOnlyVisibleWithEntirePlan(bool showEntirePlan, bool expectBonus)
    {
        var game = new Game();
        game.Players.Add(new Player(game, Faction.Green));
        game.Players.Add(new Player(game, Faction.Red));

        var battle = new BattleInitiated(game, Faction.Green)
        {
            Target = Faction.Red,
            Territory = game.Map.Arrakeen.Territory
        };
        Assert.IsNull(battle.Execute(false, true));

        var prescience = new Prescience(game, Faction.Green) { Aspect = PrescienceAspect.Dial };
        Assert.IsNull(prescience.Execute(false, true));

        var plan = new Battle(game, Faction.Red)
        {
            Forces = 1,
            Weapon = TreacheryCardManager.Items.Single(card => card.Type == TreacheryCardType.Reinforcements)
        };

        using var test = new ComponentTestContext(game);
        var component = test.RenderContext.Render<BattlePlan>(parameters => parameters
            .Add(value => value.Plan, plan)
            .Add(value => value.OpponentPlan, null)
            .Add(value => value.TraitorCalled, false)
            .Add(value => value.Facedanced, false)
            .Add(value => value.IsAggressor, false)
            .Add(value => value.ShowEntirePlan, showEntirePlan));

        var displayedTexts = component.FindComponents<MapText>()
            .Select(text => text.Instance.ToShow)
            .ToList();

        Assert.AreEqual(expectBonus, displayedTexts.Contains("+2"));
    }
}
