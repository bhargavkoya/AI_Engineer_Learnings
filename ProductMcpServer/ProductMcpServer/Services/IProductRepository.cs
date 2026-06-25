using ProductMcpServer.Models;

namespace ProductMcpServer.Services
{
    public interface IProductRepository
    {
        IEnumerable<Product> Search(string keyword, int maxResults = 10);
        Product? GetBySku(string sku);
    }
}
