
namespace LogFlow.DemoApi.Endpoints;

public static class DemoEndpoints
{
    public static void MapDemoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/demo");

        group.MapGet("/ok", (
            HttpContext context) =>
        {
            return Results.Ok();
        });

        group.MapGet("/warning", (HttpContext context) =>
        {
            return Results.StatusCode(429);
        });

        group.MapGet("/error", (HttpContext context) =>
        {
            throw new InvalidOperationException("demo error");
        });

        group.MapGet("/critical", (HttpContext context) =>
        {
            throw new ApplicationException("critical demo error");
        });
    }
}