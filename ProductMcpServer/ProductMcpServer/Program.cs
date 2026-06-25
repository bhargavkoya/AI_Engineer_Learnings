// Program.cs
using ProductMcpServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Register domain services - ProductTools will receive IProductRepository via constructor injection
builder.Services.AddScoped<IProductRepository, InMemoryProductRepository>();

builder.Services
    .AddMcpServer(options =>
    {
        // Clients see these details during the initialize handshake
        options.ServerInfo = new() { Name = "ProductServer", Version = "1.0.0" };
    })
    // Streamable HTTP transport - handles MCP over HTTP with full session support
    .WithHttpTransport(options => options.Stateless = true)
    // Scans this assembly for [McpServerToolType] and registers all [McpServerTool] methods
    .WithToolsFromAssembly();

var app = builder.Build();

// Maps: POST /mcp, GET /mcp (SSE), DELETE /mcp, GET /mcp/sse
app.MapMcp("/mcp");

// Optional health check - useful for container liveness probes
app.MapGet("/health", () => Results.Ok(new { status = "healthy", server = "ProductServer" }));

await app.RunAsync();