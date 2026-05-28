using System.Reflection.Metadata;

namespace AIAssistant.Interfaces
{
    public interface IDocumentService
    {
        Task<Document> GetByIdAsync(string documentId);
    }

    public record Document(string Id, string Title, string Author, string FullText)
    {
        public string ExtractSummary(int sentences) =>
            string.Join(" ", FullText.Split('.').Take(sentences).Select(s => s.Trim() + "."));
    }
}
