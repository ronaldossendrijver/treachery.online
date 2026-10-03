using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Treachery.Server;

public class ErrorLoggingHubFilter(IServiceScopeFactory scopeFactory, ILogger<ErrorLoggingHubFilter> logger) : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        try
        {
            return await next(invocationContext);
        }
        catch (Exception exception)
        {
            try
            {
                var userToken = invocationContext.HubMethodArguments.OfType<string>().FirstOrDefault();
                GameHub.TryGetLoggedInUser(userToken, out var user);
                var httpContext = invocationContext.Context.GetHttpContext();
                var source = $"Server/SignalR/{invocationContext.HubMethod.Name}";
                await using var scope = scopeFactory.CreateAsyncScope();
                var errorLog = scope.ServiceProvider.GetRequiredService<ErrorLogService>();
                await errorLog.RecordAsync(
                    source,
                    exception.Message,
                    exception.ToString(),
                    httpContext?.Request.Path.Value ?? string.Empty,
                    httpContext?.Request.Headers.UserAgent.ToString() ?? string.Empty,
                    user?.Id,
                    user?.Username);
            }
            catch (Exception loggingException)
            {
                logger.LogError(loggingException, "Could not record an unhandled SignalR exception.");
            }

            throw;
        }
    }
}
