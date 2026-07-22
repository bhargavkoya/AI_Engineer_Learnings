using AIWorkflow_Patterns_Dotnet.Models;
using Microsoft.EntityFrameworkCore;

namespace AIWorkflow_Patterns_Dotnet
{
    public sealed class RefundApprovalDbContext : DbContext
    {
        public RefundApprovalDbContext(DbContextOptions<RefundApprovalDbContext> options)
            : base(options) { }

        public DbSet<RefundApprovalRequest> RefundApprovals => Set<RefundApprovalRequest>();
    }
}
