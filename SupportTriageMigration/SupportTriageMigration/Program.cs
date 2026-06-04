//// Program.cs - SK version (the "before" state)
//// This is what migration typically starts from.

//using Azure.Identity;
//using Microsoft.SemanticKernel;
//using Microsoft.SemanticKernel.Agents;
//using Microsoft.SemanticKernel.ChatCompletion;
//using SupportTriageMigration;

//#pragma warning disable SKEXP0110   // AgentGroupChat is experimental

//var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!;
//var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? "gpt-4o";

//// Build kernel - required for every agent.
//var kernel = Kernel.CreateBuilder()
//    .AddAzureOpenAIChatCompletion(deployment, endpoint, new DefaultAzureCredential())
//    .Build();

//// Register tools as plugins - multi-step process.
//var plugin = KernelPluginFactory.CreateFromFunctions("SupportPlugin", [
//    KernelFunctionFactory.CreateFromMethod(SupportTools.SearchKnowledgeBase, "search_kb",
//        "Search the support knowledge base"),
//    KernelFunctionFactory.CreateFromMethod(SupportTools.CreateSupportTicket, "create_ticket",
//        "Create a support ticket")
//]);
//kernel.Plugins.Add(plugin);

//// Triage agent - handles initial query and decides routing.
//var triageAgent = new ChatCompletionAgent
//{
//    Name = "TriageAgent",
//    Instructions = """
//        You are a support triage agent.
//        1. Search the knowledge base for the customer's issue using search_kb.
//        2. If the result contains BILLING_ESCALATION_REQUIRED, respond with:
//           "ESCALATE_TO_BILLING: <original query>"
//        3. Otherwise, answer from the KB result and offer to create a ticket.
//        """,
//    Kernel = kernel
//};

//// Billing specialist agent.
//var billingAgent = new ChatCompletionAgent
//{
//    Name = "BillingAgent",
//    Instructions = """
//        You are a billing specialist.
//        Handle all billing, invoice, and payment queries.
//        Always create a ticket using create_ticket with priority high for billing issues.
//        """,
//    Kernel = kernel
//};

//// Manual routing - no built-in handoff support in AgentGroupChat.
//// This is the boilerplate that Agent Framework eliminates.
//async Task RunSKVersionAsync(string userQuery)
//{
//    Console.WriteLine($"\n[SK] Customer: {userQuery}");
//    Console.WriteLine("[SK] Triage Agent responding...\n");

//    var triageChat = new AgentGroupChat(triageAgent);
//    triageChat.AddChatMessage(new ChatMessageContent(AuthorRole.User, userQuery));

//    string triageOutput = string.Empty;
//    await foreach (var msg in triageChat.InvokeAsync())
//    {
//        triageOutput = msg.Content ?? string.Empty;
//        Console.WriteLine($"[SK] Triage: {triageOutput}");
//    }

//    // Manual routing based on triage output - fragile string matching.
//    if (triageOutput.StartsWith("ESCALATE_TO_BILLING:", StringComparison.OrdinalIgnoreCase))
//    {
//        var billingQuery = triageOutput["ESCALATE_TO_BILLING:".Length..].Trim();
//        Console.WriteLine("\n[SK] Routing to Billing Specialist...\n");

//        var billingChat = new AgentGroupChat(billingAgent);
//        billingChat.AddChatMessage(new ChatMessageContent(AuthorRole.User, billingQuery));

//        await foreach (var msg in billingChat.InvokeAsync())
//            Console.WriteLine($"[SK] Billing: {msg.Content}");
//    }
//}

//await RunSKVersionAsync("I can't find my January invoice.");
//await RunSKVersionAsync("I forgot my password.");

// Program.cs — Agent Framework version (the "after" state)
// Same use case; compare the structure with the SK version.

using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Azure.AI.OpenAI;
using Azure.Identity;
using SupportTriageMigration;

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!;
var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? "gpt-4o";

// IChatClient — provider-agnostic foundation.
IChatClient chatClient = new AzureOpenAIClient(
        new Uri(endpoint), new DefaultAzureCredential())
    .GetChatClient(deployment)
    .AsIChatClient();

// Tools registered once using AIFunctionFactory.
// No KernelPlugin wrapper, no Kernel involvement.
var tools = new[]
{
    AIFunctionFactory.Create(SupportTools.SearchKnowledgeBase),
    AIFunctionFactory.Create(SupportTools.CreateSupportTicket)
};

// Triage agent — same instructions, simpler construction.
AIAgent triageAgent = chatClient.AsAIAgent(
    name: "TriageAgent",
    instructions: """
        You are a support triage agent.
        Search the knowledge base for the customer issue.
        If the result contains BILLING_ESCALATION_REQUIRED, escalate.
        Otherwise, answer from the KB and offer to raise a ticket.
        """,
    tools: tools);

// Billing specialist agent.
AIAgent billingAgent = chatClient.AsAIAgent(
    name: "BillingAgent",
    instructions: """
        You are a billing specialist.
        Handle all billing, invoice, and payment queries.
        Always create a high-priority support ticket for billing issues.
        """,
    tools: tools);

// FIX 1 (CS0117): BuildHandoff → CreateHandoffBuilderWith(...).AddHandoff(...).Build()
// Built-in handoff — no custom routing logic required.
Workflow handoffWorkflow = AgentWorkflowBuilder
    .CreateHandoffBuilderWith(triageAgent)
    .WithHandoff(triageAgent, billingAgent)  // explicit source → target
    .Build();

async Task RunAgentFrameworkVersionAsync(string userQuery)
{
    Console.WriteLine($"\n[AF] Customer: {userQuery}");

    List<ChatMessage> messages = [new(ChatRole.User, userQuery)];

    // Streaming execution — each agent's response streams in real time.
    await using StreamingRun run = await InProcessExecution.RunStreamingAsync(
        handoffWorkflow, messages);

    await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

    string? currentAgent = null;
    await foreach (WorkflowEvent evt in run.WatchStreamAsync())
    {
        if (evt is AgentResponseUpdateEvent e)
        {
            // FIX 2 (CS1061): AgentName → ExecutorId
            if (e.ExecutorId != currentAgent)
            {
                currentAgent = e.ExecutorId;
                Console.WriteLine($"\n[AF] {currentAgent}:");
            }
            Console.Write(e.Update.Text);
        }
    }
    Console.WriteLine();
}

await RunAgentFrameworkVersionAsync("I can't find my January invoice.");
await RunAgentFrameworkVersionAsync("I forgot my password.");