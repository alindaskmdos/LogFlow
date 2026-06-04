using LogFlow.DemoApi.Endpoints;
using LogFlow.DemoApi.Middlewares;
using LogFlow.Sdk.Sinks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
     .MinimumLevel.Information()
     .Enrich.FromLogContext()
     .Enrich.WithProperty("Environment", "Development")
     .WriteTo.Console()
     .WriteTo.Logger(logFlowLogger => logFlowLogger
             .WriteTo.LogFlow(options =>
             {
                 options.Url = context.Configuration["LogFlow:Url"]
                     ?? "http://localhost:5000";

                 options.ApiKey = context.Configuration["LogFlow:ApiKey"]
                     ?? "logflow-test-1";

                 options.BatchSize = 2;
                 options.Period = TimeSpan.FromSeconds(1);
             }));
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSerilogRequestLogging();

app.MapDemoEndpoints();

app.Run();