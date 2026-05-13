using System.ComponentModel;

namespace AiProviderPoc.Api
{
    // The input to every analysis request
    public record AnalysisRequest(string TicketText, string? Context = null);

    // The structured output - must use only JSON Schema primitives (string, int, bool, arrays)
    public class TicketAnalysis
    {
        [Description("Category of issue: 'billing', 'technical', 'account', 'feature-request'")]
        public string Category { get; set; } = string.Empty;

        [Description("Severity: 'low', 'medium', 'high', 'critical'")]
        public string Severity { get; set; } = string.Empty;

        [Description("One to two sentence summary of the customer's issue")]
        public string Summary { get; set; } = string.Empty;

        [Description("Suggested action for the support agent, one sentence")]
        public string SuggestedAction { get; set; } = string.Empty;

        public bool RequiresEscalation { get; set; }

        [Description("Estimated time to resolve as human-readable string, e.g. '30 minutes' or '2 business days'")]
        public string EstimatedResolution { get; set; } = string.Empty;
    }
}
