using System.Runtime.ExceptionServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Treachery.Server;

public class ErrorLoggingMiddleware(RequestDelegate next, ILogger<ErrorLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, ErrorLogService errorLog)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            if (exception is not OperationCanceledException || !context.RequestAborted.IsCancellationRequested)
                await RecordWithoutMaskingAsync(errorLog, exception, context.Request.Path, context.Request.Headers.UserAgent.ToString());
            ExceptionDispatchInfo.Capture(exception).Throw();
            throw;
        }
    }

    private async Task RecordWithoutMaskingAsync(ErrorLogService errorLog, Exception exception, string path, string userAgent)
    {
        try
        {
            await errorLog.RecordAsync("Server/HTTP", exception.Message, exception.ToString(), path, userAgent);
        }
        catch (Exception loggingException)
        {
            logger.LogError(loggingException, "Could not record an unhandled HTTP exception.");
        }
    }
}
