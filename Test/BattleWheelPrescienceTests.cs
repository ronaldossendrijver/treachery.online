namespace Treachery.Test;

[TestClass]
[DoNotParallelize]
public sealed class BattleWheelPrescienceTests
{
    private bool _wasHidden;

    [TestInitialize]
    public void ShowWheels()
    {
        _wasHidden = BattleWheelState.Hidden;
        BattleWheelState.Hidden = false;
    }

    [TestCleanup]
    public void RestoreWheelVisibility() => BattleWheelState.Hidden = _wasHidden;

    [TestMethod]
    [DataRow(PrescienceAspect.Dial, true)]
    [DataRow(PrescienceAspect.Leader, true)]
    [DataRow(PrescienceAspect.Weapon, true)]
    [DataRow(PrescienceAspect.Defense, true)]
    [DataRow(PrescienceAspect.Dial, false)]
    [DataRow(PrescienceAspect.Leader, false)]
    [DataRow(PrescienceAspect.Weapon, false)]
    [DataRow(PrescienceAspect.Defense, false)]
    public void PrescienceShroudsOnlyTheVisibleOpponentPlan(PrescienceAspect aspect, bool greenIsAggressor)
    {
        var game = CreateBattle(greenIsAggressor ? Faction.Green : Faction.Red, greenIsAggressor ? Faction.Red : Faction.Green);
        Assert.IsNull(new Prescience(game, Faction.Green) { Aspect = aspect }.Execute(false, true));

        var plan = CreatePlan(game, Faction.Red);
        Assert.IsNull(plan.Execute(false, true));

        using var test = new ComponentTestContext(game, Faction.Green);
        var component = test.RenderContext.Render<BattlePlans>();
        var enemyWheel = component.Find(greenIsAggressor ? ".bwl-unit.is-defender" : ".bwl-unit.is-aggressor");
        var ownWheel = component.Find(greenIsAggressor ? ".bwl-unit.is-aggressor" : ".bwl-unit.is-defender");

        Assert.AreEqual("true", enemyWheel.QuerySelector(".bwl-prescience-shroud")?.GetAttribute("aria-hidden"));
        Assert.AreEqual("Prescience preview", enemyWheel.QuerySelector(".bwl-prescience-label")?.TextContent.Trim());
        Assert.IsNull(ownWheel.QuerySelector(".bwl-prescience-shroud"));
        Assert.IsNull(ownWheel.QuerySelector(".bwl-prescience-label"));

        var displayedPlan = component.FindComponents<BattlePlan>().Single(battlePlan => battlePlan.Instance.Plan.Initiator == Faction.Red);
        Assert.IsFalse(displayedPlan.Instance.ShowEntirePlan);
        Assert.AreEqual(aspect == PrescienceAspect.Dial, displayedPlan.FindComponents<MapText>().Any(text => text.Instance.ToShow == "3"));
        Assert.AreEqual(aspect == PrescienceAspect.Leader, displayedPlan.FindComponents<MapLeader>().Any());
        Assert.AreEqual(aspect == PrescienceAspect.Weapon, displayedPlan.FindComponents<MapImage>().Any(image => image.Instance.Src == test.Client.CurrentSkin.GetImageUrl(plan.Weapon)));
        Assert.AreEqual(aspect == PrescienceAspect.Defense, displayedPlan.FindComponents<MapImage>().Any(image => image.Instance.Src == test.Client.CurrentSkin.GetImageUrl(plan.Defense)));
    }

