using AIAssistant.Interfaces;

namespace AIAssistant.Stubs
{
    public class StubSearchService : ISearchService
    {
        private static readonly List<SearchResult> _docs =
        [
            new("DOC-1001", "Q1 Budget 2026", "Finance Team",
            "Overview of Q1 2026 budget allocations across infrastructure and engineering."),
        new("DOC-1002", "Architecture Decision: Plugin System", "Platform Team",
            "Decision to adopt Semantic Kernel plugins for AI capability composition."),
        new("DOC-1003", "Team Handbook 2026", "HR",
            "Guidelines for remote work, communication, and performance reviews.")
        ];

        public Task<IEnumerable<SearchResult>> SearchAsync(string query, int maxResults)
        {
            var lower = query.ToLowerInvariant();
            var results = _docs
                .Where(d => d.Title.Contains(lower, StringComparison.OrdinalIgnoreCase)
                         || d.Snippet.Contains(lower, StringComparison.OrdinalIgnoreCase))
                .Take(maxResults);
            return Task.FromResult(results);
        }
    }
}
