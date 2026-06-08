using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkAutoFunctionCallingDemo.Plugins
{
    public class ProductPlugin
    {
        // Simulates a product catalogue lookup - replace with real DB access
        private static readonly Dictionary<string, (string Name, int Stock, string Category)> _catalogue = new()
        {
            ["SKU-789"] = ("Wireless Mechanical Keyboard", 42, "Peripherals"),
            ["SKU-101"] = ("4K Webcam Pro", 0, "Video"),
            ["SKU-202"] = ("USB-C Hub 7-in-1", 18, "Accessories")
        };

        [KernelFunction("get_product_info")]
        [Description("Get product information including name, stock level, and category by SKU. " +
                     "Use when the user asks about whether a product exists or is in stock. " +
                     "Returns product name, current stock quantity, and category.")]
        public Task<string> GetProductInfoAsync(
            [Description("Product SKU code, e.g. 'SKU-789'")] string sku)
        {
            if (!_catalogue.TryGetValue(sku.ToUpperInvariant(), out var product))
                return Task.FromResult($"Product {sku} not found in catalogue.");

            var stockStatus = product.Stock > 0
                ? $"{product.Stock} units available"
                : "Out of stock";

            return Task.FromResult(
                $"Product: {product.Name} | SKU: {sku.ToUpperInvariant()} | " +
                $"Stock: {stockStatus} | Category: {product.Category}");
        }

        [KernelFunction("search_by_name")]
        [Description("Search the product catalogue by product name or keyword. " +
                     "Use when the user knows the product name but not the SKU. " +
                     "Returns matching products with their SKUs.")]
        public Task<string> SearchByNameAsync(
            [Description("Product name or keyword to search for")] string keyword)
        {
            var matches = _catalogue
                .Where(kv => kv.Value.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .Select(kv => $"- {kv.Value.Name} (SKU: {kv.Key})")
                .ToList();

            return Task.FromResult(matches.Any()
                ? $"Found {matches.Count} match(es):\n{string.Join("\n", matches)}"
                : $"No products found matching '{keyword}'.");
        }
    }
}
