using Microsoft.Extensions.Logging;
using System.Text;
using UnityEngine;
using VenusRootLoader.Patching.TextAssetData;

namespace VenusRootLoader.Patching;

internal sealed class TextAssetPatcher<T> : IResourcesTypePatcher<TextAsset>
    where T : ITextAssetSerializable
{
    private readonly ILogger<TextAssetPatcher<T>> _logger;

    private readonly char[] _textAssetsSplitLineSeparator = ['\n'];

    private Dictionary<int, T> TextAssetsChangedLines { get; } = new();
    private List<T> TextAssetsCustomLines { get; } = new();

    public TextAssetPatcher(
        string path,
        RootTextAssetPatcher rootTextAssetPatcher,
        ILogger<TextAssetPatcher<T>> logger)
    {
        _logger = logger;
        rootTextAssetPatcher.RegisterTextAssetPatcher(path, this);
    }

    internal void AddNewDataToTextAsset(T data)
    {
        TextAssetsCustomLines.Add(data);
    }

    internal void ChangeVanillaDataOfTextAsset(int lineIndex, T data)
    {
        if (TextAssetsChangedLines.ContainsKey(lineIndex))
            return;
        TextAssetsChangedLines[lineIndex] = data;
    }

    public override TextAsset PatchResource(string path, TextAsset original)
    {
        bool changedLinesExists = TextAssetsChangedLines.Count > 0;
        bool customLinesExists = TextAssetsCustomLines.Count > 0;

        if (!changedLinesExists && !customLinesExists)
            return original;

        StringBuilder sb = new();
        if (changedLinesExists)
        {
            string[] lines = original.text.Split(_textAssetsSplitLineSeparator, StringSplitOptions.RemoveEmptyEntries);
            foreach (KeyValuePair<int, T> customLine in TextAssetsChangedLines)
                lines[customLine.Key] = customLine.Value.GetTextAssetSerializedString();
            sb.Append(string.Join("\n", lines));
        }
        else
        {
            sb.Append(original.text.TrimEnd(_textAssetsSplitLineSeparator));
        }

        if (customLinesExists)
        {
            sb.Append('\n');
            sb.Append(string.Join("\n", TextAssetsCustomLines.Select(l => l.GetTextAssetSerializedString())));
        }

        string text = sb.ToString();
        _logger.LogTrace("Patching {path}:\n{text}", path, text);
        return new TextAsset(text);
    }
}