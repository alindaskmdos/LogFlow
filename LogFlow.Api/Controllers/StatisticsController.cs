using Microsoft.AspNetCore.Mvc;
using LogFlow.Api.Contracts;
using LogFlow.Api.Services.Interfaces;

namespace LogFlow.Api.Controllers;

[ApiController]
[Route("api/statistics")]
public class StatisticsController(IStatisticsService service) : ControllerBase
{
    [HttpGet("logs")]
    public async Task<ActionResult<IReadOnlyList<LogResponse>>> GetLogs(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        [FromQuery] string? level,
        [FromQuery] string? serviceName,
        [FromQuery] int limit = 100,
        CancellationToken ct = default)
    {
        var result = await service.GetLogsAsync(from, to, level, serviceName, limit, ct);
        return Ok(result);
    }

    [HttpGet("errors/frequent")]
    public async Task<ActionResult<IReadOnlyList<FrequentErrorResponse>>> GetFrequentErrors(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        [FromQuery] string? serviceName,
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetMostFrequentErrorsAsync(from, to, serviceName, limit, ct);
        return Ok(result);
    }

    [HttpGet("activity")]
    public async Task<ActionResult<IReadOnlyDictionary<DateTimeOffset, ulong>>> GetActivityGraph(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        [FromQuery] TimeSpan interval,
        [FromQuery] string? level,
        [FromQuery] string? serviceName,
        CancellationToken ct = default)
    {
        var result = await service.GetLogsActivityGraphAsync(from, to, interval, level, serviceName, ct);
        return Ok(result);
    }
}