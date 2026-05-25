using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkPluginDemo
{
    public class TravelPlugin
    {
        // Native function - deterministic C# code, no LLM call
        [KernelFunction("get_weather")]
        [Description("Get the current weather for a travel destination city. Returns temperature in Celsius and a short condition description.")]
        public Task<string> GetWeatherAsync(
            [Description("The destination city, e.g. 'Dublin', 'Tokyo', 'Cape Town'")] string city)
        {
            // In production: call a real weather API (OpenWeatherMap, etc.)
            // Here: simulated data to keep the POC self-contained
            var conditions = city.ToLower() switch
            {
                "dublin" => "12°C, overcast with light rain",
                "tokyo" => "22°C, sunny with light cloud",
                "cape town" => "18°C, partly cloudy, sea breeze",
                _ => "20°C, conditions unknown for this city"
            };

            return Task.FromResult($"Weather in {city}: {conditions}");
        }
    }
}
