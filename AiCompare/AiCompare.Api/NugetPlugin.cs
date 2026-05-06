using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace AiCompare.Api
{
    public class NugetPlugin(HttpClient httpClient)
    {
        [KernelFunction("get_package_info")]
        [Description("Fetch NuGet package metadata: latest version and description")]
        public async Task<string> GetPackageInfoAsync(
            [Description("Exact NuGet package ID, e.g. Microsoft.SemanticKernel")] string packageId)
        {
            // NuGet V3 flatcontainer API - public, no auth required
            var url = $"https://api.nuget.org/v3-flatcontainer/{packageId.ToLower()}/index.json";
            try
            {
                var response = await httpClient.GetFromJsonAsync<NugetVersionsResponse>(url);
                var latest = response?.Versions?.LastOrDefault();
                return latest is null
                    ? $"Package '{packageId}' not found on NuGet."
                    : $"Package: {packageId} | Latest version: {latest}";
            }
            catch
            {
                return $"Could not retrieve info for '{packageId}'.";
            }
        }

        private record NugetVersionsResponse(string[] Versions);
    }
}
