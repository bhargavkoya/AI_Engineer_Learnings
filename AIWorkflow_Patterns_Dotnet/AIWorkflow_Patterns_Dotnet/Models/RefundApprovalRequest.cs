namespace AIWorkflow_Patterns_Dotnet.Models
{
    public enum ApprovalStatus { Pending, Approved, Rejected }
    public sealed class RefundApprovalRequest
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public required string CustomerId { get; init; }
        public required decimal Amount { get; init; }
        public required string AiJustification { get; init; }
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
        public string? ReviewerNotes { get; set; }
    }
}
