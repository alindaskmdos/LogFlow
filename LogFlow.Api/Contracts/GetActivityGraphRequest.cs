namespace LogFlow.Api.Contracts;

public record GetActivityGraphRequest
{
    public DateTimeOffset From { get; set; }
    public DateTimeOffset To { get; set; }
    public TimeSpan Interval { get; set; }
    public string? Level { get; set; }
    public string? ServiceName { get; set; }
}
