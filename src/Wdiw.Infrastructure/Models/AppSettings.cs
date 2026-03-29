using System.Text.Json.Serialization;

namespace Wdiw.Infrastructure.Models;

public record AppSettings
{
    public AiSettings Ai { get; init; }
    public CommitSettings Commit { get; init; }

    [JsonConstructor]
    public AppSettings(AiSettings? ai = null, CommitSettings? commit = null)
    {
        Ai = ai ?? new AiSettings();
        Commit = commit ?? new CommitSettings();
    }

    public static AppSettings Default => new();
}

public enum AiProvider
{
    OpenAi,
    Gemini,
    Claude,
    Ollama,
}

public record AiSettings(
    AiProvider Provider = AiProvider.OpenAi,
    string? ApiKey = null,
    string? ModelId = null,
    string? Endpoint = null
)
{
    public string GetActiveModel() => !string.IsNullOrWhiteSpace(ModelId)
        ? ModelId
        : GetDefaultModel(Provider);

    public string GetActiveEndpoint() => !string.IsNullOrWhiteSpace(Endpoint)
        ? Endpoint
        : (Provider == AiProvider.Ollama ? "http://localhost:11434" : string.Empty);

    public static string GetDefaultModel(AiProvider provider) => provider switch
    {
        AiProvider.OpenAi => "gpt-4o",
        AiProvider.Gemini => "gemini-2.0-flash",
        AiProvider.Claude => "claude-3-5-sonnet-latest",
        AiProvider.Ollama => "llama3.2",
        _ => "gpt-4o"
    };
}

public record CommitSettings(
    bool UseSemantic = true,
    string Language = "en",
    int MaxDiffLines = 500,
    bool PushAfterCommit = false,
    bool CheckLongFiles = false,
    string? Template = null
)
{
    public const int MaxDiffLinesMinimum = 100;
}