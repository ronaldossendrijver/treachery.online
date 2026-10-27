using System.IO;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe;
using Treachery.Server;

namespace Treachery.Test;

[TestClass]
public class MembershipTests
{
    private static int nextUserId = 500000;
    private const string PasswordHash = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
    private const string WebhookSecret = "whsec_test";
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
            ["GameAdminUsername"] = "membership-admin",
            ["StripeWebhookSecret"] = WebhookSecret
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

    private async Task<string> LinkStripeCustomer(LoginInfo account)
    {
        var customerId = "cus_" + Guid.NewGuid().ToString("N");
        await using var db = CreateContext();
        var user = db.Users.Single(u => u.Id == account.UserId);
        user.StripeCustomerId = customerId;
        await db.SaveChangesAsync();
        return customerId;
    }

    internal static Event InvoicePaid(string customerId, DateTime periodEnd) => new()
    {
        Type = MembershipService.InvoicePaid,
        Data = new EventData
        {
            Object = new Invoice
            {
                CustomerId = customerId,
                Lines = new StripeList<InvoiceLineItem>
                {
                    Data = [new InvoiceLineItem { Period = new InvoiceLineItemPeriod { Start = periodEnd.AddMonths(-1), End = periodEnd } }]
                }
            }
        }
    };

    internal static Event SubscriptionDeleted(string customerId, DateTime endedAt) => new()
    {
        Type = MembershipService.SubscriptionDeleted,
        Data = new EventData { Object = new Subscription { CustomerId = customerId, EndedAt = endedAt } }
    };

    private DateTimeOffset? StoredMembership(int userId)
    {
        using var db = CreateContext();
        return db.Users.Single(u => u.Id == userId).MembershipValidUntil;
    }

    [TestMethod]
    public async Task PaidInvoiceExtendsMembershipAndUpdatesLoggedInUser()
    {
        var account = await CreateAccount();
        var customerId = await LinkStripeCustomer(account);
        var periodEnd = DateTime.UtcNow.AddDays(30);

        await using (var db = CreateContext())
            Assert.AreEqual(account.UserId, await new MembershipService(db).HandleEventAsync(InvoicePaid(customerId, periodEnd)));

        var expected = new DateTimeOffset(periodEnd, TimeSpan.Zero) + MembershipService.GracePeriod;
        Assert.AreEqual(expected, StoredMembership(account.UserId));
        var loginInfo = await hub.GetLoginInfo(account.Token);
        Assert.AreEqual(expected, loginInfo.Contents!.MembershipValidUntil);
        Assert.IsTrue(loginInfo.Contents.HasStripeCustomer);
    }

    [TestMethod]
    public async Task OlderOrDuplicateInvoiceDoesNotShortenMembership()
    {
        var account = await CreateAccount();
        var customerId = await LinkStripeCustomer(account);
        var periodEnd = DateTime.UtcNow.AddDays(60);

        await using (var db = CreateContext())
        {
            var service = new MembershipService(db);
            await service.HandleEventAsync(InvoicePaid(customerId, periodEnd));
            await service.HandleEventAsync(InvoicePaid(customerId, periodEnd));
            await service.HandleEventAsync(InvoicePaid(customerId, periodEnd.AddDays(-30)));
        }

        Assert.AreEqual(new DateTimeOffset(periodEnd, TimeSpan.Zero) + MembershipService.GracePeriod, StoredMembership(account.UserId));
    }

    [TestMethod]
    public async Task DeletedSubscriptionEndsMembership()
    {
        var account = await CreateAccount();
        var customerId = await LinkStripeCustomer(account);
        var endedAt = DateTime.UtcNow.AddMinutes(-1);

        await using (var db = CreateContext())
        {
            var service = new MembershipService(db);
            await service.HandleEventAsync(InvoicePaid(customerId, DateTime.UtcNow.AddDays(30)));
            await service.HandleEventAsync(SubscriptionDeleted(customerId, endedAt));
        }

        Assert.AreEqual(new DateTimeOffset(endedAt, TimeSpan.Zero), StoredMembership(account.UserId));
        var loginInfo = await hub.GetLoginInfo(account.Token);
        Assert.IsFalse(Membership.IsActive(loginInfo.Contents!.MembershipValidUntil));
    }

    [TestMethod]
    public async Task EventsForUnknownCustomersAndOtherTypesAreIgnored()
    {
        await using var db = CreateContext();
        var service = new MembershipService(db);
        Assert.IsNull(await service.HandleEventAsync(InvoicePaid("cus_unknown", DateTime.UtcNow.AddDays(30))));
        Assert.IsNull(await service.HandleEventAsync(new Event { Type = "customer.created", Data = new EventData { Object = new Customer() } }));
    }

    private static HttpRequest WebhookRequest(string json, string signature)
    {
        var context = new DefaultHttpContext();
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        context.Request.Headers["Stripe-Signature"] = signature;
        return context.Request;
    }

    private static string InvoicePaidJson(string customerId, long periodEnd) => $$"""
        {
          "id": "evt_test", "object": "event", "type": "invoice.paid", "api_version": "2020-01-01",
          "data": { "object": {
            "id": "in_test", "object": "invoice", "customer": "{{customerId}}",
            "lines": { "object": "list", "data": [ { "id": "il_test", "object": "line_item", "period": { "start": {{periodEnd - 2592000}}, "end": {{periodEnd}} } } ] }
          } }
        }
        """;

