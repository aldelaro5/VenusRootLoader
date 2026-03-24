using Microsoft.Extensions.Logging;
using System.Text;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers;

internal interface ITextAssetPatcher
{
    string[] SubPaths { get; }
    TextAsset PatchTextAsset(string path, TextAsset original);
}

internal sealed class TextAssetPatcher<T> : ITextAssetPatcher
    where T : Leaf
{
    private readonly ILogger<TextAssetPatcher<T>> _logger;
    private readonly ITextAssetDumper _textAssetDumper;
    private readonly ILeavesRegistry<T> _registry;
    private readonly ITextAssetParser<T> _parser;
    private readonly Func<ILeavesRegistry<T>, IEnumerable<T>>? _leavesSorter;

    public TextAssetPatcher(
        string[] subPaths,
        ILogger<TextAssetPatcher<T>> logger,
        ITextAssetDumper textAssetDumper,
        ILeavesRegistry<T> registry,
        ITextAssetParser<T> parser,
        Func<ILeavesRegistry<T>, IEnumerable<T>>? leavesSorter)
    {
        SubPaths = subPaths;
        _leavesSorter = leavesSorter;
        _logger = logger;
        _parser = parser;
        _textAssetDumper = textAssetDumper;
        _registry = registry;
    }

    public string[] SubPaths { get; }

    public TextAsset PatchTextAsset(string path, TextAsset original)
    {
        bool registryHasData = _registry.LeavesByNamedIds.Count > 0;
        if (!registryHasData)
            return original;

        IEnumerable<T> sortedLeaves = _leavesSorter is null
            ? _registry.LeavesByGameIds.Values.OrderBy(l => l.GameId)
            : _leavesSorter(_registry);
        IEnumerable<string> newLines = sortedLeaves
            .Select(customLine => _parser.GetTextAssetSerializedString(path, customLine));

        // Some game data relies on having a trailing LF for the parsing to work correctly
        StringBuilder sb = new(string.Join("\n", newLines));
        if (original.text.EndsWith("\n"))
            sb.Append('\n');

        string text = sb.ToString();
        if (_logger.IsEnabled(LogLevel.Trace))
            _textAssetDumper.DumpTextAssetContent(path, text);

        return new TextAsset(text);
    }
}