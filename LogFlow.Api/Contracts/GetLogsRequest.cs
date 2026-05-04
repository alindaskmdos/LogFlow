namespace LogFlow.Api.Contracts;

public record GetLogsRequest
{
    public DateTimeOffset From { get; set; }
    public DateTimeOffset To { get; set; }
    public string? Level { get; set; }
    public string? ServiceName { get; set; }
    public int Limit { get; set; } = 100;
}
