using LogFlow.Api.Contracts;

namespace LogFlow.Api.Infrastructure.ClickHouse.Interfaces;

public interface ILogRepository
{
    Task IngestAsync(
        IReadOnlyCollection<IngestLogRequest> logs,
        CancellationToken ct = default);
}
