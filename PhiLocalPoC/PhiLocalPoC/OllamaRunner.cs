using OllamaSharp;
using OllamaSharp.Models.Chat;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhiLocalPoC
{
    public static class OllamaRunner
    {
        public static async Task<(string Text, double ElapsedMs)> RunAsync(
            string prompt, CancellationToken ct = default)
        {
            var client = new OllamaApiClient(new Uri("http://localhost:11434"))
            {
                SelectedModel = "phi4-mini"
            };

            var messages = new List<Message>
            {
                new Message { Role = ChatRole.System, Content = "You are a helpful C# assistant. Be concise." },
                new Message { Role = ChatRole.User, Content = prompt }
            };

            var request = new ChatRequest
            {
                Messages = messages,
                Stream = true
            };

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var sb = new StringBuilder();

            await foreach (var response in client.ChatAsync(request, ct))
            {
                var token = response?.Message?.Content ?? string.Empty;
                sb.Append(token);
                Console.Write(token);
            }

            sw.Stop();
            Console.WriteLine();
            return (sb.ToString(), sw.Elapsed.TotalMilliseconds);
        }
    }
}