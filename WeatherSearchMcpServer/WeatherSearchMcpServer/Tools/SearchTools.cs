using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherSearchMcpServer.Tools
{
    // In a real implementation, inject ISearchService via DI
    // For the PoC, we return simulated results to keep the example self-contained
    [McpServerToolType]
    public sealed class SearchTools
    {
        [McpServerTool, Description("Search for articles or information by keyword.")]
        public static Task<string> SearchWeb(
            [Description("The search query string.")] string query,
            [Description("Maximum results to return. Defaults to 5.")] int limit = 5)
        {
            // Simulated results - replace with a real search API (Bing, Elasticsearch, etc.)
            var results = Enumerable.Range(1, Math.Min(limit, 5)).Select(i =>
                $"{i}. [{query} - Result {i}](https://example.com/result-{i}): " +
                $"A relevant article about '{query}' (simulated result {i} of {limit}).");

            return Task.FromResult(string.Join("\n", results));
        }
    }
}
