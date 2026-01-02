using Microsoft.Extensions.Logging;
using System.Text;
using VenusRootLoader.BaseGameCollector;

namespace VenusRootLoader.Patching.Resources.TextAsset;

internal interface ILocalizedTextAssetPatcher
{
    UnityEngine.TextAsset PatchResource(int languageId, string subpath, UnityEngine.TextAsset original);
}

internal sealed class LocalizedTextAssetPatcher<T> : ILocalizedTextAssetPatcher
{
    private readonly ILogger<LocalizedTextAssetPatcher<T>> _logger;
    private readonly ITextAssetSerializable<T> _serializable;

    private Dictionary<int, Dictionary<int, T>> TextAssetsChangedLines { get; } = new();
    private List<Dictionary<int, T>> TextAssetsCustomLines { get; } = new();

    public LocalizedTextAssetPatcher(
        string subpath,
        RootTextAssetPatcher rootTextAssetPatcher,
        ILogger<LocalizedTextAssetPatcher<T>> logger,
        ITextAssetSerializable<T> serializable)
    {
        _logger = logger;
        _serializable = serializable;
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

        string[] lines = [];
        if (subpath.Equals("Items", StringComparison.OrdinalIgnoreCase))
            lines = BaseGameItemsCollector.ItemsLanguageData[languageId];

        StringBuilder sb = new();
        if (changedLinesExists)
        {
            foreach (KeyValuePair<int, Dictionary<int, T>> customLine in changes)
                lines[customLine.Key] = _serializable.GetTextAssetSerializedString(customLine.Value[languageId]);
        }

        sb.Append(string.Join("\n", lines));

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

    private string GetLocalizedSerializedString(int languageId, Dictionary<int, T> customLineByLanguage)
    {
        if (customLineByLanguage.Count == 0)
            return _serializable.GetTextAssetSerializedString(Activator.CreateInstance<T>());

        if (customLineByLanguage.TryGetValue(languageId, out T value))
            return _serializable.GetTextAssetSerializedString(value);

        int firstLanguage = customLineByLanguage.Keys.Min();
        return _serializable.GetTextAssetSerializedString(customLineByLanguage[firstLanguage]);
    }
}