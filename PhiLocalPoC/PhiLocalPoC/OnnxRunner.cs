using Microsoft.ML.OnnxRuntimeGenAI;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PhiLocalPoC
{
    public static class OnnxRunner
    {
        public static async Task<(string Text, double ElapsedMs)> RunAsync(
            string prompt, string modelPath, CancellationToken ct = default)
        {
            using var model = new Model(modelPath);
            using var tokenizer = new Tokenizer(model);

            var fullPrompt =
                "<|system|>You are a helpful C# assistant. Be concise.<|end|>" +
                $"<|user|>{prompt}<|end|>" +
                "<|assistant|>";

            var sequences = tokenizer.Encode(fullPrompt);

            var generatorParams = new GeneratorParams(model);
            generatorParams.SetSearchOption("max_length", 512);
            generatorParams.SetSearchOption("temperature", 0.7);

            using var generator = new Generator(model, generatorParams);
            generator.AppendTokenSequences(sequences);
            using var stream = tokenizer.CreateStream();

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var sb = new System.Text.StringBuilder();

            while (!generator.IsDone() && !ct.IsCancellationRequested)
            {
                generator.GenerateNextToken();

                var token = generator.GetSequence(0)[^1];
                var text = stream.Decode(token);

                sb.Append(text);
                Console.Write(text);
                await Task.Yield();
            }

            sw.Stop();
            Console.WriteLine();
            return (sb.ToString(), sw.Elapsed.TotalMilliseconds);
        }
    }
}