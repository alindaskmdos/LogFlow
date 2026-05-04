using LogFlow.Api.Contracts;

namespace LogFlow.Api.Services.Interfaces;

public interface IStatisticsService
{
    Task<IReadOnlyList<LogResponse>> GetLogsAsync(
            DateTimeOffset from,
            DateTimeOffset to,
            string? level,
            string? service,
            int limit,
            CancellationToken ct = default);

    Task<IReadOnlyList<FrequentErrorResponse>> GetMostFrequentErrorsAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        string? service,
        int limit,
        CancellationToken ct = default);

    Task<IReadOnlyDictionary<DateTimeOffset, long>> GetLogsActivityGraphAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        TimeSpan interval,
        string? level,
        string? service,
        CancellationToken ct = default);
}
