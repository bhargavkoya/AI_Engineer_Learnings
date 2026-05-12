namespace MultiAIServiceProviders.Api.Contracts
{
    public interface IAssistantService
    {
        Task<string> AskWithPluginAsync(string prompt, CancellationToken ct = default);
    }
}
