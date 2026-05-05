namespace TokenVisualizer.Api
{
    using Microsoft.Extensions.Options;
    // TokenAnalysisService.cs
    using Microsoft.ML.Tokenizers;
    using TokenVisualizer.Api.Models;

    public class TokenAnalysisService
    {
        private readonly TiktokenTokenizer _tokenizer;
        private readonly TokenBudgetConfig _budget;

        // Pricing per 1M tokens (verify against provider pricing pages — rates change)
        private static readonly Dictionary<string, (double Input, double Output, string Note)> Pricing = new()
        {
            ["gpt-4o"] = (2.50, 10.00, "Cached input: $1.25/1M (auto, 50% off)"),
            ["gpt-4.1"] = (2.00, 8.00, "Cached input: $1.00/1M (auto, 50% off)"),
            ["gpt-4.1-mini"] = (0.40, 1.60, "Best cost/perf default for most workloads"),
            ["gpt-4.1-nano"] = (0.10, 0.40, "20x cheaper than flagship — use for classification/extraction"),
            ["claude-sonnet"] = (3.00, 15.00, "Cache hit ~90% off — use cache_control for stable prompts"),
            ["claude-haiku"] = (1.00, 5.00, "200K context at lower price than GPT-4o mini"),
            ["gemini-2.5-pro"] = (1.25, 10.00, "1M context; ~75% off with context caching"),
            ["gemini-2.5-flash"] = (0.50, 3.00, "Cheapest large-context option; strong throughput"),
        };

        public TokenAnalysisService(TiktokenTokenizer tokenizer, IOptions<TokenBudgetConfig> options)
        {
            _tokenizer = tokenizer;
            _budget = options.Value;
        }

        public AnalysisResponse Analyse(AnalysisRequest request)
        {
            int sysTokens = _tokenizer.CountTokens(request.SystemPrompt ?? "");
            int userTokens = _tokenizer.CountTokens(request.UserInput ?? "");
            int ragTokens = request.RagChunks?.Sum(s => _tokenizer.CountTokens(s)) ?? 0;

            // History is raw strings here — in real usage these would be ChatMessage objects
            int histTokens = request.History?.Sum(s => _tokenizer.CountTokens(s)) ?? 0;

            // Message overhead: 4 tokens per message + 2 reply priming tokens
            // Count: system(1) + user(1) + each history item(n)
            int msgCount = 1 + 1 + (request.History?.Count ?? 0);
            int overhead = 2 + (4 * msgCount);

            int total = sysTokens + userTokens + ragTokens + histTokens + overhead;
            double pct = (double)total / _budget.ModelContextLimit;

            var warnings = BuildWarnings(sysTokens, userTokens, ragTokens, histTokens, total, pct);
            var costs = BuildCostEstimates(total, estimatedOutputTokens: _budget.OutputReserve);

            return new AnalysisResponse
            {
                SystemPromptTokens = sysTokens,
                UserInputTokens = userTokens,
                HistoryTokens = histTokens,
                RagContextTokens = ragTokens,
                MessageOverhead = overhead,
                TotalInputTokens = total,
                ModelContextLimit = _budget.ModelContextLimit,
                PercentUsed = Math.Round(pct * 100, 1),
                IsOverBudget = total > _budget.EffectiveInputLimit,
                Warnings = warnings,
                ProviderCosts = costs,
            };
        }

        private List<string> BuildWarnings(
            int sys, int user, int rag, int hist, int total, double pct)
        {
            var w = new List<string>();
            if (sys > _budget.SystemPromptMax * 0.9) w.Add($"System prompt at {sys}/{_budget.SystemPromptMax} tokens (>90% of budget). Consider condensing.");
            if (rag > _budget.RagContextMax * 0.9) w.Add($"RAG context at {rag}/{_budget.RagContextMax} tokens (>90%). Reduce chunk count.");
            if (hist > _budget.HistoryMax * 0.9) w.Add($"History at {hist}/{_budget.HistoryMax} tokens (>90%). Oldest messages must be trimmed.");
            if (user > 4_000) w.Add($"User input is large ({user} tokens). If this is a document, chunk it before sending.");
            if (pct > 0.92) w.Add($"⚠ CRITICAL: Total at {pct:P0} of context window — truncation imminent.");
            else if (pct > 0.80) w.Add($"⚠ WARNING: Total at {pct:P0} of context window — approaching limit.");
            return w;
        }

        private static Dictionary<string, CostEstimate> BuildCostEstimates(
            int inputTokens, int estimatedOutputTokens)
        {
            return Pricing.ToDictionary(
                kvp => kvp.Key,
                kvp => new CostEstimate
                {
                    ModelName = kvp.Key,
                    InputCostUsd = inputTokens / 1_000_000.0 * kvp.Value.Input,
                    OutputCostUsd = estimatedOutputTokens / 1_000_000.0 * kvp.Value.Output,
                    TotalCostUsd = (inputTokens / 1_000_000.0 * kvp.Value.Input)
                                 + (estimatedOutputTokens / 1_000_000.0 * kvp.Value.Output),
                    Note = kvp.Value.Note
                });
        }
    }
}
