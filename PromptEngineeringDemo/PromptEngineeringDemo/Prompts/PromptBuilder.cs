using OpenAI.Chat;

namespace PromptEngineeringDemo.Prompts
{
    public static class PromptBuilder
    {
        // --- Summarisation ---

        private static readonly SystemChatMessage SummarySystemMessage = new("""
        You are a document summariser for a legal operations team.
        Always respond with a JSON object containing a single field: "bullet_points".
        The value must be a JSON array of exactly three strings.
        Each string is one complete sentence summarising a key point.
        Do not include any text outside the JSON object.
        If the document is empty or cannot be summarised, return:
        {"bullet_points": ["UNABLE_TO_SUMMARISE", "", ""]}
        """);

        public static List<ChatMessage> BuildSummaryMessages(string documentText)
        {
            // Sanitise the delimiter character to prevent injection
            string safe = documentText.Replace("<document>", "").Replace("</document>", "");

            return new List<ChatMessage>
        {
            SummarySystemMessage,
            new UserChatMessage($"<document>{safe}</document>")
        };
        }

        // --- Classification with few-shot ---

        private static readonly SystemChatMessage ClassificationSystemMessage = new("""
        You are a sentiment classifier.
        Respond with a JSON object with two fields:
        - "label": one of "positive", "negative", or "neutral"
        - "confidence": one of "high", "medium", or "low"
        No other text. No explanation. No markdown.
        """);

        // Few-shot examples built once as a static list
        private static readonly IReadOnlyList<ChatMessage> ClassificationExamples = new List<ChatMessage>
    {
        new UserChatMessage("Classify: \"Response times dropped by 60% after the cache rollout.\""),
        new AssistantChatMessage("{\"label\":\"positive\",\"confidence\":\"high\"}"),

        new UserChatMessage("Classify: \"The delivery arrived on time but everything was damaged.\""),
        new AssistantChatMessage("{\"label\":\"negative\",\"confidence\":\"high\"}"),

        new UserChatMessage("Classify: \"The system update was applied at 2am on Sunday.\""),
        new AssistantChatMessage("{\"label\":\"neutral\",\"confidence\":\"high\"}"),

        new UserChatMessage("Classify: \"The new UI looks better but the search is slower now.\""),
        new AssistantChatMessage("{\"label\":\"negative\",\"confidence\":\"medium\"}"),
    }.AsReadOnly();

        public static List<ChatMessage> BuildClassificationMessages(string inputText)
        {
            // Sanitise input before embedding
            string safe = inputText.Replace("\"", "'");

            var messages = new List<ChatMessage> { ClassificationSystemMessage };
            messages.AddRange(ClassificationExamples);
            messages.Add(new UserChatMessage($"Classify: \"{safe}\""));
            return messages;
        }

        // --- JSON extraction ---

        private static readonly SystemChatMessage ExtractionSystemMessage = new("""
        You are a data extraction engine.
        Extract the requested fields from the document provided.
        Return ONLY a JSON object matching the schema you are given.
        Do not invent data. If a field is absent, set it to null.
        Do not include markdown, code fences, or any text outside the JSON.
        """);

        public static List<ChatMessage> BuildExtractionMessages(string documentText)
        {
            string safe = documentText.Replace("<document>", "").Replace("</document>", "");

            return new List<ChatMessage>
        {
            ExtractionSystemMessage,
            new UserChatMessage($"""
                Extract customer name, order total (GBP), delivery date (YYYY-MM-DD),
                and line items (description, quantity, unit_price) from this document.

                <document>
                {safe}
                </document>
                """)
        };
        }
    }
}
