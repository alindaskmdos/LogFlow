using LogFlow.Api.Infrastructure.ClickHouse;
using LogFlow.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ClickHouseOptions>(
    builder.Configuration.GetSection("ClickHouse"));

builder.Services.AddScoped<ILogRepository, LogRepository>();
builder.Services.AddScoped<ILogIngestionService, LogIngestionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();