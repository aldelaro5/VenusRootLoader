using CommunityToolkit.Diagnostics;
using HarmonyLib;
using System.IO.Abstractions;
using System.Reflection;
using Tomlet;
using Tomlet.Attributes;
using Tomlet.Models;
using UnityEngine;
using VenusRootLoader.Api;

namespace VenusRootLoader.BudLoading;

internal interface IBudConfigManager
{
    string GetConfigPathForBud(string budId);
    void Save(string budId, Type configType, object configData, object defaultConfigData);
    object Load(string budId, Type configType);
}

internal sealed class BudConfigManager : IBudConfigManager
{
    private static readonly Type[] UnityTableTypes =
    [
        typeof(Vector2),
        typeof(Vector2Int),
        typeof(Vector3),
        typeof(Vector3Int),
        typeof(Vector4),
        typeof(Quaternion),
        typeof(Rect)
    ];

    private readonly IFileSystem _fileSystem;
    private readonly BudLoaderContext _budLoaderContext;

    private readonly TomlSerializerOptions _tomlSerializerOptions = new()
    {
        OverrideConstructorValues = false,
        IgnoreNonPublicMembers = false,
        IgnoreInvalidEnumValues = false,
        MaxTableEntriesCountToInline = 4
    };

    public BudConfigManager(IFileSystem fileSystem, BudLoaderContext budLoaderContext)
    {
        _fileSystem = fileSystem;
        _budLoaderContext = budLoaderContext;

        TomletMain.RegisterMapper(
            v =>
            {
                TomlTable table = new();
                table.Put(nameof(Vector2.x), v.x);
                table.Put(nameof(Vector2.y), v.y);
                return table;
            },
            toml =>
            {
                TomlTable table = (TomlTable)toml;
                return new Vector2(
                    table.GetFloat(nameof(Vector2.x)),
                    table.GetFloat(nameof(Vector2.y)));
            });
        TomletMain.RegisterMapper(
            v =>
            {
                TomlTable table = new();
                table.Put(nameof(Vector2Int.x), v.x);
                table.Put(nameof(Vector2Int.y), v.y);
                return table;
            },
            toml =>
            {
                TomlTable table = (TomlTable)toml;
                return new Vector2Int(
                    table.GetInteger(nameof(Vector2Int.x)),
                    table.GetInteger(nameof(Vector2Int.y)));
            });

        TomletMain.RegisterMapper(
            v =>
            {
                TomlTable table = new();
                table.Put(nameof(Vector3.x), v.x);
                table.Put(nameof(Vector3.y), v.y);
                table.Put(nameof(Vector3.z), v.z);
                return table;
            },
            toml =>
            {
                TomlTable table = (TomlTable)toml;
                return new Vector3(
                    table.GetFloat(nameof(Vector3.x)),
                    table.GetFloat(nameof(Vector3.y)),
                    table.GetFloat(nameof(Vector3.z)));
            });
        TomletMain.RegisterMapper(
            v =>
            {
                TomlTable table = new();
                table.Put(nameof(Vector3Int.x), v.x);
                table.Put(nameof(Vector3Int.y), v.y);
                table.Put(nameof(Vector3Int.z), v.z);
                return table;
            },
            toml =>
            {
                TomlTable table = (TomlTable)toml;
                return new Vector3Int(
                    table.GetInteger(nameof(Vector3Int.x)),
                    table.GetInteger(nameof(Vector3Int.y)),
                    table.GetInteger(nameof(Vector3Int.z)));
            });

        TomletMain.RegisterMapper(
            v =>
            {
                TomlTable table = new();
                table.Put(nameof(Vector4.x), v.x);
                table.Put(nameof(Vector4.y), v.y);
                table.Put(nameof(Vector4.z), v.z);
                table.Put(nameof(Vector4.w), v.w);
                return table;
            },
            toml =>
            {
                TomlTable table = (TomlTable)toml;
                return new Vector4(
                    table.GetFloat(nameof(Vector4.x)),
                    table.GetFloat(nameof(Vector4.y)),
                    table.GetFloat(nameof(Vector4.z)),
                    table.GetFloat(nameof(Vector4.w)));
            });
        TomletMain.RegisterMapper(
            v =>
            {
                TomlTable table = new();
                table.Put(nameof(Quaternion.x), v.x);
                table.Put(nameof(Quaternion.y), v.y);
                table.Put(nameof(Quaternion.z), v.z);
                table.Put(nameof(Quaternion.w), v.w);
                return table;
            },
            toml =>
            {
                TomlTable table = (TomlTable)toml;
                return new Quaternion(
                    table.GetFloat(nameof(Quaternion.x)),
                    table.GetFloat(nameof(Quaternion.y)),
                    table.GetFloat(nameof(Quaternion.z)),
                    table.GetFloat(nameof(Quaternion.w)));
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

    public string GetConfigPathForBud(string budId) =>
        _fileSystem.Path.Combine(_budLoaderContext.ConfigPath, $"{budId}.toml");

    public void Save(string budId, Type configType, object configData, object defaultConfigData)
    {
        string configPath = GetConfigPathForBud(budId);
        TomlDocument? toml = TomletMain.DocumentFrom(configType, configData, _tomlSerializerOptions);
        if (toml is null)
            ThrowHelper.ThrowFormatException("The config data serialised to a null TOML string");

        ProcessTomlEntries(toml.Entries, configType, defaultConfigData);
        _fileSystem.File.WriteAllText(configPath, toml.SerializedValue);
    }

    private void ProcessTomlEntries(
        Dictionary<string, TomlValue> entries,
        Type configType,
        object? defaultConfigData)
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
            (Type Type, object? DefaultValue) configValueInfo = ObtainConfigValueInfo(
                tomlEntry.Key,
                membersByMappedNames,
                defaultConfigData);
            bool isUnityTableType = UnityTableTypes.Contains(configValueInfo.Type);
            bool isDictionary = configValueInfo.Type.IsGenericType &&
                                configValueInfo.Type.GetGenericTypeDefinition() == typeof(Dictionary<,>);

            if (tomlEntry.Value is TomlTable tomlTable && !isUnityTableType && !isDictionary)
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
            if (isUnityTableType)
            {
                defaultValueStr = configValueInfo.DefaultValue!.ToString();
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

    private static (Type Type, object? DefaultValue) ObtainConfigValueInfo(
        string key,
        Dictionary<string, MemberInfo> memberInfos,
        object? defaultConfigData)
    {
        MemberInfo memberInfo = memberInfos[key];
        if (memberInfo is PropertyInfo propertyInfo)
        {
            return (propertyInfo.PropertyType, defaultConfigData is not null
                ? propertyInfo.GetValue(defaultConfigData, null)
                : null);
        }

        FieldInfo fieldInfo = (FieldInfo)memberInfo;
        return (fieldInfo.FieldType, defaultConfigData is not null
            ? fieldInfo.GetValue(defaultConfigData)
            : null);
    }

    public object Load(string budId, Type configType)
    {
        string configPath = GetConfigPathForBud(budId);
        return TomletMain.To(configType, _fileSystem.File.ReadAllText(configPath), _tomlSerializerOptions);
    }
}