// Program.cs - complete file for this starter
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SkStreamingChat;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);


// Standard Blazor Server services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Register Semantic Kernel with Azure OpenAI chat completion
// AddKernel() registers Kernel as Transient so each component
// gets a fresh instance - correct for Blazor Server
builder.Services.AddKernel()
    .AddAzureOpenAIChatCompletion(
        deploymentName: builder.Configuration["AzureOpenAI:DeploymentName"]!,
        endpoint: builder.Configuration["AzureOpenAI:Endpoint"]!,
        apiKey: builder.Configuration["AzureOpenAI:ApiKey"]!);

// Validate DI lifetimes at startup in development
if (builder.Environment.IsDevelopment())
{
    builder.Host.UseDefaultServiceProvider(o =>
    {
        o.ValidateScopes = true;
        o.ValidateOnBuild = true;
    });
}


var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.MapPost("/chat/stream", async (
    ChatStreamRequest req,
    Kernel kernel,
    HttpResponse resp,
    CancellationToken ct) =>
{
    // SSE requires these three headers - all are mandatory
    resp.ContentType = "text/event-stream";
    resp.Headers["Cache-Control"] = "no-cache";
    resp.Headers["X-Accel-Buffering"] = "no"; // Prevents nginx proxy buffering

    var chatHistory = new ChatHistory("You are a helpful .NET assistant.");
    chatHistory.AddUserMessage(req.Message);

    var settings = new OpenAIPromptExecutionSettings { MaxTokens = 1000 };
    var chatService = kernel.Services.GetRequiredService<IChatCompletionService>();

    // Link to request cancellation token + add 60s timeout
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    cts.CancelAfter(TimeSpan.FromSeconds(60));

    try
    {
        await foreach (var chunk in chatService.GetStreamingChatMessageContentsAsync(
            chatHistory, settings, kernel, cts.Token))
        {
            if (chunk.Content is not null)
            {
                var payload = JsonSerializer.Serialize(new { text = chunk.Content });
                // SSE frame format: "data: <payload>\n\n"
                await resp.WriteAsync($"data: {payload}\n\n", cts.Token);
                // FlushAsync is mandatory - without it chunks buffer until stream closes
                await resp.Body.FlushAsync(cts.Token);
            }
            var finishReason = chunk.Metadata?["FinishReason"]?.ToString();
            if (finishReason == "content_filter")
            {
                await resp.WriteAsync("event: content_filter\ndata: {}\n\n", ct);
                await resp.Body.FlushAsync(ct);
                return;
            }
        }

        // Signal completion so the client closes the EventSource
        await resp.WriteAsync("data: [DONE]\n\n", ct);
        await resp.Body.FlushAsync(ct);
    }
    catch (OperationCanceledException) { /* Client disconnected or 60s timeout */ }
    catch (System.ClientModel.ClientResultException ex)
    {
        var error = JsonSerializer.Serialize(new { error = ex.Message });
        await resp.WriteAsync($"event: error\ndata: {error}\n\n", ct);
        await resp.Body.FlushAsync(ct);
    }
});

app.Run();