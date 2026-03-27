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

    public UserSettings GetSettings()
    {
        if (!File.Exists(_configPath))
            throw new FileNotFoundException("The user config file was not found.", _configPath);

        try
        {
            var json = File.ReadAllText(_configPath);
            return JsonSerializer.Deserialize(json, ConfigJsonContext.Default.UserSettings)
                   ?? throw new Exception("Failed to deserialize user settings");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Invalid configuration at {_configPath}: {ex.Message}");
        }
    }

    public void SaveSettings(UserSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, ConfigJsonContext.Default.UserSettings);
        File.WriteAllText(_configPath, json);
    }

}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(UserSettings))]
[JsonConverter(typeof(JsonStringEnumConverter<AiProvider>))]
internal partial class ConfigJsonContext : JsonSerializerContext
{
}