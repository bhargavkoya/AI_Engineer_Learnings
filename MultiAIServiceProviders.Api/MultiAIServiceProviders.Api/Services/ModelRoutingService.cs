using Microsoft.Extensions.AI;
using MultiAIServiceProviders.Api.Contracts;

namespace MultiAIServiceProviders.Api.Services
{
    // ModelRoutingService.cs
    public class ModelRoutingService(
        [FromKeyedServices("fast")] IChatClient fastClient,
        [FromKeyedServices("smart")] IChatClient smartClient) : IModelRouter
    {
        public async Task<string> CompleteAsync(
            string query, bool needsReasoning, CancellationToken ct = default)
        {
            var client = needsReasoning ? smartClient : fastClient;

            var result = await client.GetResponseAsync(
                [new ChatMessage(ChatRole.User, query)], cancellationToken: ct);

            return result.Text ?? string.Empty;
        }
    }
}
