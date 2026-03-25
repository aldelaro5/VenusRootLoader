using UnityEngine;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class RootCollector
{
    internal static readonly string[] LanguageDisplayNames = MainManager.languagenames.ToArray();

    private readonly IEnumerable<IBaseGameCollector> _baseGameCollectors;

    public RootCollector(IEnumerable<IBaseGameCollector> baseGameCollectors) =>
        _baseGameCollectors = baseGameCollectors;

    internal void CollectAndRegisterBaseGameData(string baseGameId)
    {
        foreach (IBaseGameCollector baseGameCollector in _baseGameCollectors)
            baseGameCollector.CollectBaseGameData(baseGameId);
    }

    internal static string[] ReadTextAssetLines(string resourcesPathSuffix)
    {
        return Resources.Load<TextAsset>($"{TextAssetPaths.RootDataPathPrefix}{resourcesPathSuffix}").text
            .Trim(StringUtils.NewlineSplitDelimiter)
            .Split(StringUtils.NewlineSplitDelimiter);
    }

    internal static string ReadWholeTextAsset(string resourcesPathSuffix)
    {
        return Resources
            .Load<TextAsset>($"{TextAssetPaths.RootDataPathPrefix}{resourcesPathSuffix}").text
            .Trim('\n');
    }

    internal static Dictionary<int, string[]> ReadLocalizedTestAssetLines(string resourcesPathSuffix)
    {
        Dictionary<int, string[]> localizedLines = new();
        for (int i = 0; i < LanguageDisplayNames.Length; i++)
        {
            string[] lines = Resources
                .Load<TextAsset>(
                    $"{TextAssetPaths.DataSlashDialogues}{i}/{resourcesPathSuffix}")
                .text
                .Trim(StringUtils.NewlineSplitDelimiter)
                .Split(StringUtils.NewlineSplitDelimiter);
            localizedLines.Add(i, lines);
        }

        return localizedLines;
    }
}