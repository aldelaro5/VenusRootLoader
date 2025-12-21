using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;
using VenusRootLoader.JsonConverters;

namespace VenusRootLoader.Config;

internal interface IBudConfigManager
{
    string GetConfigPathForBud(string budId);
    void Save(string budId, object configData);
    object Load(string budId, Type configType);
}

internal sealed class BudConfigManager : IBudConfigManager
{
    private readonly IFileSystem _fileSystem;
    private readonly BudLoaderContext _budLoaderContext;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Converters =
        {
            Vector2JsonConverter.Instance,
            Vector2IntJsonConverter.Instance,
            Vector3JsonConverter.Instance,
            Vector3IntJsonConverter.Instance,
            Vector4JsonConverter.Instance,
            QuaternionJsonConverter.Instance,
            RectJsonConverter.Instance,
            ColorJsonConverter.Instance,
            new JsonStringEnumConverter()
        },
        WriteIndented = true
    };

    public BudConfigManager(IFileSystem fileSystem, BudLoaderContext budLoaderContext)
    {
        _fileSystem = fileSystem;
        _budLoaderContext = budLoaderContext;
    }

    public string GetConfigPathForBud(string budId) =>
        _fileSystem.Path.Combine(_budLoaderContext.ConfigPath, $"{budId}.jsonc");

    public void Save(string budId, object configData)
    {
        string configPath = GetConfigPathForBud(budId);
        string json = JsonSerializer.Serialize(configData, _jsonSerializerOptions);
        File.WriteAllText(configPath, json);
    }

    public object Load(string budId, Type configType)
    {
        string configPath = GetConfigPathForBud(budId);
        using JsonDocument jsonDocument = JsonDocument.Parse(_fileSystem.File.ReadAllText(configPath));
        object? configData = jsonDocument.Deserialize(configType, _jsonSerializerOptions);
        return configData ?? throw new Exception($"The config file located at {configPath} contained \"null\"");
    }
}