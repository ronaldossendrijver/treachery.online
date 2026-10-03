using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
}
