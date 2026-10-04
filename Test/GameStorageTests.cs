using System.IO;
using System.Text;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Treachery.Server;

namespace Treachery.Test;

[TestClass]
[DoNotParallelize]
public sealed class GameStorageTests
{
    private const string LegacyMigration = "20260729104158_NullabilityUpdates";

    [TestMethod]
    public void CompressionPreservesUnicodeAndEmptyStrings()
    {
        foreach (var json in new[] { "", "{}", "{\"name\":\"\\u00e9\\u6e38\"}", "{\"name\":\"\u00e9\u6e38\"}" })
        {
            var compressed = CompressedJson.Compress(json);
            Assert.AreEqual(json, CompressedJson.Decompress(compressed));
            if (json.Length > 0)
            {
                Assert.AreEqual((byte)0x1f, compressed[0]);
                Assert.AreEqual((byte)0x8b, compressed[1]);
            }
            else
            {
                Assert.IsEmpty(compressed);
            }
        }
    }

    [TestMethod]
    public void RepeatedEventJsonUsesLessStorage()
    {
        var json = "[" + string.Join(",", Enumerable.Repeat(
            "{\"type\":\"GameEvent\",\"player\":\"Test player\",\"turn\":10,\"time\":\"2026-09-29T23:00:00Z\"}", 1000)) + "]";

        var compressed = CompressedJson.Compress(json);

        Assert.IsLessThan(Encoding.UTF8.GetByteCount(json) / 4, compressed.Length);
        Assert.AreEqual(json, CompressedJson.Decompress(compressed));
    }

    [TestMethod]
    public void CorruptCompressionFailsExplicitly()
    {
        Assert.ThrowsExactly<InvalidDataException>(() => CompressedJson.Decompress([0xff, 0xff, 0xff, 0xff]));
        var compressed = CompressedJson.Compress("{\"events\":[]}");
        Assert.ThrowsExactly<InvalidDataException>(() => CompressedJson.Decompress(compressed[..^1]));
        Assert.ThrowsExactly<InvalidDataException>(() => CompressedJson.Decompress(compressed[..^8]));
    }

    [TestCleanup]
    public void RestoreCompressionSetting() => CompressedJson.CompressGameJson = false;

    [TestMethod]
    public void CompressionIsDisabledByDefault()
    {
        Assert.IsFalse(CompressedJson.CompressGameJson);
        Assert.AreEqual("{\"a\":1}", Encoding.UTF8.GetString(CompressedJson.Encode("{\"a\":1}")));
    }

    [TestMethod]
    public void DecodeAcceptsCompressedAndPlainJson()
    {
        foreach (var json in new[] { "", "{}", "{\"name\":\"\u00e9\u6e38\"}" })
        {
            Assert.AreEqual(json, CompressedJson.Decode(CompressedJson.Compress(json)));
            Assert.AreEqual(json, CompressedJson.Decode(Encoding.UTF8.GetBytes(json)));
        }
    }

    [TestMethod]
    public void LegacyDatabaseIsReadableWithoutCompressionAtStartup()
    {
        using var database = new TestDatabase();
        var state = GameState.GetStateAsString(CreateGame());
        const string participation = "{}";
        using (var legacy = database.CreateContext())
        {
            legacy.GetService<IMigrator>().Migrate(LegacyMigration);
            InsertGame(legacy, 1, state, participation);
            InsertGame(legacy, 2, CompressedJson.Compress(state), participation);
            InsertGame(legacy, 3, CompressedJson.Compress(state), CompressedJson.Compress(participation));
        }

        using var migrated = database.CreateContext();
        Assert.AreEqual(0, GameStorageMigration.Migrate(migrated));
        AssertStorageTypes(migrated, "PersistedGames", 1, "text", "text");
        AssertStorageTypes(migrated, "PersistedGames", 2, "blob", "text");
        AssertStorageTypes(migrated, "PersistedGames", 3, "blob", "blob");
        foreach (var row in migrated.PersistedGames.AsNoTracking())
        {
            Assert.AreEqual(state, row.GameState);
            Assert.AreEqual(participation, row.GameParticipation);
        }
    }

