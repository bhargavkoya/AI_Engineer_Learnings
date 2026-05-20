using System.ComponentModel;

namespace ReviewAnalyser
{
    public record ReviewAnalysis(
        
        // The product or item being reviewed
        string ProductName,

        // Summary of the review - 2-3 sentences, plain text, no markdown
        [property: Description("2-3 sentence summary of the review, plain text")]
        string Summary,

        // Sentiment classification - constrained vocabulary via Description
        [property: Description("Exactly one of: 'positive', 'neutral', 'negative'")]
        string Sentiment,

        // Numeric score 1-10
        int Score,

        // Key themes extracted from the review
        string[] Tags,

        // ISO 8601 date the review was submitted - nullable because it may not be in the review text
        [property: Description("ISO 8601 date string only, e.g. 2026-05-01. Empty string if not mentioned.")]
        string ReviewDate
    );
}
