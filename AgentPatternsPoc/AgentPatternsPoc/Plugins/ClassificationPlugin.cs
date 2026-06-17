using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentPatternsPoc.Plugins
{
    public class ClassificationPlugin
    {
        // The LLM calls this to get classification guidance from a rules engine
        [KernelFunction("get_classification_rules")]
        [Description("Returns severity classification rules for bug reports")]
        public string GetClassificationRules() => """
        Critical: System down, data loss, security breach
        High: Feature unavailable for >10% of users
        Low: Cosmetic, documentation, minor UX
        """;
    }
}
