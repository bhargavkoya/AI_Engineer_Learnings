#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0110

using AgentPatternsPoc.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentPatternsPoc.Demos
{
    public static class GroupChatDemo
    {
        public static async Task RunAsync(string bugReport)
        {
            var kernel = KernelFactory.Create();
            var writerKernel = KernelFactory.Create();
            writerKernel.Plugins.AddFromObject(new ClassificationPlugin());
            writerKernel.Plugins.AddFromObject(new CrmPlugin());

            var writerAgent = new ChatCompletionAgent
            {
                Name = "TriageWriter",
                Instructions = """
                Write a bug triage report. Use get_classification_rules and find_affected_customers.
                Include: severity, affected customers, recommended steps.
                Revise if the reviewer provides feedback. Write APPROVED when done.
                """,
                Kernel = writerKernel,
                Arguments = new KernelArguments(new OpenAIPromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                })
            };

            var reviewerAgent = new ChatCompletionAgent
            {
                Name = "TriageReviewer",
                Instructions = """
                Review the triage report for completeness and technical accuracy.
                Feedback must be specific. If the report is complete, say APPROVED.
                """,
                Kernel = kernel // Reviewer has no tools - pure reasoning
            };

            var groupChat = new AgentGroupChat(writerAgent, reviewerAgent)
            {
                ExecutionSettings = new()
                {
                    SelectionStrategy = new SequentialSelectionStrategy(),
                    TerminationStrategy = new ApprovalTerminationStrategy
                    {
                        Agents = [reviewerAgent],
                        MaximumIterations = 6
                    }
                }
            };

            groupChat.AddChatMessage(new ChatMessageContent(AuthorRole.User, bugReport));

            Console.WriteLine("=== GROUP CHAT ===");
            await foreach (var msg in groupChat.InvokeAsync())
            {
                Console.WriteLine($"[{msg.AuthorName}]: {msg.Content}");
                Console.WriteLine("---");
            }
        }
    }
}

public class ApprovalTerminationStrategy : TerminationStrategy
{
    protected override Task<bool> ShouldAgentTerminateAsync(
        Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken ct)
    {
        var lastMessage = history.LastOrDefault()?.Content ?? string.Empty;
        return Task.FromResult(lastMessage.Contains("APPROVED", StringComparison.OrdinalIgnoreCase));
    }
}

#pragma warning restore SKEXP0001
#pragma warning restore SKEXP0110