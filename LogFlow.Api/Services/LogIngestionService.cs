using System.Threading.Channels;
using LogFlow.Api.Contracts;
using LogFlow.Api.Services.Interfaces;

namespace LogFlow.Api.Services;

public class LogIngestionService(LogChannel channel) : ILogIngestionService
{
    private const int MaxBatchSize = 1000;

    public async Task IngestAsync(
        IReadOnlyCollection<IngestLogRequest> logs,
        string serviceName,
        CancellationToken ct = default)
    {
        if (logs.Count == 0)
            throw new ArgumentException("Logs batch is empty.", nameof(logs));

        if (logs.Count > MaxBatchSize)
            throw new ArgumentException($"Logs batch size cannot exceed {MaxBatchSize}.", nameof(logs));

        if (string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentException("Service name is required.", nameof(serviceName));

        var normalizedLogs = logs.Select(x => x with { Service = serviceName }).ToArray();

        await channel.WriteAsync(normalizedLogs, ct);
    }
}
