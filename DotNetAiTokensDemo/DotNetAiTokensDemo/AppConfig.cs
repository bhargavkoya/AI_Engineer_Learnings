using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetAiTokensDemo
{
    public sealed class AppConfig
    {
        public string ApiKey { get; init; } = string.Empty;
        public string Model { get; init; } = "gpt-4.1-mini";

        // Example prices; replace with current provider pricing
        public decimal InputPricePerMillion { get; init; } = 0.50m;
        public decimal OutputPricePerMillion { get; init; } = 1.50m;
    }
}
