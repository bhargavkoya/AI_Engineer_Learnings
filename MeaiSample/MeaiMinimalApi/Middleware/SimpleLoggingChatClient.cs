namespace MeaiMinimalApi.Middleware
{
    using Microsoft.Extensions.AI;
    using Microsoft.Extensions.Logging;

    public sealed class SimpleLoggingChatClient(
        IChatClient innerClient,
        ILogger<SimpleLoggingChatClient> logger)
        : DelegatingChatClient(innerClient)
    {
        public override async Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var promptLength = messages.Sum(m => m.Text?.Length ?? 0);
            logger.LogInformation("Sending AI request. Approx prompt chars: {PromptLength}", promptLength);

            var response = await base.GetResponseAsync(messages, options, cancellationToken);

            logger.LogInformation("Received AI response. Response chars: {ResponseLength}", response.Text?.Length ?? 0);
            return response;
        }
    }
}
