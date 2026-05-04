using System.Collections.ObjectModel;
using ClickHouse.Driver;
using ClickHouse.Driver.ADO.Parameters;
using LogFlow.Api.Contracts;
using LogFlow.Api.Infrastructure.ClickHouse.Interfaces;

namespace LogFlow.Api.Infrastructure.ClickHouse;

public class StatisticsRepository(ClickHouseClient client) : IStatisticsRepository
{
    public async Task<IReadOnlyDictionary<DateTimeOffset, long>> GetLogsActivityGraphAsync(DateTimeOffset from, DateTimeOffset to, TimeSpan interval, string? level, string? service, CancellationToken ct = default)
    {
        long seconds = (long)interval.TotalSeconds;

        var sql = $@"
            SELECT 
                COUNT(*) AS TotalCount,
                toStartOfInterval(Timestamp, INTERVAL {seconds} SECOND) AS Interval
            FROM logs
            WHERE Timestamp >= {{from:DateTime64(3)}} 
                AND Timestamp <= {{to:DateTime64(3)}}";

        if (!string.IsNullOrEmpty(level))
            sql += " AND Level = {level:String}";
        if (!string.IsNullOrEmpty(service))
            sql += " AND Service = {service:String}";

        sql += " GROUP BY Interval ORDER BY Interval";

        var parameters = new ClickHouseParameterCollection();
        parameters.Add(new ClickHouseDbParameter { ParameterName = "from", Value = from.UtcDateTime });
        parameters.Add(new ClickHouseDbParameter { ParameterName = "to", Value = to.UtcDateTime });
        if (!string.IsNullOrEmpty(level))
            parameters.Add(new ClickHouseDbParameter { ParameterName = "level", Value = level });
        if (!string.IsNullOrEmpty(service))
            parameters.Add(new ClickHouseDbParameter { ParameterName = "service", Value = service });

        await using var reader = await client.ExecuteReaderAsync(sql, parameters, cancellationToken: ct);

        var totalCountCol = reader.GetOrdinal("TotalCount");
        var intervalCol = reader.GetOrdinal("Interval");

        var graph = new Dictionary<DateTimeOffset, long>();

        while (await reader.ReadAsync(ct))
        {
            var timestamp = new DateTimeOffset(reader.GetDateTime(intervalCol), TimeSpan.Zero);
            var totalCount = reader.GetInt64(totalCountCol);

            graph.Add(timestamp, totalCount);
        }

        return graph;
    }

