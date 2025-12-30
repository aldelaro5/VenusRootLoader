using Microsoft.Extensions.Logging;
using System.Text;
using VenusRootLoader.BaseGameCollector;

namespace VenusRootLoader.Patching.Resources.TextAsset;

internal sealed class TextAssetPatcher<T> : ResourcesTypePatcher<UnityEngine.TextAsset>
    where T : ITextAssetSerializable
{
    private readonly ILogger<TextAssetPatcher<T>> _logger;

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

    public override UnityEngine.TextAsset PatchResource(string path, UnityEngine.TextAsset original)
    {
        bool changedLinesExists = TextAssetsChangedLines.Count > 0;
        bool customLinesExists = TextAssetsCustomLines.Count > 0;

        if (!changedLinesExists && !customLinesExists)
            return original;

        string[] lines = [];
        if (path.Equals("Data/ItemData", StringComparison.OrdinalIgnoreCase))
            lines = BaseGameItemsCollector.ItemsData;

        StringBuilder sb = new();
        if (changedLinesExists)
        {
            foreach (KeyValuePair<int, T> customLine in TextAssetsChangedLines)
                lines[customLine.Key] = customLine.Value.GetTextAssetSerializedString();
        }

        sb.Append(string.Join("\n", lines));

        if (customLinesExists)
        {
            sb.Append('\n');
            sb.Append(string.Join("\n", TextAssetsCustomLines.Select(l => l.GetTextAssetSerializedString())));
        }

        string text = sb.ToString();
        _logger.LogTrace("Patching {path}:\n{text}", path, text);
        return new UnityEngine.TextAsset(text);
    }
}