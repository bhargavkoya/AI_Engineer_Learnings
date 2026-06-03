using Microsoft.SemanticKernel;
using SkFilterDemo.Services;

namespace SkFilterDemo.Filters
{
    public class ContentSafetyFilter : IFunctionInvocationFilter
    {
        private readonly IContentSafetyService _safety;
        private readonly ILogger<ContentSafetyFilter> _logger;

        public ContentSafetyFilter(IContentSafetyService safety, ILogger<ContentSafetyFilter> logger)
        {
            _safety = safety;
            _logger = logger;
        }

        public async Task OnFunctionInvocationAsync(
            FunctionInvocationContext context,
            Func<FunctionInvocationContext, Task> next)
        {
            foreach (var arg in context.Arguments)
            {
                if (arg.Value is not string text || string.IsNullOrWhiteSpace(text))
                    continue;

                if (!await _safety.IsSafeAsync(text))
                {
                    _logger.LogWarning(
                        "[SK] Safety blocked {Plugin}.{Function} on arg '{ArgKey}'",
                        context.Function.PluginName, context.Function.Name, arg.Key);

                    // Return a refusal without calling the AI
                    context.Result = new FunctionResult(
                        context.Function, "I'm not able to help with that request.");
                    return;
                }
            }

            await next(context);
        }
    }
}
