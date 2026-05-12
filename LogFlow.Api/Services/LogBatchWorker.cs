using LogFlow.Api.Contracts;
using LogFlow.Api.Infrastructure.ClickHouse.Interfaces;

namespace LogFlow.Api.Services;

public class LogBatchWorker(LogChannel logChannel, IServiceScopeFactory factory, ILogger<LogBatchWorker> logger) : BackgroundService
{
    private readonly int Capacity = 1000;
    private readonly List<IngestLogRequest> logs = new List<IngestLogRequest>();

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (await logChannel.Reader.WaitToReadAsync(ct))
        {
            while (logChannel.Reader.TryRead(out var log) && logs.Count < Capacity)
                logs.Add(log);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(30));
            try
            {
                while (logs.Count < Capacity && await logChannel.Reader.WaitToReadAsync(cts.Token))
                {
                    while (logs.Count < Capacity && logChannel.Reader.TryRead(out var log))
                        logs.Add(log);
                }
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested) { }

            if (logs.Count > 0)
                await FlushAsync(ct);
        }
    }

    private async Task FlushAsync(CancellationToken ct = default)
    {
        await using var scope = factory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILogRepository>();

        try
        {
            await repository.IngestAsync(logs, ct);
            logs.Clear();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
    }
}
