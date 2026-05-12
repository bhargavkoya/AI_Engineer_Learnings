// Program.cs
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using MultiAIServiceProviders.Api.Contracts;
using MultiAIServiceProviders.Api.Repos;
using MultiAIServiceProviders.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Build-time DI validation — catches captive dependencies before first request
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// Create one AzureOpenAIClient — both chat clients share the connection pool
var azureClient = new AzureOpenAIClient(
    new Uri(builder.Configuration["AzureOpenAI:Endpoint"]!),
    new AzureKeyCredential(builder.Configuration["AzureOpenAI:ApiKey"]!));

// Register two IChatClient implementations under different keys
builder.Services.AddKeyedSingleton<IChatClient>("fast",
    azureClient.GetChatClient(
        builder.Configuration["AzureOpenAI:FastDeployment"]!)
        .AsIChatClient());

builder.Services.AddKeyedSingleton<IChatClient>("smart",
    azureClient.GetChatClient(
        builder.Configuration["AzureOpenAI:SmartDeployment"]!)
        .AsIChatClient());

// Plugin dependency — Scoped to match DbContext if you add EF Core later
builder.Services.AddScoped<IProductRepository, InMemoryProductRepository>();
builder.Services.AddScoped<ProductSearchPlugin>();

// Register Semantic Kernel with the Azure connector and a stateless plugin
builder.Services.AddKernel()
    .AddAzureOpenAIChatCompletion(
        deploymentName: builder.Configuration["AzureOpenAI:SmartDeployment"]!,
        endpoint: builder.Configuration["AzureOpenAI:Endpoint"]!,
        apiKey: builder.Configuration["AzureOpenAI:ApiKey"]!);
// Stateless plugins can go here: .Plugins.AddFromType<DateTimePlugin>()

builder.Services.AddScoped<IModelRouter, ModelRoutingService>();
builder.Services.AddScoped<IAssistantService, AssistantService>();

var app = builder.Build();

// Fast endpoint - cost-optimised routing
app.MapPost("/chat/fast", async (IModelRouter router, ChatRequest req) =>
    Results.Ok(await router.CompleteAsync(req.Message, needsReasoning: false)));

// Smart endpoint - reasoning-capable routing
app.MapPost("/chat/smart", async (IModelRouter router, ChatRequest req) =>
    Results.Ok(await router.CompleteAsync(req.Message, needsReasoning: true)));

// Assistant endpoint - uses Kernel + plugin
app.MapPost("/assistant", async (IAssistantService assistant, AssistantRequest req) =>
    Results.Ok(await assistant.AskWithPluginAsync(req.Prompt)));

app.Run();

// Request models
public record ChatRequest(string Message);
public record AssistantRequest(string Prompt);