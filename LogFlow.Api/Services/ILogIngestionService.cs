using LogFlow.Api.Contracts;

namespace LogFlow.Api.Services;

public interface ILogIngestionService
{
    Task IngestAsync(IReadOnlyCollection<IngestLogRequest> logs, CancellationToken ct = default);
}
