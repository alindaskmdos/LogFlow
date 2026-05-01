using LogFlow.Api.Contracts;

namespace LogFlow.Api.Infrastructure.ClickHouse;

public interface ILogRepository
{
    Task InsertBatchAsync(IReadOnlyCollection<IngestLogRequest> logs, CancellationToken ct = default);
}
