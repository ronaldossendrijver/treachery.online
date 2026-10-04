using System.Threading;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Treachery.Server;

namespace Treachery.Test;

[TestClass]
public class GameHubInputValidationTests
{
    private static int nextUserId = 100000;
    private SqliteConnection connection = null!;
    private IConfiguration configuration = null!;
    private DbContextOptions<TreacheryContext> options = null!;
    private GameHub hub = null!;
    private const string PasswordHash = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

    [TestInitialize]
    public void Initialize()
    {
        connection = new SqliteConnection($"Data Source={Guid.NewGuid():N};Mode=Memory;Cache=Shared");
        connection.Open();
        configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:TreacheryDatabase"] = connection.ConnectionString
        }).Build();
        options = new DbContextOptionsBuilder<TreacheryContext>().UseSqlite(connection.ConnectionString).Options;
        using var db = CreateContext();
        db.Database.EnsureCreated();
        db.Database.ExecuteSqlRaw("INSERT INTO sqlite_sequence(name,seq) VALUES ('Users',{0})",
            Interlocked.Add(ref nextUserId, 100));
        hub = new GameHub(options, configuration)
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

    private async Task<LoginInfo> CreateAccount(string playerName = "Player")
    {
        var result = await hub.RequestCreateUser(Guid.NewGuid().ToString("N"), PasswordHash,
            "player@example.com", playerName);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Contents);
        return result.Contents;
    }

    private static string? InvalidValue(string kind) => kind switch
    {
        "null" => null,
        "empty" => "",
        "short" => " abc ",
        "longName" => new string('x', 41),
        "hugeName" => new string('x', 34965),
        "longEmail" => BoundaryEmail(255),
        "badEmail" => "not-an-email",
        "displayEmail" => "Player <player@example.com>",
        "controlEmail" => "play\r\ner@example.com",
        "shortHash" => new string('a', 63),
        "longHash" => new string('a', 65),
        "nonHexHash" => new string('z', 64),
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    private static string BoundaryEmail(int length) =>
        $"{new string('a', 64)}@{new string('b', 63)}.{new string('c', 63)}.{new string('d', length - 197)}.com";

    [TestMethod]
    public async Task AdminErrorLogRejectsAnonymousRequestWithoutThrowing()
    {
        var result = await hub.GetAdminErrorLog(null!, null, null, null, null);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ErrorType.InvalidUserNameOrPassword, result.Error);
    }
    [TestMethod]
    public async Task CreationAndUpdateAcceptEmailAt254CharacterBoundary()
    {
        var email = BoundaryEmail(254);
        Assert.AreEqual(254, email.Length);
        var created = await hub.RequestCreateUser(Guid.NewGuid().ToString("N"), PasswordHash, email, "Player");
        Assert.IsTrue(created.Success);
        Assert.IsNotNull(created.Contents);
        var updatedEmail = email.Replace('a', 'e');
        var updated = await hub.RequestUpdateUserInfo(created.Contents.Token, "", "Player", updatedEmail);
        Assert.IsTrue(updated.Success);
        using var db = CreateContext();
        Assert.AreEqual(updatedEmail, db.Users.Single().Email);
    }

    [TestMethod]
    [DataRow("username", "null", ErrorType.UserNameTooShort)]
    [DataRow("username", "short", ErrorType.UserNameTooShort)]
    [DataRow("username", "longName", ErrorType.UserNameTooLong)]
    [DataRow("player", "null", ErrorType.PlayerNameTooShort)]
    [DataRow("player", "short", ErrorType.PlayerNameTooShort)]
    [DataRow("player", "longName", ErrorType.PlayerNameTooLong)]
    [DataRow("player", "hugeName", ErrorType.PlayerNameTooLong)]
    [DataRow("email", "null", ErrorType.InvalidEmail)]
    [DataRow("email", "badEmail", ErrorType.InvalidEmail)]
    [DataRow("email", "displayEmail", ErrorType.InvalidEmail)]
    [DataRow("email", "controlEmail", ErrorType.InvalidEmail)]
    [DataRow("email", "longEmail", ErrorType.EmailTooLong)]
    [DataRow("hash", "null", ErrorType.InvalidPasswordHash)]
    [DataRow("hash", "empty", ErrorType.InvalidPasswordHash)]
    [DataRow("hash", "shortHash", ErrorType.InvalidPasswordHash)]
    [DataRow("hash", "longHash", ErrorType.InvalidPasswordHash)]
    [DataRow("hash", "nonHexHash", ErrorType.InvalidPasswordHash)]
    public async Task CreateAccountRejectsInvalidFieldsBeforeSaving(string field, string kind, ErrorType expected)
    {
        var value = InvalidValue(kind);
        var result = await hub.RequestCreateUser(
            field == "username" ? value! : Guid.NewGuid().ToString("N"),
            field == "hash" ? value! : PasswordHash,
            field == "email" ? value! : "player@example.com",
            field == "player" ? value! : "Player");
        Assert.IsFalse(result.Success);
        Assert.AreEqual(expected, result.Error);
        using var db = CreateContext();
        Assert.AreEqual(0, db.Users.Count());
    }

    [TestMethod]
    [DataRow("player", "null", ErrorType.PlayerNameTooShort)]
    [DataRow("player", "short", ErrorType.PlayerNameTooShort)]
    [DataRow("player", "longName", ErrorType.PlayerNameTooLong)]
    [DataRow("player", "hugeName", ErrorType.PlayerNameTooLong)]
    [DataRow("email", "null", ErrorType.InvalidEmail)]
    [DataRow("email", "badEmail", ErrorType.InvalidEmail)]
    [DataRow("email", "displayEmail", ErrorType.InvalidEmail)]
    [DataRow("email", "controlEmail", ErrorType.InvalidEmail)]
    [DataRow("email", "longEmail", ErrorType.EmailTooLong)]
    [DataRow("hash", "shortHash", ErrorType.InvalidPasswordHash)]
    [DataRow("hash", "longHash", ErrorType.InvalidPasswordHash)]
    [DataRow("hash", "nonHexHash", ErrorType.InvalidPasswordHash)]
    public async Task UpdateRejectsInvalidFieldsWithoutChangingDatabaseOrCache(string field, string kind, ErrorType expected)
    {
        var account = await CreateAccount();
        var value = InvalidValue(kind);
        var result = await hub.RequestUpdateUserInfo(account.Token,
            field == "hash" ? value! : PasswordHash,
            field == "player" ? value! : "Changed player",
            field == "email" ? value! : "changed@example.com");
        Assert.IsFalse(result.Success);
        Assert.AreEqual(expected, result.Error);
        using var db = CreateContext();
        var saved = db.Users.Single();
        Assert.AreEqual("Player", saved.PlayerName);
        Assert.AreEqual("player@example.com", saved.Email);
        Assert.AreEqual(PasswordHash, saved.HashedPassword);
        var cached = await hub.GetLoginInfo(account.Token);
        Assert.IsNotNull(cached.Contents);
        Assert.AreEqual(saved.PlayerName, cached.Contents.PlayerName);
        Assert.AreEqual(saved.Email, cached.Contents.Email);
    }

    [TestMethod]
    public async Task CreationAndUpdateAcceptBoundaryNamesAndPreserveOptionalPassword()
    {
        var userName = Guid.NewGuid().ToString("N") + "12345678";
        var playerName = new string('p', 40);
        var created = await hub.RequestCreateUser($" {userName} ", PasswordHash,
            " player@example.com ", $" {playerName} ");
        Assert.IsTrue(created.Success);
        Assert.IsNotNull(created.Contents);
        var updated = await hub.RequestUpdateUserInfo(created.Contents.Token, "", " Four ", " next@example.com ");
        Assert.IsTrue(updated.Success);
        using var db = CreateContext();
        var saved = db.Users.Single();
        Assert.AreEqual(userName, saved.Name);
        Assert.AreEqual("Four", saved.PlayerName);
        Assert.AreEqual("next@example.com", saved.Email);
        Assert.AreEqual(PasswordHash, saved.HashedPassword);
    }

    [TestMethod]
    [DataRow("null")]
    [DataRow("empty")]
    [DataRow("shortHash")]
    [DataRow("longHash")]
    [DataRow("nonHexHash")]
    public async Task PasswordResetRejectsInvalidHashWithoutConsumingToken(string kind)
    {
        var account = await CreateAccount();
        using (var db = CreateContext())
        {
            var user = db.Users.Single();
            user.PasswordResetToken = "reset-token";
            user.PasswordResetTokenCreated = DateTimeOffset.Now;
            db.SaveChanges();
        }
        var result = await hub.RequestSetPassword(account.UserName, "reset-token", InvalidValue(kind)!);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ErrorType.InvalidPasswordHash, result.Error);
        using var verification = CreateContext();
        var saved = verification.Users.Single();
        Assert.AreEqual(PasswordHash, saved.HashedPassword);
        Assert.AreEqual("reset-token", saved.PasswordResetToken);
    }

    [TestMethod]
    public async Task PasswordResetAcceptsValidHashAndClearsToken()
    {
        var account = await CreateAccount();
        using (var db = CreateContext())
        {
            var user = db.Users.Single();
            user.PasswordResetToken = "reset-token";
            user.PasswordResetTokenCreated = DateTimeOffset.Now;
            db.SaveChanges();
        }
        var replacementHash = new string('b', 64);
        var result = await hub.RequestSetPassword(account.UserName, "reset-token", replacementHash);
        Assert.IsTrue(result.Success);
        using var verification = CreateContext();
        var saved = verification.Users.Single();
        Assert.AreEqual(replacementHash, saved.HashedPassword);
        Assert.AreEqual("", saved.PasswordResetToken);
    }

    [TestMethod]
    public async Task CreateGameRejectsOversizedNameAndInvalidHashBeforePersistence()
    {
        var account = await CreateAccount();
        var longName = await hub.RequestCreateGame(new string('g', 129), account.Token, "", "", "");
        Assert.IsFalse(longName.Success);
        Assert.AreEqual(ErrorType.GameNameTooLong, longName.Error);
        var badHash = await hub.RequestCreateGame("Game", account.Token, new string('x', 4001), "", "");
        Assert.IsFalse(badHash.Success);
        Assert.AreEqual(ErrorType.InvalidPasswordHash, badHash.Error);
        using var db = CreateContext();
        Assert.AreEqual(0, db.PersistedGames.Count());
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(PasswordHash)]
    public async Task CreateGameAcceptsBoundaryNameAndOptionalHash(string passwordHash)
    {
        var account = await CreateAccount();
        var name = new string('g', 128);
        var result = await hub.RequestCreateGame(name, account.Token, passwordHash, "", "");
        Assert.IsTrue(result.Success);
        using var db = CreateContext();
        var saved = db.PersistedGames.Single();
        Assert.AreEqual(name, saved.GameName);
        Assert.AreEqual(passwordHash, saved.HashedPassword);
    }

    [TestMethod]
    public async Task GameCreatedWithClientPasswordHashCanBeJoinedWithSamePassword()
    {
        var host = await CreateAccount();
        var clientHash = Treachery.Client.Support.GetHash("xxx");
        var game = await hub.RequestCreateGame("Protected game", host.Token, clientHash, "", "");
        Assert.IsTrue(game.Success, game.Error.ToString());
        Assert.IsNotNull(game.Contents);

        var player = await CreateAccount();
        var wrong = await hub.RequestJoinGame(player.Token, game.Contents.GameId, Treachery.Client.Support.GetHash("yyy"), -1);
        Assert.IsFalse(wrong.Success);
        var join = await hub.RequestJoinGame(player.Token, game.Contents.GameId, Treachery.Client.Support.GetHash("xxx"), -1);
        Assert.IsTrue(join.Success, join.Error.ToString());
    }

    [TestMethod]
    public async Task CreateGameWithNullStateAndSkinReturnsLoadableGameState()
    {
        var account = await CreateAccount();
        var result = await hub.RequestCreateGame("Game", account.Token, "", null, null);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Contents);
        var state = GameState.Load(result.Contents.GameState);
        var loadError = Game.TryLoad(state, result.Contents.Participation, false, false, out var loadedGame);

        Assert.IsNull(loadError);
        Assert.IsNotNull(loadedGame);
    }

    [TestMethod]
    public async Task LegacyOversizedPlayerNameCannotPropagateIntoNewGamesOrParticipation()
    {
        var username = Guid.NewGuid().ToString("N");
        using (var db = CreateContext())
        {
            db.Users.Add(new User
            {
                Name = username, PlayerName = new string('p', 34965),
                Email = "legacy@example.com", HashedPassword = PasswordHash
            });
            db.SaveChanges();
        }
        var login = await hub.RequestLogin(Game.LatestVersion, username, PasswordHash);
        Assert.IsTrue(login.Success);
        Assert.IsNotNull(login.Contents);
        var account = login.Contents;
        var create = await hub.RequestCreateGame("Game", account.Token, "", "", "");
        Assert.AreEqual(ErrorType.PlayerNameTooLong, create.Error);
        var schedule = await hub.RequestScheduleGame(account.Token, DateTimeOffset.Now.AddDays(1),
            null, 6, 10, [], false);
        Assert.AreEqual(ErrorType.PlayerNameTooLong, schedule.Error);

        var host = await CreateAccount();
        var game = await hub.RequestCreateGame("Host's game", host.Token, "", "", "");
        Assert.IsTrue(game.Success);
        Assert.IsNotNull(game.Contents);
        var join = await hub.RequestJoinGame(account.Token, game.Contents.GameId, "", -1);
        Assert.AreEqual(ErrorType.PlayerNameTooLong, join.Error);
        var observe = await hub.RequestObserveGame(account.Token, game.Contents.GameId, "");
        Assert.AreEqual(ErrorType.PlayerNameTooLong, observe.Error);

        using var verification = CreateContext();
        Assert.AreEqual(1, verification.PersistedGames.Count());
        Assert.AreEqual(0, verification.ScheduledGames.Count());
        Assert.DoesNotContain(new string('p', 34965), verification.PersistedGames.Single().GameParticipation);
        var repaired = await hub.RequestUpdateUserInfo(account.Token, "", "Repaired", "legacy@example.com");
        Assert.IsTrue(repaired.Success);
        Assert.AreEqual("Repaired", verification.Users.AsNoTracking().Single(user => user.Id == account.UserId).PlayerName);
    }
}
