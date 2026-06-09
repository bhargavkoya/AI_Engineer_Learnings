namespace ChatHistoryApi.Models
{
    public class ChatSession
    {
        public string id { get; set; } = "";           // Cosmos DB document ID
        public string SessionId { get; set; } = "";
        public DateTime UpdatedAt { get; set; }
        public List<ChatMessageRecord> Messages { get; set; } = [];
    }

    public record ChatRequest(string Message);

    public record ChatResponse(string Reply, int MessageCount);

    public record ChatMessageRecord(string Role, string Content);
}
