using LogFlow.Api.Contracts;

namespace LogFlow.Api.Services.Interfaces;

public interface ILogIngestionService
{
    Task IngestAsync(IReadOnlyCollection<IngestLogRequest> logs, CancellationToken ct = default);
}
