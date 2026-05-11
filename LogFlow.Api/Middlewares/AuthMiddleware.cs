using LogFlow.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LogFlow.Api.Middlewares;

public class AuthMiddleware(RequestDelegate next)
{
    private const string ServiceNameItemKey = "ServiceName";

    public async Task InvokeAsync(HttpContext context, IApiKeyService service)
    {
        if (context.Request.Path.StartsWithSegments("/openapi") ||
                context.Request.Path.StartsWithSegments("/scalar") ||
                context.Request.Path.StartsWithSegments("/swagger") ||
                context.Request.Path.StartsWithSegments("/health"))
        {
            await next(context);
            return;
        }

        var apiKey = context.Request.Headers["x-api-key"].ToString(); ;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            await WriteUnauthorized(context, context.RequestAborted);
            return;
        }

        var serviceName = await service.GetServiceNameAsync(apiKey, context.RequestAborted);
        serviceName = serviceName?.Trim();

        if (string.IsNullOrWhiteSpace(serviceName))
        {
            await WriteUnauthorized(context, context.RequestAborted);
            return;
        }

        context.Items[ServiceNameItemKey] = serviceName;
        await next(context);
    }

    private static async Task WriteUnauthorized(HttpContext context, CancellationToken ct)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = "Invalid or missing API key",
            Instance = context.Request.Path
        };

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken: ct);
    }
}
