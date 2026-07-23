using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenAI.Chat;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(sp =>
{
    var kernelBuilder = Kernel.CreateBuilder();
    kernelBuilder.AddAzureOpenAIChatCompletion(
        builder.Configuration["AzureOpenAI:Deployment"]!,
        builder.Configuration["AzureOpenAI:Endpoint"]!,
        builder.Configuration["AzureOpenAI:ApiKey"]!);
    kernelBuilder.Plugins.AddFromType<PricingLookupPlugin>();
    return kernelBuilder.Build();
});

builder.Services.AddOpenTelemetry()
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource("Otel_Ai_Dotnet")
        .AddSource("Microsoft.SemanticKernel*")
        .AddConsoleExporter())
    .WithMetrics(m => m
        .AddMeter("Otel_Ai_Dotnet.Tokens")
        .AddConsoleExporter());

builder.Services.Configure<Dictionary<string, ModelPricing>>(
    builder.Configuration.GetSection("ModelPricing"));

builder.Services.AddSingleton<CostCalculator>(sp =>
{
    var options = sp.GetRequiredService<IOptions<Dictionary<string, ModelPricing>>>().Value;
    if (options.Count == 0)
    {
        throw new InvalidOperationException(
            "No ModelPricing configured - cost metrics would silently report as zero.");
    }
    return new CostCalculator(options);
});

var app = builder.Build();

app.MapPost("/chat", async (ChatRequest req, Kernel kernel) =>
{
    var requestActivity = Activity.Current;

    var chatService = kernel.GetRequiredService<IChatCompletionService>();
    var history = new ChatHistory();
    history.AddUserMessage(req.Message);

    return Results.Stream(async stream =>
    {
        ChatTokenUsage? finalUsage = null;

        await foreach (var chunk in chatService.GetStreamingChatMessageContentsAsync(history, kernel: kernel))
        {
            await stream.WriteAsync(Encoding.UTF8.GetBytes(chunk.Content ?? ""));

            if (chunk.Metadata?.TryGetValue("Usage", out var usageObj) == true
                && usageObj is ChatTokenUsage usage)
            {
                finalUsage = usage;
            }
        }

        if (finalUsage is not null)
        {
            var tags = new TagList { { "endpoint", "chat" } };
            AiMetrics.PromptTokens.Add(finalUsage.InputTokenCount, tags);
            AiMetrics.CompletionTokens.Add(finalUsage.OutputTokenCount, tags);

            requestActivity?.SetTag("ai.tokens.prompt", finalUsage.InputTokenCount);
            requestActivity?.SetTag("ai.tokens.completion", finalUsage.OutputTokenCount);
        }
    }, "text/plain");
});

app.Run();

// ---- Supporting types ----

public record ChatRequest(string Message);

public static class Telemetry
{
    public static readonly ActivitySource Source = new("Otel_Ai_Dotnet");
}

public static class AiMetrics
{
    public static readonly Meter Meter = new("Otel_Ai_Dotnet.Tokens");
    public static readonly Counter<long> PromptTokens =
        Meter.CreateCounter<long>("ai.tokens.prompt", description: "Prompt tokens consumed");
    public static readonly Counter<long> CompletionTokens =
        Meter.CreateCounter<long>("ai.tokens.completion", description: "Completion tokens consumed");
    public static readonly Counter<double> CostUsd =
        Meter.CreateCounter<double>("ai.cost.usd", description: "Estimated cost in USD");
}

public record ModelPricing(double PromptPricePer1K, double CompletionPricePer1K);

public class CostCalculator
{
    private readonly Dictionary<string, ModelPricing> _pricing;
    public CostCalculator(Dictionary<string, ModelPricing> pricing) => _pricing = pricing;

    public double Calculate(string model, int promptTokens, int completionTokens)
    {
        if (!_pricing.TryGetValue(model, out var pricing))
            throw new InvalidOperationException($"No pricing configured for model '{model}'");

        return (promptTokens / 1000.0 * pricing.PromptPricePer1K)
             + (completionTokens / 1000.0 * pricing.CompletionPricePer1K);
    }
}

public class PricingLookupPlugin
{
    private readonly HttpClient _http;
    public PricingLookupPlugin(HttpClient http) => _http = http;

    [KernelFunction("GetPrice")]
    public async Task<string> GetPriceAsync(string sku)
    {
        using var activity = Telemetry.Source.StartActivity("PricingLookup.GetPrice");
        activity?.SetTag("sku", sku);

        using var apiActivity = Telemetry.Source.StartActivity("PricingLookup.CallDownstreamApi");
        var response = await _http.GetStringAsync($"/pricing/{sku}");
        return response;
    }
}