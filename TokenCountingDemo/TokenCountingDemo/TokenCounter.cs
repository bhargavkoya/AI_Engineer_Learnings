using Microsoft.ML.Tokenizers;
using OpenAI.Chat;

namespace TokenCountingDemo
{
    public static class TokenCounter
    {
        public static int CountChatTokens(
            IEnumerable<ChatMessage> messages,
            TiktokenTokenizer tokenizer)
        {
            int total = 2; // reply priming

            foreach (var message in messages)
            {
                total += 4; // per-message overhead
                total += tokenizer.CountTokens(GetText(message));
            }

            return total;
        }

        private static string GetText(ChatMessage message) => message switch
        {
            UserChatMessage u =>
                string.Join(" ", u.Content.Select(p => p.Text ?? string.Empty)),
            AssistantChatMessage a =>
                string.Join(" ", a.Content?.Select(p => p.Text ?? string.Empty) ?? []),
            SystemChatMessage s =>
                string.Join(" ", s.Content.Select(p => p.Text ?? string.Empty)),
            _ => string.Empty
        };
    }
}
