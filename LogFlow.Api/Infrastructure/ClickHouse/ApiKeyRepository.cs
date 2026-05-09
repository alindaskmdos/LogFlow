using ClickHouse.Driver;
using ClickHouse.Driver.ADO.Parameters;
using LogFlow.Api.Infrastructure.ClickHouse.Interfaces;

namespace LogFlow.Api.Infrastructure.ClickHouse;

public class ApiKeyRepository(ClickHouseClient client) : IApiKeyRepository
{
    public async Task<string?> GetServiceNameByHashAsync(
        string hash,
        CancellationToken ct = default)
    {
        const string sql = @"
            SELECT ServiceName 
            FROM api_keys 
            WHERE ApiKeyHash = {hash:String} AND IsActive = 1
            LIMIT 1";

        var parameters = new ClickHouseParameterCollection
        {
            new ClickHouseDbParameter { ParameterName = "hash", Value = hash }
        };

        await using var reader = await client.ExecuteReaderAsync(sql, parameters, cancellationToken: ct);

        if (await reader.ReadAsync(ct))
        {
            return reader.GetString(0);
        }

        return null;
    }
}