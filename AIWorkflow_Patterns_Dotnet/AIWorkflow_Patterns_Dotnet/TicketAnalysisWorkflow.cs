using AIWorkflow_Patterns_Dotnet.Models;
using Microsoft.Extensions.AI;

namespace AIWorkflow_Patterns_Dotnet
{
    public sealed class TicketAnalysisWorkflow
    {
        private readonly IChatClient _chatClient;
        private readonly ILogger<TicketAnalysisWorkflow> _logger;

        public TicketAnalysisWorkflow(IChatClient chatClient, ILogger<TicketAnalysisWorkflow> logger)
        {
            _chatClient = chatClient;
            _logger = logger;
        }

        public async Task<TicketAnalysis> AnalyzeAsync(string ticketText, CancellationToken ct)
        {
            // Fan-out: three independent calls started concurrently, not awaited yet.
            var sentimentTask = RunSentimentCheckAsync(ticketText, ct);
            var complianceTask = RunComplianceCheckAsync(ticketText, ct);
            var piiTask = RunPiiCheckAsync(ticketText, ct);

            // Fan-in: wait for all three, but don't let one failure take down the others.
            var results = await Task.WhenAll(
                SafeRunAsync(sentimentTask, fallback: "unknown"),
                SafeRunAsync(complianceTask, fallback: "true"),
                SafeRunAsync(piiTask, fallback: "false"));

            return new TicketAnalysis(
                Sentiment: results[0],
                IsCompliant: bool.Parse(results[1]),
                ContainsPii: bool.Parse(results[2]));
        }

        // Wraps each branch so a single failed check degrades gracefully instead of
        // throwing away the two checks that succeeded.
        private async Task<string> SafeRunAsync(Task<string> task, string fallback)
        {
            try
            {
                return await task;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fan-out branch failed, using fallback {Fallback}", fallback);
                return fallback;
            }
        }

        private async Task<string> RunSentimentCheckAsync(string text, CancellationToken ct)
        {
            var response = await _chatClient.GetResponseAsync(
                $"Classify sentiment as positive, neutral, or negative. Reply with one word:\n\n{text}",
                cancellationToken: ct);
            return response.Text.Trim();
        }

        private async Task<string> RunComplianceCheckAsync(string text, CancellationToken ct)
        {
            var response = await _chatClient.GetResponseAsync(
                $"Does this ticket violate our content policy? Reply true or false only:\n\n{text}",
                cancellationToken: ct);
            return response.Text.Trim().ToLowerInvariant();
        }

        private async Task<string> RunPiiCheckAsync(string text, CancellationToken ct)
        {
            var response = await _chatClient.GetResponseAsync(
                $"Does this text contain personal identifying information? Reply true or false only:\n\n{text}",
                cancellationToken: ct);
            return response.Text.Trim().ToLowerInvariant();
        }
    }
}
