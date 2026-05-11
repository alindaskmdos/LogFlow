using ClickHouse.Driver;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class ClickHouseHealthCheck(ClickHouseClient client) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            await client.ExecuteScalarAsync("SELECT 1");

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message, exception: ex);
        }
    }
}