using AIWorkflow_Patterns_Dotnet.Models;
using Microsoft.Extensions.AI;

namespace AIWorkflow_Patterns_Dotnet
{
    public sealed class ProposeRefundStep
    {
        private readonly IChatClient _chatClient;
        private readonly RefundApprovalDbContext _db;

        public ProposeRefundStep(IChatClient chatClient, RefundApprovalDbContext db)
        {
            _chatClient = chatClient;
            _db = db;
        }

        public async Task<Guid> ExecuteAsync(string customerId, string complaintText, CancellationToken ct)
        {
            var response = await _chatClient.GetResponseAsync<RefundProposal>(
                $"Based on this complaint, propose a refund amount and justification:\n\n{complaintText}",
                cancellationToken: ct);

            var request = new RefundApprovalRequest
            {
                CustomerId = customerId,
                Amount = response.Result.Amount,
                AiJustification = response.Result.Justification
            };

            _db.RefundApprovals.Add(request);
            await _db.SaveChangesAsync(ct); // Workflow state persisted - the pipeline pauses here.

            return request.Id;
        }
    }
    public sealed record RefundProposal(decimal Amount, string Justification);
}
