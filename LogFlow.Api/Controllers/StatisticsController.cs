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
        [FromQuery] GetLogsRequest request,
        CancellationToken ct = default)
    {
        var result = await service.GetLogsAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("errors/frequent")]
    public async Task<ActionResult<IReadOnlyList<FrequentErrorResponse>>> GetFrequentErrors(
        [FromQuery] GetFrequentErrorsRequest request,
        CancellationToken ct = default)
    {
        var result = await service.GetMostFrequentErrorsAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("activity")]
    public async Task<ActionResult<IReadOnlyDictionary<DateTimeOffset, ulong>>> GetActivityGraph(
        [FromQuery] GetActivityGraphRequest request,
        CancellationToken ct = default)
    {
        var result = await service.GetLogsActivityGraphAsync(request, ct);
        return Ok(result);
    }
}