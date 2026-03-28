using Microsoft.Extensions.Logging;
using System.Text;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers;

/// <summary>
/// A patcher that handles patching global line based <see cref="TextAsset"/> given that the resources path
/// starts with any strings among a list.
/// </summary>
internal interface ITextAssetPatcher
{
    /// <summary>
    /// The list of subpaths that this patcher handles. Any resources path excluding <c>Data/</c> that starts with any
    /// element of this array will be processed by this patcher.
    /// </summary>
    string[] SubPaths { get; }

    /// <summary>
    /// Patches the original <see cref="TextAsset"/> given that the game requested to load it using the resources path
    /// <paramref name="path"/> excluding the <c>Data/</c> prefix.
    /// </summary>
    /// <param name="path">The resources path the game requested to load excluding the <c>Data/</c> prefix.</param>
    /// <param name="original">The original <see cref="TextAsset"/> that would be returned if the patcher wasn't present.</param>
    /// <returns>The patched <see cref="TextAsset"/>.</returns>
    TextAsset PatchTextAsset(string path, TextAsset original);
}

/// <summary>
/// An <see cref="ITextAssetPatcher"/> whose <see cref="TextAsset"/> represents <see cref="Leaf"/> data.
/// It relies on a <see cref="ITextAssetParser{T}"/> to do the actual conversion from <see cref="Leaf"/> to string.
/// </summary>
/// <typeparam name="TLeaf">The <see cref="Leaf"/> type</typeparam>
internal sealed class TextAssetPatcher<TLeaf> : ITextAssetPatcher
    where TLeaf : Leaf
{
    private readonly ILogger<TextAssetPatcher<TLeaf>> _logger;
    private readonly ITextAssetDumper _textAssetDumper;
    private readonly ILeavesRegistry<TLeaf> _registry;
    private readonly ITextAssetParser<TLeaf> _parser;
    private readonly Func<ILeavesRegistry<TLeaf>, IEnumerable<TLeaf>>? _leavesSorter;

    public TextAssetPatcher(
        string[] subPaths,
        ILogger<TextAssetPatcher<TLeaf>> logger,
        ITextAssetDumper textAssetDumper,
        ILeavesRegistry<TLeaf> registry,
        ITextAssetParser<TLeaf> parser,
        Func<ILeavesRegistry<TLeaf>, IEnumerable<TLeaf>>? leavesSorter)
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

        IEnumerable<TLeaf> sortedLeaves = _leavesSorter is null
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