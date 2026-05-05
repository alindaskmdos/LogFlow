using ClickHouse.Driver;
using Microsoft.Extensions.Options;
using LogFlow.Api.Contracts;
using LogFlow.Api.Infrastructure.ClickHouse;
using LogFlow.Api.Infrastructure.ClickHouse.Interfaces;
using LogFlow.Api.Services;
using LogFlow.Api.Services.Interfaces;
using FluentValidation;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using LogFlow.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ClickHouseOptions>(
    builder.Configuration.GetSection("ClickHouse"));
builder.Services.AddSingleton<ClickHouseClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<ClickHouseOptions>>().Value;
    return new ClickHouseClient(options.ConnectionString);
});
builder.Services.AddScoped<ILogRepository, LogRepository>();
builder.Services.AddScoped<ILogIngestionService, LogIngestionService>();
builder.Services.AddScoped<IStatisticsRepository, StatisticsRepository>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();

builder.Services.AddControllers();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

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

app.UseExceptionHandler();

app.MapControllers();

app.Run();