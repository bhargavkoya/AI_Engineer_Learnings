using ProductMcpServer.Models;

namespace ProductMcpServer.Services
{
    public class InMemoryProductRepository : IProductRepository
    {
        // Seed data - in a real server this would query a database or external API
        private static readonly List<Product> _catalogue = new()
    {
        new("KBD-001", "Wireless Mechanical Keyboard", "Electronics", 89.99m, 142),
        new("MSE-002", "Ergonomic Optical Mouse", "Electronics", 44.99m, 234),
        new("HUB-003", "7-Port USB-C Hub", "Electronics", 34.99m, 89),
        new("DSK-004", "Anti-Fatigue Standing Mat", "Office", 49.99m, 67),
        new("LMP-005", "Monitor LED Light Bar", "Electronics", 59.99m, 156),
        new("CHR-006", "Mesh Back Office Chair", "Office", 299.99m, 23),
        new("MON-007", "27-inch 4K USB-C Monitor", "Electronics", 549.99m, 18),
    };

        public IEnumerable<Product> Search(string keyword, int maxResults = 10)
        {
            // Case-insensitive search across name and category
            return _catalogue
                .Where(p => p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                         || p.Category.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .Take(maxResults);
        }

        public Product? GetBySku(string sku)
        {
            return _catalogue.FirstOrDefault(
                p => p.Sku.Equals(sku, StringComparison.OrdinalIgnoreCase));
        }
    }
}
