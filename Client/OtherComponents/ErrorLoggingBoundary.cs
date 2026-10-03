using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Treachery.Client.OtherComponents;

public class ErrorLoggingBoundary : ErrorBoundary
{
    [Inject] private IGameService GameService { get; set; } = null!;
    [Inject] private IJSRuntime JavaScript { get; set; } = null!;

    protected override async Task OnErrorAsync(Exception exception)
    {
        var url = string.Empty;
        var userAgent = string.Empty;
        try
        {
            url = await JavaScript.InvokeAsync<string>("treacheryErrorLogging.getUrl");
            userAgent = await JavaScript.InvokeAsync<string>("treacheryErrorLogging.getUserAgent");
        }
        catch (JSException javascriptException)
        {
            Support.Log($"Unable to collect browser details for a Blazor error: {javascriptException}");
        }

        await GameService.ReportClientError(new ClientErrorReport
        {
            Source = "Blazor",
            Message = exception.Message,
            Details = exception.ToString(),
            Url = url,
            UserAgent = userAgent
        });
    }
}
