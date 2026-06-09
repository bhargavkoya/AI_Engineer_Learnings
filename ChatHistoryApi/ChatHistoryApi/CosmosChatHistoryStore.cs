using ChatHistoryApi.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace ChatHistoryApi
{
    public class CosmosChatHistoryStore(CosmosClient cosmosClient)
    {
        private readonly Container _container =
            cosmosClient.GetContainer("chatbot-db", "chat-sessions");

        public async Task<ChatHistory> LoadAsync(
            string sessionId,
            string systemPrompt,
            CancellationToken ct = default)
        {
            try
            {
                var response = await _container.ReadItemAsync<ChatSession>(
                    id: sessionId,
                    partitionKey: new PartitionKey(sessionId),
                    cancellationToken: ct);

                var history = new ChatHistory();

                foreach (var msg in response.Resource.Messages)
                {
                    history.Add(new ChatMessageContent(
                        new AuthorRole(msg.Role),
                        msg.Content));
                }

                return history;
            }
            catch (CosmosException ex)
                when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new ChatHistory(systemPrompt);
            }
        }

        public async Task SaveAsync(
            string sessionId,
            ChatHistory chatHistory,
            CancellationToken ct = default)
        {
            var session = new ChatSession
            {
                id = sessionId,
                SessionId = sessionId,
                UpdatedAt = DateTime.UtcNow,
                Messages = chatHistory
                    .Select(m => new ChatMessageRecord(
                        m.Role.ToString(),
                        m.Content ?? ""))
                    .ToList(),
            };

            await _container.UpsertItemAsync(
                item: session,
                partitionKey: new PartitionKey(sessionId),
                cancellationToken: ct);
        }
    }
}
