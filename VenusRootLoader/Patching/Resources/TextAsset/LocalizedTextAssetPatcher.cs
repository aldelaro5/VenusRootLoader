using Microsoft.Extensions.Logging;
using System.Text;
using VenusRootLoader.BaseGameData;

namespace VenusRootLoader.Patching.Resources.TextAsset;

internal interface ILocalizedTextAssetPatcher
{
    UnityEngine.TextAsset PatchResource(int languageId, string subpath, UnityEngine.TextAsset original);
}

internal sealed class LocalizedTextAssetPatcher<T> : ILocalizedTextAssetPatcher
    where T : ITextAssetSerializable
{
    private readonly char[] _textAssetsSplitLineSeparator = ['\n'];
    private readonly ILogger<LocalizedTextAssetPatcher<T>> _logger;

    private Dictionary<int, Dictionary<int, T>> TextAssetsChangedLines { get; } = new();
    private List<Dictionary<int, T>> TextAssetsCustomLines { get; } = new();

    public LocalizedTextAssetPatcher(
        string subpath,
        RootTextAssetPatcher rootTextAssetPatcher,
        ILogger<LocalizedTextAssetPatcher<T>> logger)
    {
        _logger = logger;
        rootTextAssetPatcher.RegisterLocalizedTextAssetPatcher(subpath, this);
    }

    internal void AddNewDataToTextAsset(Dictionary<int, T> data)
    {
        TextAssetsCustomLines.Add(data);
    }

    internal void ChangeVanillaDataOfTextAsset(int lineIndex, Dictionary<int, T> data)
    {
        if (TextAssetsChangedLines.ContainsKey(lineIndex))
            return;
        TextAssetsChangedLines[lineIndex] = data;
    }

    public UnityEngine.TextAsset PatchResource(int languageId, string subpath, UnityEngine.TextAsset original)
    {
        List<KeyValuePair<int, Dictionary<int, T>>> changes = new();
        foreach (KeyValuePair<int, Dictionary<int, T>> l in TextAssetsChangedLines)
        {
            if (l.Value.ContainsKey(languageId))
                changes.Add(l);
        }

        bool changedLinesExists = changes.Count > 0;
        bool customLinesExists = TextAssetsCustomLines.Count > 0;

        if (!changedLinesExists && !customLinesExists)
            return original;

        string[] lines = original.text.Split(_textAssetsSplitLineSeparator, StringSplitOptions.RemoveEmptyEntries);
        // Workaround a game bug where not all languages has the last line about BigBerry
        if (subpath.Equals("Items", StringComparison.OrdinalIgnoreCase) &&
            lines.Length != BaseGameDataCollector.ItemNamedIds.Length)
        {
            lines = lines.Append("RESERVED@Desc@Desc@a").ToArray();
        }

        StringBuilder sb = new();
        if (changedLinesExists)
        {
            foreach (KeyValuePair<int, Dictionary<int, T>> customLine in changes)
                lines[customLine.Key] = customLine.Value[languageId].GetTextAssetSerializedString();
            sb.Append(string.Join("\n", lines));
        }
        else
        {
            sb.Append(original.text.TrimEnd(_textAssetsSplitLineSeparator));
        }

        if (customLinesExists)
        {
            sb.Append('\n');
            sb.Append(
                string.Join("\n", TextAssetsCustomLines.Select(l => GetLocalizedSerializedString(languageId, l))));
        }

        string text = sb.ToString();
        _logger.LogTrace("Patching {path} for language {language}:\n{text}", subpath, languageId, text);
        return new UnityEngine.TextAsset(text);
    }

    private static string GetLocalizedSerializedString(int languageId, Dictionary<int, T> customLineByLanguage)
    {
        if (customLineByLanguage.Count == 0)
            return Activator.CreateInstance<T>().GetTextAssetSerializedString();

        if (customLineByLanguage.TryGetValue(languageId, out T value))
            return value.GetTextAssetSerializedString();

        int firstLanguage = customLineByLanguage.Keys.Min();
        return customLineByLanguage[firstLanguage].GetTextAssetSerializedString();
    }
}