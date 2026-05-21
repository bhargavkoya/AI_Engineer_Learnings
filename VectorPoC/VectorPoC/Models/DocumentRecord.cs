using Microsoft.Extensions.VectorData;

namespace VectorPoC.Models
{
    //[VectorStoreCollection("documents")]
    public class DocumentRecord
    {
        [VectorStoreKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [VectorStoreData(IsIndexed = true)]
        public string Title { get; set; } = string.Empty;

        [VectorStoreData]
        public string Content { get; set; } = string.Empty;

        // IsIndexed = true on Category enables category-scoped filtering
        [VectorStoreData(IsIndexed = true)]
        public string Category { get; set; } = string.Empty;

        // 1536 dimensions matches text-embedding-3-small
        // Change this if you switch embedding models - must match exactly
        [VectorStoreVector(1536, DistanceFunction = DistanceFunction.CosineDistance)]
        public ReadOnlyMemory<float> ContentEmbedding { get; set; }
    }
}