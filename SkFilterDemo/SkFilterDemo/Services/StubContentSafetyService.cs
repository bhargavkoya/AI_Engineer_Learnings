namespace SkFilterDemo.Services
{
    // Partial example - replace with Azure AI Content Safety in production
    public class StubContentSafetyService : IContentSafetyService
    {
        // A simplistic keyword blocklist - replace with a real moderation API
        private static readonly string[] _blockedTerms = ["weapon", "explosive", "self-harm"];

        public Task<bool> IsSafeAsync(string text, CancellationToken ct = default)
        {
            var lowerText = text.ToLowerInvariant();
            var isSafe = !_blockedTerms.Any(term => lowerText.Contains(term));
            return Task.FromResult(isSafe);
        }
    }
}