    [TestMethod]
    public void LegacyDatabaseMigratesAndRestoresGameWithoutChangingMetadata()
    {
        CompressedJson.CompressGameJson = true;
        using var database = new TestDatabase();
        var game = CreateGame();
        var state = GameState.GetStateAsString(game);
        var participation = Utilities.Serialize(game.Participation);
        using (var legacy = database.CreateContext())
        {
            legacy.GetService<IMigrator>().Migrate(LegacyMigration);
            InsertGame(legacy, 1, state, participation);
            InsertGame(legacy, 2, CompressedJson.Compress(state), participation);
            InsertGame(legacy, 3, CompressedJson.Compress(state), CompressedJson.Compress(participation));
            legacy.Database.ExecuteSqlRaw(
                """INSERT INTO "ArchivedGames" ("Id", "GameName", "CreatorUserId", "GameState", "GameParticipation") VALUES (1, 'Archive', 42, {0}, {1})""",
                state, participation);
            legacy.Users.Add(new User
            {
                Name = "Existing user",
                PlayerName = "Existing player",
                Email = "player@example.test",
                HashedPassword = "user-password"
            });
            legacy.SaveChanges();
        }

        using (var migrated = database.CreateContext())
        {
            Assert.AreEqual(3, GameStorageMigration.Migrate(migrated));
            AssertStorageTypes(migrated, "PersistedGames", 1, "blob", "blob");
            AssertStorageTypes(migrated, "ArchivedGames", 1, "blob", "blob");
            Assert.AreEqual(0, GameStorageMigration.Migrate(migrated));
            Assert.IsFalse(migrated.Database.HasPendingModelChanges());

            foreach (var row in migrated.PersistedGames.AsNoTracking())
            {
                Assert.AreEqual(state, row.GameState);
                Assert.AreEqual(participation, row.GameParticipation);
                Assert.AreEqual($"game-{row.Id}", row.GameId);
                Assert.AreEqual("Existing game", row.GameName);
                Assert.AreEqual(42, row.CreatorUserId);
                Assert.AreEqual("game-password", row.HashedPassword);
                Assert.IsTrue(row.ObserversRequirePassword);
                Assert.IsTrue(row.StatisticsSent);
                Assert.AreEqual(DateTimeOffset.Parse("2026-09-28T12:00:00+00:00"), row.CreationDate);
                Assert.AreEqual(DateTimeOffset.Parse("2026-09-29T12:00:00+00:00"), row.LastAction);
                Assert.AreEqual(DateTimeOffset.Parse("2026-09-29T11:00:00+00:00"), row.LastAsyncPlayMessageSent);

                var restoredParticipation = Utilities.Deserialize<Participation>(row.GameParticipation);
                Assert.IsNotNull(restoredParticipation);
                Assert.IsNull(Game.TryLoad(GameState.Load(row.GameState),
                    restoredParticipation, false, true, out var restored));
                Assert.IsNotNull(restored);
                Assert.AreEqual(game.Version, restored.Version);
                Assert.AreEqual(game.Seed, restored.Seed);
                Assert.AreEqual(game.CurrentPhase, restored.CurrentPhase);
                Assert.AreEqual(game.History.Count, restored.History.Count);
                Assert.AreEqual(game.Participation.PlayerNames[1], restored.Participation.PlayerNames[1]);
            }

            var archived = migrated.ArchivedGames.Single();
            Assert.AreEqual(state, archived.GameState);
            Assert.AreEqual(participation, archived.GameParticipation);
            Assert.AreEqual("Archive", archived.GameName);
            Assert.AreEqual(42, archived.CreatorUserId);
            Assert.AreEqual("Existing user", migrated.Users.Single().Name);
        }

        using var restarted = database.CreateContext();
        Assert.AreEqual(0, GameStorageMigration.Migrate(restarted));
        Assert.AreEqual(state, restarted.PersistedGames.AsNoTracking().First().GameState);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void NewSavesAndUpdatesRoundTrip(bool compress)
    {
        CompressedJson.CompressGameJson = compress;
        using var database = new TestDatabase();
        var game = CreateGame();
        var state = GameState.GetStateAsString(game);
        var participation = Utilities.Serialize(game.Participation);
        var info = Utilities.Serialize(Treachery.Shared.GameInfo.FromGame(game));
        using (var context = database.CreateContext())
        {
            Assert.AreEqual(0, GameStorageMigration.Migrate(context));
            context.PersistedGames.Add(new PersistedGame
            {
                Id = 1, GameId = "new-game", GameState = state, GameInfo = info, GameParticipation = participation
            });
            context.ArchivedGames.Add(new ArchivedGame
            {
                Id = 1, GameName = "Archive", GameState = state, GameParticipation = participation
            });
            context.SaveChanges();
            AssertStorageTypes(context, "PersistedGames", 1, "blob", "blob");
            AssertStorageTypes(context, "ArchivedGames", 1, "blob", "blob");
            Assert.AreEqual(compress, IsStoredCompressed(context, "PersistedGames", 1));
            Assert.AreEqual(compress, IsStoredCompressed(context, "ArchivedGames", 1));
            context.ChangeTracker.Clear();
            var saved = context.PersistedGames.Single();
            Assert.AreEqual(state, saved.GameState);
            Assert.AreEqual(info, saved.GameInfo);
            Assert.AreEqual(participation, saved.GameParticipation);
            saved.GameState = state + " ";
            saved.GameInfo = info;
            saved.GameParticipation = "{}";
            context.SaveChanges();
        }

        using var restarted = database.CreateContext();
        Assert.AreEqual(0, GameStorageMigration.Migrate(restarted));
        Assert.AreEqual(state + " ", restarted.PersistedGames.Single().GameState);
        Assert.AreEqual(info, restarted.PersistedGames.Single().GameInfo);
        Assert.AreEqual("{}", restarted.PersistedGames.Single().GameParticipation);
        Assert.AreEqual(state, restarted.ArchivedGames.Single().GameState);
    }

    [TestMethod]
    public void InterruptedMigrationResumesWithoutRecompressingCompletedRows()
    {
        CompressedJson.CompressGameJson = true;
        using var database = new TestDatabase();
        using (var context = database.CreateContext())
        {
            context.GetService<IMigrator>().Migrate(LegacyMigration);
            InsertGame(context, 1, "{\"events\":[]}", "{}");
            InsertGame(context, 2, "{\"events\":[]}", "{}");
            context.Database.Migrate();
            // Install after the schema rebuild so the trigger survives until conversion.
            context.Database.ExecuteSqlRaw("""
                CREATE TRIGGER fail_second_game BEFORE UPDATE ON "PersistedGames"
                WHEN NEW."Id" = 2
                BEGIN SELECT RAISE(ABORT, 'Simulated interrupted conversion'); END;
                """);
            Assert.ThrowsExactly<SqliteException>(() => GameStorageMigration.Migrate(context));
            AssertStorageTypes(context, "PersistedGames", 1, "blob", "blob");
            AssertStorageTypes(context, "PersistedGames", 2, "text", "text");
            context.Database.ExecuteSqlRaw("DROP TRIGGER fail_second_game;");
        }

        using var restarted = database.CreateContext();
        Assert.AreEqual(1, GameStorageMigration.Migrate(restarted));
        Assert.AreEqual(0, GameStorageMigration.Migrate(restarted));
        foreach (var row in restarted.PersistedGames.AsNoTracking())
        {
            Assert.AreEqual("{\"events\":[]}", row.GameState);
            Assert.AreEqual("{}", row.GameParticipation);
        }
    }

    [TestMethod]
    public void SchemaDowngradeIsRejectedRatherThanLeavingUnreadableJson()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        GameStorageMigration.Migrate(context);
        Assert.ThrowsExactly<NotSupportedException>(() => context.GetService<IMigrator>().Migrate(LegacyMigration));
    }

