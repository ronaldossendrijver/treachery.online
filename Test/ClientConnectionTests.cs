using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Treachery.Test;

[TestClass]
public sealed class ClientConnectionTests
{
    private const string ClientTimeout =
        "The server closed the connection with the following error: Connection closed with an error. " +
        "OperationCanceledException: Client hasn't sent a message/ping within the configured ClientTimeoutInterval.";
    private const string ClosePrefix = "The server closed the connection with the following error: ";

    [TestMethod]
    [DataRow(ClientTimeout, true, true)]
    [DataRow("The server closed the connection with the following error: Unexpected server failure.", true, false)]
    [DataRow(ClientTimeout, false, false)]
    [DataRow("OperationCanceledException: Client hasn't sent a message/ping within the configured ClientTimeoutInterval.", false, false)]
    [DataRow("Invalid game event.", false, false)]
    public async Task OnlyClientPingTimeoutReturnsConnectionUnavailableWithoutRetrying(string message, bool closeConnection, bool handled)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddSignalR();
        builder.Services.AddSingleton<TestLifetimeManager>();
        builder.Services.AddSingleton<HubLifetimeManager<FailingHub>>(services => services.GetRequiredService<TestLifetimeManager>());
        var calls = new InvocationCounter();
        builder.Services.AddSingleton(calls);
        await using var server = builder.Build();
        server.MapHub<FailingHub>("/gameHub");
        await server.StartAsync();

        var navigation = new TestNavigationManager(server.Urls.Single() + "/");
        await using var client = new Client.Client(navigation, new Browser(Substitute.For<IJSRuntime>()));
        client.RefreshHandler += () => { };
        var connection = (HubConnection)typeof(Client.Client)
            .GetField("_connection", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(client)!;
        await connection.StartAsync();
        var invoke = typeof(Client.Client).GetMethod("InvokeHub", BindingFlags.NonPublic | BindingFlags.Instance)!
            .MakeGenericMethod(typeof(VoidContents));
        var request = (Task<Result<VoidContents>>)invoke.Invoke(client, [nameof(FailingHub.Fail), new object?[] { message, closeConnection }])!;

        if (handled)
        {
            var result = await request;
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ErrorType.ConnectionUnavailable, result.Error);
        }
        else
        {
            var exception = await Assert.ThrowsExactlyAsync<HubException>(async () => await request);
            StringAssert.EndsWith(exception.Message, message);
        }

        Assert.AreEqual(1, calls.Count, "A failed invocation must not be automatically replayed.");
    }

    public sealed class InvocationCounter
    {
        public int Count { get; set; }
    }

    public sealed class TestLifetimeManager(ILogger<DefaultHubLifetimeManager<FailingHub>> logger)
        : DefaultHubLifetimeManager<FailingHub>(logger)
    {
        public HubConnectionContext Connection { get; private set; } = null!;

        public override Task OnConnectedAsync(HubConnectionContext connection)
        {
            Connection = connection;
            return base.OnConnectedAsync(connection);
        }
    }

    public sealed class FailingHub(InvocationCounter calls, TestLifetimeManager lifetime) : Hub
    {
        public async Task Fail(string message, bool closeConnection)
        {
            calls.Count++;
            if (closeConnection)
            {
                await lifetime.Connection.WriteAsync(new CloseMessage(message[ClosePrefix.Length..], allowReconnect: false));
                return;
            }
            throw new HubException(message);
        }
    }

    private sealed class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager(string baseUri) => Initialize(baseUri, baseUri);
    }
}
