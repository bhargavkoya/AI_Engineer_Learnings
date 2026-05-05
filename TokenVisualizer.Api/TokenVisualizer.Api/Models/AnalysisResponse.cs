namespace TokenVisualizer.Api.Models
{
    public class AnalysisResponse
    {
        public int SystemPromptTokens { get; set; }
        public int UserInputTokens { get; set; }
        public int HistoryTokens { get; set; }
        public int RagContextTokens { get; set; }
        public int MessageOverhead { get; set; }
        public int TotalInputTokens { get; set; }
        public int ModelContextLimit { get; set; }
        public double PercentUsed { get; set; }
        public bool IsOverBudget { get; set; }
        public List<string> Warnings { get; set; } = [];
        public Dictionary<string, CostEstimate> ProviderCosts { get; set; } = [];
    }
}
