using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Treachery.Server;

public sealed class SignalRProtocolErrorLoggingLoggerProvider(
    IServiceScopeFactory scopeFactory,
    IHttpContextAccessor httpContextAccessor) : ILoggerProvider
{
    private const string HubConnectionHandlerCategory = "Microsoft.AspNetCore.SignalR.HubConnectionHandler";
    private const int ErrorProcessingRequestEventId = 2;

    public ILogger CreateLogger(string categoryName) => new ProtocolLogger(this, categoryName);

    public void Dispose()
    {
    }

    private async Task RecordAsync(string category, Exception exception)
    {
        try
        {
            var request = httpContextAccessor.HttpContext?.Request;
            var path = request?.Path.Value ?? "/gameHub";
            var userAgent = request?.Headers.UserAgent.ToString() ?? string.Empty;
            await using var scope = scopeFactory.CreateAsyncScope();
            var errorLog = scope.ServiceProvider.GetRequiredService<ErrorLogService>();
            await errorLog.RecordAsync(
                "Server/SignalR/Protocol",
                exception.Message,
                $"{category}{Environment.NewLine}{exception}",
                path,
                userAgent);
        }
        catch (Exception loggingException)
        {
            Console.Error.WriteLine($"Could not record a SignalR protocol error: {loggingException}");
        }
    }

    private static bool IsJsonProtocolError(Exception? exception)
        => exception is InvalidDataException
        {
            Message: "Error reading JSON."
        } or InvalidDataException
        {
            InnerException: JsonException
        };

    private sealed class ProtocolLogger(SignalRProtocolErrorLoggingLoggerProvider provider, string categoryName) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel)
            => categoryName.StartsWith(HubConnectionHandlerCategory, StringComparison.Ordinal)
               && logLevel >= LogLevel.Debug;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (logLevel < LogLevel.Debug ||
                eventId.Id != ErrorProcessingRequestEventId ||
                exception is null ||
                !IsJsonProtocolError(exception))
            {
                return;
            }

            _ = provider.RecordAsync(categoryName, exception);
        }
    }
}
