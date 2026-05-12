namespace MultiAIServiceProviders.Api.Contracts
{
    public interface IModelRouter
    {
        Task<string> CompleteAsync(string query, bool needsReasoning, CancellationToken ct = default);
    }
}