    [TestMethod]
    public async Task ServerStartupMigratesLegacyDatabaseBeforeServingRequests()
    {
        using var database = new TestDatabase();
        var game = CreateGame();
        var state = GameState.GetStateAsString(game);
        using (var legacy = database.CreateContext())
        {
            legacy.GetService<IMigrator>().Migrate(LegacyMigration);
            InsertGame(legacy, 1, state, Utilities.Serialize(game.Participation));
        }

        using var host = Host.CreateDefaultBuilder([])
            .ConfigureAppConfiguration((_, configuration) => configuration.AddConfiguration(database.CreateConfiguration()))
            .ConfigureLogging(logging => logging.ClearProviders())
            .ConfigureWebHostDefaults(web => web.UseStartup<Startup>()
                .UseEnvironment(Environments.Production)
                .UseUrls("http://127.0.0.1:0"))
            .Build();
        await host.StartAsync();
        try
        {
            using var context = database.CreateContext();
            AssertStorageTypes(context, "PersistedGames", 1, "text", "text");
            Assert.AreEqual(state, context.PersistedGames.Single().GameState);
            var server = host.Services.GetRequiredService<IServer>();
            var addresses = server.Features.Get<IServerAddressesFeature>();
            Assert.IsNotNull(addresses);
            using var client = new HttpClient { BaseAddress = new Uri(addresses.Addresses.Single()) };
            using var response = await client.PostAsync("/gameHub/negotiate?negotiateVersion=1", null);
            response.EnsureSuccessStatusCode();

            PersistedGame? backfilledGame = null;
            var backfillDeadline = DateTimeOffset.UtcNow.AddSeconds(5);
            while (DateTimeOffset.UtcNow < backfillDeadline)
            {
                using var pollingContext = database.CreateContext();
                backfilledGame = await pollingContext.PersistedGames.AsNoTracking().SingleAsync();
                if (backfilledGame.GameInfo != null)
                    break;

                await Task.Delay(25);
            }

            Assert.IsNotNull(backfilledGame?.GameInfo);
            var info = Utilities.Deserialize<Treachery.Shared.GameInfo>(backfilledGame.GameInfo);
            Assert.IsNotNull(info);
            Assert.IsTrue(info.HasDetails);
            Assert.AreEqual(2, info.MaxPlayers);
            Assert.AreEqual(2, info.NrOfPlayers);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    private static Game CreateGame()
    {
        var participation = new Participation
        {
            PlayerNames = new Dictionary<int, string> { [1] = "Player \u00e9", [2] = "Other player" },
            SeatedPlayers = new Dictionary<int, int> { [1] = 0, [2] = 1 },
            Hosts = [1],
            Observers = [3]
        };
        var game = new Game(Game.LatestVersion, participation);
        Assert.IsNull(new EstablishPlayers(game, Faction.None)
        {
            Seed = 122,
            Settings = new GameSettings
            {
                NumberOfPlayers = 2,
                MaximumTurns = 10,
                InitialRules = [Rule.PlayersChooseFactions],
                AllowedFactionsInPlay = [Faction.Black, Faction.Green]
            }
        }.Execute(false, true));
        Assert.IsNotEmpty(game.History);
        return game;
    }

    private static void InsertGame(TreacheryContext context, int id, object state, object participation)
    {
        context.Database.ExecuteSqlRaw("""
            INSERT INTO "PersistedGames"
                ("Id", "GameId", "GameName", "CreatorUserId", "CreationDate", "GameState",
                 "GameParticipation", "HashedPassword", "ObserversRequirePassword", "StatisticsSent",
                 "LastAsyncPlayMessageSent", "LastAction")
            VALUES ({0}, {1}, 'Existing game', 42, '2026-09-28 12:00:00+00:00', {2}, {3},
                    'game-password', 1, 1, '2026-09-29 11:00:00+00:00', '2026-09-29 12:00:00+00:00')
            """, id, $"game-{id}", state, participation);
    }

    private static void AssertStorageTypes(TreacheryContext context, string table, int id, string stateType, string participationType)
    {
        context.Database.OpenConnection();
        try
        {
            using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = $"""
                SELECT typeof("GameState"), typeof("GameParticipation") FROM "{table}" WHERE "Id" = {id}
                """;
            using var reader = command.ExecuteReader();
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(stateType, reader.GetString(0));
            Assert.AreEqual(participationType, reader.GetString(1));
        }
        finally
        {
            context.Database.CloseConnection();
        }
    }

    private static bool IsStoredCompressed(TreacheryContext context, string table, int id)
    {
        context.Database.OpenConnection();
        try
        {
            using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = $"""SELECT "GameState" FROM "{table}" WHERE "Id" = {id}""";
            return CompressedJson.IsCompressed((byte[])command.ExecuteScalar()!);
        }
        finally
        {
            context.Database.CloseConnection();
        }
    }

    private sealed class TestDatabase : IDisposable
    {
        private readonly string path = Path.Combine(Path.GetTempPath(), $"treachery-storage-{Guid.NewGuid():N}.db");

        public IConfiguration CreateConfiguration()
        {
            return new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:TreacheryDatabase"] = new SqliteConnectionStringBuilder
                {
                    DataSource = path, Pooling = false
                }.ToString()
            }).Build();
        }

        public TreacheryContext CreateContext() =>
            new(new DbContextOptionsBuilder<TreacheryContext>().Options, CreateConfiguration());

        public void Dispose() => File.Delete(path);
    }
}
