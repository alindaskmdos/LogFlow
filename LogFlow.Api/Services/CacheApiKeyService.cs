using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using LogFlow.Api.Services.Interfaces;

namespace LogFlow.Api.Services;

public class CacheApiKeyService(IDistributedCache cache, ApiKeyService service) : IApiKeyService
{
    private readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
        SlidingExpiration = TimeSpan.FromMinutes(10)
    };

    public async Task<string?> GetServiceNameAsync(string apiKey, CancellationToken ct = default)
    {
        var cacheKey = $"apikey:{ComputeSha256HexLower(apiKey)}";

        var cached = await cache.GetStringAsync(cacheKey, ct);

        if (cached != null)
            return cached == "NULL" ? null : cached;

        var serviceName = await service.GetServiceNameAsync(apiKey, ct);

        await cache.SetStringAsync(cacheKey, serviceName ?? "NULL", CacheOptions, ct);

        return serviceName;
    }

    private static string ComputeSha256HexLower(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
