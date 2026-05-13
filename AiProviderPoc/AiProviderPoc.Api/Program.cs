using AiProviderPoc.Api;
using Anthropic;
using Azure.AI.OpenAI;
using Google.GenAI;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;          // ApiKeyCredential

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Read provider selection from config — change "AI:Provider" to switch providers
string provider = config["AI:Provider"]
    ?? throw new InvalidOperationException("AI:Provider must be set in appsettings");

// Register IChatClient — each branch returns a different SDK's implementation
builder.Services.AddSingleton<IChatClient>(_ =>
{
    return provider.ToLowerInvariant() switch
    {
        "azure" =>
            // Azure AI Foundry: needs endpoint + key + deployment name
            new AzureOpenAIClient(
                new Uri(config["AI:Azure:Endpoint"]!),
                new ApiKeyCredential(config["AI:Azure:Key"]!))
            .GetChatClient(config["AI:Azure:Deployment"]!)
            .AsIChatClient(),

        "openai" =>
            // OpenAI direct: needs API key + model name
            new OpenAIClient(config["AI:OpenAI:Key"]!)
            .GetChatClient(config["AI:OpenAI:Model"]!)
            .AsIChatClient(),

        "anthropic" =>
            // AnthropicClient expects a ClientOptions instance (not a string API key).
            // Create a ClientOptions and pass it to the constructor.
            new AnthropicClient(
                new Anthropic.Core.ClientOptions
                {
                    ApiKey = config["AI:Anthropic:Key"]!
                })
            .AsIChatClient(),

        "gemini" =>
            // Google.GenAI: named params on Client constructor, no ClientConfig class needed.
            // Uses GOOGLE_APPLICATION_CREDENTIALS / ADC for auth; no API key required for Vertex AI.
            new Client(
                project: config["AI:Gemini:ProjectId"]!,
                location: config["AI:Gemini:Location"]!,
                vertexAI: true)
            .AsIChatClient(config["AI:Gemini:Model"]!),

        _ => throw new InvalidOperationException($"Unknown provider: '{provider}'. Valid: azure, openai, anthropic, gemini")
    };
});

// Register the service — depends only on IChatClient
builder.Services.AddScoped<TicketAnalysisService>();

var app = builder.Build();

// Single endpoint — provider is invisible here
app.MapPost("/analyze", async (AnalysisRequest request, TicketAnalysisService service, CancellationToken ct) =>
{
    var analysis = await service.AnalyzeAsync(request, ct);
    return Results.Ok(analysis);
});

app.Run();