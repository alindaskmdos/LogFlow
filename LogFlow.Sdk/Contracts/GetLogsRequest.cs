namespace LogFlow.Sdk.Contracts;

public record GetLogsRequest
{
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
    public string? Level { get; init; }
    public int Limit { get; init; } = 100;
}
