using Microsoft.Extensions.AI;
using OllamaSharp;
using PhiLocalPoC;

const string onnxModelPath = "./phi4-mini-onnx/cpu_and_mobile/cpu-int4-rtn-block-32-acc-level-4";
const string testPrompt = "Explain the difference between IEnumerable and IQueryable in C# in three sentences.";

Console.WriteLine("=== APPROACH 1: Ollama + OllamaSharp ===");
Console.WriteLine();
var (ollamaText, ollamaMs) = await OllamaRunner.RunAsync(testPrompt);

Console.WriteLine();
Console.WriteLine("=== APPROACH 2: ONNX Runtime GenAI ===");
Console.WriteLine();
var (onnxText, onnxMs) = await OnnxRunner.RunAsync(testPrompt, onnxModelPath);

Console.WriteLine();
Console.WriteLine("=== TIMING COMPARISON ===");
Console.WriteLine($"Ollama:           {ollamaMs:F0}ms");
Console.WriteLine($"ONNX Runtime:     {onnxMs:F0}ms");
Console.WriteLine($"ONNX speedup:     {ollamaMs / onnxMs:F1}x");

// Character count is an approximation - both are the same underlying model
Console.WriteLine($"Output lengths:   Ollama={ollamaText.Length} chars, ONNX={onnxText.Length} chars");