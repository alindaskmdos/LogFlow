namespace LogFlow.Api.Services.Interfaces;

public interface IApiKeyService
{
    Task<string?> GetServiceNameAsync(
        string apiKey, 
        CancellationToken ct = default);
}
