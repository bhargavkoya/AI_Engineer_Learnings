using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkAgentDemo.Plugins
{
    public class CalculatorPlugin
    {
        [KernelFunction("multiply")]
        [Description("Multiplies two numbers. Use for any multiplication request.")]
        public double Multiply(
            [Description("The first number")] double a,
            [Description("The second number")] double b) => a * b;

        [KernelFunction("add")]
        [Description("Adds two numbers together. Use for addition or summing.")]
        public double Add(
            [Description("The first number")] double a,
            [Description("The second number")] double b) => a + b;

        [KernelFunction("divide")]
        [Description("Divides the first number by the second. Returns an error if divisor is zero.")]
        public string Divide(
            [Description("The dividend")] double a,
            [Description("The divisor - must not be zero")] double b)
        {
            // Guard against division by zero before the LLM ever sees a result
            if (b == 0) return "Error: cannot divide by zero.";
            return (a / b).ToString("F4"); // 4 decimal places for clarity
        }
    }
}
