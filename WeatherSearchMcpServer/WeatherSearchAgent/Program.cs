using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

// --- 1. Connect to the MCP server via stdio ---
// The SDK spawns the server process - adjust the path to match your repo layout
var serverProjectPath = Path.GetFullPath(
    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..",
    "WeatherSearchMcpServer", "WeatherSearchMcpServer.csproj"));

await using var mcpClient = await McpClient.CreateAsync(
    new StdioClientTransport(new StdioClientTransportOptions
    {
        Name = "WeatherSearchServer",
        Command = "dotnet",
        Arguments = ["run", "--project", serverProjectPath]
    }));

// --- 2. Discover tools ---
var tools = await mcpClient.ListToolsAsync();
Console.WriteLine($"MCP tools available: {string.Join(", ", tools.Select(t => t.Name))}");

// --- 3. Build the chat client (uses DefaultAzureCredential - no hard-coded keys) ---
IChatClient chatClient = new AzureOpenAIClient(
    new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
        ?? throw new InvalidOperationException("Set AZURE_OPENAI_ENDPOINT")),
    new DefaultAzureCredential())
    .GetChatClient("gpt-4o")      // ← get the strongly-typed ChatClient
    .AsIChatClient();             // ← then adapt to Microsoft.Extensions.AI interface

// --- 4. Build the chat options - pass MCP tools directly ---
var chatOptions = new ChatOptions
{
    // McpClientTool implements AIFunction, so it plugs straight in
    Tools = [.. tools]
};

// --- 5. Run a multi-turn conversation ---
var chatHistory = new List<ChatMessage>
{
    new(ChatRole.System, """
        You are a helpful assistant with access to weather and search tools.
        Always use the available tools to fetch real data before answering.
        """),
    new(ChatRole.User, "Are there any active weather alerts in California?")
};

Console.WriteLine("\n--- Agent Response ---\n");

// GetStreamingResponseAsync handles the tool call / tool result loop automatically
await foreach (var update in chatClient.GetStreamingResponseAsync(chatHistory, chatOptions))
{
    Console.Write(update);
}

Console.WriteLine("\n\n--- Second turn ---\n");

// Add the assistant's response to history and ask a follow-up
chatHistory.Add(new(ChatRole.User,
    "Also search for recent news about California wildfire season."));

await foreach (var update in chatClient.GetStreamingResponseAsync(chatHistory, chatOptions))
{
    Console.Write(update);
}