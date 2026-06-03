using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel;

namespace SkFilterDemo.Filters
{
    public class SemanticCacheFilter : IFunctionInvocationFilter
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<SemanticCacheFilter> _logger;
        private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(30);

        public SemanticCacheFilter(IMemoryCache cache, ILogger<SemanticCacheFilter> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task OnFunctionInvocationAsync(
            FunctionInvocationContext context,
            Func<FunctionInvocationContext, Task> next)
        {
            // Only cache prompt functions - check PluginName to restrict scope
            if (context.Function.PluginName != "SummarizePlugin")
            {
                await next(context);
                return;
            }

            var key = BuildKey(context);

            if (_cache.TryGetValue(key, out string? cached))
            {
                _logger.LogInformation("[SK] Cache HIT for key {CacheKey}", key);
                context.Result = new FunctionResult(context.Function, cached);
                return; // Short-circuit - AI not called
            }

            await next(context); // Cache miss - call the AI

            if (context.Result.GetValue<string>() is string result)
            {
                _cache.Set(key, result, Ttl);
                _logger.LogInformation("[SK] Cache STORED for key {CacheKey}", key);
            }
        }

        private static string BuildKey(FunctionInvocationContext ctx)
        {
            var args = string.Join("|", ctx.Arguments
                .OrderBy(a => a.Key)
                .Select(a => $"{a.Key}={a.Value}"));
            return $"sk:{ctx.Function.PluginName}:{ctx.Function.Name}:{args}";
        }
    }
}
