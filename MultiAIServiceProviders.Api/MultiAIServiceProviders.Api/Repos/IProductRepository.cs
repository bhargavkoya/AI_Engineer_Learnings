namespace MultiAIServiceProviders.Api.Repos
{
    public interface IProductRepository
    {
        Task<IEnumerable<string>> SearchAsync(string keyword);
    }
}
