using LogFlow.Api.Contracts;
using LogFlow.Api.Infrastructure.ClickHouse.Interfaces;
using LogFlow.Api.Services.Interfaces;

namespace LogFlow.Api.Services;

public class LogIngestionService(ILogRepository repository) : ILogIngestionService
{
    private const int MaxBatchSize = 1000;

    public async Task IngestAsync(IReadOnlyCollection<IngestLogRequest> logs, CancellationToken ct = default)
    {
        if (logs.Count == 0)
        {
            throw new ArgumentException("Logs batch is empty.", nameof(logs));
        }

        if (logs.Count > MaxBatchSize)
            throw new ArgumentException($"Logs batch size cannot exceed {MaxBatchSize}.", nameof(logs));

        await repository.InsertAsync(logs, ct);
    }
}
