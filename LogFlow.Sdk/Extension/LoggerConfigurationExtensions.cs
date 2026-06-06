using Serilog.Configuration;
using Serilog;
using Serilog.Sinks.PeriodicBatching;
using LogFlow.Sdk.Options;

namespace LogFlow.Sdk.Sinks;

public static class LoggerConfigurationExtensions
{
    public static LoggerConfiguration LogFlow(
        this LoggerSinkConfiguration writeTo,
        Action<LogFlowOptions> configureOptions)
    {
        var options = new LogFlowOptions();
        configureOptions(options);

        var client = new LogFlowClient(new HttpClient(), options);
        var sink = new LogFlowSink(client, options);

        var batchingOptions = new PeriodicBatchingSinkOptions
        {
            BatchSizeLimit = options.BatchSize,
            Period = options.Period
        };

        var batchingSink = new PeriodicBatchingSink(sink, batchingOptions);

        return writeTo.Sink(batchingSink);
    }
}
