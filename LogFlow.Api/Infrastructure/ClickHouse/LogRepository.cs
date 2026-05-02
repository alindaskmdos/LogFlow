using ClickHouse.Client.Copy;
using ClickHouse.Client.ADO;
using LogFlow.Api.Contracts;
using Microsoft.Extensions.Options;

namespace LogFlow.Api.Infrastructure.ClickHouse;

public class LogRepository(IOptions<ClickHouseOptions> options) : ILogRepository
{
    public async Task InsertBatchAsync(IReadOnlyCollection<IngestLogRequest> logs, CancellationToken ct = default)
    {
        await using var connection = new ClickHouseConnection(options.Value.ConnectionString);

        await connection.OpenAsync(ct);

        using var command = connection.CreateCommand();

        using var bulkCopy = new ClickHouseBulkCopy(connection)
        {
            DestinationTableName = "logs",
            BatchSize = 1000,
            ColumnNames = new[]
            {
                "Timestamp", "Service", "Environment", "Level", "Message",
                "Exception", "TraceId", "SpanId", "RequestPath", "Method",
                "StatusCode", "ElapsedMs", "Properties"
            }
        };

        var rows = logs.Select(log => new object[]
        {
                        log.Timestamp.UtcDateTime,
            log.Service,
            log.Environment,
            log.Level,
            log.Message,
            log.Exception ?? string.Empty,
            log.TraceId ?? string.Empty,
            log.SpanId ?? string.Empty,
            log.RequestPath ?? string.Empty,
            log.Method ?? string.Empty,
            log.StatusCode ?? 0,
            log.ElapsedMs ?? 0,
            log.Properties ?? "{}"
        }).ToList();

        await bulkCopy.InitAsync();
        await bulkCopy.WriteToServerAsync(rows, ct);
    }

}
