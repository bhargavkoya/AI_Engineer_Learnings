using Azure;
using Azure.AI.OpenAI;
using MeaiMinimalApi.Contracts;
using MeaiMinimalApi.Middleware;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();

// Build the provider-backed client once
builder.Services.AddSingleton<IChatClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    var endpoint = config["AzureOpenAI:Endpoint"]
        ?? throw new InvalidOperationException("AzureOpenAI:Endpoint is missing.");

    var apiKey = config["AzureOpenAI:ApiKey"]
        ?? throw new InvalidOperationException("AzureOpenAI:ApiKey is missing.");

    var deployment = config["AzureOpenAI:Deployment"]
        ?? throw new InvalidOperationException("AzureOpenAI:Deployment is missing.");

    var azureClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));

    IChatClient baseClient =
        azureClient
            .GetChatClient(deployment)
            .AsIChatClient();

    var logger = sp.GetRequiredService<ILogger<SimpleLoggingChatClient>>();

    // Wrap with custom middleware
    return new SimpleLoggingChatClient(baseClient, logger);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/chat", async (
    ChatRequest request,
    IChatClient chatClient,
    CancellationToken ct) =>
{
    var messages = new[]
    {
        new ChatMessage(ChatRole.System, "You are a concise assistant for .NET developers."),
        new ChatMessage(ChatRole.User, request.Message)
    };

    var response = await chatClient.GetResponseAsync(
        messages,
        new ChatOptions
        {
            Temperature = 0.2f
        },
        ct);

    return Results.Ok(new ChatReply(response.Text ?? string.Empty));
});

app.Run();