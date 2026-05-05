// Program.cs — complete

using Microsoft.ML.Tokenizers;
using TokenVisualizer.Api;
using TokenVisualizer.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<TokenBudgetConfig>(
    builder.Configuration.GetSection("TokenBudget"));

// Tokenizer — singleton: expensive to create, cheap to reuse, thread-safe
builder.Services.AddSingleton(_ =>
    TiktokenTokenizer.CreateForModel("gpt-4o"));

// Analysis service
builder.Services.AddSingleton<TokenAnalysisService>();

// DI validation — catches lifetime mismatches at startup
builder.Host.UseDefaultServiceProvider(o =>
{
    o.ValidateScopes = true;
    o.ValidateOnBuild = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    c.SwaggerDoc("v1", new() { Title = "Token Visualiser", Version = "v1" }));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

// Analysis endpoint
app.MapPost("/api/token-analysis", (
    AnalysisRequest request,
    TokenAnalysisService service) =>
    Results.Ok(service.Analyse(request)))
    .WithName("AnalyseTokens")
    .WithSummary("Analyse token usage and cost across providers for a prompt composition")
    .Produces<AnalysisResponse>();

// Tokenization debug endpoint
app.MapGet("/api/tokenize", (string text, TiktokenTokenizer tokenizer) =>
{
    var ids = tokenizer.EncodeToIds(text);
    return Results.Ok(new
    {
        text,
        tokenCount = ids.Count,
        tokenIds = ids,
        charPerToken = Math.Round((double)text.Length / ids.Count, 2),
        note = "GPT-4o tokenizer (cl100k_base). Counts vary by model."
    });
})
.WithName("TokenizeText")
.Produces<object>();

app.Run();