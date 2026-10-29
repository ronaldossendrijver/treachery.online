using System.Threading;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Treachery.Server;

namespace Treachery.Test;

[TestClass]
public class UsageLogTests
{
    private static int nextUserId = 600000;
    private const string PasswordHash = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
    private const string AdminName = "usage-admin";
    private SqliteConnection connection = null!;
    private IConfiguration configuration = null!;
    private DbContextOptions<TreacheryContext> options = null!;
    private GameHub hub = null!;

    [TestInitialize]
    public void Initialize()
    {
        connection = new SqliteConnection($"Data Source={Guid.NewGuid():N};Mode=Memory;Cache=Shared");
        connection.Open();
        configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:TreacheryDatabase"] = connection.ConnectionString,
            ["GameAdminUsername"] = AdminName
        }).Build();
        options = new DbContextOptionsBuilder<TreacheryContext>().UseSqlite(connection.ConnectionString).Options;
        using var db = CreateContext();
        db.Database.EnsureCreated();
        db.Database.ExecuteSqlRaw("INSERT INTO sqlite_sequence(name,seq) VALUES ('Users',{0})",
            Interlocked.Add(ref nextUserId, 100));
        hub = new GameHub(options, configuration, new GameInfoSummaryCache())
        {
            Context = Substitute.For<HubCallerContext>(),
            Groups = Substitute.For<IGroupManager>(),
            Clients = Substitute.For<IHubCallerClients<IGameClient>>()
        };
        hub.Context.ConnectionId.Returns(Guid.NewGuid().ToString());
        hub.Clients.Group(Arg.Any<string>()).Returns(Substitute.For<IGameClient>());
    }

    [TestCleanup]
    public void Cleanup()
    {
        hub.Dispose();
        connection.Dispose();
    }

    private TreacheryContext CreateContext() => new(options, configuration);

    private async Task<LoginInfo> CreateAccount(string? userName = null)
    {
        var result = await hub.RequestCreateUser(userName ?? Guid.NewGuid().ToString("N"), PasswordHash, "player@example.com", "Player");
        Assert.IsTrue(result.Success, result.Error.ToString());
        return result.Contents!;
    }

    private List<UsageEvent> Events(int? userId = null, string? gameId = null)
    {
        using var db = CreateContext();
        return db.UsageEvents.AsNoTracking()
            .Where(e => (userId == null || e.UserId == userId) && (gameId == null || e.GameId == gameId))
            .OrderBy(e => e.Id)
            .ToList();
    }

    private static UsageEvent Usage(UsageEventType type, DateTime occurredAt, int? userId, UsageData? data = null) => new()
    {
        Type = type,
        OccurredAt = occurredAt,
        UserId = userId,
        Data = data == null ? null : JsonSerializer.Serialize(data)
    };

    [TestMethod]
    public async Task RegistrationAndLoginsAreRecordedAndActivityOncePerDay()
    {
        var userName = Guid.NewGuid().ToString("N");
        var account = await CreateAccount(userName);

        Assert.IsTrue((await hub.RequestLogin(Game.LatestVersion, userName, PasswordHash)).Success);
        Assert.IsTrue((await hub.RequestLogin(Game.LatestVersion, userName, PasswordHash)).Success);
        await hub.GetLoginInfo(account.Token);
        await hub.GetLoginInfo(account.Token);

        var types = Events(account.UserId).Select(e => e.Type).ToList();
        CollectionAssert.AreEqual(new[]
        {
            UsageEventType.UserRegistered,
            UsageEventType.Login,
            UsageEventType.UserActive,
            UsageEventType.Login
        }, types);
    }

    [TestMethod]
    public async Task GameLifecycleIsRecordedWithGameDetails()
    {
        var host = await CreateAccount();
        var created = await hub.RequestCreateGame("Game", host.Token, "", "", "");
        Assert.IsTrue(created.Success, created.Error.ToString());
        var gameId = created.Contents!.GameId;

        var started = await hub.RequestEstablishPlayers(host.Token, gameId, new EstablishPlayers
        {
            Settings = new GameSettings
            {
                NumberOfPlayers = 3,
                AllowedFactionsInPlay = [Faction.Green, Faction.Black, Faction.Yellow],
                InitialRules = [Rule.AdvancedBots]
            },
            Seed = 1
        });
        Assert.IsTrue(started.Success, started.Error.ToString());
        Assert.IsTrue((await hub.RequestCloseGame(host.Token, gameId)).Success);

        var events = Events(gameId: gameId);
        CollectionAssert.AreEqual(new[] { UsageEventType.GameCreated, UsageEventType.GameStarted, UsageEventType.GameClosed },
            events.Select(e => e.Type).ToArray());
        Assert.IsTrue(events.All(e => e.UserId == host.UserId));

        var startData = JsonSerializer.Deserialize<UsageData>(events[1].Data!)!;
        Assert.AreEqual(1, startData.Players);
        Assert.AreEqual(2, startData.Bots);
        Assert.IsTrue(startData.AdvancedBots);
        Assert.AreEqual(3, startData.MaximumPlayers);
        Assert.IsNotNull(startData.MaximumTurns);
        Assert.AreEqual(2, startData.BotFactions!.Split(';').Length);
        Assert.AreEqual(1, startData.HumanFactions!.Split(';').Length);
        CollectionAssert.AreEquivalent(
            new[] { Faction.Green, Faction.Black, Faction.Yellow },
            startData.BotFactions.Split(';').Concat(startData.HumanFactions.Split(';')).Select(Enum.Parse<Faction>).ToArray());
        Assert.IsTrue(startData.BotRules!.Split(';').Contains(nameof(Rule.AdvancedBots)));
    }

    [TestMethod]
    public void OversizedDataDropsRulesButStaysValidJson()
    {
        var json = UsageLog.Serialize(new UsageData
        {
            Players = 2,
            BotFactions = "Green;Black",
            Rules = string.Join(";", Enumerable.Repeat(nameof(Rule.AdvancedBots), 500))
        });

        Assert.IsTrue(json.Length <= UsageLog.MaximumDataLength);
        var data = JsonSerializer.Deserialize<UsageData>(json)!;
        Assert.AreEqual(2, data.Players);
        Assert.AreEqual("Green;Black", data.BotFactions);
        Assert.IsNull(data.Rules);
    }

    [TestMethod]
    public void StatisticsAreAggregatedPerPeriodIncludingEmptyPeriods()
    {
        var monday = new DateTime(2026, 10, 19, 0, 0, 0, DateTimeKind.Utc);
        var events = new[]
        {
            Usage(UsageEventType.Login, monday.AddHours(10), 1),
            Usage(UsageEventType.Login, monday.AddHours(11), 1),
            Usage(UsageEventType.GameStarted, monday.AddHours(12), 2, new UsageData { Players = 2, Bots = 1, AdvancedBots = true }),
            Usage(UsageEventType.GameStarted, monday.AddDays(2), 3, new UsageData { Players = 4, Bots = 0, AdvancedBots = false, AsyncPlay = true }),
            Usage(UsageEventType.GameFinished, monday.AddDays(8), null, new UsageData { Turn = 10, DurationMinutes = 180 })
        };
        var from = new DateTimeOffset(monday);
        var to = from.AddDays(14);

        var daily = UsageLog.Compute(events, from, to, UsageGranularity.Day);
        Assert.AreEqual(14, daily.Periods.Length);
        Assert.AreEqual(2, daily.Periods[0].Count(UsageEventType.Login));
        Assert.AreEqual(2, daily.Periods[0].ActiveUsers);
        Assert.AreEqual(0, daily.Periods[1].ActiveUsers);

        var weekly = UsageLog.Compute(events, from, to, UsageGranularity.Week);
        Assert.AreEqual(2, weekly.Periods.Length);
        Assert.AreEqual(2, weekly.Periods[0].Count(UsageEventType.GameStarted));
        Assert.AreEqual(1, weekly.Periods[0].GamesWithBots);
        Assert.AreEqual(1, weekly.Periods[0].GamesWithAdvancedBots);
        Assert.AreEqual(1, weekly.Periods[0].AsyncGames);
        Assert.AreEqual(3.0, weekly.Periods[0].AveragePlayers);
        Assert.AreEqual(1, weekly.Periods[1].Count(UsageEventType.GameFinished));

        Assert.AreEqual(3, weekly.Total.ActiveUsers);
        Assert.AreEqual(10.0, weekly.Total.AverageTurns);
        Assert.AreEqual(3.0, weekly.Total.AverageDurationHours);
    }

    [TestMethod]
    public void CsvQuotesData()
    {
        var csv = UsageLog.ToCsv([new UsageEvent
        {
            Type = UsageEventType.GameStarted,
            OccurredAt = new DateTime(2026, 1, 2, 3, 4, 5),
            UserId = 7,
            Data = "{\"Players\":2}"
        }]);
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.AreEqual("OccurredAtUtc,Type,UserId,GameId,Data", lines[0]);
        Assert.AreEqual("2026-01-02 03:04:05,GameStarted,7,,\"{\"\"Players\"\":2}\"", lines[1]);
    }

    [TestMethod]
    public async Task OnlyAdminCanRetrieveStatistics()
    {
        var player = await CreateAccount();
        var admin = await CreateAccount(AdminName);
        var from = DateTimeOffset.UtcNow.AddDays(-1);
        var to = DateTimeOffset.UtcNow.AddDays(1);

        Assert.IsFalse((await hub.GetAdminUsageStats(player.Token, from, to, UsageGranularity.Day)).Success);
        Assert.IsFalse((await hub.GetAdminUsageCsv(player.Token, from, to)).Success);

        var stats = await hub.GetAdminUsageStats(admin.Token, from, to, UsageGranularity.Day);
        Assert.IsTrue(stats.Success, stats.Error.ToString());
        Assert.IsTrue(stats.Contents!.Total.Count(UsageEventType.UserRegistered) >= 2);

        var csv = await hub.GetAdminUsageCsv(admin.Token, from, to);
        Assert.IsTrue(csv.Success);
        StringAssert.Contains(csv.Contents, "UserRegistered");
    }

    [TestMethod]
    public async Task MembershipChangesAreRecorded()
    {
        var account = await CreateAccount();
        var customerId = "cus_" + Guid.NewGuid().ToString("N");
        await using (var db = CreateContext())
        {
            db.Users.Single(u => u.Id == account.UserId).StripeCustomerId = customerId;
            await db.SaveChangesAsync();
        }

        await using (var db = CreateContext())
        {
            var service = new MembershipService(db);
            await service.HandleEventAsync(MembershipTests.InvoicePaid(customerId, DateTime.UtcNow.AddDays(30)));
            await service.HandleEventAsync(MembershipTests.InvoicePaid(customerId, DateTime.UtcNow.AddDays(60)));
            await service.HandleEventAsync(MembershipTests.SubscriptionDeleted(customerId, DateTime.UtcNow));
        }

        CollectionAssert.AreEqual(new[]
            {
                UsageEventType.MembershipActivated,
                UsageEventType.MembershipRenewed,
                UsageEventType.MembershipEnded
            },
            Events(account.UserId).Where(e => e.Type.ToString().StartsWith("Membership")).Select(e => e.Type).ToArray());
    }
}
