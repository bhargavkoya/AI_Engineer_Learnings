namespace MultiAIServiceProviders.Api.Repos
{
    public class InMemoryProductRepository : IProductRepository
    {
        private static readonly string[] Products =
            ["Laptop Pro 16", "Wireless Keyboard", "USB-C Hub", "4K Monitor", "Mechanical Keyboard"];

        public Task<IEnumerable<string>> SearchAsync(string keyword) =>
            Task.FromResult(
                Products.Where(p => p.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
    }
}