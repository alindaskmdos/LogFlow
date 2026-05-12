using ClickHouse.Driver;
using FluentValidation;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using Serilog;
using HealthChecks.UI.Client;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.RateLimiting;
using LogFlow.Api.Contracts;
using LogFlow.Api.Infrastructure.ClickHouse;
using LogFlow.Api.Infrastructure.ClickHouse.Interfaces;
using LogFlow.Api.Services;
using LogFlow.Api.Services.Interfaces;
using LogFlow.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ClickHouseOptions>(
    builder.Configuration.GetSection("ClickHouse"));
builder.Services.AddSingleton<ClickHouseClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<ClickHouseOptions>>().Value;
    return new ClickHouseClient(options.ConnectionString);
});
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = "logflow:";
});
builder.Services.AddSingleton<LogChannel>();
builder.Services.AddHostedService<LogBatchWorker>();
builder.Services.AddScoped<ILogRepository, LogRepository>();
builder.Services.AddScoped<ILogIngestionService, LogIngestionService>();
builder.Services.AddScoped<IStatisticsRepository, StatisticsRepository>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
builder.Services.AddScoped<ApiKeyService>();
builder.Services.AddScoped<IApiKeyService, CacheApiKeyService>();

builder.Services.AddControllers();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Name = "x-api-key",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Введите API-ключ"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("ApiKey", document)] = []
    });
});

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("LogIngestionPolicy", opt =>
    {
        opt.PermitLimit = 1000;
        opt.Window = TimeSpan.FromSeconds(10);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 100;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, token) =>
    {
        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Too Many Requests",
            Detail = "API rate limit exceeded. Please try again later.",
            Instance = context.HttpContext.Request.Path
        };

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, token);
    };
});

builder.Services.AddHealthChecks()
    .AddCheck<ClickHouseHealthCheck>("clickhouse", HealthStatus.Unhealthy)
    .AddCheck<SeqHealthCheck>("seq", HealthStatus.Degraded)
    .AddRedis(builder.Configuration["Redis:ConnectionString"]!)
    .AddSeqPublisher(options =>
    {
        options.Endpoint = builder.Configuration["Serilog:WriteTo:1:Args:serverUrl"]!;
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var client = scope.ServiceProvider.GetRequiredService<ClickHouseClient>();
    client.RegisterBinaryInsertType<IngestLogRequest>();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseExceptionHandler();

app.UseRateLimiter();

app.UseMiddleware<AuthMiddleware>();

app.MapControllers();

app.Run();