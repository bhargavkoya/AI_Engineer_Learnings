using AIWorkflow_Patterns_Dotnet.Models;
using Microsoft.Extensions.AI;

namespace AIWorkflow_Patterns_Dotnet
{
    public sealed class AssessRefundStep
    {
        private readonly IChatClient _chatClient;

        public AssessRefundStep(IChatClient chatClient) => _chatClient = chatClient;

        public async Task<RefundAssessment> ExecuteAsync(
            TicketInput input, TicketAnalysis analysis, CancellationToken ct)
        {
            if (analysis.ContainsPii)
            {
                // Business rule short-circuit - don't even ask the model for refund
                // amounts on tickets containing PII until that's scrubbed.
                return new RefundAssessment(false, 0m, "Ticket contains PII - needs redaction first.");
            }

            var response = await _chatClient.GetResponseAsync<RefundAssessment>(
                $"Sentiment: {analysis.Sentiment}. Compliant: {analysis.IsCompliant}. " +
                $"Ticket: {input.TicketText}\n\nAssess whether a refund is warranted and how much.",
                cancellationToken: ct);

            return response.Result;
        }
    }
}
