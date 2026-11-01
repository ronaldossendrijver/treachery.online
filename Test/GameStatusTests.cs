using System.Reflection;
using Treachery.Shared;

namespace Treachery.Test;

[TestClass]
public sealed class GameStatusTests
{
    [TestMethod]
    [DataRow(Phase.Bidding, AuctionType.Normal)]
    [DataRow(Phase.Bidding, AuctionType.WhiteOnceAround)]
    [DataRow(Phase.BlackMarketBidding, AuctionType.BlackMarketNormal)]
    [DataRow(Phase.BlackMarketBidding, AuctionType.BlackMarketOnceAround)]
    public void BiddingHighlightsOnlyTheCurrentPlayerAndFollowsTheNextBid(Phase phase, AuctionType auction)
    {
        var game = CreateBiddingGame(phase, auction);
        var first = game.BidSequence!.CurrentPlayer;
        var status = GameStatus.DetermineStatus(game, first, true);
        Assert.AreEqual(first, game.Players.Single(status.PlayerShouldBeHighlighted));
        Assert.AreEqual(first, game.Players.Single(player => status.WaitingForMe(player, false)));

        game.BidSequence.NextPlayer();
        var next = game.BidSequence.CurrentPlayer;
        Assert.AreNotEqual(first, next);
        status = GameStatus.DetermineStatus(game, null, false);
        Assert.AreEqual(next, game.Players.Single(status.PlayerShouldBeHighlighted));
    }

    [TestMethod]
    [DataRow(Phase.Bidding, AuctionType.WhiteSilent)]
    [DataRow(Phase.BlackMarketBidding, AuctionType.BlackMarketSilent)]
    public void SilentBiddingHighlightsAllPlayersWhoStillNeedToBid(Phase phase, AuctionType auction)
    {
        var game = CreateBiddingGame(phase, auction);
        var submitted = game.Players[0];
        game.Bids.Add(submitted.Faction, new Bid(game, submitted.Faction) { Passed = true });
        var status = GameStatus.DetermineStatus(game, null, false);
        Assert.IsFalse(status.PlayerShouldBeHighlighted(submitted));
        Assert.HasCount(2, game.Players.Where(status.PlayerShouldBeHighlighted).ToList());
    }

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
        Assert.IsTrue(status.PlayerShouldBeHighlighted(player));
    }

    private static Game CreateBiddingGame(Phase phase, AuctionType auction)
    {
        var game = new Game();
        game.Settings.NumberOfPlayers = 3;
        var factions = new[] { Faction.Green, Faction.Red, Faction.Yellow };
        for (var seat = 0; seat < factions.Length; seat++)
        {
            game.Players.Add(new Player(game, factions[seat]) { Seat = seat });
        }

        typeof(Game).GetProperty(nameof(Game.CurrentPhase))!.SetValue(game, phase);
        typeof(Game).GetProperty(nameof(Game.CurrentAuctionType))!.SetValue(game, auction);
        typeof(Game).GetProperty(nameof(Game.BidSequence))!.SetValue(game, new PlayerSequence(game));
        return game;
    }
}