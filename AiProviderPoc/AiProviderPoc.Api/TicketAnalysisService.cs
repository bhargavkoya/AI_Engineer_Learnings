using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AiProviderPoc.Api
{
    public class TicketAnalysisService(IChatClient chatClient)
    {
        // The system prompt is the stable contract - versioned and centralized here
        private const string SystemPrompt =
            """
        You are a support ticket classifier for an enterprise software product.
        Analyze the provided support ticket and return a structured assessment.
        Be concise and accurate. Do not invent information not present in the ticket.
        """;

        public async Task<TicketAnalysis> AnalyzeAsync(AnalysisRequest request, CancellationToken ct = default)
        {
            // Build the message list - system prompt is fixed, user content is the ticket
            var messages = new List<ChatMessage>
        {
            new(ChatRole.System, SystemPrompt),
            new(ChatRole.User, request.TicketText)
        };

            // Use JSON mode via ChatOptions - provider-agnostic structured output
            // For strict schema enforcement with Azure/OpenAI, use the SDK-direct approach instead
            var options = new ChatOptions
            {
                ResponseFormat = ChatResponseFormat.Json
            };

            ChatResponse response = await chatClient.GetResponseAsync(messages, options, ct);

            // Deserialize - IChatClient normalizes the response shape across all providers
            return JsonSerializer.Deserialize<TicketAnalysis>(response.Text,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException("Model returned unparseable JSON");
        }
    }
}
