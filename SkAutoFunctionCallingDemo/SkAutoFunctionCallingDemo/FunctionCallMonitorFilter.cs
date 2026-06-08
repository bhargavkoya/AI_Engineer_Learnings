using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace SkAutoFunctionCallingDemo;

public sealed class FunctionCallMonitorFilter(ILogger<FunctionCallMonitorFilter> logger)
    : IAutoFunctionInvocationFilter
{
    public async Task OnAutoFunctionInvocationAsync(
        AutoFunctionInvocationContext context,
        Func<AutoFunctionInvocationContext, Task> next)
    {
        logger.LogInformation(
            "[AutoFunctionInvocation] Invoking: {Plugin}.{Function} | Attempt: {Attempt}",
            context.Function.PluginName,
            context.Function.Name,
            context.RequestSequenceIndex);

        await next(context);

        logger.LogInformation(
            "[AutoFunctionInvocation] Completed: {Plugin}.{Function}",
            context.Function.PluginName,
            context.Function.Name);
    }
}