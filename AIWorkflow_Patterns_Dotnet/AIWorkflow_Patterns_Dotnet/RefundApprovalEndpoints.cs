using AIWorkflow_Patterns_Dotnet.Models;

namespace AIWorkflow_Patterns_Dotnet
{
    // Step 2: a human reviews via a minimal API endpoint. The AI cannot call this itself.
    public static class RefundApprovalEndpoints
    {
        public static void MapRefundApprovalEndpoints(this WebApplication app)
        {
            app.MapPost("/refund-approvals/{id:guid}/decision", async (
                Guid id,
                ApprovalDecisionDto decision,
                RefundApprovalDbContext db,
                IRefundIssuer refundIssuer,
                CancellationToken ct) =>
            {
                var request = await db.RefundApprovals.FindAsync([id], ct);
                if (request is null) return Results.NotFound();

                request.Status = decision.Approved ? ApprovalStatus.Approved : ApprovalStatus.Rejected;
                request.ReviewerNotes = decision.Notes;
                await db.SaveChangesAsync(ct);

                // Step 3 only runs after explicit human approval - the actual side effect.
                if (request.Status == ApprovalStatus.Approved)
                {
                    await refundIssuer.IssueRefundAsync(request.CustomerId, request.Amount, ct);
                }

                return Results.Ok(request);
            });
        }
    }

    public sealed record ApprovalDecisionDto(bool Approved, string? Notes);

    public interface IRefundIssuer
    {
        Task IssueRefundAsync(string customerId, decimal amount, CancellationToken ct);
    }
}
