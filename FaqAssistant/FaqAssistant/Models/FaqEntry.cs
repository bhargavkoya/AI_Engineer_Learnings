using Microsoft.Extensions.VectorData;

namespace FaqAssistant.Models
{
    public class FaqEntry
    {
        // Unique identifier - Cosmos DB maps this to its reserved "id" field
        [VectorStoreKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // Filterable: lets callers scope results by topic area
        [VectorStoreData(IsIndexed = true)]
        public string Category { get; set; } = string.Empty;

        // The original question text - displayed in the answer
        [VectorStoreData(IsFullTextIndexed = true)]
        public string Question { get; set; } = string.Empty;

        // The authoritative answer to surface in the LLM context
        [VectorStoreData]
        public string Answer { get; set; } = string.Empty;

        // text-embedding-3-small → exactly 1536 dimensions
        // If you change embedding models, recreate the collection
        [VectorStoreVector(1536,
            DistanceFunction= DistanceFunction.CosineSimilarity,
            IndexKind= IndexKind.Hnsw)]
        public ReadOnlyMemory<float> QuestionEmbedding { get; set; }
    }
}
