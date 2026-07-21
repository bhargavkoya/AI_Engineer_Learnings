// Program.cs
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using OpenAI.Chat;
using Securing_NET_AI_Apps;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(builder.Configuration["KeyVault:Uri"]!),
        new DefaultAzureCredential());
}

// Register the redactor and Azure OpenAI client using resolved config
builder.Services.AddSingleton<IPiiRedactor, RegexPiiRedactor>();
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new AzureOpenAIClient(
        new Uri(config["AzureOpenAI:Endpoint"]!),
        new AzureKeyCredential(config["AzureOpenAI:ApiKey"]!));
});

var app = builder.Build();

app.MapPost("/api/chat", async (
    ChatRequest request,
    IPiiRedactor redactor,
    AzureOpenAIClient client,
    ILogger<Program> logger) =>
{
    // Step A: redact before anything else touches the message
    var redaction = redactor.Redact(request.Message);

    // Step B: structured roles - user input never mixes into the system prompt string
    var chatClient = client.GetChatClient("gpt-4o-mini");
    var completion = await chatClient.CompleteChatAsync(
    [
        new SystemChatMessage(SystemPrompts.SupportBot),
        new UserChatMessage(redaction.RedactedText),
    ]);

    var responseText = completion.Value.Content[0].Text;

    // Step C: output validation before returning anything to the client
    var suspiciousMarkers = new[] { "system prompt", "order status inquiries only", "I was instructed" };
    var isSafe = !suspiciousMarkers.Any(m => responseText.Contains(m, StringComparison.OrdinalIgnoreCase));

    if (!isSafe)
    {
        logger.LogWarning("Blocked suspicious model output. UserId={UserId}", request.UserId);
        return Results.Ok(new { response = "I can help with order status questions only." });
    }

    // Step D: log metadata only, never raw content
    logger.LogInformation(
        "Chat handled. UserId={UserId} PiiRedacted={Redacted} MatchCount={Count}",
        request.UserId, redaction.WasRedacted, redaction.MatchCount);

    return Results.Ok(new { response = responseText });
});

public record ChatRequest(string UserId, string Message);
