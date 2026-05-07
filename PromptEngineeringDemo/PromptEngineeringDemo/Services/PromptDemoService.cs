using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI.Chat;
using PromptEngineeringDemo.Models;
using PromptEngineeringDemo.Prompts;
using System.Text.Json;

namespace PromptEngineeringDemo.Services
{
    public class PromptDemoService
    {
        private readonly Kernel _kernel;
        private readonly ChatClient _chatClient;

        public PromptDemoService(Kernel kernel, ChatClient chatClient)
        {
            _kernel = kernel;
            _chatClient = chatClient;
        }

        public async Task<SummaryOutput?> SummariseAsync(string documentText)
        {
            // Use structured output to guarantee schema compliance
            var settings = new OpenAIPromptExecutionSettings
            {
                ResponseFormat = typeof(SummaryOutput)
            };

            var messages = PromptBuilder.BuildSummaryMessages(documentText);
            // Convert to Semantic Kernel's prompt format for structured output support
            var result = await _kernel.InvokePromptAsync(
                BuildSkPrompt(messages),
                new(settings)
            );

            return JsonSerializer.Deserialize<SummaryOutput>(result.ToString());
        }

        public async Task<ClassificationOutput?> ClassifyAsync(string text)
        {
            // Use raw ChatClient for few-shot - message array control is cleaner here
            var messages = PromptBuilder.BuildClassificationMessages(text);
            var response = await _chatClient.CompleteChatAsync(messages);
            var json = response.Value.Content[0].Text;

            return JsonSerializer.Deserialize<ClassificationOutput>(json);
        }

        public async Task<DocumentExtraction?> ExtractAsync(string documentText)
        {
            var settings = new OpenAIPromptExecutionSettings
            {
                ResponseFormat = typeof(DocumentExtraction)
            };

            var messages = PromptBuilder.BuildExtractionMessages(documentText);
            var result = await _kernel.InvokePromptAsync(
                BuildSkPrompt(messages),
                new(settings)
            );

            return JsonSerializer.Deserialize<DocumentExtraction>(result.ToString());
        }

        // Helper: collapses message list into a single prompt string for SK invocation
        // In production, use KernelFunction with message history for full control
        private static string BuildSkPrompt(List<ChatMessage> messages)
        {
            return string.Join("\n\n", messages.Select(m =>
                m is SystemChatMessage s ? s.Content[0].Text :
                m is UserChatMessage u ? u.Content[0].Text :
                m is AssistantChatMessage a ? a.Content[0].Text : string.Empty
            ));
        }
    }
}
