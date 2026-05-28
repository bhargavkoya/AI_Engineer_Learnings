using AIAssistant.Interfaces;

namespace AIAssistant.Stubs
{
    public class StubDocumentService : IDocumentService
    {
        private static readonly Dictionary<string, Document> _store = new()
        {
            ["DOC-1001"] = new("DOC-1001", "Q1 Budget 2026", "Finance Team",
                "The Q1 2026 budget allocates $2.4M to infrastructure. " +
                "Cloud spend increased 18% year-on-year due to AI workloads. " +
                "Cost optimisation initiatives are targeting a 12% reduction by Q2."),
            ["DOC-1002"] = new("DOC-1002", "Architecture Decision: Plugin System", "Platform Team",
                "The team evaluated LangChain, Semantic Kernel, and a custom solution. " +
                "Semantic Kernel was selected for its native .NET support and DI integration. " +
                "The plugin model allows teams to add capabilities without modifying the core AI layer.")
        };

        public Task<Document> GetByIdAsync(string documentId) =>
            Task.FromResult(_store.TryGetValue(documentId, out var doc)
                ? doc
                : throw new KeyNotFoundException($"Document {documentId} not found."));
    }
}
