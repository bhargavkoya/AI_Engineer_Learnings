// Program.cs
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;

var builder = WebApplication.CreateBuilder(args);

// ── Embedding generator - shared across all providers ──
builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
{
    var client = new AzureOpenAIClient(
        new Uri(builder.Configuration["AzureOpenAI:Endpoint"]!),
        new AzureKeyCredential(builder.Configuration["AzureOpenAI:ApiKey"]!));

    return client.GetEmbeddingClient("text-embedding-3-small")
                 .AsIEmbeddingGenerator();
});

// ── Vector store - swap provider via config ──
var provider = builder.Configuration["VectorStore:Provider"] ?? "qdrant";

switch (provider.ToLowerInvariant())
{
    case "qdrant":
        // Local Docker default - override in env for Qdrant Cloud
        builder.Services.AddQdrantVectorStore(
            host: builder.Configuration["Qdrant:Host"] ?? "localhost",
            port: 6334,
            https: false);
        break;

    case "azureaisearch":
        builder.Services.AddAzureAISearchVectorStore(
            new Uri(builder.Configuration["AzureSearch:Endpoint"]!),
            new AzureKeyCredential(builder.Configuration["AzureSearch:Key"]!));
        break;

    default:
        throw new InvalidOperationException(
            $"Unknown vector store provider: '{provider}'. Use 'qdrant' or 'azureaisearch'.");
}

builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();