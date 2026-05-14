namespace LogFlow.Sdk.Contracts;

public record GetFrequentErrorsRequest
{
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
    public int Limit { get; init; } = 10;
}
