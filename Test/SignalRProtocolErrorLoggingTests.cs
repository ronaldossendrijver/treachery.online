using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Treachery.Server;

namespace Treachery.Test;

[TestClass]
public sealed class SignalRProtocolErrorLoggingTests
{
    [TestMethod]
    public async Task RecordsSignalRJsonProtocolErrorsInErrorLog()
    {
        var connectionString = $"Data Source={Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:TreacheryDatabase"] = connectionString
            })
            .Build();
        var options = new DbContextOptionsBuilder<TreacheryContext>()
            .UseSqlite(connectionString)
            .Options;
        await using (var db = new TreacheryContext(options, configuration))
            await db.Database.EnsureCreatedAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/gameHub";
        httpContext.Request.Headers.UserAgent = "SignalR protocol test";

        var services = new ServiceCollection();
        services.AddScoped(_ => new TreacheryContext(options, configuration));
        services.AddScoped<ErrorLogService>();
        services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor { HttpContext = httpContext });
        services.AddSingleton<ILoggerProvider, SignalRProtocolErrorLoggingLoggerProvider>();
        services.AddLogging(logging =>
            logging.AddFilter<SignalRProtocolErrorLoggingLoggerProvider>(
                "Microsoft.AspNetCore.SignalR.HubConnectionHandler",
                LogLevel.Debug));
        using var serviceProvider = services.BuildServiceProvider();

        var logger = serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("Microsoft.AspNetCore.SignalR.HubConnectionHandler`1");
        var exception = new InvalidDataException(
            "Error reading JSON.",
            new JsonException("Invalid value at $.arguments[2]."));
        logger.Log(
            LogLevel.Debug,
            new EventId(2, "ErrorProcessingRequest"),
            "Error when processing requests.",
            exception,
            static (message, _) => message);

        ErrorLogEntry? entry = null;
        var timeout = DateTime.UtcNow.AddSeconds(5);
        while (entry is null && DateTime.UtcNow < timeout)
        {
            await using var db = new TreacheryContext(options, configuration);
            entry = await db.ErrorLogs.SingleOrDefaultAsync();
            if (entry is null)
                await Task.Delay(10);
        }

        Assert.IsNotNull(entry);
        Assert.AreEqual("Server/SignalR/Protocol", entry.Source);
        Assert.AreEqual("Error reading JSON.", entry.Message);
        StringAssert.Contains(entry.Details, "$.arguments[2]");
        Assert.AreEqual("/gameHub", entry.Url);
        Assert.AreEqual("SignalR protocol test", entry.UserAgent);
    }
}