    [TestMethod]
    public async Task WebhookAppliesSignedEventsAndRejectsInvalidSignatures()
    {
        var account = await CreateAccount();
        var customerId = await LinkStripeCustomer(account);
        var periodEnd = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds();
        var json = InvoicePaidJson(customerId, periodEnd);

        await using (var db = CreateContext())
        {
            var forged = await MembershipService.HandleWebhookAsync(
                WebhookRequest(json, EventUtility.GenerateSignatureHeader(json, "whsec_wrong")), configuration, db);
            Assert.AreEqual(StatusCodes.Status400BadRequest, (forged as IStatusCodeHttpResult)?.StatusCode);
        }
        Assert.IsNull(StoredMembership(account.UserId));

        await using (var db = CreateContext())
        {
            var signed = await MembershipService.HandleWebhookAsync(
                WebhookRequest(json, EventUtility.GenerateSignatureHeader(json, WebhookSecret)), configuration, db);
            Assert.AreEqual(StatusCodes.Status200OK, (signed as IStatusCodeHttpResult)?.StatusCode);
        }
        Assert.AreEqual(DateTimeOffset.FromUnixTimeSeconds(periodEnd) + MembershipService.GracePeriod, StoredMembership(account.UserId));
    }

    private async Task<(LoginInfo Host, string GameId, VoidResult Result)> EstablishGame(bool advancedBots, string? hostUserName = null)
    {
        var host = await CreateAccount(hostUserName);
        var created = await hub.RequestCreateGame("Game", host.Token, "", "", "");
        Assert.IsTrue(created.Success, created.Error.ToString());
        var settings = new GameSettings
        {
            NumberOfPlayers = 3,
            AllowedFactionsInPlay = [Faction.Green, Faction.Black, Faction.Yellow],
            InitialRules = advancedBots ? [Rule.AdvancedBots] : []
        };
        var result = await hub.RequestEstablishPlayers(host.Token, created.Contents!.GameId, new EstablishPlayers { Settings = settings, Seed = 1 });
        return (host, created.Contents.GameId, result);
    }

    [TestMethod]
    public async Task AdvancedBotsAreAvailableToEveryoneWhileMembershipIsNotRequired()
    {
        var (_, _, result) = await EstablishGame(advancedBots: true);
        Assert.IsTrue(result.Success, result.Error.ToString());
        Assert.IsFalse((await hub.Connect()).Contents!.MembershipRequired);
    }

    [TestMethod]
    public async Task AdvancedBotsRequireMembershipWhenEnabled()
    {
        configuration["MembershipRequired"] = "true";
        Assert.IsTrue((await hub.Connect()).Contents!.MembershipRequired);

        var (_, _, nonMember) = await EstablishGame(advancedBots: true);
        Assert.AreEqual(ErrorType.MembershipRequired, nonMember.Error);

        var (_, _, basicBots) = await EstablishGame(advancedBots: false);
        Assert.IsTrue(basicBots.Success, basicBots.Error.ToString());

        var (_, _, admin) = await EstablishGame(advancedBots: true, hostUserName: "membership-admin");
        Assert.IsTrue(admin.Success, admin.Error.ToString());
    }

    [TestMethod]
    public async Task ActiveMemberCanUseAdvancedBots()
    {
        configuration["MembershipRequired"] = "true";
        var member = await CreateAccount();
        var customerId = await LinkStripeCustomer(member);
        await using (var db = CreateContext())
            await new MembershipService(db).HandleEventAsync(InvoicePaid(customerId, DateTime.UtcNow.AddDays(30)));

        var created = await hub.RequestCreateGame("Game", member.Token, "", "", "");
        var result = await hub.RequestEstablishPlayers(member.Token, created.Contents!.GameId, new EstablishPlayers
        {
            Settings = new GameSettings
            {
                NumberOfPlayers = 3,
                AllowedFactionsInPlay = [Faction.Green, Faction.Black, Faction.Yellow],
                InitialRules = [Rule.AdvancedBots]
            },
            Seed = 1
        });
        Assert.IsTrue(result.Success, result.Error.ToString());
    }

    [TestMethod]
    public async Task NonMemberCannotLoadGameWithAdvancedBots()
    {
        var (host, gameId, established) = await EstablishGame(advancedBots: true);
        Assert.IsTrue(established.Success, established.Error.ToString());
        var state = (await hub.RequestGameState(host.Token, gameId)).Contents!.GameState;

        configuration["MembershipRequired"] = "true";
        var nonMember = await CreateAccount();

        var create = await hub.RequestCreateGame("Loaded", nonMember.Token, "", state, "");
        Assert.AreEqual(ErrorType.MembershipRequired, create.Error);

        var ownGame = await hub.RequestCreateGame("Own", nonMember.Token, "", "", "");
        var load = await hub.RequestLoadGame(nonMember.Token, ownGame.Contents!.GameId, state, "");
        Assert.AreEqual(ErrorType.MembershipRequired, load.Error);
    }
}
