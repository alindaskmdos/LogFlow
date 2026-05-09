using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using LogFlow.Api.Contracts;
using LogFlow.Api.Services.Interfaces;

namespace LogFlow.Api.Controllers;

[ApiController]
[Route("log/[controller]")]
public class LogIngestionController(ILogIngestionService service) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("LogIngestionPolicy")]
    public async Task<IActionResult> IngestLog(
        IReadOnlyCollection<IngestLogRequest> request,
        CancellationToken ct = default)
    {
        var serviceName = HttpContext.Items["ServiceName"] as string;

        if (string.IsNullOrWhiteSpace(serviceName))
            return Problem(
                statusCode: 500,
                title: "Internal error",
                detail: "ServiceName not found in httpcontext");

        await service.IngestAsync(request, serviceName, ct);

        return Accepted();
    }
}
