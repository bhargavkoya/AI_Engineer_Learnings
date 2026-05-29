using FaqAssistant.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace FaqAssistant.Services
{
    public class FaqVectorService(
    VectorStore vectorStore,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    ILogger<FaqVectorService> logger)
    {
        private const string CollectionName = "faq-entries";

        private VectorStoreCollection<string, FaqEntry> GetCollection()
            => vectorStore.GetCollection<string, FaqEntry>(CollectionName);

        public async Task EnsureCollectionAsync(CancellationToken ct = default)
        {
            logger.LogInformation("Ensuring collection '{Name}' exists", CollectionName);
            await GetCollection().EnsureCollectionExistsAsync(ct);
        }

        public async Task UpsertFaqAsync(FaqEntry entry, CancellationToken ct = default)
        {
            var embeddings = await embeddingGenerator.GenerateAsync(
                [entry.Question], cancellationToken: ct);

            entry.QuestionEmbedding = embeddings[0].Vector;

            await GetCollection().UpsertAsync(entry, ct);
            logger.LogInformation("Upserted FAQ entry '{Id}'", entry.Id);
        }

        public async Task<IList<FaqEntry>> SearchAsync(
            string userQuery,
            string? category = null,
            int topK = 3,
            CancellationToken ct = default)
        {
            var queryEmbeddings = await embeddingGenerator.GenerateAsync(
                [userQuery], cancellationToken: ct);

            // SearchAsync returns IAsyncEnumerable directly
            var results = GetCollection().SearchAsync(
                queryEmbeddings[0].Vector,
                topK,
                cancellationToken: ct);

            var entries = new List<FaqEntry>();
            await foreach (var result in results)
            {
                if (category == null || result.Record.Category == category)
                    entries.Add(result.Record);
            }

            return entries;
        }
    }
}