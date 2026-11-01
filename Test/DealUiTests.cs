using Microsoft.AspNetCore.Components.Web;

namespace Treachery.Test;

[TestClass]
[DoNotParallelize]
public sealed class DealUiTests
{
    [TestMethod]
    public void PlayerMenuOffersToTheSelectedFactionAndSupportsKeyboardTabs()
    {
        var game = CreateGame(Faction.Green, Faction.Red, Faction.Yellow);
        using var test = new ComponentTestContext(game);
        test.Client.Actions.Returns([typeof(Donated), typeof(DealOffered)]);
        DealOffered? submitted = null;
        test.Client.RequestGameEvent(Arg.Do<DealOffered>(value => submitted = value))
            .Returns(Task.FromResult<string?>(null));

        var menu = test.RenderContext.Render<FactionActionMenu>(parameters => parameters
            .Add(value => value.SelectedFaction, Faction.Red));

        CollectionAssert.AreEqual(
            new[] { "Give spice", "Offer a deal", "Faction info" },
            menu.FindAll("[role=tab]").Select(tab => tab.TextContent.Trim()).ToArray());
        menu.Find("#map-faction-Red-tab-deal").Click();
        CollectionAssert.AreEqual(new[] { Faction.Red }, menu.FindComponent<DealOfferedComponent>().Instance.to);

        menu.Find("textarea").Change("Do not move into Arrakeen");
        menu.Find(".ui-btn-confirm").Click();

        Assert.IsNotNull(submitted);
        Assert.AreEqual(Faction.Green, submitted.Initiator);
        CollectionAssert.AreEqual(new[] { Faction.Red }, submitted.To);
        Assert.AreEqual("Do not move into Arrakeen", submitted.Text);

        menu.Find("#map-faction-Red-tab-deal").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });
        Assert.AreEqual("true", menu.Find("#map-faction-Red-tab-info").GetAttribute("aria-selected"));
        menu.Find("#map-faction-Red-tab-info").KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });
        Assert.AreEqual("true", menu.Find("#map-faction-Red-tab-deal").GetAttribute("aria-selected"));
    }

    [TestMethod]
    public void ChangingPlayersResetsTheDefaultRecipientAndDoesNotTargetAnAlly()
    {
        var game = CreateGame(Faction.Green, Faction.Red, Faction.Yellow);
        game.GetPlayer(Faction.Green)!.Ally = Faction.Yellow;
        using var test = new ComponentTestContext(game);
        test.Client.Actions.Returns([typeof(DealOffered)]);

        var menu = test.RenderContext.Render<FactionActionMenu>(parameters => parameters
            .Add(value => value.SelectedFaction, Faction.Red));
        CollectionAssert.AreEqual(new[] { Faction.Red }, menu.FindComponent<DealOfferedComponent>().Instance.to);

        menu.Render(parameters => parameters.Add(value => value.SelectedFaction, Faction.Yellow));
        Assert.IsEmpty(menu.FindComponent<DealOfferedComponent>().Instance.to);

        menu.Render(parameters => parameters.Add(value => value.SelectedFaction, Faction.Green));
        Assert.IsEmpty(menu.FindComponent<DealOfferedComponent>().Instance.to);
    }

    [TestMethod]
    public void ObserversAndPlayersWithoutTheActionCannotOfferDeals()
    {
        var game = CreateGame(Faction.Green, Faction.Red);
        using var test = new ComponentTestContext(game);
        test.Client.Actions.Returns([typeof(DealOffered)]);
        test.Client.IsObserver.Returns(true);

        var menu = test.RenderContext.Render<FactionActionMenu>(parameters => parameters
            .Add(value => value.SelectedFaction, Faction.Red));
        Assert.IsEmpty(menu.FindAll("#map-faction-Red-tab-deal"));

        test.Client.IsObserver.Returns(false);
        test.Client.Actions.Returns([]);
        menu.Render();
        Assert.IsEmpty(menu.FindAll("#map-faction-Red-tab-deal"));
    }

    [TestMethod]
    public void DealsDialogKeepsPendingOfferActionsButMovesCurrentDealsToTheBoard()
    {
        var game = CreateGame(Faction.Green, Faction.Red, Faction.Yellow);
        game.Deals.Add(new Deal { BoundFaction = Faction.Yellow, ConsumingFaction = Faction.Green, Text = "Board-only agreement", End = Phase.TurnConcluded });
        game.DealOffers.Add(new DealOffered(game, Faction.Red) { To = [Faction.Green], Text = "Incoming offer", EndPhase = Phase.TurnConcluded });
        game.DealOffers.Add(new DealOffered(game, Faction.Green) { To = [Faction.Yellow], Text = "Outgoing offer", EndPhase = Phase.TurnConcluded });

        using var test = new ComponentTestContext(game);
        test.Client.InGame.Returns(true);
        test.Client.Actions.Returns([typeof(DealOffered)]);
        DealAccepted? accepted = null;
        DealOffered? withdrawn = null;
        test.Client.RequestGameEvent(Arg.Do<DealAccepted>(value => accepted = value))
            .Returns(Task.FromResult<string?>(null));
        test.Client.RequestGameEvent(Arg.Do<DealOffered>(value => withdrawn = value))
            .Returns(Task.FromResult<string?>(null));

        var panel = test.RenderContext.Render<DealsPanel>();

        Assert.AreEqual("2", panel.Find(".deals-count").TextContent.Trim());
        StringAssert.Contains(panel.Markup, "Incoming offer");
        StringAssert.Contains(panel.Markup, "Outgoing offer");
        Assert.DoesNotContain("Board-only agreement", panel.Markup);
        Assert.IsEmpty(panel.FindComponents<DealOfferedComponent>());

        panel.FindAll("button").Single(button => button.TextContent.Trim() == "Accept").Click();
        panel.FindAll("button").Single(button => button.TextContent.Trim() == "Withdraw").Click();
        Assert.IsNotNull(accepted);
        Assert.AreEqual(Faction.Red, accepted.BoundFaction);
        Assert.IsNotNull(withdrawn);
        Assert.IsTrue(withdrawn.Cancel);
    }

    [TestMethod]
    public void UnplaceableDealsAndEarlyOffersRemainAccessible()
    {
        var game = CreateGame(Faction.Green, Faction.Red);
        game.Deals.Add(new Deal { BoundFaction = Faction.Yellow, ConsumingFaction = Faction.Green, Text = "Unplaced agreement", End = Phase.TurnConcluded });
        using var test = new ComponentTestContext(game);
        test.Client.InGame.Returns(true);
        test.Client.Actions.Returns([typeof(DealOffered)]);

        var panel = test.RenderContext.Render<DealsPanel>();
        StringAssert.Contains(panel.Markup, "Current deals unavailable on the map");
        StringAssert.Contains(panel.Markup, "Unplaced agreement");

        SetPhase(game, Phase.SelectingFactions);
        panel.Render();
        Assert.HasCount(1, panel.FindComponents<DealOfferedComponent>());
    }

    [TestMethod]
    public void IndicatorsShowEachConsumerAndEscapeDealText()
    {
        var game = CreateGame(Faction.Yellow, Faction.Green, Faction.Red);
        game.Deals.Add(new Deal { BoundFaction = Faction.Yellow, ConsumingFaction = Faction.Green, Text = "Share storm information", End = Phase.TurnConcluded });
        game.Deals.Add(new Deal { BoundFaction = Faction.Yellow, ConsumingFaction = Faction.Red, Text = "<img src=x onerror=alert(1)>", End = Phase.TurnConcluded });
        using var test = new ComponentTestContext(game, Faction.Yellow);

        var layout = DealIndicatorLayout.Create(game, test.Client.CurrentSkin);
        var rendered = test.RenderContext.Render<DealIndicators>(parameters => parameters
            .Add(value => value.Layout, layout));
        var indicators = rendered.FindAll(".map-deal-indicator");

        Assert.HasCount(2, indicators);
        Assert.HasCount(2, rendered.FindAll(".map-deal-link[aria-hidden=true]"));
        Assert.AreEqual(test.Client.CurrentSkin.GetImageUrl(Faction.Green), indicators[0].QuerySelector("img")!.GetAttribute("src"));
        Assert.AreEqual(test.Client.CurrentSkin.GetImageUrl(Faction.Red), indicators[1].QuerySelector("img")!.GetAttribute("src"));
        StringAssert.Contains(indicators[0].GetAttribute("aria-label")!, "Share storm information");
        StringAssert.Contains(indicators[1].GetAttribute("data-game-popover")!, "&lt;img");
        Assert.DoesNotContain("<img src=x", indicators[1].GetAttribute("data-game-popover")!);

        game.Deals.RemoveAt(0);
        rendered.Render(parameters => parameters.Add(value => value.Layout, DealIndicatorLayout.Create(game, test.Client.CurrentSkin)));
        Assert.HasCount(1, rendered.FindAll(".map-deal-indicator:not(.deal-leaving)"));
        Assert.HasCount(1, rendered.FindAll(".map-deal-indicator.deal-leaving[aria-hidden=true]"));
        Assert.AreEqual(test.Client.CurrentSkin.GetImageUrl(Faction.Red), rendered.Find(".map-deal-indicator:not(.deal-leaving) img").GetAttribute("src"));
    }

    [TestMethod]
    [DataRow(2, false)]
    [DataRow(3, true)]
    [DataRow(6, false)]
    [DataRow(6, true)]
    [DataRow(12, false)]
    public void IndicatorLayoutKeepsClearOfNamesTokensBidsAndEachOther(int playerCount, bool bidding)
    {
        var factions = Enum.GetValues<Faction>().Where(faction => faction != Faction.None).Take(playerCount).ToArray();
        var game = CreateGame(factions);
        foreach (var player in game.Players)
        {
            if (player.Seat / 2 * 2 + 1 < playerCount)
            {
                player.Ally = factions[player.Seat % 2 == 0 ? player.Seat + 1 : player.Seat - 1];
            }

            player.TechTokens.AddRange([TechToken.Graveyard, TechToken.Ships, TechToken.Resources]);
            player.Bribes = 1;
            player.ResourcesAfterBidding = 1;
            if (player.Faction == Faction.Pink)
            {
                player.Ambassadors.AddRange([Ambassador.Green, Ambassador.Red]);
            }
        }

        if (bidding)
        {
            SetPhase(game, Phase.Bidding);
            foreach (var player in game.Players)
            {
                game.Bids[player.Faction] = new Bid(game, player.Faction) { Amount = 10 + player.Seat };
            }
        }

        var dealsPerPlayer = playerCount > 6 ? 4 : 6;
        foreach (var owner in factions)
        {
            for (var index = 0; index < dealsPerPlayer; index++)
            {
                game.Deals.Add(new Deal { BoundFaction = owner, ConsumingFaction = factions[(index + 1) % factions.Length], End = Phase.TurnConcluded });
            }
        }

        var skin = DefaultSkin.Default;
        var ptr = skin.PlayerTokenRadius;
        var layout = DealIndicatorLayout.Create(game, skin);
        Assert.HasCount(playerCount * dealsPerPlayer, layout.Indicators);

        for (var i = 0; i < layout.Indicators.Count; i++)
        {
            var indicator = layout.Indicators[i];
            var center = new PointD(indicator.X + indicator.Size / 2, indicator.Y + indicator.Size / 2);
            var radius = indicator.Size / 2;
            var distance = Distance(center, skin.PlanetCenter);
            Assert.AreEqual(DealIndicatorLayout.IndicatorSize(ptr), indicator.Size, 0.001);
            Assert.IsGreaterThan(skin.PlanetRadius + 0.15 * ptr - 0.001, distance - radius, "Indicators must not cover territories.");
            Assert.IsLessThanOrEqualTo(layout.VisualRadius + 0.001, distance + radius);

            foreach (var player in game.Players)
            {
                var logo = PlayerTokenLayout.Position(game, skin, player.Seat);
                Assert.IsGreaterThan(1.5 * ptr + radius - 0.002, Distance(center, logo), "Indicator overlaps a logo.");
                if (player.HasAlly)
                {
                    Assert.IsGreaterThan(0.7 * ptr + radius - 0.002, Distance(center, new PointD(logo.X + 0.6 * ptr, logo.Y + 0.85 * ptr)), "Indicator overlaps an ally logo.");
                }

                foreach (var token in PlayerTokenLayout.PeripheralItems(game, skin, player))
                {
                    Assert.IsGreaterThan(token.Radius + radius - 0.002, Distance(center, token.Center), "Indicator overlaps an item in front of a shield.");
                }

                // Player name: 10px font, at most 6 PTR wide, above the top of a normal or enlarged logo.
                var nameHalfWidth = Math.Min(6 * ptr, player.Name.Length * 0.42 * 10 + 4) / 2;
                Assert.IsFalse(CircleHitsRectangle(center, radius, logo.X - nameHalfWidth, logo.Y - 1.5 * ptr - 4.8, logo.X + nameHalfWidth, logo.Y - ptr + 7.2),
                    $"Indicator {i} overlaps the name of {player.Faction}.");
            }

            for (var j = i + 1; j < layout.Indicators.Count; j++)
            {
                var other = layout.Indicators[j];
                Assert.IsGreaterThanOrEqualTo(2 * radius, Distance(center, new PointD(other.X + other.Size / 2, other.Y + other.Size / 2)) + 0.001,
                    $"Indicators {i} and {j} overlap.");
            }
        }
    }

    [TestMethod]
    public void TwoDealsSitCloseToTheirOwnerAndLeaveTheNameInPlace()
    {
        var game = CreateGame(Faction.Yellow, Faction.Green, Faction.Red);
        game.Deals.Add(new Deal { BoundFaction = Faction.Yellow, ConsumingFaction = Faction.Green, End = Phase.TurnConcluded });
        game.Deals.Add(new Deal { BoundFaction = Faction.Yellow, ConsumingFaction = Faction.Red, End = Phase.TurnConcluded });
        var skin = DefaultSkin.Default;
        var layout = DealIndicatorLayout.Create(game, skin);
        Assert.HasCount(2, layout.Indicators);
        var owner = PlayerTokenLayout.Position(game, skin, game.GetPlayer(Faction.Yellow)!.Seat);

        foreach (var indicator in layout.Indicators)
        {
            var distance = Distance(new PointD(indicator.X + indicator.Size / 2, indicator.Y + indicator.Size / 2), owner);
            Assert.IsLessThanOrEqualTo(2.5 * skin.PlayerTokenRadius, distance);
            Assert.IsLessThan(1.5 * skin.PlayerTokenRadius, indicator.Size);
        }

        using var test = new ComponentTestContext(game);
        test.Client.InGame.Returns(true);
        var players = test.RenderContext.Render<Players>();
        var names = players.FindComponents<MapText>().Where(text => text.Instance.ToShow == game.BotName).ToArray();
        Assert.HasCount(game.Players.Count, names);
        foreach (var player in game.Players)
        {
            var logo = PlayerTokenLayout.Position(game, skin, player.Seat);
            Assert.IsTrue(names.Any(name => Math.Abs(name.Instance.X - logo.X) < 0.001 && Math.Abs(name.Instance.Y - (logo.Y - skin.PlayerTokenRadius)) < 0.001),
                $"The name of {player.Faction} moved away from the top of its logo.");
        }
    }

    [TestMethod]
    public void DealsStayCloseToTheirOwnerWhenItemsAreInFrontOfTheShield()
    {
        var game = CreateGame(Faction.Green, Faction.Yellow, Faction.Red, Faction.Orange, Faction.Blue, Faction.Grey);
        SetPhase(game, Phase.Bidding);
        foreach (var player in game.Players)
        {
            player.TechTokens.Add(TechToken.Graveyard);
            player.Bribes = 10;
            game.Bids[player.Faction] = new Bid(game, player.Faction) { Amount = 1 };
        }

        game.Deals.Add(new Deal { BoundFaction = Faction.Green, ConsumingFaction = Faction.Yellow, End = Phase.TurnConcluded });
        game.Deals.Add(new Deal { BoundFaction = Faction.Green, ConsumingFaction = Faction.Red, End = Phase.TurnConcluded });
        var skin = DefaultSkin.Default;
        var layout = DealIndicatorLayout.Create(game, skin);
        Assert.HasCount(2, layout.Indicators);
        var owner = PlayerTokenLayout.Position(game, skin, game.GetPlayer(Faction.Green)!.Seat);

        foreach (var indicator in layout.Indicators)
        {
            var distance = Distance(new PointD(indicator.X + indicator.Size / 2, indicator.Y + indicator.Size / 2), owner);
            Assert.IsLessThanOrEqualTo(2.5 * skin.PlayerTokenRadius, distance, "A deal indicator was pushed away from its owner.");
        }

        game.Bids.Clear();
        var withoutBids = DealIndicatorLayout.Create(game, skin);
        CollectionAssert.AreEqual(layout.Indicators.Select(i => (i.X, i.Y)).ToArray(), withoutBids.Indicators.Select(i => (i.X, i.Y)).ToArray(),
            "Bid texts must not move deal indicators.");

        game.Bids[Faction.Green] = new Bid(game, Faction.Green) { Amount = 1 };
        using var test = new ComponentTestContext(game);
        var bidText = test.RenderContext.Render<Bids>().Find("span");
        StringAssert.Contains(bidText.GetAttribute("style")!, "pointer-events:none", "Bid texts must let indicators below them be hovered.");
    }

    [TestMethod]
    public void CenteredMapTextCentersItsGlyphsOnThePoint()
    {
        using var test = new ComponentTestContext(CreateGame(Faction.Green, Faction.Red));
        var centered = test.RenderContext.Render<MapText>(parameters => parameters
            .Add(value => value.ToShow, "10").Add(value => value.Font, "bold 8px Verdana").Add(value => value.CenterOnPoint, true)).Find("span");
        var style = centered.GetAttribute("style")!;
        Assert.AreEqual("map-text-centered", centered.GetAttribute("class"));
        StringAssert.Contains(style, "translateY(-50%)");
        Assert.IsGreaterThan(style.IndexOf("font:", StringComparison.Ordinal), style.IndexOf("line-height:1;", StringComparison.Ordinal),
            "The line height must follow the font shorthand, which would reset it.");

        var normal = test.RenderContext.Render<MapText>(parameters => parameters.Add(value => value.ToShow, "Name")).Find("span");
        StringAssert.Contains(normal.GetAttribute("style")!, "translateY(-40%)");
        Assert.IsNull(normal.GetAttribute("class"));
    }

    private static double Distance(PointD a, PointD b) => Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));

    private static bool CircleHitsRectangle(PointD center, double radius, double left, double top, double right, double bottom)
    {
        var dx = center.X - Math.Clamp(center.X, left, right);
        var dy = center.Y - Math.Clamp(center.Y, top, bottom);
        return dx * dx + dy * dy < radius * radius;
    }

    [TestMethod]
    public void PlayerLogosHaveGoldRimsAndEnlargeTheCurrentBidder()
    {
        var game = CreateGame(Faction.Green, Faction.Red, Faction.Yellow);
        SetPhase(game, Phase.Bidding);
        typeof(Game).GetProperty(nameof(Game.CurrentAuctionType))!.SetValue(game, AuctionType.Normal);
        typeof(Game).GetProperty(nameof(Game.BidSequence))!.SetValue(game, new PlayerSequence(game));
        var current = game.BidSequence!.CurrentPlayer;
        using var test = new ComponentTestContext(game);
        test.Client.InGame.Returns(true);
        test.Client.Status.Returns(GameStatus.DetermineStatus(game, current, true));
        var players = test.RenderContext.Render<Players>();

        Assert.HasCount(3, players.FindAll(".map-faction-logo[data-player-faction]"));
        Assert.AreEqual(current.Faction.ToString(), players.Find("[data-player-acting=true]").GetAttribute("data-player-faction"));
        foreach (var image in players.FindComponents<MapImage>().Where(image => image.Instance.AdditionalAttributes?.ContainsKey("data-player-faction") == true))
        {
            var isCurrent = image.Instance.AdditionalAttributes!["data-player-faction"].ToString() == current.Faction.ToString();
            Assert.AreEqual((isCurrent ? 3 : 2) * test.Client.CurrentSkin.PlayerTokenRadius, image.Instance.Width);
        }
    }

    private static Game CreateGame(params Faction[] factions)
    {
        var game = new Game();
        game.Settings.NumberOfPlayers = factions.Length;
        for (var seat = 0; seat < factions.Length; seat++)
        {
            game.Players.Add(new Player(game, factions[seat]) { Seat = seat, Resources = 20 });
        }

        SetPhase(game, Phase.Contemplate);
        return game;
    }

    private static void SetPhase(Game game, Phase phase) =>
        typeof(Game).GetProperty(nameof(Game.CurrentPhase))!.SetValue(game, phase);
}
