using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using SkAutoFunctionCallingDemo;
using SkAutoFunctionCallingDemo.Plugins;

// Set up a console logger so filter output is visible
using var loggerFactory = LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Information));

var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        deploymentName: Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? "gpt-4o",
        endpoint: Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!,
        apiKey: Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY")!)
    .Build();

// Register both plugins - order here does not affect which one the model calls first
kernel.Plugins.AddFromObject(new ProductPlugin(), "ProductPlugin");
kernel.Plugins.AddFromObject(new PricingPlugin(), "PricingPlugin");

// Add the monitoring filter - this gives us planner-equivalent visibility
var logger = loggerFactory.CreateLogger<FunctionCallMonitorFilter>();
kernel.AutoFunctionInvocationFilters.Add(new FunctionCallMonitorFilter(logger));

// Continuation of Program.cs

var settings = new AzureOpenAIPromptExecutionSettings
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

// Test query 1: requires both ProductPlugin and PricingPlugin
Console.WriteLine("=== Query 1: Stock + Price ===");
var result1 = await kernel.InvokePromptAsync(
    "Is SKU-789 in stock? And what is the current price?",
    new KernelArguments(settings));
Console.WriteLine(result1.GetValue<string>());

Console.WriteLine();

// Test query 2: model should use search_by_name then get_current_price
Console.WriteLine("=== Query 2: Name-based search + price ===");
var result2 = await kernel.InvokePromptAsync(
    "What's the price of the webcam?",
    new KernelArguments(settings));
Console.WriteLine(result2.GetValue<string>());

Console.WriteLine();

// Test query 3: bulk pricing - tests that the model picks the right function
Console.WriteLine("=== Query 3: Bulk pricing ===");
var result3 = await kernel.InvokePromptAsync(
    "We want to order 60 units of SKU-202. What would that cost per unit?",
    new KernelArguments(settings));
Console.WriteLine(result3.GetValue<string>());