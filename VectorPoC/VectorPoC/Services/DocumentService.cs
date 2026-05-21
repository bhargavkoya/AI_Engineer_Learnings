using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using VectorPoC.Models;

namespace VectorPoC.Services
{
    public class DocumentService(
        VectorStore vectorStore,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        ILogger<DocumentService> logger)
    {
        // Lazy collection init - creates the index if it doesn't exist
        private async Task<VectorStoreCollection<string, DocumentRecord>>
            GetCollectionAsync(CancellationToken ct)
        {
            var collection = vectorStore
                .GetCollection<string, DocumentRecord>("documents");

            // EnsureCollectionExistsAsync is idempotent - safe to call on every startup
            await collection.EnsureCollectionExistsAsync(ct);
            return collection;
        }

        public async Task IndexDocumentAsync(
            string title,
            string content,
            string category,
            CancellationToken ct = default)
        {
            var collection = await GetCollectionAsync(ct);

            // Generate embedding - this is the only network call besides the upsert
            var embedding = await embeddingGenerator.GenerateVectorAsync(content, cancellationToken: ct);

            var record = new DocumentRecord
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Content = content,
                Category = category,
                ContentEmbedding = embedding
            };

            await collection.UpsertAsync(record, cancellationToken: ct);

            logger.LogInformation("Indexed document '{Title}' in category '{Category}'",
                title, category);
        }

        public async Task<IReadOnlyList<DocumentRecord>> SearchAsync(
            string query,
            string? categoryFilter = null,
            int topK = 5,
            CancellationToken ct = default)
        {
            var collection = await GetCollectionAsync(ct);

            // The query must be embedded with the same model as the documents
            var queryEmbedding = await embeddingGenerator
                .GenerateVectorAsync(query, cancellationToken: ct);

            var searchOptions = new VectorSearchOptions<DocumentRecord>
            {
                // Filter is optional - pass null for unscoped search
                Filter = categoryFilter is not null
                    ? r => r.Category == categoryFilter
                    : null
            };

            // top is now a direct parameter, not a property on options
            var results = collection.SearchAsync(
                queryEmbedding, topK, searchOptions, ct);

            var records = new List<DocumentRecord>();
            await foreach (var result in results.WithCancellation(ct))
            {
                records.Add(result.Record);
            }

            return records;
        }
    }
}