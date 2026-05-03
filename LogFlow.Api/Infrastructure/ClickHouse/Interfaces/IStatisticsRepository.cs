using LogFlow.Api.Contracts;

namespace LogFlow.Api.Infrastructure.ClickHouse.Interfaces;

public interface IStatisticsRepository
{
    Task<List<LogResponse>> GetLogsAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        string? level,
        string? service,
        int limit,
        CancellationToken ct = default);

    Task<List<FrequentErrorResponse>> GetMostFrequentErrorsAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        string? service,
        int limit,
        CancellationToken ct = default);

    Task<Dictionary<DateTimeOffset, long>> GetLogsActivityGraphAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        TimeSpan interval,
        string? level,
        string? service,
        CancellationToken ct = default);
}