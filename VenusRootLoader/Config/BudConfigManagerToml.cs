using CommunityToolkit.Diagnostics;
using HarmonyLib;
using System.Globalization;
using System.IO.Abstractions;
using System.Reflection;
using Tomlet;
using Tomlet.Attributes;
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
        List<MemberInfo> memberInfos = AccessTools.GetDeclaredProperties(configType)
            .Cast<MemberInfo>()
            .ToList();
        memberInfos.AddRange(AccessTools.GetDeclaredFields(configType));
        Dictionary<string, MemberInfo> membersByMappedNames = memberInfos.ToDictionary(
            m => m.GetCustomAttribute<TomlPropertyAttribute>()?.GetMappedString() ?? m.Name,
            m => m);

        foreach (KeyValuePair<string, TomlValue> tomlEntry in entries)
        {
            (Type Type, object DefaultValue) configValueInfo = ObtainConfigValueInfo(
                tomlEntry.Key,
                membersByMappedNames,
                defaultConfigData);
            bool isRect = configValueInfo.Type == typeof(Rect);
            bool isDictionary = configValueInfo.Type.IsGenericType &&
                                configValueInfo.Type.GetGenericTypeDefinition() == typeof(Dictionary<,>);

            if (tomlEntry.Value is TomlTable tomlTable && !isRect && !isDictionary)
            {
                tomlTable.ForceNoInline = true;
                ProcessTomlEntries(tomlTable.Entries, configValueInfo.Type, configValueInfo.DefaultValue);
                continue;
            }

            TomlValue? defaultTomlValue = TomletMain.ValueFrom(
                configValueInfo.Type,
                configValueInfo.DefaultValue,
                _tomlSerializerOptions);

            string defaultValueStr;
            if (isRect)
            {
                defaultValueStr = configValueInfo.DefaultValue.ToString();
            }
            else if (defaultTomlValue is TomlArray { CanBeSerializedInline: false } tomlArray)
            {
                defaultValueStr = $"\n{tomlArray.SerializeTableArray(tomlEntry.Key)}";
            }
            else
            {
                defaultValueStr = defaultTomlValue?.SerializedValue ?? "null";
            }

            string newComment = $"Type: {configValueInfo.Type.FullDescription()}" +
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

    private static (Type Type, object DefaultValue) ObtainConfigValueInfo(
        string key,
        Dictionary<string, MemberInfo> memberInfos,
        object defaultConfigData)
    {
        MemberInfo memberInfo = memberInfos[key];
        if (memberInfo is PropertyInfo propertyInfo)
            return (propertyInfo.PropertyType, propertyInfo.GetGetMethod().Invoke(defaultConfigData, null));
        FieldInfo fieldInfo = (FieldInfo)memberInfo;
        return (fieldInfo.FieldType, fieldInfo.GetValue(defaultConfigData));
    }

    public object Load(string budId, Type configType)
    {
        string configPath = GetConfigPathForBud(budId);
        return TomletMain.To(configType, _fileSystem.File.ReadAllText(configPath), _tomlSerializerOptions);
    }
}