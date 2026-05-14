using System.Net.Http.Json;
using LogFlow.Sdk.Contracts;
using LogFlow.Sdk.Options;

namespace LogFlow.Sdk;

public class LogFlowClient
{
    private readonly HttpClient _client;

    public LogFlowClient(HttpClient client, LogFlowOptions options)
    {
        _client = client;

        if (_client.BaseAddress == null && !string.IsNullOrWhiteSpace(options.Url))
            _client.BaseAddress = new Uri(options.Url.TrimEnd('/') + "/", UriKind.Absolute);

        _client.DefaultRequestHeaders.Remove("x-api-key");
        if (!string.IsNullOrWhiteSpace(options.ApiKey))
            _client.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);
    }

    public async Task IngestAsync(IReadOnlyCollection<IngestLogRequest> logs, CancellationToken ct = default)
    {
        if (logs.Count == 0)
            return;

        using var response = await _client.PostAsJsonAsync("log/LogIngestion", logs, ct);
        response.EnsureSuccessStatusCode();
    }
}
