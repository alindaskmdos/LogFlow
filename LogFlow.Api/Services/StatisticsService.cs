using LogFlow.Api.Contracts;
using LogFlow.Api.Infrastructure.ClickHouse.Interfaces;
using LogFlow.Api.Services.Interfaces;

namespace LogFlow.Api.Services;

public class StatisticsService(IStatisticsRepository repository) : IStatisticsService
{
    private const int MaxLimit = 1000;
    private const int MaxDaysRange = 30;

    private static void ValidateDateRange(DateTimeOffset from, DateTimeOffset to)
    {
        if (from >= to)
            throw new ArgumentException("'from' must be earlier than 'to'");
        if ((to - from).TotalDays > MaxDaysRange)
            throw new ArgumentException($"Date range cannot exceed {MaxDaysRange} days");
    }

    public Task<IReadOnlyList<LogResponse>> GetLogsAsync(
        DateTimeOffset from, DateTimeOffset to,
        string? level, string? service,
        int limit, CancellationToken ct = default)
    {
        ValidateDateRange(from, to);
        limit = Math.Clamp(limit, 1, MaxLimit);
        return repository.GetLogsAsync(from, to, level, service, limit, ct);
    }

    public Task<IReadOnlyList<FrequentErrorResponse>> GetMostFrequentErrorsAsync(
        DateTimeOffset from, DateTimeOffset to,
        string? service, int limit,
        CancellationToken ct = default)
    {
        ValidateDateRange(from, to);
        limit = Math.Clamp(limit, 1, 100);
        return repository.GetMostFrequentErrorsAsync(from, to, service, limit, ct);
    }

    public Task<IReadOnlyDictionary<DateTimeOffset, long>> GetLogsActivityGraphAsync(
        DateTimeOffset from, DateTimeOffset to,
        TimeSpan interval, string? level, string? service,
        CancellationToken ct = default)
    {
        ValidateDateRange(from, to);

        if (interval < TimeSpan.FromMinutes(1))
            throw new ArgumentException("Interval cannot be less than 1 minute");
        if (interval > TimeSpan.FromDays(1))
            throw new ArgumentException("Interval cannot exceed 1 day");

        return repository.GetLogsActivityGraphAsync(from, to, interval, level, service, ct);
    }
}