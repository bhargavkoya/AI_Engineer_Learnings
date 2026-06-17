// Program.cs - MCP server host

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using WeatherSearchMcpServer.Tools;

var builder = Host.CreateApplicationBuilder(args);

// Register both tool classes
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<WeatherTools>()
    .WithTools<SearchTools>();

// Log to stderr so stdout stays clean for MCP message framing
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Warning;
});

// Shared HttpClient for the NWS API - injected by the SDK into WeatherTools methods
using var httpClient = new HttpClient { BaseAddress = new Uri("https://api.weather.gov") };
httpClient.DefaultRequestHeaders.UserAgent.Add(
    new ProductInfoHeaderValue("weather-search-mcp", "1.0"));
builder.Services.AddSingleton(httpClient);

await builder.Build().RunAsync();