    [TestMethod]
    [DataRow(Faction.Green)]
    [DataRow(Faction.Blue)]
    public void NexusPrescienceShroudsCunningAndSecretAllyPreviews(Faction viewer)
    {
        var game = CreateBattle(viewer, Faction.Red);
        Assert.IsNull(new NexusPlayed(game, viewer)
        {
            Faction = Faction.Green,
            GreenPrescienceAspect = PrescienceAspect.Weapon
        }.Execute(false, true));
        var plan = CreatePlan(game, Faction.Red);
        Assert.IsNull(plan.Execute(false, true));

        using var test = new ComponentTestContext(game, viewer);
        var component = test.RenderContext.Render<BattlePlans>();
        var wheel = component.Find(".bwl-unit.is-defender");
        var displayedPlan = component.FindComponents<BattlePlan>().Single();

        Assert.IsNotNull(wheel.QuerySelector(".bwl-prescience-shroud"));
        Assert.IsNotNull(wheel.QuerySelector(".bwl-prescience-label"));
        Assert.IsFalse(displayedPlan.Instance.ShowEntirePlan);
        Assert.IsTrue(displayedPlan.FindComponents<MapImage>().Any(image => image.Instance.Src == test.Client.CurrentSkin.GetImageUrl(plan.Weapon)));
        Assert.IsFalse(displayedPlan.FindComponents<MapImage>().Any(image => image.Instance.Src == test.Client.CurrentSkin.GetImageUrl(plan.Defense)));
        Assert.AreEqual(0, displayedPlan.FindComponents<MapLeader>().Count);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void OwnPlanIsNotShroudedEvenWhenOpponentUsesPrescience(bool useKarma)
    {
        var game = CreateBattle(Faction.Green, Faction.Red);
        Assert.IsNull(CreatePrescience(game, useKarma ? "karma" : "regular").Execute(false, true));
        Assert.IsNull(CreatePlan(game, Faction.Red).Execute(false, true));

        using var test = new ComponentTestContext(game, Faction.Red);
        var component = test.RenderContext.Render<BattlePlans>();

        Assert.AreEqual(0, component.FindAll(".bwl-prescience-shroud").Count);
        Assert.AreEqual(0, component.FindAll(".bwl-prescience-label").Count);
        Assert.IsTrue(component.FindComponents<BattlePlan>().Single().Instance.ShowEntirePlan);
    }

    [TestMethod]
    public void OpponentPlanWithoutPrescienceIsNotShown()
    {
        var game = CreateBattle(Faction.Green, Faction.Red);
        Assert.IsNull(CreatePlan(game, Faction.Red).Execute(false, true));

        using var test = new ComponentTestContext(game, Faction.Green);
        var component = test.RenderContext.Render<BattlePlans>();

        Assert.AreEqual(0, component.FindAll(".bwl-prescience-shroud").Count);
        Assert.AreEqual(0, component.FindAll(".bwl-prescience-label").Count);
        Assert.AreEqual(0, component.FindComponents<BattlePlan>().Count);
    }

    [TestMethod]
    [DataRow("regular")]
    [DataRow("nexus")]
    [DataRow("karma")]
    public void UnsubmittedPlanIsNotShrouded(string source)
    {
        var game = CreateBattle(Faction.Green, Faction.Red);
        Assert.IsNull(CreatePrescience(game, source).Execute(false, true));

        using var test = new ComponentTestContext(game, Faction.Green);
        var component = test.RenderContext.Render<BattlePlans>();

        Assert.AreEqual(0, component.FindAll(".bwl-prescience-shroud").Count);
        Assert.AreEqual(0, component.FindAll(".bwl-prescience-label").Count);
        Assert.AreEqual(0, component.FindComponents<BattlePlan>().Count);
    }

    [TestMethod]
    [DataRow("regular")]
    [DataRow("nexus")]
    [DataRow("karma")]
    public void ShroudDisappearsWhenBothPlansAreRevealed(string source)
    {
        var game = CreateBattle(Faction.Green, Faction.Red);
        Assert.IsNull(CreatePrescience(game, source).Execute(false, true));
        Assert.IsNull(CreatePlan(game, Faction.Red).Execute(false, true));

        using var test = new ComponentTestContext(game, Faction.Green);
        var component = test.RenderContext.Render<BattlePlans>();
        Assert.AreEqual(1, component.FindAll(".bwl-prescience-shroud").Count);
        Assert.AreEqual(1, component.FindAll(".bwl-prescience-label").Count);
        Assert.AreEqual(source == "karma", component.FindComponents<BattlePlan>().Single().Instance.ShowEntirePlan);

        Assert.IsNull(CreatePlan(game, Faction.Green).Execute(false, true));
        component.Render();

        Assert.AreEqual(0, component.FindAll(".bwl-prescience-shroud").Count);
        Assert.AreEqual(0, component.FindAll(".bwl-prescience-label").Count);
        Assert.AreEqual(2, component.FindComponents<BattlePlan>().Count);
        Assert.IsTrue(component.FindComponents<BattlePlan>().All(battlePlan => battlePlan.Instance.ShowEntirePlan));
    }

    private static GameEvent CreatePrescience(Game game, string source) => source switch
    {
        "regular" => new Prescience(game, Faction.Green) { Aspect = PrescienceAspect.Weapon },
        "nexus" => new NexusPlayed(game, Faction.Green) { Faction = Faction.Green, GreenPrescienceAspect = PrescienceAspect.Weapon },
        "karma" => new KarmaPrescience(game, Faction.Green),
        _ => throw new ArgumentOutOfRangeException(nameof(source))
    };

    private static Game CreateBattle(Faction aggressor, Faction defender)
    {
        var game = new Game { CurrentMainPhase = MainPhase.Battle };
        game.Players.Add(new Player(game, aggressor));
        game.Players.Add(new Player(game, defender));
        Assert.IsNull(new BattleInitiated(game, aggressor)
        {
            Target = defender,
            Territory = game.Map.Arrakeen.Territory
        }.Execute(false, true));
        return game;
    }

    private static Battle CreatePlan(Game game, Faction faction) => new(game, faction)
    {
        Forces = 3,
        Hero = LeaderManager.Leaders.First(leader => leader.Faction == faction),
        Weapon = TreacheryCardManager.Get(1),
        Defense = TreacheryCardManager.Get(9)
    };
}
