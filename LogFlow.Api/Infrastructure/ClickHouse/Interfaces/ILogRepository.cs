using LogFlow.Api.Contracts;

namespace LogFlow.Api.Infrastructure.ClickHouse.Interfaces;

public interface ILogRepository
{
    Task InsertAsync(
        IReadOnlyCollection<IngestLogRequest> logs,
        CancellationToken ct = default);
}
