using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using OllamaSharp;
using Mscc.GenerativeAI.Microsoft;
using Anthropic;
using Wdiw.Infrastructure.Abstractions;
using Wdiw.Infrastructure.Models;
using Wdiw.Infrastructure.Persistence;
using Wdiw.Infrastructure.Services;

namespace Wdiw.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IGitService, GitCliService>();
        services.AddSingleton<IConfigRepository, JsonConfigRepository>();

        services.AddChatClient(sp =>
        {
            var config = sp.GetRequiredService<IConfigRepository>().GetSettings();

            IChatClient client = config.Ai.Provider switch
            {
                AiProvider.OpenAi => new OpenAIClient(config.Ai.ApiKey)
                    .GetChatClient(config.Ai.GetActiveModel()).AsIChatClient(),

                AiProvider.Gemini => new GeminiChatClient(
                    apiKey: config.Ai.ApiKey,
                    model: config.Ai.GetActiveModel()),

                AiProvider.Claude => new AnthropicClient(new Anthropic.Core.ClientOptions { ApiKey = config.Ai.ApiKey })
                    .AsIChatClient(config.Ai.GetActiveModel()),

                AiProvider.Ollama => new OllamaApiClient(
                    uri: new Uri(config.Ai.GetActiveEndpoint()),
                    defaultModel: config.Ai.GetActiveModel()
                ),

                _ => throw new NotSupportedException($"Provider {config.Ai.Provider} is not supported.")
            };

            return client
                .AsBuilder()
                .UseLogging() 
                .Build();
        });

        return services;
    }
}