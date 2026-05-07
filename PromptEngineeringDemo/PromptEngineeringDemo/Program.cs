using Azure.AI.OpenAI;
using Microsoft.SemanticKernel;
using PromptEngineeringDemo.Services;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration.GetSection("AzureOpenAI");
string endpoint = config["Endpoint"]!;
string apiKey = config["ApiKey"]!;
string deployment = config["DeploymentName"]!;

// Register Semantic Kernel
builder.Services.AddSingleton(sp =>
    Kernel.CreateBuilder()
        .AddAzureOpenAIChatCompletion(deployment, endpoint, apiKey)
        .Build()
);

// Register ChatClient for direct API access in few-shot scenarios
builder.Services.AddSingleton(sp =>
    new AzureOpenAIClient(new Uri(endpoint), new Azure.AzureKeyCredential(apiKey))
        .GetChatClient(deployment)
);

builder.Services.AddScoped<PromptDemoService>();

var app = builder.Build();

// Summarisation endpoint
app.MapPost("/summarise", async (SummariseRequest req, PromptDemoService svc) =>
{
    var result = await svc.SummariseAsync(req.Text);
    return result is null ? Results.Problem("Failed to summarise") : Results.Ok(result);
});

// Classification endpoint
app.MapPost("/classify", async (ClassifyRequest req, PromptDemoService svc) =>
{
    var result = await svc.ClassifyAsync(req.Text);
    return result is null ? Results.Problem("Failed to classify") : Results.Ok(result);
});

// Extraction endpoint
app.MapPost("/extract", async (ExtractRequest req, PromptDemoService svc) =>
{
    var result = await svc.ExtractAsync(req.Document);
    return result is null ? Results.Problem("Failed to extract") : Results.Ok(result);
});

app.Run();

record SummariseRequest(string Text);
record ClassifyRequest(string Text);
record ExtractRequest(string Document);