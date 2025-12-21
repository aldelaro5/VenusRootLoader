using CommunityToolkit.Diagnostics;
using System.Globalization;
using System.IO.Abstractions;
using System.Reflection;
using Tomlet;
using Tomlet.Models;
using UnityEngine;

namespace VenusRootLoader.Config;

internal interface IBudConfigManager
{
    string GetConfigPathForBud(string budId);
    void Save(string budId, Type configType, object configData, object defaultConfigData);
    object Load(string budId, Type configType);
}

internal sealed class BudConfigManager : IBudConfigManager
{
    private static readonly char[] Separator = [','];

    private readonly IFileSystem _fileSystem;
    private readonly BudLoaderContext _budLoaderContext;

    private readonly TomlSerializerOptions _tomlSerializerOptions = new()
    {
        OverrideConstructorValues = false,
        IgnoreNonPublicMembers = false,
        IgnoreInvalidEnumValues = false
    };

    public BudConfigManager(IFileSystem fileSystem, BudLoaderContext budLoaderContext)
    {
        _fileSystem = fileSystem;
        _budLoaderContext = budLoaderContext;

        TomletMain.RegisterMapper(
            v => new TomlString($"({v.x:G9}, {v.y:G9})"),
            toml =>
            {
                string[] valuesSplit = ValidateAndExtractVectorComponents(toml, nameof(Vector2), 2);
                return new Vector2(
                    float.Parse(valuesSplit[0], CultureInfo.InvariantCulture),
                    float.Parse(valuesSplit[1], CultureInfo.InvariantCulture));
            });
        TomletMain.RegisterMapper(
            v => new TomlString($"({v.x}, {v.y})"),
            toml =>
            {
                string[] valuesSplit = ValidateAndExtractVectorComponents(toml, nameof(Vector2Int), 2);
                return new Vector2Int(
                    int.Parse(valuesSplit[0], CultureInfo.InvariantCulture),
                    int.Parse(valuesSplit[1], CultureInfo.InvariantCulture));
            });

        TomletMain.RegisterMapper(
            v => new TomlString($"({v.x:G9}, {v.y:G9}, {v.z:G9})"),
            toml =>
            {
                string[] valuesSplit = ValidateAndExtractVectorComponents(toml, nameof(Vector3), 3);
                return new Vector3(
                    float.Parse(valuesSplit[0], CultureInfo.InvariantCulture),
                    float.Parse(valuesSplit[1], CultureInfo.InvariantCulture),
                    float.Parse(valuesSplit[2], CultureInfo.InvariantCulture));
            });
        TomletMain.RegisterMapper(
            v => new TomlString($"({v.x}, {v.y}, {v.y})"),
            toml =>
            {
                string[] valuesSplit = ValidateAndExtractVectorComponents(toml, nameof(Vector3Int), 3);
                return new Vector3Int(
                    int.Parse(valuesSplit[0], CultureInfo.InvariantCulture),
                    int.Parse(valuesSplit[1], CultureInfo.InvariantCulture),
                    int.Parse(valuesSplit[2], CultureInfo.InvariantCulture));
            });

        TomletMain.RegisterMapper(
            v => new TomlString($"({v.x:G9}, {v.y:G9}, {v.z:G9}, {v.w:G9})"),
            toml =>
            {
                string[] valuesSplit = ValidateAndExtractVectorComponents(toml, nameof(Vector4), 4);
                return new Vector4(
                    float.Parse(valuesSplit[0], CultureInfo.InvariantCulture),
                    float.Parse(valuesSplit[1], CultureInfo.InvariantCulture),
                    float.Parse(valuesSplit[2], CultureInfo.InvariantCulture),
                    float.Parse(valuesSplit[3], CultureInfo.InvariantCulture));
            });
        TomletMain.RegisterMapper(
            v => new TomlString($"({v.x:G9}, {v.y:G9}, {v.z:G9}, {v.w:G9})"),
            toml =>
            {
                string[] valuesSplit = ValidateAndExtractVectorComponents(toml, nameof(Quaternion), 4);
                return new Quaternion(
                    float.Parse(valuesSplit[0], CultureInfo.InvariantCulture),
                    float.Parse(valuesSplit[1], CultureInfo.InvariantCulture),
                    float.Parse(valuesSplit[2], CultureInfo.InvariantCulture),
                    float.Parse(valuesSplit[3], CultureInfo.InvariantCulture));
            });

        TomletMain.RegisterMapper(
            v => new TomlString($"#{ColorUtility.ToHtmlStringRGBA(v)}"),
            toml =>
            {
                if (!ColorUtility.TryParseHtmlString(toml.StringValue, out Color value))
                {
                    ThrowHelper.ThrowFormatException<Color>(
                        $"A {nameof(Color)} value must be in the format #RRGGBBAA with hexadecimal components");
                }

                return value;
            });

        TomletMain.RegisterMapper(
            v =>
            {
                TomlTable table = new();
                table.Put(nameof(Rect.x), v.x);
                table.Put(nameof(Rect.y), v.y);
                table.Put(nameof(Rect.width), v.width);
                table.Put(nameof(Rect.height), v.height);
                return table;
            },
            toml =>
            {
                TomlTable table = (TomlTable)toml;
                return new Rect(
                    table.GetFloat(nameof(Rect.x)),
                    table.GetFloat(nameof(Rect.y)),
                    table.GetFloat(nameof(Rect.width)),
                    table.GetFloat(nameof(Rect.height)));
            });
    }

