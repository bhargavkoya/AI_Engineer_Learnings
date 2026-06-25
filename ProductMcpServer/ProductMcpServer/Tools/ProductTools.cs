using ModelContextProtocol.Server;
using ProductMcpServer.Services;
using System.ComponentModel;
using System.Text;

namespace ProductMcpServer.Tools
{
    // Marks this class for automatic discovery by WithToolsFromAssembly()
    [McpServerToolType]
    public class ProductTools
    {
        private readonly IProductRepository _products;

        // The SDK resolves this from the DI container per request
        public ProductTools(IProductRepository products)
        {
            _products = products;
        }

        // Tool 1: Search the catalogue
        [McpServerTool(Name ="search_products")]
        [Description("Search the product catalogue by keyword. Returns matching products with SKU, name, price, and stock level. Use this before get_product when you don't know the SKU.")]
        public string SearchProducts(
            [Description("Search keyword. Matches against product name and category. Example: 'keyboard', 'office', 'USB'")]
        string keyword)
        {
            var results = _products.Search(keyword).ToList();

            if (results.Count == 0)
                return $"No products found matching '{keyword}'.";

            var sb = new StringBuilder();
            sb.AppendLine($"Found {results.Count} product(s) matching '{keyword}':");
            sb.AppendLine();

            foreach (var p in results)
            {
                // Format is readable by both the LLM and a human reviewing logs
                sb.AppendLine($"SKU: {p.Sku}");
                sb.AppendLine($"  Name:     {p.Name}");
                sb.AppendLine($"  Category: {p.Category}");
                sb.AppendLine($"  Price:    £{p.Price:F2}");
                sb.AppendLine($"  Stock:    {p.StockLevel} units");
                sb.AppendLine();
            }

            return sb.ToString().TrimEnd();
        }

        // Tool 2: Fetch a single product by SKU
        [McpServerTool(Name ="get_product")]
        [Description("Retrieve full details for a single product by its exact SKU code. Returns an error message if the SKU is not found. Use search_products first if you don't know the SKU.")]
        public string GetProduct(
            [Description("Exact product SKU code, e.g. 'KBD-001'. Case-insensitive.")]
        string sku)
        {
            var product = _products.GetBySku(sku);

            if (product is null)
                return $"Product with SKU '{sku}' was not found. Use search_products to find valid SKUs.";

            // Structured text response - clear field labels help the LLM parse the result
            return $"""
            Product Details
            ===============
            SKU:      {product.Sku}
            Name:     {product.Name}
            Category: {product.Category}
            Price:    £{product.Price:F2}
            Stock:    {product.StockLevel} units available
            """;
        }
    }
}
