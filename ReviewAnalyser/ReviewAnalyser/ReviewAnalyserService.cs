using Azure;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;
using System.Text.Json.Schema;

namespace ReviewAnalyser
{
    public class ReviewAnalyserService(ChatClient chatClient, ILogger<ReviewAnalyserService> logger)
    {
        // Pre-build the ResponseFormat once at startup - no need to reconstruct per request
        // "review_analysis" must be snake_case; spaces or hyphens cause a 400
        private static readonly ChatResponseFormat SchemaFormat =
            ChatResponseFormat.CreateJsonSchemaFormat(
                                "review_analysis",
                                jsonSchema: BinaryData.FromBytes(
                                    JsonSerializer.SerializeToUtf8Bytes(
                                        JsonSerializerOptions.Default.GetJsonSchemaAsNode(typeof(ReviewAnalysis)))),
                                jsonSchemaIsStrict: true);

        public async Task<ReviewAnalysis?> AnalyseAsync(string reviewText)
        {
            var options = new ChatCompletionOptions
            {
                ResponseFormat = SchemaFormat,
                MaxOutputTokenCount = 512  // Cap output tokens - prevents runaway consumption
            };

            List<ChatMessage> messages =
            [
                new SystemChatMessage(
                "You are a precise product review analyst. " +
                "Extract structured information from the provided review. " +
                "Return only the requested fields - no additional commentary."),
            new UserChatMessage($"Analyse this review: {reviewText}")
            ];

            try
            {
                ClientResult<ChatCompletion> result =
                    await chatClient.CompleteChatAsync(messages, options);

                string json = result.Value.Content.FirstOrDefault()?.Text ?? string.Empty;

                logger.LogDebug("Review analysis JSON: {Json}", json);

                // With strict: true, this will succeed if the API call succeeded
                ReviewAnalysis analysis = JsonSerializer.Deserialize<ReviewAnalysis>(json)!;

                logger.LogInformation(
                    "Review analysed. Sentiment: {Sentiment}, Score: {Score}",
                    analysis.Sentiment, analysis.Score);

                return analysis;
            }
            catch (RequestFailedException ex) when (ex.Status == 400)
            {
                // Schema was rejected by the API - this is a code defect, not a transient failure
                // Do NOT retry: the same schema will produce the same 400
                logger.LogError(ex,
                    "Structured output schema rejected (400). " +
                    "Check for DateTime, Uri, TimeSpan in ReviewAnalysis record. " +
                    "ErrorCode: {ErrorCode}", ex.ErrorCode);
                return null;
            }
            catch (RequestFailedException ex) when (ex.Status == 429)
            {
                // SDK has exhausted its retry budget (default: 5 with our configuration)
                logger.LogWarning(ex,
                    "Azure OpenAI rate limit exceeded after SDK retries. " +
                    "Consider increasing deployment quota or adding request queuing.");
                return null;
            }
            catch (RequestFailedException ex)
            {
                // Catch-all for other API errors (503, 504, content filter, etc.)
                logger.LogError(ex,
                    "Azure OpenAI API error. Status: {Status}, ErrorCode: {ErrorCode}",
                    ex.Status, ex.ErrorCode);
                return null;
            }
        }
    }
}
