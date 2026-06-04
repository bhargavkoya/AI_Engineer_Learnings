using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTriageMigration
{
    public static class SupportTools
    {
        // Simulated knowledge base search.
        // In production this would call Azure AI Search or similar.
        [Description("Search the support knowledge base for articles related to the query.")]
        public static string SearchKnowledgeBase(
            [Description("The customer question or issue description")] string query)
        {
            // Simplified KB - replace with real vector search in production
            if (query.Contains("password", StringComparison.OrdinalIgnoreCase))
                return "KB-101: Reset your password at account.example.com/reset. Link expires in 24 hours.";
            if (query.Contains("invoice", StringComparison.OrdinalIgnoreCase) ||
                query.Contains("billing", StringComparison.OrdinalIgnoreCase))
                return "BILLING_ESCALATION_REQUIRED"; // Signal to the triage agent
            return "KB-000: No matching article found. Please describe the issue in more detail.";
        }

        [Description("Raise a support ticket for the described issue and return the ticket ID.")]
        public static string CreateSupportTicket(
            [Description("Brief description of the issue")] string summary,
            [Description("Priority: low, medium, high")] string priority)
        {
            // Simulated ticket creation
            var ticketId = $"TKT-{Random.Shared.Next(10000, 99999)}";
            return $"Ticket {ticketId} created. Priority: {priority}. Expected response: 4 hours.";
        }
    }
}
