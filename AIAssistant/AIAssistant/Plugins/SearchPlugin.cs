using AIAssistant.Interfaces;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace AIAssistant.Plugins
{
    public class SearchPlugin
    {
        private readonly ISearchService _search;

        public SearchPlugin(ISearchService search) => _search = search;

        [KernelFunction("search_documents")]
        [Description(
            "Search the internal document repository by keyword or phrase. " +
            "Returns up to 5 matching documents with ID, title, author, and a short snippet. " +
            "Use when the user asks to find, locate, or look up documents, reports, or files.")]
        public async Task<string> SearchDocumentsAsync(
            [Description("Search query - keywords or a short phrase")] string query,
            [Description("Maximum number of results to return, default 5")] int maxResults = 5)
        {
            var results = await _search.SearchAsync(query, maxResults);

            if (!results.Any())
                return $"No documents found for '{query}'.";

            // Structured format so the LLM can extract document IDs for follow-up calls
            return string.Join("\n", results.Select((r, i) =>
                $"{i + 1}. [{r.Id}] {r.Title} by {r.Author} - {r.Snippet}"));
        }
    }
}
