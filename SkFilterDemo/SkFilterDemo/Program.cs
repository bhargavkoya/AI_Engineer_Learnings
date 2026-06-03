// Program.cs
using Microsoft.SemanticKernel;
using SkFilterDemo.Filters;
using SkFilterDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Register infrastructure
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IContentSafetyService, StubContentSafetyService>();

// 2. Register filters - order = execution order (outermost first)
builder.Services.AddSingleton<IFunctionInvocationFilter, StructuredLoggingFilter>();
builder.Services.AddSingleton<IFunctionInvocationFilter, ContentSafetyFilter>();
builder.Services.AddSingleton<IFunctionInvocationFilter, SemanticCacheFilter>();

// 3. Register Semantic Kernel - filters above are auto-discovered
builder.Services.AddKernel()
    .AddAzureOpenAIChatCompletion(
        deploymentName: builder.Configuration["AzureOpenAI:Deployment"]!,
        endpoint: builder.Configuration["AzureOpenAI:Endpoint"]!,
        apiKey: builder.Configuration["AzureOpenAI:ApiKey"]!);

var app = builder.Build();

// 4. Map the endpoint - Kernel is injected by the host DI
app.MapPost("/summarize", async (SummarizeRequest req, Kernel kernel) =>
{
    // InvokePromptAsync routes through the full filter pipeline
    var result = await kernel.InvokePromptAsync(
        "Summarize the following text in 3 bullet points:\n\n{{$input}}",
        new KernelArguments { ["input"] = req.Text });

    return Results.Ok(new { summary = result.ToString() });
});

app.Run();

record SummarizeRequest(string Text);