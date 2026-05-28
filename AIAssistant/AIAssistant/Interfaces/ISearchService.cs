using Microsoft.OpenApi;

namespace AIAssistant.Interfaces
{
    public interface ISearchService
    {
        Task<IEnumerable<SearchResult>> SearchAsync(string query, int maxResults);
    }

    public record SearchResult(string Id, string Title, string Author, string Snippet);
}
