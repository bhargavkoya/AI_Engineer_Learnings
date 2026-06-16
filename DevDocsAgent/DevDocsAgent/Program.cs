using Azure.AI.OpenAI;
using Azure.Identity;
using DevDocsAgent.Tools;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

var builder = WebApplication.CreateBuilder(args);

var endpoint = builder.Configuration["AzureOpenAI:Endpoint"]
    ?? throw new InvalidOperationException("AzureOpenAI:Endpoint is required");
var deployment = builder.Configuration["AzureOpenAI:Deployment"] ?? "gpt-4o";

// Register the agent using factory delegate — tools go here, not on a service
builder.AddAIAgent("devdocs", (sp, key) =>
{
    var chatClient = new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
        .GetChatClient(deployment);

    return chatClient.AsAIAgent(
        instructions: """
            You are a helpful assistant for .NET developers at this company.
            When asked a question, use the search_docs tool to find relevant
            internal documentation before answering. Always cite the documentation
            you found. If the tool returns no results, say so honestly.
            """,
        tools: [AIFunctionFactory.Create(DocumentationSearchTool.SearchDocs)]
    );
});

// MCP server hosting
builder.Services.AddMcpServer()
    .WithHttpTransport();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Expose agent via OpenAI Chat Completions protocol
var agent = app.Services.GetRequiredKeyedService<AIAgent>("devdocs");
app.MapOpenAIChatCompletions(agent);

// MCP endpoint - connect VS Code or Claude Desktop to http://localhost:5000/mcp
app.MapMcp("/mcp");

// Health endpoint - required for container deployments and load balancers
app.MapGet("/health", () => Results.Ok(new { status = "healthy", agent = "devdocs" }));

app.Run();