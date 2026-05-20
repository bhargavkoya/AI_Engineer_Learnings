// Program.cs
using Azure;
using Azure.AI.OpenAI;
using ReviewAnalyser;
using System.ClientModel.Primitives;

var builder = WebApplication.CreateBuilder(args);

// Register the Azure OpenAI client as a singleton -
// ChatClient is thread-safe and expensive to construct
builder.Services.AddSingleton(sp =>
{
    // Structured outputs require 2024-08-01-preview or later
    var clientOptions = new AzureOpenAIClientOptions(
        AzureOpenAIClientOptions.ServiceVersion.V2024_10_21);

    // SDK retries 429s automatically; increase from 3 to 5 for production workloads
    clientOptions.RetryPolicy = new ClientRetryPolicy(maxRetries: 5);

    return new AzureOpenAIClient(
        new Uri(builder.Configuration["AzureOpenAI:Endpoint"]!),
        new AzureKeyCredential(builder.Configuration["AzureOpenAI:Key"]!),
        clientOptions);
});

builder.Services.AddSingleton(sp =>
{
    var azureClient = sp.GetRequiredService<AzureOpenAIClient>();
    // Replace with your deployment name from Azure AI Foundry
    return azureClient.GetChatClient(builder.Configuration["AzureOpenAI:DeploymentName"]!);
});

builder.Services.AddScoped<ReviewAnalyserService>();

var app = builder.Build();
app.UseHttpsRedirection();

// Define the endpoint - a POST that accepts the review text in the request body
app.MapPost("/analyse-review", async (ReviewRequest request, ReviewAnalyserService service) =>
{
    if (string.IsNullOrWhiteSpace(request.ReviewText))
        return Results.BadRequest("ReviewText cannot be empty.");

    ReviewAnalysis? result = await service.AnalyseAsync(request.ReviewText);

    return result is null
        ? Results.Problem("Analysis failed. Check logs for schema or service errors.")
        : Results.Ok(result);
});

app.Run();

// The request body model - a simple record to receive the review text
public record ReviewRequest(string ReviewText);