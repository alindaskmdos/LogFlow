namespace LogFlow.Api.Contracts;

public record IngestLogRequest
{
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public string Service { get; init; } = string.Empty;
    public string Environment { get; init; } = string.Empty;
    public string Level { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
    public string? Exception { get; init; }

    public string? TraceId { get; init; }
    public string? SpanId { get; init; }

    public string? RequestPath { get; init; }
    public string? Method { get; init; }
    public int? StatusCode { get; init; }
    public long? ElapsedMs { get; init; }

    public string? Properties { get; init; }
}
