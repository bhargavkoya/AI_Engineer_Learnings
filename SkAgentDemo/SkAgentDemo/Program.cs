using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using SkAgentDemo.Plugins;

// --- Step 4a: Build the Kernel ---
var builder = Kernel.CreateBuilder();

builder.AddAzureOpenAIChatCompletion(
    deploymentName: Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT")
                    ?? throw new InvalidOperationException("AZURE_OPENAI_DEPLOYMENT not set"),
    endpoint: Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
                    ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT not set"),
    apiKey: Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY")
                    ?? throw new InvalidOperationException("AZURE_OPENAI_KEY not set")
);

// --- Step 4b: Register all three plugins ---
// AddFromType<T> scans the class for [KernelFunction] methods via reflection
builder.Plugins.AddFromType<CalculatorPlugin>();
builder.Plugins.AddFromType<DatePlugin>();
builder.Plugins.AddFromType<TaskPlugin>();

Kernel kernel = builder.Build();

// --- Step 4c: Create the ChatCompletionAgent ---
var agent = new ChatCompletionAgent
{
    Name = "SkAssistant",
    Instructions = """
        You are a concise assistant for .NET developers.
        Always use available tools rather than guessing at calculations or dates.
        For task operations, confirm the action taken.
        If a tool returns an error, report it to the user clearly.
        """,
    Kernel = kernel,
    Arguments = new KernelArguments(
        new AzureOpenAIPromptExecutionSettings
        {
            // Auto: LLM picks tools; SK invokes them and loops until a text response is ready
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        })
};

// --- Step 4d: Run the interactive loop ---
ChatHistory chat = [];
Console.WriteLine("SK Agent Demo - type 'exit' to quit.\n");

while (true)
{
    Console.Write("You: ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input)) continue;
    if (input.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

    // Add user message to history before invoking
    chat.Add(new ChatMessageContent(AuthorRole.User, input));

    Console.Write("Agent: ");

    await foreach (var response in agent.InvokeAsync(chat))
    {
        // Add agent reply to history to preserve conversational context
        chat.Add(response);
        Console.WriteLine(response.Message.Content);
    }

    Console.WriteLine();
}