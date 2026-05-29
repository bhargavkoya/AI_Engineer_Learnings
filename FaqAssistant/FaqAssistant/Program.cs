using Azure;
using Azure.AI.OpenAI;
using FaqAssistant.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using Microsoft.SemanticKernel.Connectors.InMemory;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

// --- Embedding Generator ---
builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(_ =>
{
    var azureClient = new AzureOpenAIClient(
        new Uri(builder.Configuration["AzureOpenAI:Endpoint"]!),
        new AzureKeyCredential(builder.Configuration["AzureOpenAI:ApiKey"]!));

    return azureClient
        .GetEmbeddingClient(builder.Configuration["AzureOpenAI:EmbeddingModel"]!)
        .AsIEmbeddingGenerator();
});

// --- Vector Store Backend (environment-driven) ---
var provider = builder.Configuration["VectorStore:Provider"] ?? "InMemory";
switch (provider)
{
    case "AzureAISearch":
        builder.Services.AddAzureAISearchVectorStore(
            new Uri(builder.Configuration["AzureSearch:Endpoint"]!),
            new AzureKeyCredential(builder.Configuration["AzureSearch:Key"]!));
        break;

    case "CosmosNoSql":
        builder.Services.AddCosmosNoSqlVectorStore(
            builder.Configuration["CosmosDB:ConnectionString"]!,
            databaseName: "FaqDatabase");
        break;

    default:
        builder.Services.AddInMemoryVectorStore();
        break;
}

// --- Semantic Kernel (chat only) ---
builder.Services.AddKernel()
    .AddAzureOpenAIChatCompletion(
        deploymentName: builder.Configuration["AzureOpenAI:ChatModel"]!,
        endpoint: builder.Configuration["AzureOpenAI:Endpoint"]!,
        apiKey: builder.Configuration["AzureOpenAI:ApiKey"]!);

// --- Application Services ---
builder.Services.AddScoped<FaqVectorService>();
builder.Services.AddScoped<FaqAssistantService>();

var app = builder.Build();
app.MapControllers();
app.Run();