using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Treachery.Bots;
using Treachery.Server;

namespace Treachery.Test;

[TestClass]
[DoNotParallelize]
public sealed class ErrorLogTests
{
    [TestMethod]
    public async Task ErrorEntriesArePersistedAndRemovedAfterThirtyDays()
    {
        var path = Path.Combine(Path.GetTempPath(), $"treachery-errors-{Guid.NewGuid():N}.db");
        try
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:TreacheryDatabase"] = new SqliteConnectionStringBuilder
                {
                    DataSource = path,
                    Pooling = false
                }.ToString()
            }).Build();

            await using var context = new TreacheryContext(new DbContextOptionsBuilder<TreacheryContext>().Options, configuration);
            GameStorageMigration.Migrate(context);
            var errorLog = new ErrorLogService(context);
            await errorLog.RecordAsync("Client/JavaScript", new string('x', 5000), "stack", "/game", "test agent", 7, "test user");
            context.ErrorLogs.Add(new ErrorLogEntry
            {
                OccurredAt = DateTime.UtcNow.AddDays(-31),
                Source = "Server/HTTP",
                Message = "expired",
                Details = "",
                Url = "/",
                UserAgent = ""
            });
            await context.SaveChangesAsync();

            Assert.AreEqual(1, await errorLog.DeleteExpiredAsync());
            var retained = await context.ErrorLogs.AsNoTracking().SingleAsync();
            Assert.AreEqual("Client/JavaScript", retained.Source);
            Assert.AreEqual(Game.LatestVersion, retained.GameVersion);
            Assert.AreEqual(4000, retained.Message.Length);
            Assert.AreEqual(7, retained.UserId);
            Assert.AreEqual("test user", retained.Username);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [TestMethod]
    public async Task BotDecisionErrorStoresCompressedSavegameAndRetentionRemovesIt()
    {
        var path = Path.Combine(Path.GetTempPath(), $"treachery-bot-errors-{Guid.NewGuid():N}.db");
        try
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:TreacheryDatabase"] = new SqliteConnectionStringBuilder
                {
                    DataSource = path,
                    Pooling = false
                }.ToString()
            }).Build();

            await using var context = new TreacheryContext(new DbContextOptionsBuilder<TreacheryContext>().Options, configuration);
            GameStorageMigration.Migrate(context);
            var errorLog = new ErrorLogService(context);
            var savegame = Utilities.Serialize(new GameState { Version = Game.LatestVersion, Events = [] });
            var decision = new InvalidBotDecision(
                DateTime.UtcNow,
                Faction.Yellow,
                2,
                "Shipment",
                "Test decision",
                "Test validation error",
                savegame);

            await errorLog.RecordBotDecisionAsync("test-game-id", decision);

            var entry = await context.ErrorLogs.AsNoTracking().SingleAsync();
            var snapshot = await context.ErrorLogSnapshots.AsNoTracking().SingleAsync();
            Assert.AreEqual("Server/BotDecision", entry.Source);
            Assert.AreEqual("test-game-id", entry.GameId);
            Assert.AreEqual("Faction: Yellow, seat: 2" + Environment.NewLine +
                            "Decision: Test decision" + Environment.NewLine +
                            "Validation error: Test validation error", entry.Details);
            Assert.AreEqual(savegame, snapshot.GameState);
            Assert.IsTrue(GameState.Load(snapshot.GameState).Version == Game.LatestVersion);

            await context.Database.OpenConnectionAsync();
            await using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT GameState FROM ErrorLogSnapshots";
                var compressedSnapshot = (byte[])(await command.ExecuteScalarAsync())!;
                Assert.IsTrue(CompressedJson.IsCompressed(compressedSnapshot));
            }
            await context.Database.CloseConnectionAsync();

            var retainedEntry = await context.ErrorLogs.SingleAsync();
            retainedEntry.OccurredAt = DateTime.UtcNow.AddDays(-31);
            await context.SaveChangesAsync();
            Assert.AreEqual(1, await errorLog.DeleteExpiredAsync());
            Assert.AreEqual(0, await context.ErrorLogSnapshots.CountAsync());
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