    private static string[] ValidateAndExtractVectorComponents(
        TomlValue toml,
        string typeName,
        int expectedComponentCount)
    {
        ReadOnlySpan<char> bytes = toml.StringValue.AsSpan();
        if (bytes[0] != '(' || bytes[^1] != ')')
        {
            ThrowHelper.ThrowFormatException<Vector2>(
                $"A {typeName} value must begin with \"(\" and end with \")\"");
        }

        string values = bytes[1..^1].ToString();
        string[] valuesSplit = values.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

        if (valuesSplit.Length != expectedComponentCount)
        {
            ThrowHelper.ThrowFormatException<Vector2>(
                $"A {typeName} value must have {expectedComponentCount} components separated by commas");
        }

        return valuesSplit;
    }

    public string GetConfigPathForBud(string budId) =>
        _fileSystem.Path.Combine(_budLoaderContext.ConfigPath, $"{budId}.toml");

    public void Save(string budId, Type configType, object configData, object defaultConfigData)
    {
        string configPath = GetConfigPathForBud(budId);
        TomlDocument? toml = TomletMain.DocumentFrom(configType, configData, _tomlSerializerOptions);
        if (toml is null)
            ThrowHelper.ThrowFormatException("The config data serialised to a null TOML string");

        ProcessTomlEntries(toml.Entries, configType, defaultConfigData);

        File.WriteAllText(configPath, toml.SerializedValue);
    }

    private void ProcessTomlEntries(
        Dictionary<string, TomlValue> entries,
        Type configType,
        object defaultConfigData)
    {
        PropertyInfo[] propertyInfos = configType.GetProperties();
        FieldInfo[] fieldInfos =
            configType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        foreach (KeyValuePair<string, TomlValue> tomlEntry in entries)
        {
            var configValueInfo = ObtainConfigValueInfo(
                tomlEntry.Key,
                propertyInfos,
                fieldInfos,
                defaultConfigData);
            bool isRect = configValueInfo.type == typeof(Rect);
            bool isDictionary = configValueInfo.type.IsGenericType &&
                                 configValueInfo.type.GetGenericTypeDefinition() == typeof(Dictionary<,>);

            if (tomlEntry.Value is TomlTable tomlTable && (!isRect && !isDictionary))
            {
                tomlTable.ForceNoInline = true;
                ProcessTomlEntries(tomlTable.Entries, configValueInfo.type, configValueInfo.defaultValue);
                continue;
            }

            TomlValue? defaultTomlValue = TomletMain.ValueFrom(
                configValueInfo.type,
                configValueInfo.defaultValue,
                _tomlSerializerOptions);

            string defaultValueStr;
            if (isRect)
            {
                defaultValueStr = configValueInfo.defaultValue.ToString();
            }
            else if (defaultTomlValue is TomlArray { CanBeSerializedInline: false } tomlArray)
            {
                defaultValueStr = $"\n{tomlArray.SerializeTableArray(configValueInfo.name)}";
            }
            else
            {
                defaultValueStr = defaultTomlValue?.SerializedValue ?? "null";
            }

            string newComment = $"Type: {configValueInfo.type.Name}" +
                                $"\nDefault value: {defaultValueStr}";

            string? comments = tomlEntry.Value.Comments.PrecedingComment;
            if (string.IsNullOrEmpty(comments))
            {
                tomlEntry.Value.Comments.PrecedingComment = newComment;
                continue;
            }

            comments += $"\n{newComment}";
            tomlEntry.Value.Comments.PrecedingComment = comments;
        }
    }

    private (Type type, object defaultValue, string name) ObtainConfigValueInfo(
        string key,
        PropertyInfo[] propertyInfos,
        FieldInfo[] fieldInfos,
        object defaultConfigData)
    {
        PropertyInfo? prop = propertyInfos.SingleOrDefault(p => p.Name == key);
        if (prop is not null)
            return (prop.PropertyType, prop.GetValue(defaultConfigData, null), prop.Name);
        FieldInfo fieldInfo = fieldInfos.Single(f => f.Name == key);
        return (fieldInfo.FieldType, fieldInfo.GetValue(defaultConfigData), fieldInfo.Name);
    }

    public object Load(string budId, Type configType)
    {
        string configPath = GetConfigPathForBud(budId);
        return TomletMain.To(configType, _fileSystem.File.ReadAllText(configPath), _tomlSerializerOptions);
    }
}