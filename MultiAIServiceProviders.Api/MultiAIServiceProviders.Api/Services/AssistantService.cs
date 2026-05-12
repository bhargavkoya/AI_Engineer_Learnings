using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MultiAIServiceProviders.Api.Contracts;
using MultiAIServiceProviders.Api.Repos;

namespace MultiAIServiceProviders.Api.Services
{
    // AssistantService.cs
    public class AssistantService(Kernel kernel, IServiceProvider sp) : IAssistantService
    {
        public async Task<string> AskWithPluginAsync(string prompt, CancellationToken ct = default)
        {
            // Pull the Scoped plugin from the request-scoped service provider
            var plugin = sp.GetRequiredService<ProductSearchPlugin>();
            kernel.Plugins.AddFromObject(plugin, "Products");

            var settings = new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            var result = await kernel.InvokePromptAsync(
                prompt, new KernelArguments(settings), cancellationToken: ct);

            return result.GetValue<string>() ?? string.Empty;
        }
    }
}
