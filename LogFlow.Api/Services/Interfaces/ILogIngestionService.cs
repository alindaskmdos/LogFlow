using LogFlow.Api.Contracts;

namespace LogFlow.Api.Services.Interfaces;

public interface ILogIngestionService
{
    Task<bool> IngestAsync(
        IReadOnlyCollection<IngestLogRequest> logs,
        string serviceName,
        CancellationToken ct = default);
}
