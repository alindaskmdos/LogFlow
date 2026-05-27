using System.Threading.Channels;
using LogFlow.Api.Contracts;

namespace LogFlow.Api.Services;

public class LogChannel
{
    private readonly int Capacity = 1000;
    private readonly Channel<IngestLogRequest> _channel;

    public ChannelReader<IngestLogRequest> Reader => _channel.Reader;

    public LogChannel()
    {
        var options = new BoundedChannelOptions(Capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };

        _channel = Channel.CreateBounded<IngestLogRequest>(options);
    }

    public async Task<bool> WriteAsync(
        IEnumerable<IngestLogRequest> logs,
        TimeSpan timeout,
        CancellationToken ct = default)
    {
        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

        try
        {
            foreach (var log in logs)
                await _channel.Writer.WriteAsync(log, linkedCts.Token);

            return true;
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            return false;
        }
    }
}
