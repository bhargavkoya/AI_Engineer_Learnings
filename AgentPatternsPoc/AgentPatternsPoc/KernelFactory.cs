using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentPatternsPoc
{
    public static class KernelFactory
    {
        public static Kernel Create()
        {
            return Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion(
                    deploymentName: "chat-deployment",
                    endpoint: Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!,
                    apiKey: Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY")!)
                .Build();
        }
    }
}
