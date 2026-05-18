// Program.cs (complete)
using System.Linq;
using Azure.AI.OpenAI;
using Microsoft.ML.Tokenizers;
using OpenAI.Chat;
using TokenCountingDemo;

var builder = WebApplication.CreateBuilder(args);

var azureConfig = builder.Configuration.GetSection("AzureOpenAI");
string endpoint = azureConfig["Endpoint"]!;
string deployment = azureConfig["DeploymentName"]!;
int maxInputTokens = int.Parse(azureConfig["MaxInputTokens"] ?? "8000");

// Register tokenizer as Singleton
builder.Services.AddSingleton(_ =>
    TiktokenTokenizer.CreateForModel("gpt-4o"));

// Register Azure OpenAI client
builder.Services.AddSingleton(_ =>
    new AzureOpenAIClient(new Uri(endpoint), new Azure.Identity.DefaultAzureCredential())
        .GetChatClient(deployment));

var app = builder.Build();

// Middleware registration — order matters
app.UseExceptionHandler("/error");

// Token limit middleware: applied to all POST /api/chat requests
// Pass maxTokens from configuration — not hard-coded
app.Use(async (ctx, next) =>
{
    var tokenizer = ctx.RequestServices.GetRequiredService<TiktokenTokenizer>();
    var mw = new TokenLimitMiddleware(next, tokenizer,
        ctx.RequestServices.GetRequiredService<ILogger<TokenLimitMiddleware>>(),
        maxTokens: maxInputTokens);
    await mw.InvokeAsync(ctx);
});

app.MapPost("/api/chat", async (
    ChatRequest request,
    ChatClient chatClient,
    TiktokenTokenizer tokenizer,
    ILogger<Program> logger) =>
{
    var messages = new List<ChatMessage>
    {
        new SystemChatMessage("You are a helpful .NET assistant."),
        new UserChatMessage(request.Prompt)
    };

    // Second-level check: accurate chat-message token count
    int chatTokens = TokenCounter.CountChatTokens(messages, tokenizer);
    logger.LogInformation("Chat-level token count: {Tokens}", chatTokens);

    // CompleteChatAsync returns a wrapper (e.g. ClientResult<ChatCompletion>).
    // Unwrap the actual ChatCompletion instance via .Value before accessing .Usage.
    var completionResult = await chatClient.CompleteChatAsync(messages);
    var completion = completionResult.Value;

    // Log actual usage from the API response for calibration
    logger.LogInformation(
        "Actual usage — Input: {Input}, Output: {Output}",
        completion.Usage.InputTokenCount,
        completion.Usage.OutputTokenCount);

    return Results.Ok(new
    {
        reply = string.Concat(completion.Content.Select(p => p.Text ?? string.Empty)),
        usage = new
        {
            inputTokens = completion.Usage.InputTokenCount,
            outputTokens = completion.Usage.OutputTokenCount
        }
    });
});

app.Run();

public record ChatRequest(string Prompt);