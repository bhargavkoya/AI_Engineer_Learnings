using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

namespace MultiAIServiceProviders.Api.Repos
{
    public class ProductSearchPlugin(IProductRepository repository)
    {
        [KernelFunction("search_products")]
        [Description("Search products by keyword. Returns a JSON array of matching product names.")]
        public async Task<string> SearchAsync(string keyword) =>
            JsonSerializer.Serialize(await repository.SearchAsync(keyword));
    }
}
