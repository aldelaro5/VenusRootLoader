using Microsoft.Extensions.Logging;
using System.Text;
using UnityEngine;
using VenusRootLoader.Patching.TextAssetData;

namespace VenusRootLoader.Patching;

internal interface ILocalizedTextAssetPatcher
{
    TextAsset PatchResource(int languageId, string subpath, TextAsset original);
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

    public TextAsset PatchResource(int languageId, string subpath, TextAsset original)
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

        StringBuilder sb = new();
        if (changedLinesExists)
        {
            string[] lines = original.text.Split(_textAssetsSplitLineSeparator, StringSplitOptions.RemoveEmptyEntries);
            foreach (KeyValuePair<int, Dictionary<int, T>> customLine in changes)
                lines[customLine.Key] = customLine.Value[languageId].GetTextAssetSerializedString();
            sb.Append(string.Join("\n", lines));
        }
        else
        {
            sb.Append(original.text.TrimEnd(_textAssetsSplitLineSeparator));
        }

        if (subpath.Equals("Items", StringComparison.OrdinalIgnoreCase))
        {
            string[] lines = original.text.Split(_textAssetsSplitLineSeparator, StringSplitOptions.RemoveEmptyEntries);
            // TODO: This is the amount of vanilla items, we need a way to not hardcode this
            if (lines.Length != 187)
            {
                sb.Append('\n');
                sb.Append("RESERVED@Desc@Desc@a");
            }
        }

        if (customLinesExists)
        {
            sb.Append('\n');
            sb.Append(
                string.Join("\n", TextAssetsCustomLines.Select(l => GetLocalizedSerializedString(languageId, l))));
        }

        string text = sb.ToString();
        _logger.LogTrace("Patching {path} for language {language}:\n{text}", subpath, languageId, text);
        return new TextAsset(text);
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