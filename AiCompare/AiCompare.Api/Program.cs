// Program.cs - MEAI version
using AiCompare.Api;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

var endpoint = builder.Configuration["AzureOpenAI:Endpoint"]
    ?? throw new InvalidOperationException("AzureOpenAI:Endpoint is not configured.");
var apiKey = builder.Configuration["AzureOpenAI:ApiKey"]
    ?? throw new InvalidOperationException("AzureOpenAI:ApiKey is not configured.");
var deploymentName = builder.Configuration["AzureOpenAI:Deployment"]
    ?? throw new InvalidOperationException("AzureOpenAI:Deployment is not configured.");

//MEAI integration with Azure OpenAI
builder.Services.AddChatClient(
    new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey))
        .GetChatClient(deploymentName)
        .AsIChatClient());
builder.Services.AddScoped<SupportChatService>();

// SK integration with Azure OpenAI and plugin registration
builder.Services.AddHttpClient<NugetPlugin>();
builder.Services.AddKernel()
    .AddAzureOpenAIChatCompletion(
        deploymentName: builder.Configuration["AzureOpenAI:Deployment"]!,
        endpoint: builder.Configuration["AzureOpenAI:Endpoint"]!,
        apiKey: builder.Configuration["AzureOpenAI:ApiKey"]!)
    .Plugins.AddFromType<NugetPlugin>();

builder.Services.AddScoped<SupportChatServiceSK>();




builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();