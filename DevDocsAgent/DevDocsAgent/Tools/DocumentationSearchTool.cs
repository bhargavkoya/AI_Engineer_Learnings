using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace DevDocsAgent.Tools
{
    public static class DocumentationSearchTool
    {
        private static readonly Dictionary<string, string> _docs = new()
        {
            ["dependency-injection"] = "ASP.NET Core DI: use IServiceCollection in Program.cs. " +
                "Register services with AddScoped, AddSingleton, or AddTransient.",
            ["minimal-api"] = "Minimal APIs: map endpoints with app.MapGet/MapPost. " +
                "Use TypedResults for OpenAPI-compatible responses.",
            ["agent-framework"] = "Agent Framework: install Microsoft.Agents.AI.Hosting. " +
                "Register agents with builder.AddAIAgent(). Expose via MapOpenAIChatCompletions."
        };

        [Description("Search the internal developer documentation for a given topic. " +
                     "Returns the most relevant documentation snippet.")]
        public static string SearchDocs(
            [Description("The topic or keyword to search for, e.g. 'dependency injection'")] string topic)
        {
            var match = _docs
                .FirstOrDefault(kv => kv.Key.Contains(topic.ToLower()) ||
                                      topic.ToLower().Contains(kv.Key));

            return match.Value is not null
                ? match.Value
                : $"No documentation found for '{topic}'. Try 'dependency-injection', 'minimal-api', or 'agent-framework'.";
        }
    }
}