using System.Text.Json;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
using LogFlow.Sdk.Contracts;
using LogFlow.Sdk.Options;

namespace LogFlow.Sdk.Sinks;

public class LogFlowSink(LogFlowClient client, LogFlowOptions options) : IBatchedLogEventSink
{
    private static readonly string[] AllowedSources =
    {
        "Serilog.AspNetCore.RequestLoggingMiddleware",
        // "Microsoft.AspNetCore.Hosting.Diagnostics"
    };

    public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
    {
        var logs = new List<IngestLogRequest>();

        foreach (var log in batch)
        {
            if (options.IncludeOnlyRequestLogs && !IsRequestLog(log))
                continue;

            var newLog = new IngestLogRequest
            {
                Timestamp = log.Timestamp,
                Environment = GetProperty(log, "Environment") ?? string.Empty,
                Level = log.Level.ToString(),
                Message = log.RenderMessage(),
                Exception = log.Exception?.ToString(),
                TraceId = log.TraceId?.ToString(),
                SpanId = log.SpanId?.ToString(),
                RequestPath = GetProperty(log, "RequestPath"),
                Method = GetProperty(log, "RequestMethod")
                    ?? GetProperty(log, "Method"),
                StatusCode = GetProperty(log, "StatusCode"),
                ElapsedMs = GetProperty(log, "Elapsed")
                    ?? GetProperty(log, "ElapsedMs"),
                Properties = log.Properties.Count == 0
                    ? null
                    : JsonSerializer.Serialize(log.Properties.ToDictionary(
                        item => item.Key,
                        item => item.Value.ToString().Trim('"')))
            };

            logs.Add(newLog);
        }

        if (logs.Count == 0)
            return;

        await client.IngestAsync(logs);
    }

    private static bool IsRequestLog(LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue("SourceContext", out var sourceValue))
        {
            var source = sourceValue.ToString().Trim('"');
            return Array.Exists(AllowedSources, s => s == source);
        }
        return false;
    }

    private static string? GetProperty(LogEvent logEvent, string propertyName)
    {
        return logEvent.Properties.TryGetValue(propertyName, out var value)
            ? value.ToString().Trim('"')
            : null;
    }

    public Task OnEmptyBatchAsync()
    {
        return Task.CompletedTask;
    }
}