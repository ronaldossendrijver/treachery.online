using System;
using System.Threading.Tasks;
using Treachery.Server;

namespace Treachery.Test;

[TestClass]
public sealed class BotStallRecoveryTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 1, 10, 0, 0, TimeSpan.Zero);

    [TestMethod]
    [DataRow(Phase.Bidding)]
    [DataRow(Phase.NonOrangeShip)]
    [DataRow(Phase.NonOrangeMove)]
    [DataRow(Phase.SelectingTraitors)]
    public async Task NudgesWaitingBotsIncludingSequencesAndMixedHumanDecisions(Phase phase)
    {
        var managed = CreateGame(phase);
        var attempts = 0;

        var nudged = await managed.NudgeBotsIfStalledAsync(Now, () =>
        {
            attempts++;
            return Task.CompletedTask;
        });

        Assert.IsTrue(nudged);
        Assert.AreEqual(1, attempts);
        Assert.AreEqual(Now, managed.LastBotAttempt);
    }

    [TestMethod]
    public async Task RetriesEveryTwelveSecondsWhenNudgeMakesNoProgress()
    {
        var managed = CreateGame();
        var attempts = 0;
        Task Nudge()
        {
            attempts++;
            return Task.CompletedTask;
        }

        Assert.IsTrue(await managed.NudgeBotsIfStalledAsync(Now, Nudge));
        Assert.IsFalse(await managed.NudgeBotsIfStalledAsync(Now.AddMilliseconds(11999), Nudge));
        Assert.IsTrue(await managed.NudgeBotsIfStalledAsync(Now.AddSeconds(12), Nudge));
        Assert.AreEqual(2, attempts);
    }

    [TestMethod]
    [DataRow("activity")]
    [DataRow("attempt")]
    [DataRow("event")]
    public async Task WaitsTwelveSecondsAfterActivityOrBotAttempt(string kind)
    {
        var managed = CreateGame();
        var recent = Now.AddMilliseconds(-11999);
        if (kind == "activity")
            managed.LastActivity = recent;
        else if (kind == "attempt")
            managed.LastBotAttempt = recent;
        else
            managed.Game.History.Add(new Bid(managed.Game, Faction.Green) { Time = recent });

        Assert.IsFalse(await managed.NudgeBotsIfStalledAsync(Now, UnexpectedNudge));
        Assert.IsTrue(await managed.NudgeBotsIfStalledAsync(Now.AddMilliseconds(1), () => Task.CompletedTask));
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(4)]
    public async Task PreservesSelectedSpeed(int speed)
    {
        var managed = CreateGame();
        managed.Game.Participation.BotsSpeed = speed;

        Assert.IsTrue(await managed.NudgeBotsIfStalledAsync(Now, () => Task.CompletedTask));
        Assert.AreEqual(speed, managed.Game.Participation.BotsSpeed);
    }

    [TestMethod]
    public async Task DoesNotNudgePausedUnloadedUnstartedOrEndedGames()
    {
        var paused = CreateGame();
        paused.Game.Participation.BotsSpeed = 0;
        Assert.IsFalse(await paused.NudgeBotsIfStalledAsync(Now, UnexpectedNudge));
        Assert.AreEqual(0, paused.Game.Participation.BotsSpeed);
        Assert.IsFalse(await new ManagedGame().NudgeBotsIfStalledAsync(Now, UnexpectedNudge));
        Assert.IsFalse(await CreateGame(Phase.AwaitingPlayers).NudgeBotsIfStalledAsync(Now, UnexpectedNudge));

        var ended = CreateGame();
        ended.Game.CurrentMainPhase = MainPhase.Ended;
        Assert.IsFalse(await ended.NudgeBotsIfStalledAsync(Now, UnexpectedNudge));
    }

    [TestMethod]
    [DataRow(Phase.Bidding)]
    [DataRow(Phase.NonOrangeShip)]
    [DataRow(Phase.NonOrangeMove)]
    public async Task DoesNotNudgeWhenOnlyCurrentHumanMustAct(Phase phase)
    {
        var managed = CreateGame(phase);
        managed.Game.Participation.SeatedPlayers[1] = 0;
        managed.Game.Participation.SeatedPlayers.Remove(2);

        Assert.IsFalse(await managed.NudgeBotsIfStalledAsync(Now, UnexpectedNudge));
    }

    [TestMethod]
    public async Task OnlyNudgesHostPhasesWhenBotsMayActAsHosts()
    {
        var managed = CreateGame(Phase.BeginningOfBidding);
        managed.Game.Participation.Hosts.Add(1);
        managed.Game.Participation.Observers.Add(1);
        Assert.IsFalse(await managed.NudgeBotsIfStalledAsync(Now, UnexpectedNudge));

        managed.Game.Participation.Hosts.Clear();
        managed.Game.Participation.Observers.Clear();
        Assert.IsFalse(await managed.NudgeBotsIfStalledAsync(Now, UnexpectedNudge));

        managed.Game.Participation.Observers.Add(1);
        Assert.IsTrue(await managed.NudgeBotsIfStalledAsync(Now, () => Task.CompletedTask));
    }

    [TestMethod]
    public async Task SkipsBusyGamesWithoutQueuingDuplicateActions()
    {
        var managed = CreateGame();
        var entered = new TaskCompletionSource();
        var release = new TaskCompletionSource();
        var action = managed.ProcessEventAsync(async () =>
        {
            entered.SetResult();
            await release.Task;
            return true;
        });
        await entered.Task;
        try
        {
            Assert.IsFalse(await managed.NudgeBotsIfStalledAsync(Now, UnexpectedNudge));
        }
        finally
        {
            release.SetResult();
            await action;
        }

        Assert.IsTrue(await managed.NudgeBotsIfStalledAsync(Now, () => Task.CompletedTask));
    }

    [TestMethod]
    public async Task ConcurrentChecksOnlyRunOneNudge()
    {
        var managed = CreateGame();
        var entered = new TaskCompletionSource();
        var release = new TaskCompletionSource();
        var first = managed.NudgeBotsIfStalledAsync(Now, async () =>
        {
            entered.SetResult();
            await release.Task;
        });
        await entered.Task;
        try
        {
            Assert.IsFalse(await managed.NudgeBotsIfStalledAsync(Now, UnexpectedNudge));
        }
        finally
        {
            release.SetResult();
            Assert.IsTrue(await first);
        }

        Assert.IsFalse(await managed.NudgeBotsIfStalledAsync(Now, UnexpectedNudge));
    }

    [TestMethod]
    public async Task SurfacesNudgeFailuresAndAllowsRetryAfterCooldown()
    {
        var managed = CreateGame();

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
            managed.NudgeBotsIfStalledAsync(Now, () => throw new InvalidOperationException("Bot failed")));
        Assert.IsFalse(await managed.NudgeBotsIfStalledAsync(Now.AddSeconds(11), UnexpectedNudge));
        Assert.IsTrue(await managed.NudgeBotsIfStalledAsync(Now.AddSeconds(12), () => Task.CompletedTask));
    }

    private static Task UnexpectedNudge() => throw new AssertFailedException("Bots should not be nudged.");

    private static ManagedGame CreateGame(Phase phase = Phase.Bidding)
    {
        var game = new Game();
        game.Settings.NumberOfPlayers = 2;
        game.Players.Add(new Player(game, Faction.Green) { Seat = 0 });
        game.Players.Add(new Player(game, Faction.Red) { Seat = 1 });
        game.Participation.SeatedPlayers[2] = 1;
        typeof(Game).GetProperty(nameof(Game.CurrentPhase))!.SetValue(game, phase);
        typeof(Game).GetProperty(nameof(Game.CurrentAuctionType))!.SetValue(game, AuctionType.Normal);
        typeof(Game).GetProperty(nameof(Game.BidSequence))!.SetValue(game, new PlayerSequence(game));
        typeof(Game).GetProperty(nameof(Game.ShipmentAndMoveSequence))!.SetValue(game, new PlayerSequence(game));
        return new ManagedGame { Game = game, LastActivity = Now.AddSeconds(-30) };
    }
}
