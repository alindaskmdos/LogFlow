using ClickHouse.Driver;
using LogFlow.Api.Contracts;
using LogFlow.Api.Infrastructure.ClickHouse.Interfaces;

namespace LogFlow.Api.Infrastructure.ClickHouse;

public class LogRepository(ClickHouseClient client) : ILogRepository
{
    public async Task InsertAsync(IReadOnlyCollection<IngestLogRequest> logs, CancellationToken ct = default)
    {
        await client.InsertBinaryAsync("logs", logs, cancellationToken: ct);
    }
}
