using AgentPatternsPoc.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentPatternsPoc.Demos
{
    public static class SingleAgentDemo
    {
        public static async Task RunAsync(string bugReport,CancellationToken ctt)
        {
            var kernel = KernelFactory.Create();

            // All tools on one kernel - this is intentional for the single-agent pattern
            kernel.Plugins.AddFromObject(new ClassificationPlugin());
            kernel.Plugins.AddFromObject(new CrmPlugin());

            var agent = new ChatCompletionAgent
            {
                Name = "TriageAgent",
                Instructions = """
                You are a bug triage agent. For every bug report:
                1. Use get_classification_rules to determine severity
                2. Use find_affected_customers with the affected service name
                3. Draft a short triage report: severity, affected customers, recommended action
                """,
                Kernel = kernel,
                Arguments = new KernelArguments(new OpenAIPromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                })
            };

            ChatHistory history = [];
            history.AddUserMessage(bugReport);

            Console.WriteLine("=== SINGLE AGENT ===");
            var agentThread = new ChatHistoryAgentThread(history);
            await foreach (var msg in agent.InvokeAsync(agentThread, cancellationToken: ctt)) ;
        }
    }
}
