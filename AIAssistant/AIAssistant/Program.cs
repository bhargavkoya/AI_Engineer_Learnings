using AIAssistant.Interfaces;
using AIAssistant.Plugins;
using AIAssistant.Stubs;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

var builder = WebApplication.CreateBuilder(args);

// 1. Domain services - singletons because they wrap thread-safe infrastructure
builder.Services.AddSingleton<ICalendarService, StubCalendarService>();
builder.Services.AddSingleton<ISearchService, StubSearchService>();
builder.Services.AddSingleton<IDocumentService, StubDocumentService>();

// 2. Plugins - singletons so dependencies are created once
builder.Services.AddSingleton<CalendarPlugin>();
builder.Services.AddSingleton<SearchPlugin>();
builder.Services.AddSingleton<SummaryPlugin>();

// 3. Plugin collection - assembled once, reused across all requests
builder.Services.AddSingleton<KernelPluginCollection>(sp =>
[
    KernelPluginFactory.CreateFromObject(sp.GetRequiredService<CalendarPlugin>()),
    KernelPluginFactory.CreateFromObject(sp.GetRequiredService<SearchPlugin>()),
    KernelPluginFactory.CreateFromObject(sp.GetRequiredService<SummaryPlugin>()),
]);

// 4. Chat completion service - singleton, shared across all kernels
builder.Services.AddSingleton<IChatCompletionService>(_ =>
    new AzureOpenAIChatCompletionService(
        deploymentName: builder.Configuration["AzureOpenAI:Deployment"]!,
        endpoint: builder.Configuration["AzureOpenAI:Endpoint"]!,
        apiKey: builder.Configuration["AzureOpenAI:ApiKey"]!));

// 5. Kernel - transient so each request gets a clean instance
//    The kernel is cheap to construct; plugins are the expensive part and are reused
builder.Services.AddTransient(sp =>
{
    var plugins = sp.GetRequiredService<KernelPluginCollection>();
    return new Kernel(sp, plugins);
});

builder.Services.AddControllers();
var app = builder.Build();
app.MapControllers();
app.Run();