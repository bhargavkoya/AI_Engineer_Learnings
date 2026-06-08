using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkAutoFunctionCallingDemo.Plugins
{
    public class PricingPlugin
    {
        // Simulates a live pricing service - replace with HttpClient or gRPC call
        private static readonly Dictionary<string, decimal> _prices = new()
        {
            ["SKU-789"] = 89.99m,
            ["SKU-101"] = 149.00m,
            ["SKU-202"] = 34.50m
        };

        [KernelFunction("get_current_price")]
        [Description("Get the current selling price for a product by SKU. " +
                     "Use when the user asks about pricing, cost, or how much a product costs. " +
                     "Returns the current price in USD. May differ from the catalogue list price.")]
        public Task<string> GetCurrentPriceAsync(
            [Description("Product SKU code, e.g. 'SKU-789'")] string sku)
        {
            if (!_prices.TryGetValue(sku.ToUpperInvariant(), out var price))
                return Task.FromResult($"No pricing data available for {sku}.");

            return Task.FromResult($"Current price for {sku.ToUpperInvariant()}: ${price:F2} USD");
        }

        [KernelFunction("get_bulk_pricing")]
        [Description("Get bulk pricing tiers for a product. " +
                     "Use when the user asks about discounts, volume pricing, or ordering in large quantities. " +
                     "Returns pricing for 1, 10, and 50+ unit quantities.")]
        public Task<string> GetBulkPricingAsync(
            [Description("Product SKU code")] string sku)
        {
            if (!_prices.TryGetValue(sku.ToUpperInvariant(), out var basePrice))
                return Task.FromResult($"No pricing data available for {sku}.");

            return Task.FromResult(
                $"Bulk pricing for {sku.ToUpperInvariant()}:\n" +
                $"  1-9 units:  ${basePrice:F2} each\n" +
                $"  10-49 units: ${basePrice * 0.90m:F2} each (10% off)\n" +
                $"  50+ units:  ${basePrice * 0.80m:F2} each (20% off)");
        }
    }
}
