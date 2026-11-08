namespace Treachery.Test;

[TestClass]
[DoNotParallelize]
public sealed class DonatedComponentTests
{
    [TestMethod]
    [DataRow(5, 25)]
    [DataRow(99, 119)]
    [DataRow(-5, 15)]
    [DataRow(-99, 0)]
    public void HostCanAdjustOwnSpiceFromThePlayerPanel(int amount, int expectedResources)
    {
        var game = CreateGame();
        game.Rules.Add(Rule.CardsCanBeTraded);
        using var test = new ComponentTestContext(game);
        test.Client.IsHost.Returns(true);
        test.Client.Actions.Returns(game.GetApplicableEvents(game.GetPlayer(Faction.Green), true));
        Donated? submitted = null;
        test.Client.RequestGameEvent(Arg.Do<Donated>(value => submitted = value))
            .Returns(Task.FromResult<string?>(null));

        var menu = test.RenderContext.Render<FactionActionMenu>(parameters => parameters
            .Add(value => value.SelectedFaction, Faction.Green));

        Assert.AreEqual("true", menu.Find("#map-faction-Green-tab-gift").GetAttribute("aria-selected"));
        Assert.IsEmpty(menu.FindComponents<CheckboxComponent>());
        Assert.IsEmpty(menu.FindComponents<SelectFromImageComponent<TreacheryCard>>());
        var picker = menu.FindComponent<SelectResourcesComponent>();
        Assert.AreEqual(-99, picker.Instance.Min);
        Assert.AreEqual(99, picker.Instance.Max);
        for (var step = 0; step < Math.Abs(amount); step++)
        {
            menu.Find(amount > 0 ? ".arrow-up" : ".arrow-down").Click();
        }

        menu.Find(".ui-btn-confirm").Click();

        Assert.IsNotNull(submitted);
        Assert.AreEqual(Faction.Green, submitted.Initiator);
        Assert.AreEqual(Faction.Green, submitted.Target);
        Assert.AreEqual(amount, submitted.Resources);
        Assert.IsTrue(submitted.FromBank);
        Assert.IsNull(submitted.Card);
        Assert.IsNull(submitted.Execute(true, true));
        Assert.AreEqual(expectedResources, game.GetPlayer(Faction.Green)!.Resources);
        Assert.AreEqual(20, game.GetPlayer(Faction.Red)!.Resources);
    }

    [TestMethod]
    public void HostCanUseTheBankWhenTheOnlyOtherPlayerIsAnAlly()
    {
        var game = CreateGame();
        var player = game.GetPlayer(Faction.Green)!;
        player.Ally = Faction.Red;
        player.Resources = 0;
        Assert.IsEmpty(Donated.ValidTargets(game, player));
        Assert.Contains(typeof(Donated), game.GetApplicableEvents(player, true));
        Assert.DoesNotContain(typeof(Donated), game.GetApplicableEvents(player, false));
        using var test = new ComponentTestContext(game);
        test.Client.IsHost.Returns(true);
        test.Client.Actions.Returns(game.GetApplicableEvents(player, true));

        var menu = test.RenderContext.Render<FactionActionMenu>(parameters => parameters
            .Add(value => value.SelectedFaction, Faction.Green));
        Assert.HasCount(1, menu.FindComponents<DonatedComponent>());

        menu.Render(parameters => parameters.Add(value => value.SelectedFaction, Faction.Red));
        menu.Find("input[type=checkbox]").Change(true);
        Assert.AreEqual(99, menu.FindComponent<SelectResourcesComponent>().Instance.Max);
    }

    [TestMethod]
    [DataRow(false, false, true)]
    [DataRow(true, true, true)]
    [DataRow(true, false, false)]
    public void OwnFactionGiftRemainsHiddenWithoutHostPlayerAccess(bool isHost, bool isObserver, bool hasAction)
    {
        using var test = new ComponentTestContext(CreateGame());
        test.Client.IsHost.Returns(isHost);
        test.Client.IsObserver.Returns(isObserver);
        test.Client.Actions.Returns(hasAction ? [typeof(Donated)] : []);

        var menu = test.RenderContext.Render<FactionActionMenu>(parameters => parameters
            .Add(value => value.SelectedFaction, Faction.Green));

        Assert.IsEmpty(menu.FindAll("#map-faction-Green-tab-gift"));
        Assert.IsEmpty(menu.FindComponents<DonatedComponent>());
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void GiftsToOtherPlayersStillDefaultToThePlayersOwnSpice(bool isHost)
    {
        var game = CreateGame();
        using var test = new ComponentTestContext(game);
        test.Client.IsHost.Returns(isHost);
        test.Client.Actions.Returns([typeof(Donated)]);
        Donated? submitted = null;
        test.Client.RequestGameEvent(Arg.Do<Donated>(value => submitted = value))
            .Returns(Task.FromResult<string?>(null));

        var menu = test.RenderContext.Render<FactionActionMenu>(parameters => parameters
            .Add(value => value.SelectedFaction, Faction.Red));
        Assert.AreEqual(20, menu.FindComponent<SelectResourcesComponent>().Instance.Max);
        menu.Find(".ui-btn-confirm").Click();

        Assert.IsNotNull(submitted);
        Assert.AreEqual(Faction.Red, submitted.Target);
        Assert.IsFalse(submitted.FromBank);
        Assert.AreEqual(1, submitted.Resources);
    }

    [TestMethod]
    public void SwitchingFromOwnFactionRestoresTheNormalDonationMode()
    {
        using var test = new ComponentTestContext(CreateGame());
        test.Client.IsHost.Returns(true);
        test.Client.Actions.Returns([typeof(Donated)]);

        var menu = test.RenderContext.Render<FactionActionMenu>(parameters => parameters
            .Add(value => value.SelectedFaction, Faction.Green));
        Assert.AreEqual(99, menu.FindComponent<SelectResourcesComponent>().Instance.Max);

        menu.Render(parameters => parameters.Add(value => value.SelectedFaction, Faction.Red));
        Assert.AreEqual(20, menu.FindComponent<SelectResourcesComponent>().Instance.Max);
        Assert.IsFalse(menu.FindComponent<CheckboxComponent>().Instance.Value);
    }

    private static Game CreateGame()
    {
        var game = new Game();
        game.CurrentMainPhase = MainPhase.Contemplate;
        game.Settings.NumberOfPlayers = 2;
        game.Players.Add(new Player(game, Faction.Green) { Seat = 0, Resources = 20 });
        game.Players.Add(new Player(game, Faction.Red) { Seat = 1, Resources = 20 });
        typeof(Game).GetProperty(nameof(Game.CurrentPhase))!.SetValue(game, Phase.Contemplate);
        return game;
    }
}
