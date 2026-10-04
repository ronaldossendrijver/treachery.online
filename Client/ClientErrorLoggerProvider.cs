using System.Threading;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Treachery.Client;

/// <summary>
/// Records unhandled Blazor exceptions (logged by the renderer) in the server error log without
/// replacing the page, so the default non-blocking error banner remains the only visible effect.
/// </summary>
public sealed class ClientErrorLoggerProvider(IServiceProvider services) : ILoggerProvider
{
    private int _reporting;

    public ILogger CreateLogger(string categoryName) => new ClientErrorLogger(this, categoryName);

    public void Dispose()
    {
    }

    private async Task Report(string category, string message, Exception? exception)
    {
        if (Interlocked.Exchange(ref _reporting, 1) == 1)
            return;

        try
        {
            var userAgent = string.Empty;
            try
            {
                userAgent = await services.GetRequiredService<IJSRuntime>()
                    .InvokeAsync<string>("treacheryErrorLogging.getUserAgent");
            }
            catch (Exception javascriptException)
            {
                Support.Log($"Unable to collect browser details for a Blazor error: {javascriptException}");
            }

            var navigation = services.GetRequiredService<NavigationManager>();
            await services.GetRequiredService<IGameService>().ReportClientError(new ClientErrorReport
            {
                Source = "Blazor",
                Message = exception?.Message ?? message,
                Details = $"{category}\n{exception?.ToString() ?? message}",
                Url = "/" + navigation.ToBaseRelativePath(navigation.Uri),
                UserAgent = userAgent
            });
        }
        catch (Exception reportException)
        {
            Support.Log($"Unable to report Blazor error: {reportException}");
        }
        finally
        {
            Interlocked.Exchange(ref _reporting, 0);
        }
    }

    private sealed class ClientErrorLogger(ClientErrorLoggerProvider provider, string category) : ILogger
    {
        private readonly bool _isComponentsCategory = category.StartsWith("Microsoft.AspNetCore.Components", StringComparison.Ordinal);

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => _isComponentsCategory && logLevel >= LogLevel.Error;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            _ = provider.Report(category, formatter(state, exception), exception);
        }
    }
}
