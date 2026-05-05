namespace TokenVisualizer.Api.Models
{
    public class AnalysisRequest
    {
        public string? SystemPrompt { get; set; }
        public string? UserInput { get; set; }
        public List<string>? History { get; set; } // raw strings for simplicity in this POC
        public List<string>? RagChunks { get; set; }
        public string Model { get; set; } = "gpt-4o";
    }
}
