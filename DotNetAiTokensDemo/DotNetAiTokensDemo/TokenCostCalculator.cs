using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetAiTokensDemo
{
    public static class TokenCostCalculator
    {
        public static decimal EstimateCost(
            int inputTokens,
            int outputTokens,
            decimal inputPricePerMillion,
            decimal outputPricePerMillion)
        {
            var inputCost = (inputTokens / 1_000_000m) * inputPricePerMillion;
            var outputCost = (outputTokens / 1_000_000m) * outputPricePerMillion;
            return inputCost + outputCost;
        }
    }
}
