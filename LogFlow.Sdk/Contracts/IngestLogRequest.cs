using System.Text.Json.Serialization;

namespace LogFlow.Sdk.Contracts;

public record IngestLogRequest
{
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    [JsonIgnore]
    public string Service { get; init; } = string.Empty;
    public string? Environment { get; init; } = string.Empty;
    public string Level { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
    public string? Exception { get; init; }

    public string? TraceId { get; init; }
    public string? SpanId { get; init; }

    public string? RequestPath { get; init; }
    public string? Method { get; init; }
    public string? StatusCode { get; init; }
    public string? ElapsedMs { get; init; }

    public string? Properties { get; init; }
}
