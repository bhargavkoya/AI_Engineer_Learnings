using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetAiTokensDemo
{
    // ChatService.cs
    using OpenAI;
    using OpenAI.Chat;

    public sealed class ChatService
    {
        private readonly AppConfig _config;
        private readonly OpenAIClient _client;

        public ChatService(AppConfig config, OpenAIClient client)
        {
            _config = config;
            _client = client;
        }

        public async Task RunSinglePromptAsync(string prompt, CancellationToken ct = default)
        {
            var chatClient = _client.GetChatClient(_config.Model);

            var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage("You are a helpful assistant for .NET developers. Keep answers under 150 words."),
            ChatMessage.CreateUserMessage(prompt)
        };

            var response = await chatClient.CompleteChatAsync(messages, cancellationToken: ct);

            var completion = response.Value;

            Console.WriteLine();
            Console.WriteLine("=== MODEL RESPONSE ===");
            Console.WriteLine(completion.Content[0].Text);
            Console.WriteLine();

            var usage = completion.Usage;
            if (usage is null)
            {
                Console.WriteLine("Token usage not available from provider / SDK.");
                return;
            }

            int inputTokens = usage.InputTokenCount;
            int outputTokens = usage.OutputTokenCount;
            int totalTokens = usage.TotalTokenCount;

            Console.WriteLine("=== TOKEN USAGE ===");
            Console.WriteLine($"Input tokens : {inputTokens}");
            Console.WriteLine($"Output tokens: {outputTokens}");
            Console.WriteLine($"Total tokens : {totalTokens}");

            var cost = TokenCostCalculator.EstimateCost(
                inputTokens,
                outputTokens,
                _config.InputPricePerMillion,
                _config.OutputPricePerMillion);

            Console.WriteLine();
            Console.WriteLine($"Estimated cost for this call: ${cost:F6}");
            Console.WriteLine();
        }
    }
}
