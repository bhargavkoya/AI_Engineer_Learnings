using Microsoft.SemanticKernel;
using System.Diagnostics;

namespace SkFilterDemo.Filters
{
    public class StructuredLoggingFilter : IFunctionInvocationFilter
    {
        private readonly ILogger<StructuredLoggingFilter> _logger;

        public StructuredLoggingFilter(ILogger<StructuredLoggingFilter> logger)
            => _logger = logger;

        public async Task OnFunctionInvocationAsync(
            FunctionInvocationContext context,
            Func<FunctionInvocationContext, Task> next)
        {
            var sw = Stopwatch.StartNew();

            _logger.LogInformation(
                "[SK] Invoking {Plugin}.{Function}",
                context.Function.PluginName, context.Function.Name);

            try
            {
                await next(context);
                sw.Stop();

                // Extract token usage if this was a chat completion call
                var tokenInfo = context.Result.Metadata?.TryGetValue("Usage", out var u) == true
                    ? $" | tokens: {u}"
                    : string.Empty;

                _logger.LogInformation(
                    "[SK] Completed {Plugin}.{Function} in {ElapsedMs}ms{TokenInfo}",
                    context.Function.PluginName, context.Function.Name,
                    sw.ElapsedMilliseconds, tokenInfo);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex,
                    "[SK] Failed {Plugin}.{Function} after {ElapsedMs}ms",
                    context.Function.PluginName, context.Function.Name,
                    sw.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
