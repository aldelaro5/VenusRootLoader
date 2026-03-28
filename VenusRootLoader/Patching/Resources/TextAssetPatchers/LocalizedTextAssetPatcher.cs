using Microsoft.Extensions.Logging;
using System.Text;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers;

/// <summary>
/// A patcher that handles patching localized line based <see cref="TextAsset"/> given that the resources path after
/// the Dialogues directory starts with any strings among a list.
/// </summary>
internal interface ILocalizedTextAssetPatcher
{
    /// <summary>
    /// The list of subpaths that this patcher handles. Any resources path excluding <c>Data/DialoguesX/</c> that starts with any
    /// element of this array will be processed by this patcher.
    /// </summary>
    string[] SubPaths { get; }

    /// <summary>
    /// Patches the original <see cref="TextAsset"/> given that the game requested to load it using the resources path
    /// <paramref name="subpath"/> excluding the <c>Data/DialoguesX/</c> prefix where X is <paramref name="languageId"/>.
    /// </summary>
    /// <param name="languageId">The language game id the localized <see cref="TextAsset"/> is associated with.</param>
    /// <param name="subpath">The resources path the game requested to load excluding the <c>Data/DialoguesX/</c> prefix.</param>
    /// <param name="original">The original <see cref="TextAsset"/> that would be returned if the patcher wasn't present.</param>
    /// <returns>The patched <see cref="TextAsset"/>.</returns>
    TextAsset PatchLocalisedTextAsset(int languageId, string subpath, TextAsset original);
}

/// <summary>
/// An <see cref="ILocalizedTextAssetPatcher"/> whose <see cref="TextAsset"/> represents <see cref="Leaf"/> data.
/// It relies on a <see cref="ILocalizedTextAssetParser{T}"/> to do the actual conversion from <see cref="Leaf"/> to string.
/// </summary>
/// <typeparam name="TLeaf">The <see cref="Leaf"/> type</typeparam>
internal sealed class LocalizedTextAssetPatcher<TLeaf> : ILocalizedTextAssetPatcher
    where TLeaf : Leaf
{
    private readonly ILogger<LocalizedTextAssetPatcher<TLeaf>> _logger;
    private readonly ITextAssetDumper _textAssetDumper;
    private readonly ILeavesRegistry<TLeaf> _registry;
    private readonly ILocalizedTextAssetParser<TLeaf> _parser;
    private readonly Func<ILeavesRegistry<TLeaf>, IEnumerable<TLeaf>>? _leavesSorter;

    public LocalizedTextAssetPatcher(
        string[] subPaths,
        ILogger<LocalizedTextAssetPatcher<TLeaf>> logger,
        ITextAssetDumper textAssetDumper,
        ILeavesRegistry<TLeaf> registry,
        ILocalizedTextAssetParser<TLeaf> parser,
        Func<ILeavesRegistry<TLeaf>, IEnumerable<TLeaf>>? leavesSorter)
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
        IEnumerable<TLeaf> sortedLeaves = _leavesSorter is null
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