    public async Task<IReadOnlyList<LogResponse>> GetLogsAsync(
        DateTimeOffset from, DateTimeOffset to,
        string? level, string? service,
        int limit,
        CancellationToken ct = default)
    {
        var sql = @"
        SELECT
            Timestamp, Service, Environment, Level, Message,
            Exception, TraceId, SpanId, RequestPath, Method,
            StatusCode, ElapsedMs, Properties
        FROM logs
        WHERE Timestamp >= {from:DateTime64(3)} 
          AND Timestamp <= {to:DateTime64(3)}";

        if (!string.IsNullOrEmpty(level))
            sql += " AND Level = {level:String}";
        if (!string.IsNullOrEmpty(service))
            sql += " AND Service = {service:String}";

        sql += " LIMIT {limit:Int32}";

        var parameters = new ClickHouseParameterCollection();
        parameters.Add(new ClickHouseDbParameter { ParameterName = "from", Value = from.UtcDateTime });
        parameters.Add(new ClickHouseDbParameter { ParameterName = "to", Value = to.UtcDateTime });
        if (!string.IsNullOrEmpty(level))
            parameters.Add(new ClickHouseDbParameter { ParameterName = "level", Value = level });
        if (!string.IsNullOrEmpty(service))
            parameters.Add(new ClickHouseDbParameter { ParameterName = "service", Value = service });
        parameters.Add(new ClickHouseDbParameter { ParameterName = "limit", Value = limit });

        await using var reader = await client.ExecuteReaderAsync(sql, parameters, cancellationToken: ct);

        var tsCol = reader.GetOrdinal("Timestamp");
        var serviceCol = reader.GetOrdinal("Service");
        var envCol = reader.GetOrdinal("Environment");
        var levelCol = reader.GetOrdinal("Level");
        var messageCol = reader.GetOrdinal("Message");
        var exceptionCol = reader.GetOrdinal("Exception");
        var traceIdCol = reader.GetOrdinal("TraceId");
        var spanIdCol = reader.GetOrdinal("SpanId");
        var requestPathCol = reader.GetOrdinal("RequestPath");
        var methodCol = reader.GetOrdinal("Method");
        var statusCodeCol = reader.GetOrdinal("StatusCode");
        var elapsedMsCol = reader.GetOrdinal("ElapsedMs");
        var propertiesCol = reader.GetOrdinal("Properties");

        var logs = new List<LogResponse>(limit);
        while (await reader.ReadAsync(ct))
        {
            logs.Add(new LogResponse
            {
                Timestamp = new DateTimeOffset(reader.GetDateTime(tsCol), TimeSpan.Zero),
                Service = reader.GetString(serviceCol),
                Environment = reader.GetString(envCol),
                Level = reader.GetString(levelCol),
                Message = reader.GetString(messageCol),
                Exception = reader.IsDBNull(exceptionCol) ? null : reader.GetString(exceptionCol),
                TraceId = reader.IsDBNull(traceIdCol) ? null : reader.GetString(traceIdCol),
                SpanId = reader.IsDBNull(spanIdCol) ? null : reader.GetString(spanIdCol),
                RequestPath = reader.IsDBNull(requestPathCol) ? null : reader.GetString(requestPathCol),
                Method = reader.IsDBNull(methodCol) ? null : reader.GetString(methodCol),
                StatusCode = reader.IsDBNull(statusCodeCol) ? null : reader.GetInt32(statusCodeCol),
                ElapsedMs = reader.IsDBNull(elapsedMsCol) ? null : reader.GetInt64(elapsedMsCol),
                Properties = reader.IsDBNull(propertiesCol) ? null : reader.GetString(propertiesCol)
            });
        }

        return logs;
    }

    public async Task<IReadOnlyList<FrequentErrorResponse>> GetMostFrequentErrorsAsync(DateTimeOffset from, DateTimeOffset to, string? service, int limit, CancellationToken ct = default)
    {
        var sql = @"
            SELECT
                Message,
                Exception, 
                COUNT(*) AS TotalCount,
                MAX(Timestamp) AS LastOccurrence
            FROM logs
            WHERE Level = 'Error'
            AND Timestamp >= {from:DateTime64(3)} 
            AND Timestamp <= {to:DateTime64(3)}";

        if (!string.IsNullOrEmpty(service))
            sql += " AND Service = {service:String}";

        sql += @" 
            GROUP BY Message, Exception
            ORDER BY TotalCount Desc
            LIMIT {limit:Int32}";

        var parameters = new ClickHouseParameterCollection();
        parameters.Add(new ClickHouseDbParameter { ParameterName = "from", Value = from.UtcDateTime });
        parameters.Add(new ClickHouseDbParameter { ParameterName = "to", Value = to.UtcDateTime });
        if (!string.IsNullOrEmpty(service))
            parameters.Add(new ClickHouseDbParameter { ParameterName = "service", Value = service });
        parameters.Add(new ClickHouseDbParameter { ParameterName = "limit", Value = limit });

        await using var reader = await client.ExecuteReaderAsync(sql, parameters, cancellationToken: ct);

        var messageCol = reader.GetOrdinal("Message");
        var exceptionCol = reader.GetOrdinal("Exception");
        var totalCountCol = reader.GetOrdinal("TotalCount");
        var lastOccurrenceCol = reader.GetOrdinal("LastOccurrence");

        var logs = new List<FrequentErrorResponse>();
        while (await reader.ReadAsync(ct))
        {
            logs.Add(new FrequentErrorResponse
            {
                Message = reader.GetString(messageCol),
                Exception = reader.IsDBNull(exceptionCol) ? null : reader.GetString(exceptionCol),
                TotalCount = reader.GetInt64(totalCountCol),
                LastOccurrence = new DateTimeOffset(reader.GetDateTime(lastOccurrenceCol), TimeSpan.Zero)
            });
        }

        return logs;
    }
}
