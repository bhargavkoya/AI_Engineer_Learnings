using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;

namespace AiCompare.Api
{
    public class SupportChatServiceAgent(Kernel kernel)
    {
        // Create once per service lifetime - not per request
        private readonly ChatCompletionAgent _agent = new()
        {
            Kernel = kernel,
            Name = "NuGetSupportAgent",
            Instructions = """
            You are an IT support agent specialised in .NET and NuGet packages.
            When the user asks about a package, always fetch live data using get_package_info.
            If a question is outside your domain, say so clearly instead of guessing.
            """
        };

        public async Task<string> AskAsync(string question, CancellationToken ct = default)
        {
            // Thread tracks conversation history across multiple AskAsync calls
            // In production, persist the thread per user session in a state store
            var thread = new ChatHistoryAgentThread();

            var responseBuilder = new StringBuilder();
            await foreach (var message in _agent.InvokeAsync(
                new ChatMessageContent(AuthorRole.User, question),
                thread,
                cancellationToken: ct))
            {
                responseBuilder.Append(message.Message);
            }

            return responseBuilder.ToString();
        }
    }
}
