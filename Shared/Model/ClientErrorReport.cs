namespace Treachery.Shared.Model;

public class ClientErrorReport
{
    public string Source { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public int? LineNumber { get; set; }
    public int? ColumnNumber { get; set; }
}
