namespace AiCompare.Api
{
    // SupportChatService.cs - MEAI-only
    using Microsoft.Extensions.AI;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class SupportChatService(IChatClient chatClient)
    {
        public async Task<string> AskAsync(string question, CancellationToken ct = default)
        {
            var messages = new List<ChatMessage>
            {
                new(ChatRole.System,
                    "You are an IT support agent specialised in .NET packages. " +
                    "Answer based on what you know. If you are unsure, say so."),
                new(ChatRole.User, question)
            };

            var result = await chatClient.GetResponseAsync(messages, cancellationToken: ct);
            return result?.Text ?? string.Empty;
        }
    }
}
