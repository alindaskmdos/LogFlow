using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace LogFlow.DemoApi.Middlewares;

public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var statusCode = exception switch
        {
            InvalidOperationException => StatusCodes.Status400BadRequest,
            TimeoutException => StatusCodes.Status504GatewayTimeout,
            ApplicationException => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError
        };

        logger.LogError(
            exception,
            "exception while {Method} {Path}",
            context.Request.Method,
            context.Request.Path);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(statusCode),
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] =
            Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

        problemDetails.Extensions["exceptionType"] =
            exception.GetType().FullName;

        if (environment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] =
                exception.ToString();
        }

        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static string GetTitle(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad request",
            StatusCodes.Status404NotFound => "Not found",
            StatusCodes.Status503ServiceUnavailable => "Service unavailable",
            StatusCodes.Status504GatewayTimeout => "Gateway timeout",
            _ => "Internal server error"
        };
    }
}