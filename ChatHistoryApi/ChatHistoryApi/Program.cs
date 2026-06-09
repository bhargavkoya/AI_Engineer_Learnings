using ChatHistoryApi;
using ChatHistoryApi.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// CosmosClient is thread-safe - always Singleton
builder.Services.AddSingleton(_ => new CosmosClient(
    connectionString: builder.Configuration["CosmosDB:ConnectionString"],
    clientOptions: new CosmosClientOptions
    {
        ConnectionMode = ConnectionMode.Direct,
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    }));

// Semantic Kernel - Singleton; the kernel holds no per-request state
builder.Services.AddSingleton(sp =>
{
    var cfg = builder.Configuration;
    return Kernel.CreateBuilder()
        .AddAzureOpenAIChatCompletion(
            deploymentName: cfg["AzureOpenAI:DeploymentName"]!,
            endpoint: cfg["AzureOpenAI:Endpoint"]!,
            apiKey: cfg["AzureOpenAI:ApiKey"]!)
        .Build();
});

// Scoped - new instance per request
builder.Services.AddScoped<CosmosChatHistoryStore>();
builder.Services.AddScoped<HybridChatService>();

var app = builder.Build();

// POST /api/chat/{sessionId}
app.MapPost("/api/chat/{sessionId}", async (
    string sessionId,
    ChatRequest request,
    CosmosChatHistoryStore historyStore,
    HybridChatService chatService,
    CancellationToken ct) =>
{
    const string SystemPrompt =
        "You are a helpful .NET assistant specialising in Semantic Kernel and Azure OpenAI.";

    // 1. Load or create history for this session from Cosmos DB
    var chatHistory = await historyStore.LoadAsync(sessionId, SystemPrompt, ct);

    // 2. Chat - sliding window + summarisation applied inside
    var reply = await chatService.ChatAsync(chatHistory, request.Message, ct);

    // 3. Persist the updated history back to Cosmos DB
    await historyStore.SaveAsync(sessionId, chatHistory, ct);

    return Results.Ok(new ChatResponse(reply, chatHistory.Count));
});

app.Run();