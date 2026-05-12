using ClickHouse.Driver;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class SeqHealthCheck(IHttpClientFactory factory, IConfiguration config) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            var client = factory.CreateClient();
            var response = await client.GetAsync($"{config["Serilog:WriteTo:1:Args:serverUrl"]}/api", ct);

            return response.IsSuccessStatusCode ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message, exception: ex);
        }
    }
}