namespace LogFlow.Sdk.Options;

public class LogFlowOptions
{
    public string Url { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int BatchSize { get; set; } = 100;
    public TimeSpan Period { get; set; } = TimeSpan.FromSeconds(10);
    public bool IncludeOnlyRequestLogs { get; set; } = false;
}