namespace LogFlow.Api.Contracts;

public record FrequentErrorResponse
{
    public string Message { get; init; } = string.Empty;
    public string? Exception { get; init; }
    public ulong TotalCount { get; init; }
    public DateTimeOffset LastOccurrence { get; init; }
}
