using ClickHouse.Driver;
using LogFlow.Api.Contracts;
using LogFlow.Api.Infrastructure.ClickHouse;
using LogFlow.Api.Infrastructure.ClickHouse.Interfaces;
using LogFlow.Api.Services;
using LogFlow.Api.Services.Interfaces;
using Microsoft.Extensions.Options;

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

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.MapControllers();

app.Run();