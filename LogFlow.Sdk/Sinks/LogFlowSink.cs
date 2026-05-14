using System.Text.Json;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
using LogFlow.Sdk.Contracts;

namespace LogFlow.Sdk.Sinks;

public class LogFlowSink(LogFlowClient client) : IBatchedLogEventSink
{
    public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
    {
        List<IngestLogRequest> logs = new();
        foreach (var log in batch)
        {
            var newLog = new IngestLogRequest()
            {
                Timestamp = log.Timestamp,
                Environment = log.Properties.TryGetValue("Environment", out var environment)
                    ? environment.ToString().Trim('"') : null,
                Level = log.Level.ToString(),
                Message = log.RenderMessage(),
                Exception = log.Exception?.ToString(),
                TraceId = log.TraceId?.ToString(),
                SpanId = log.SpanId?.ToString(),
                RequestPath = log.Properties.TryGetValue("RequestPath", out var requestPath)
                    ? requestPath.ToString().Trim('"') : null,
                Method = log.Properties.TryGetValue("Method", out var method)
                    ? method.ToString().Trim('"') : null,
                StatusCode = log.Properties.TryGetValue("StatusCode", out var statusCode)
                    ? statusCode.ToString().Trim('"') : null,
                ElapsedMs = log.Properties.TryGetValue("ElapsedMs", out var elapsedMs)
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

    public Task OnEmptyBatchAsync()
    {
        return Task.CompletedTask;
    }

}
