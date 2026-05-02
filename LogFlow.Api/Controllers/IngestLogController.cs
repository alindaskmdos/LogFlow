using LogFlow.Api.Contracts;
using LogFlow.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LogFlow.Api.Controllers;

[ApiController]
[Route("log/[controller]")]
public class IngestLogController(ILogIngestionService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> IngestLog(IReadOnlyCollection<IngestLogRequest> request, CancellationToken ct = default)
    {
        await service.IngestAsync(request, ct);

        return Accepted();
    }
}
