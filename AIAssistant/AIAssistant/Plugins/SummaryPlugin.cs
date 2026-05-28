using AIAssistant.Interfaces;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace AIAssistant.Plugins
{
    public class SummaryPlugin
    {
        private readonly IDocumentService _documents;

        public SummaryPlugin(IDocumentService documents) => _documents = documents;

        [KernelFunction("summarise_document")]
        [Description(
            "Retrieve and summarise a document by its unique ID. " +
            "Returns the title, author, and a 3-sentence summary. " +
            "Use after search_documents to get full content of a specific result. " +
            "Requires a document ID from search results in the format DOC-####.")]
        public async Task<string> SummariseDocumentAsync(
            [Description("Document ID from search results, e.g. DOC-1001")] string documentId)
        {
            var doc = await _documents.GetByIdAsync(documentId);
            return $"Title: {doc.Title}\nAuthor: {doc.Author}\nSummary: {doc.ExtractSummary(sentences: 3)}";
        }
    }
}
