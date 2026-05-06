using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AiCompare.Api
{
    public class SupportChatServiceSK(Kernel kernel)
    {
        public async Task<string> AskAsync(string question, CancellationToken ct = default)
        {
            var settings = new OpenAIPromptExecutionSettings
            {
                // Auto = SK handles the full tool-use loop without any extra code here
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            var result = await kernel.InvokePromptAsync(
                $"System: You are an IT support agent for .NET developers.\n\nUser: {question}",
                new KernelArguments(settings),
                cancellationToken: ct);

            return result.GetValue<string>() ?? string.Empty;
        }
    }
}
