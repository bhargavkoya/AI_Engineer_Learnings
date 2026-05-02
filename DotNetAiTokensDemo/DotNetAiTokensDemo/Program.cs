// Program.cs
using DotNetAiTokensDemo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI;
using OpenAI.Chat;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddUserSecrets<Program>(optional: true);
    })
    .ConfigureServices((context, services) =>
    {
        var cfgSection = context.Configuration.GetSection("OpenAI");
        var appConfig = new AppConfig
        {
            ApiKey = cfgSection["ApiKey"] ?? throw new InvalidOperationException("OpenAI:ApiKey not configured."),
            Model = cfgSection["Model"] ?? "gpt-4.1-mini",
            // You could also bind prices here from config if you want.
        };
        services.AddSingleton(appConfig);

        services.AddSingleton(sp =>
        {
            return new OpenAIClient(appConfig.ApiKey);
        });

        services.AddSingleton<ChatService>();
    })
    .Build();

var chatService = host.Services.GetRequiredService<ChatService>();

Console.WriteLine("DotNet AI Tokens Demo");
Console.WriteLine("Type your prompt and press Enter. Empty line to exit.");

while (true)
{
    Console.Write("> ");
    var prompt = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(prompt))
        break;

    await chatService.RunSinglePromptAsync(prompt);
}

Console.WriteLine("Goodbye!");