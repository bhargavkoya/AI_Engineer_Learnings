using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WeatherSearchMcpServer.Extensions
{
    internal static class HttpClientExtensions
    {
        public static async Task<JsonDocument> ReadJsonDocumentAsync(
            this HttpClient client,
            string requestUri,
            CancellationToken cancellationToken = default)
        {
            using var response = await client.GetAsync(requestUri, cancellationToken);
            response.EnsureSuccessStatusCode();

            // Parse directly from the response stream - avoids double-buffering large responses
            return await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken);
        }
    }
}
