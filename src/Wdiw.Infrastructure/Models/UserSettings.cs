namespace Wdiw.Infrastructure.Models;

public enum AiProvider
{
    OpenAi,
    Gemini,
    Claude,
    Ollama,
}

public record UserSettings(
    AiProvider Provider,
    string? ModelId,
    string? OllamaEndpoint,
    string? ApiKey
)
{
    public string DefaultAiModel => Provider switch
    {
        AiProvider.OpenAi => "gpt-4o",
        AiProvider.Gemini => "gemini-2.0-flash",
        AiProvider.Claude => "claude-3-5-sonnet-latest",
        AiProvider.Ollama => "llama3.2",
        _ => throw new ArgumentException(),
    };

    public string DefaultOllamaEndpoint => "http://localhost:11434";
}