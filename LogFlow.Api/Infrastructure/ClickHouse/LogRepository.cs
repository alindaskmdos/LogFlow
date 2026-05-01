using ClickHouse.Ado;
using LogFlow.Api.Contracts;

namespace LogFlow.Api.Infrastructure.ClickHouse;

public class LogRepository(ClickHouseOptions options) : ILogRepository
{
    public async Task InsertBatchAsync(IReadOnlyCollection<IngestLogRequest> logs, CancellationToken ct = default)
    {
        await using var connection = new ClickHouseConnection(options.ConnectionString);

        await connection.OpenAsync();

        using var command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO logs
            (
                Timestamp,
                Service,
                Environment,
                Level,
                Message,
                Exception,
                TraceId,
                SpanId,
                RequestPath,
                Method,
                StatusCode,
                ElapsedMs,
                Properties
            )
            VALUES @values
            """;


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
        }).ToArray();

        command.Parameters.Add(new ClickHouseParameter
        {
            ParameterName = "values",
            Value = rows
        });

        await command.ExecuteNonQueryAsync(ct);
    }

}
