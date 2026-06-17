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
    public static class OrchestratorWorkerDemo
    {
        public static async Task RunAsync(string bugReport)
        {
            // Worker kernels have only the tools they need
            var classifierKernel = KernelFactory.Create();
            classifierKernel.Plugins.AddFromObject(new ClassificationPlugin());

            var crmKernel = KernelFactory.Create();
            crmKernel.Plugins.AddFromObject(new CrmPlugin());

            // Pure reasoning kernel for the orchestrator - no domain tools
            var orchKernel = KernelFactory.Create();

            var classifierAgent = new ChatCompletionAgent
            {
                Name = "Classifier",
                Instructions = "Classify bug severity using get_classification_rules. Return JSON: { severity, reason }",
                Kernel = classifierKernel,
                Arguments = new KernelArguments(new OpenAIPromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                })
            };

            var crmAgent = new ChatCompletionAgent
            {
                Name = "CRMAgent",
                Instructions = "Find customers affected by the issue using find_affected_customers. Return a comma-separated list.",
                Kernel = crmKernel,
                Arguments = new KernelArguments(new OpenAIPromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                })
            };

            // The orchestrator calls each worker in sequence using AgentGroupChat
            // Sequential strategy ensures Classifier → CRMAgent order is respected
            var pipeline = new AgentGroupChat(classifierAgent, crmAgent)
            {
                ExecutionSettings = new()
                {
                    SelectionStrategy = new SequentialSelectionStrategy(),
                    TerminationStrategy = new MaxIterationTerminationStrategy(2)
                }
            };

            pipeline.AddChatMessage(new ChatMessageContent(AuthorRole.User, bugReport));

            Console.WriteLine("=== ORCHESTRATOR-WORKER ===");
            await foreach (var msg in pipeline.InvokeAsync())
            {
                Console.WriteLine($"[{msg.AuthorName}]: {msg.Content}");
            }
        }
    }

    // Simple max-iteration guard
    public class MaxIterationTerminationStrategy : TerminationStrategy
    {
        private readonly int _max;
        public MaxIterationTerminationStrategy(int max) => _max = max;

        protected override Task<bool> ShouldAgentTerminateAsync(
            Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken ct)
            => Task.FromResult(history.Count >= _max);
    }
}

#pragma warning restore SKEXP0001
#pragma warning restore SKEXP0110