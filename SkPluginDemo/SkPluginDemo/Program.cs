// Program.cs
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using SkPluginDemo;

// ---- Kernel setup ----
var builder = Kernel.CreateBuilder();

builder.AddAzureOpenAIChatCompletion(
    deploymentName: Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? "gpt-4o",
    endpoint: Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!,
    apiKey: Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY")!);

// Register the native plugin
builder.Plugins.AddFromType<TravelPlugin>("travel");

var kernel = builder.Build();

// Add the semantic function to the same "travel" plugin namespace
var recommendFunction = kernel.CreateFunctionFromPrompt(
    promptTemplate: """
        You are a travel advisor. Given the weather below, suggest three specific 
        activities for a traveler visiting this city today. Be concrete - name 
        actual locations or experiences, not generic suggestions.

        City: {{$city}}
        Weather: {{$weather}}
        
        Format your response as a numbered list.
        """,
    functionName: "recommend_activities",
    description: "Given a city name and its current weather, suggest three specific travel activities appropriate for the conditions.");

kernel.Plugins.AddFromFunctions("travel", [recommendFunction]);

// ---- Execution settings: allow LLM to call functions automatically ----
var settings = new AzureOpenAIPromptExecutionSettings
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

// ---- Chat loop with memory ----
var history = new ChatHistory();
history.AddSystemMessage(
    "You are a travel assistant. When a user asks about a city, " +
    "use get_weather to check current conditions, then use recommend_activities " +
    "to suggest what to do. Always call both tools before responding.");

Console.WriteLine("Travel Assistant - type a city name or question (Ctrl+C to exit)");
Console.WriteLine(new string('-', 55));

var chatService = kernel.GetRequiredService<IChatCompletionService>();

while (true)
{
    Console.Write("\nYou: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input)) continue;

    history.AddUserMessage(input);

    var response = await chatService.GetChatMessageContentAsync(
        history,
        executionSettings: settings,
        kernel: kernel);

    history.AddAssistantMessage(response.Content ?? string.Empty);

    Console.WriteLine($"\nAssistant: {response.Content}");
}