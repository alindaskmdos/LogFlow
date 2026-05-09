using LogFlow.Api.Contracts;

namespace LogFlow.Api.Services.Interfaces;

public interface IStatisticsService
{
    Task<IReadOnlyList<LogResponse>> GetLogsAsync(
        GetLogsRequest request,
        string serviceName,
        CancellationToken ct = default);

    Task<IReadOnlyList<FrequentErrorResponse>> GetMostFrequentErrorsAsync(
        GetFrequentErrorsRequest request,
        string serviceName,
        CancellationToken ct = default);

    Task<IReadOnlyDictionary<DateTimeOffset, ulong>> GetLogsActivityGraphAsync(
        GetActivityGraphRequest request,
        string serviceName,
        CancellationToken ct = default);
}
