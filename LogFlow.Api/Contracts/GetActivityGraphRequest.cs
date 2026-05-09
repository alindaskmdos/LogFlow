namespace LogFlow.Api.Contracts;

public record GetActivityGraphRequest
{
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
    public TimeSpan Interval { get; init; }
    public string? Level { get; init; }
}
