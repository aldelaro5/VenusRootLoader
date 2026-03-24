using Microsoft.Extensions.Logging;
using System.Text;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers;

internal interface ILocalizedTextAssetPatcher
{
    string[] SubPaths { get; }
    TextAsset PatchLocalisedTextAsset(int languageId, string subpath, TextAsset original);
}

internal sealed class LocalizedTextAssetPatcher<T> : ILocalizedTextAssetPatcher
    where T : Leaf
{
    private readonly ILogger<LocalizedTextAssetPatcher<T>> _logger;
    private readonly ITextAssetDumper _textAssetDumper;
    private readonly ILeavesRegistry<T> _registry;
    private readonly ILocalizedTextAssetParser<T> _parser;
    private readonly Func<ILeavesRegistry<T>, IEnumerable<T>>? _leavesSorter;

    public LocalizedTextAssetPatcher(
        string[] subPaths,
        ILogger<LocalizedTextAssetPatcher<T>> logger,
        ITextAssetDumper textAssetDumper,
        ILeavesRegistry<T> registry,
        ILocalizedTextAssetParser<T> parser,
        Func<ILeavesRegistry<T>, IEnumerable<T>>? leavesSorter)
    {
        SubPaths = subPaths;
        _leavesSorter = leavesSorter;
        _registry = registry;
        _logger = logger;
        _parser = parser;
        _textAssetDumper = textAssetDumper;
    }

    public string[] SubPaths { get; }

    public TextAsset PatchLocalisedTextAsset(int languageId, string subpath, TextAsset original)
    {
        string assetName = subpath[(subpath.LastIndexOf('/') + 1)..];
        IEnumerable<T> sortedLeaves = _leavesSorter is null
            ? _registry.LeavesByGameIds.Values.OrderBy(l => l.GameId)
            : _leavesSorter(_registry);
        IEnumerable<string> newLines = sortedLeaves
            .Select(customLine => _parser.GetTextAssetSerializedString(assetName, languageId, customLine));

        // Some game data relies on having a trailing LF for the parsing to work correctly
        StringBuilder sb = new(string.Join("\n", newLines));
        if (original.text.EndsWith("\n"))
            sb.Append('\n');

        string text = sb.ToString();
        if (_logger.IsEnabled(LogLevel.Trace))
            _textAssetDumper.DumpTextAssetContent(subpath, text);

        return new TextAsset(text);
    }
}