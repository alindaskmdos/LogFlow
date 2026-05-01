using LogFlow.Api.Infrastructure.ClickHouse;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ClickHouseOptions>(
    builder.Configuration.GetSection("ClickHouse"));


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();