using Microsoft.ML.Tokenizers;

namespace TokenCountingDemo
{
    public class TokenLimitMiddleware(
    RequestDelegate next,
    TiktokenTokenizer tokenizer,
    ILogger<TokenLimitMiddleware> logger,
    int maxTokens)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Method == HttpMethods.Post &&
                context.Request.ContentType?.Contains("application/json") == true)
            {
                context.Request.EnableBuffering();

                using var reader = new StreamReader(
                    context.Request.Body, leaveOpen: true);

                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                int estimate = tokenizer.CountTokens(body);

                logger.LogInformation(
                    "Token estimate for {Path}: {Tokens}/{Max}",
                    context.Request.Path, estimate, maxTokens);

                if (estimate > maxTokens)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "prompt_too_large",
                        message =
                            $"Request body estimated at {estimate} tokens " +
                            $"which exceeds the limit of {maxTokens}.",
                        estimatedTokens = estimate,
                        limit = maxTokens
                    });
                    return;
                }
            }

            await next(context);
        }
    }
}
