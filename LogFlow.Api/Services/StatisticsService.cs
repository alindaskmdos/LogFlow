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
        GetLogsRequest request,
        string serviceName,
        CancellationToken ct = default)
    {
        ValidateDateRange(request.From, request.To);
        request = request with { Limit = Math.Clamp(request.Limit, 1, MaxLimit) };
        return repository.GetLogsAsync(request, serviceName, ct);
    }

    public Task<IReadOnlyList<FrequentErrorResponse>> GetMostFrequentErrorsAsync(
        GetFrequentErrorsRequest request,
        string serviceName,
        CancellationToken ct = default)
    {
        ValidateDateRange(request.From, request.To);
        request = request with { Limit = Math.Clamp(request.Limit, 1, MaxLimit) };
        return repository.GetMostFrequentErrorsAsync(request, serviceName, ct);
    }

    public Task<IReadOnlyDictionary<DateTimeOffset, ulong>> GetLogsActivityGraphAsync(
        GetActivityGraphRequest request,
        string serviceName,
        CancellationToken ct = default)
    {
        ValidateDateRange(request.From, request.To);

        if (request.Interval < TimeSpan.FromMinutes(1))
            throw new ArgumentException("Interval cannot be less than 1 minute");
        if (request.Interval > TimeSpan.FromDays(1))
            throw new ArgumentException("Interval cannot exceed 1 day");

        return repository.GetLogsActivityGraphAsync(request, serviceName, ct);
    }
}