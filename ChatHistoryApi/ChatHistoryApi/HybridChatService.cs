using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;

namespace ChatHistoryApi
{
    public class HybridChatService(Kernel kernel)
    {
        private readonly IChatCompletionService _chatCompletion =
            kernel.Services.GetRequiredService<IChatCompletionService>();

        // Sliding window: keep at most this many non-system messages at all times
        private const int SlidingWindowMax = 40;

        // Summarisation trigger: summarise when count exceeds this
        private const int SummarizationThreshold = 30;

        // After summarisation, keep this many raw recent messages
        private const int RecentMessagesToRetain = 10;

        public async Task<string> ChatAsync(
            ChatHistory chatHistory,
            string userMessage,
            CancellationToken ct = default)
        {
            chatHistory.AddUserMessage(userMessage);

            // Always apply the sliding window first - cheap and synchronous
            ApplySlidingWindow(chatHistory, SlidingWindowMax);

            var response = await _chatCompletion.GetChatMessageContentAsync(
                chatHistory,
                kernel: kernel,
                cancellationToken: ct);

            chatHistory.AddAssistantMessage(response.Content ?? "");

            // Check if summarisation is needed after adding the reply
            int nonSystemCount = chatHistory.Count - 1;
            if (nonSystemCount >= SummarizationThreshold)
            {
                await SummarizeOldTurnsAsync(chatHistory, ct);
            }

            return response.Content ?? "";
        }

        private static void ApplySlidingWindow(ChatHistory history, int maxMessages)
        {
            int nonSystemCount = history.Count - 1;
            int excess = nonSystemCount - maxMessages;

            if (excess > 0)
            {
                // Remove from index 1 to preserve the system prompt at index 0
                history.RemoveRange(1, excess);
            }
        }

        private async Task SummarizeOldTurnsAsync(
            ChatHistory history,
            CancellationToken ct)
        {
            int oldMessageCount = (history.Count - 1) - RecentMessagesToRetain;

            if (oldMessageCount <= 0) return;

            var oldMessages = history.Skip(1).Take(oldMessageCount).ToList();
            var recentMessages = history.Skip(1 + oldMessageCount).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Summarize this conversation excerpt. Include key facts, " +
                          "decisions, code topics discussed, and unresolved questions:");
            sb.AppendLine();

            foreach (var msg in oldMessages)
            {
                sb.AppendLine($"{msg.Role}: {msg.Content}");
            }

            var summaryHistory = new ChatHistory(
                "You are a concise conversation summarizer. Output 3–5 sentences max.");
            summaryHistory.AddUserMessage(sb.ToString());

            var summaryResponse = await _chatCompletion.GetChatMessageContentAsync(
                summaryHistory,
                kernel: kernel,
                cancellationToken: ct);

            var summary = summaryResponse.Content ?? "Earlier context omitted.";
            var systemMsg = history[0];

            history.Clear();
            history.Add(systemMsg);
            history.Add(new ChatMessageContent(
                AuthorRole.Assistant,
                $"[Conversation summary]: {summary}"));

            foreach (var msg in recentMessages)
            {
                history.Add(msg);
            }
        }
    }
}
