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

            IChatClient client = config.Provider switch
            {
                AiProvider.OpenAi => new OpenAIClient(config.ApiKey)
                    .GetChatClient(config.ModelId ?? config.DefaultAiModel).AsIChatClient(),

                AiProvider.Gemini => new GeminiChatClient(
                    apiKey: config.ApiKey,
                    model: config.ModelId ?? config.DefaultAiModel),

                AiProvider.Claude => new AnthropicClient(new Anthropic.Core.ClientOptions { ApiKey = config.ApiKey })
                    .AsIChatClient(config.ModelId ?? config.DefaultAiModel),

                AiProvider.Ollama => new OllamaApiClient(
                    uri: new Uri(config.OllamaEndpoint ?? config.DefaultOllamaEndpoint),
                    defaultModel: config.ModelId ?? config.DefaultAiModel
                ),

                _ => throw new NotSupportedException($"Provider {config.Provider} is not supported.")
            };

            return client
                .AsBuilder()
                .UseLogging() 
                .Build();
        });

        return services;
    }
}