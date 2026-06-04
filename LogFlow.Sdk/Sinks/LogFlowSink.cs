using System.Text.Json;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
using LogFlow.Sdk.Contracts;

namespace LogFlow.Sdk.Sinks;

public class LogFlowSink(LogFlowClient client) : IBatchedLogEventSink
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
            if (!IsRequestLog(log))
                continue;

            var newLog = new IngestLogRequest()
            {
                Timestamp = log.Timestamp,
                Environment = log.Properties.TryGetValue("Environment", out var environment)
                    ? environment.ToString().Trim('"') : string.Empty,
                Level = log.Level.ToString(),
                Message = log.RenderMessage(),
                Exception = log.Exception?.ToString(),
                TraceId = log.TraceId?.ToString(),
                SpanId = log.SpanId?.ToString(),
                RequestPath = log.Properties.TryGetValue("RequestPath", out var requestPath)
                    ? requestPath.ToString().Trim('"') : null,
                Method = log.Properties.TryGetValue("RequestMethod", out var method)
                    ? method.ToString().Trim('"') : null,
                StatusCode = log.Properties.TryGetValue("StatusCode", out var statusCode)
                    ? statusCode.ToString().Trim('"') : null,
                ElapsedMs = log.Properties.TryGetValue("Elapsed", out var elapsedMs)
                    ? elapsedMs.ToString().Trim('"') : null,
                Properties = log.Properties.Count == 0
                    ? null : JsonSerializer.Serialize(log.Properties.ToDictionary(
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

    public Task OnEmptyBatchAsync()
    {
        return Task.CompletedTask;
    }
}