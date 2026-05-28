using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace AIAssistant.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly Kernel _kernel;

        // Kernel is transient - a fresh instance per request with all plugins registered
        public AIController(Kernel kernel) => _kernel = kernel;

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] AskRequest request)
        {
            var settings = new AzureOpenAIPromptExecutionSettings
            {
                // Auto(): SK invokes whatever functions the LLM requests - no manual loop needed
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            var result = await _kernel.InvokePromptAsync(
                request.Prompt,
                new KernelArguments(settings));

            return Ok(new { answer = result.ToString() });
        }
    }

    public record AskRequest(string Prompt);
}
