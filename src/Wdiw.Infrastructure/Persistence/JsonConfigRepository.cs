using System.Text.Json;
using System.Text.Json.Serialization;
using Wdiw.Infrastructure.Abstractions;
using Wdiw.Infrastructure.Models;

namespace Wdiw.Infrastructure.Persistence;

public class JsonConfigRepository : IConfigRepository
{
    private readonly string _configPath;

    public JsonConfigRepository()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var configDir = Path.Combine(appData, "wdiw");
        _configPath = Path.Combine(configDir, "config.json");

        if (!Directory.Exists(configDir))
        {
            Directory.CreateDirectory(configDir);
        }
    }

    public AppSettings GetSettings()
    {
        if (!File.Exists(_configPath))
        {
            var defaultSettings = AppSettings.Default;
            SaveSettings(defaultSettings);
            return defaultSettings;
        }

        try
        {
            var json = File.ReadAllText(_configPath);
            return JsonSerializer.Deserialize(json, ConfigJsonContext.Default.AppSettings)
                   ?? AppSettings.Default;
        }
        catch
        {
            return AppSettings.Default;
        }
    }

    public void SaveSettings(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, ConfigJsonContext.Default.AppSettings);
        File.WriteAllText(_configPath, json);
    }
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    Converters = [typeof(JsonStringEnumConverter<AiProvider>)] 
)]

[JsonSerializable(typeof(AppSettings))]
[JsonSerializable(typeof(AiSettings))]
[JsonSerializable(typeof(CommitSettings))]
[JsonSerializable(typeof(AiProvider))]
internal partial class ConfigJsonContext : JsonSerializerContext
{
}