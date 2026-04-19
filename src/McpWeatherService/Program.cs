using McpWeatherService.Endpoints;
using McpWeatherService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddWeatherServices(builder.Configuration);

var app = builder.Build();

app.MapHealthEndpoints();
app.MapMcp("/mcp");

app.Run();

public partial class Program;
