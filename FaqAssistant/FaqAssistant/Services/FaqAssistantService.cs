using Microsoft.SemanticKernel;

namespace FaqAssistant.Services
{
    public class FaqAssistantService(FaqVectorService faqService, Kernel kernel)
    {
        public async Task<string> AskAsync(
            string question,
            string? category = null,
            CancellationToken ct = default)
        {
            // Step 1: Retrieve the top 3 most relevant FAQ entries
            var relevantEntries = await faqService.SearchAsync(question, category, topK: 3, ct);

            if (relevantEntries.Count == 0)
            {
                return "No relevant FAQ entries found. Please contact support.";
            }

            // Step 2: Build context from retrieved entries
            var contextBuilder = new System.Text.StringBuilder();
            contextBuilder.AppendLine("Use the following FAQ entries to answer the question.");
            contextBuilder.AppendLine("Only use information from the entries below. Do not invent answers.");
            contextBuilder.AppendLine();

            for (int i = 0; i < relevantEntries.Count; i++)
            {
                var entry = relevantEntries[i];
                contextBuilder.AppendLine($"Entry {i + 1} [{entry.Category}]:");
                contextBuilder.AppendLine($"Q: {entry.Question}");
                contextBuilder.AppendLine($"A: {entry.Answer}");
                contextBuilder.AppendLine();
            }

            contextBuilder.AppendLine($"User question: {question}");
            contextBuilder.AppendLine("Answer:");

            // Step 3: Invoke the LLM with the assembled context prompt
            var response = await kernel.InvokePromptAsync(contextBuilder.ToString(),
                cancellationToken: ct);

            return response.ToString();
        }
    }
}
