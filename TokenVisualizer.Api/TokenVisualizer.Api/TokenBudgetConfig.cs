namespace TokenVisualizer.Api.Models
{
    public class TokenBudgetConfig
    {
        // Total model context window (must be > 0 to avoid divide-by-zero)
        public int ModelContextLimit { get; set; } = 131072;

        // Effective available tokens for input (after reserving output tokens)
        public int EffectiveInputLimit { get; set; } = 129024;

        // Estimated tokens reserved for model output
        public int OutputReserve { get; set; } = 2048;

        // Limits used for warning thresholds
        public int SystemPromptMax { get; set; } = 1024;
        public int RagContextMax { get; set; } = 65536;
        public int HistoryMax { get; set; } = 8192;
    }
}