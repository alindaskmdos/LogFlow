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
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        };

        _channel = Channel.CreateBounded<IngestLogRequest>(options);
    }

    public async Task WriteAsync(IEnumerable<IngestLogRequest> logs, CancellationToken ct = default)
    {
        foreach (var log in logs)
            await _channel.Writer.WriteAsync(log, ct);
    }
}
