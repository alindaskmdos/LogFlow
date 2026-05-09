namespace LogFlow.Api.Infrastructure.ClickHouse.Interfaces;

public interface IApiKeyRepository
{
    Task<string?> GetServiceNameByHashAsync(
        string hash,
        CancellationToken ct = default);
}