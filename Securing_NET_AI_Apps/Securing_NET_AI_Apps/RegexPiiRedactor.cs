using System.Text.RegularExpressions;

namespace Securing_NET_AI_Apps
{
    public sealed record RedactionResult(string RedactedText, bool WasRedacted, int MatchCount);
    public sealed class RegexPiiRedactor : IPiiRedactor
    {
        // Order matters: match more specific patterns before generic ones
        private static readonly (string Label, Regex Pattern)[] Patterns =
        [
            ("EMAIL", new Regex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled)),
        ("SSN", new Regex(@"\b\d{3}-\d{2}-\d{4}\b", RegexOptions.Compiled)),
        ("CREDIT_CARD", new Regex(@"\b(?:\d[ -]*?){13,16}\b", RegexOptions.Compiled)),
        ("PHONE", new Regex(@"\b(\+?1[-.\s]?)?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}\b", RegexOptions.Compiled)),
    ];

        public RedactionResult Redact(string input)
        {
            var redacted = input;
            var matchCount = 0;

            foreach (var (label, pattern) in Patterns)
            {
                redacted = pattern.Replace(redacted, match =>
                {
                    matchCount++;
                    return $"[REDACTED_{label}]"; // Keep the placeholder informative for debugging
                });
            }

            return new RedactionResult(redacted, matchCount > 0, matchCount);
        }
    }
}
