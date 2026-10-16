using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text;
using UnityEngine;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

/// <summary>
/// A service to call all the collector's collection methods. This is meant to be resolved from Entry early on boot.
/// It also contains convenience methods for the collectors to use.
/// </summary>
internal sealed class RootCollector
{
    internal static readonly string[] LanguageDisplayNames = MainManager.languagenames.ToArray();

    private readonly IEnumerable<IBaseGameCollector> _baseGameCollectors;

    public RootCollector(IEnumerable<IBaseGameCollector> baseGameCollectors) =>
        _baseGameCollectors = baseGameCollectors;

    internal void CollectAndRegisterBaseGameData()
    {
        foreach (IBaseGameCollector baseGameCollector in _baseGameCollectors)
            baseGameCollector.CollectBaseGameData();
    }

    internal static string[] ReadTextAssetLines(string resourcesPathSuffix)
    {
        return Resources.Load<TextAsset>($"{ResourcesPaths.RootDataPathPrefix}{resourcesPathSuffix}").text
            .Trim(StringUtils.NewlineSplitDelimiter)
            .Split(StringUtils.NewlineSplitDelimiter);
    }

    internal static string ReadWholeTextAsset(string resourcesPathSuffix)
    {
        return Resources
            .Load<TextAsset>($"{ResourcesPaths.RootDataPathPrefix}{resourcesPathSuffix}").text
            .Trim('\n');
    }

    internal static Dictionary<int, string[]> ReadLocalizedTestAssetLines(string resourcesPathSuffix)
    {
        Dictionary<int, string[]> localizedLines = new();
        for (int i = 0; i < LanguageDisplayNames.Length; i++)
        {
            string[] lines = Resources
                .Load<TextAsset>(
                    $"{ResourcesPaths.DataSlashDialogues}{i}/{resourcesPathSuffix}")
                .text
                .Trim(StringUtils.NewlineSplitDelimiter)
                .Split(StringUtils.NewlineSplitDelimiter);
            localizedLines.Add(i, lines);
        }

        return localizedLines;
    }

    internal static void LogCollectedAmount(
        Microsoft.Extensions.Logging.ILogger logger,
        ILeavesRegistry registry,
        int amount)
    {
        logger.LogInformation(
            "Collected and registered {amount} base game {registryName}",
            amount,
            registry.RegistryName);
    }

    internal static TDelegate CreateOpenDelegateFromMethod<TDelegate>(MethodInfo method)
        where TDelegate : Delegate
    {
        return (TDelegate)Delegate.CreateDelegate(
            typeof(TDelegate),
            null,
            method);
    }

    internal static Dictionary<string, MethodInfo> GetLeavesMethods(
        Type topLevelType,
        string methodName,
        string? methodSegmentName = null)
    {
        StringBuilder sb = new(methodName);
        sb.Append('_');
        if (!string.IsNullOrEmpty(methodSegmentName))
        {
            sb.Append(methodSegmentName);
            sb.Append('_');
        }

        string prefix = sb.ToString();
        return topLevelType
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static)
            .Where(x => x.Name.StartsWith(prefix))
            .ToDictionary(x => x.Name.Remove(0, prefix.Length), x => x);
    }
}