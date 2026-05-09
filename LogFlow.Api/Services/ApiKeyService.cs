using System.Security.Cryptography;
using System.Text;
using LogFlow.Api.Infrastructure.ClickHouse.Interfaces;
using LogFlow.Api.Services.Interfaces;

namespace LogFlow.Api.Services;

public class ApiKeyService(IApiKeyRepository repository) : IApiKeyService
{
    public async Task<string?> GetServiceNameAsync(string apiKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }

        var hash = ComputeSha256HexLower(apiKey);
        return await repository.GetServiceNameByHashAsync(hash, ct);
    }

    private static string ComputeSha256HexLower(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
