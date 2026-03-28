using Microsoft.Extensions.Logging;
using System.Text;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers;

internal interface IOrderingTextAssetPatcher
{
    string SubPath { get; }
    TextAsset PatchTextAsset(string path, TextAsset original);
}

/// <summary>
/// An <see cref="IOrderingTextAssetPatcher"/> whose <see cref="TextAsset"/> represents
/// <see cref="IOrderedLeavesRegistry{TLeaf}"/> ordering data. It relies on a <see cref="IOrderingTextAssetParser{T}"/>
/// to do the actual conversion from all the <see cref="Leaf"/> to string.
/// </summary>
/// <typeparam name="TLeaf">The <see cref="Leaf"/> type</typeparam>
internal sealed class OrderingTextAssetPatcher<TLeaf> : IOrderingTextAssetPatcher
    where TLeaf : Leaf
{
    private readonly IOrderedLeavesRegistry<TLeaf> _orderedLeaves;
    private readonly ILogger<OrderingTextAssetPatcher<TLeaf>> _logger;
    private readonly ITextAssetDumper _textAssetDumper;
    private readonly IOrderingTextAssetParser<TLeaf> _parser;

    public OrderingTextAssetPatcher(
        string subPaths,
        ILogger<OrderingTextAssetPatcher<TLeaf>> logger,
        ITextAssetDumper textAssetDumper,
        IOrderedLeavesRegistry<TLeaf> orderedLeaves,
        IOrderingTextAssetParser<TLeaf> parser)
    {
        SubPath = subPaths;
        _logger = logger;
        _parser = parser;
        _textAssetDumper = textAssetDumper;
        _orderedLeaves = orderedLeaves;
    }

    public string SubPath { get; }

    public TextAsset PatchTextAsset(string path, TextAsset original)
    {
        bool registryHasData = _orderedLeaves.Registry.LeavesByNamedIds.Count > 0;
        if (!registryHasData)
            return original;

        // Some game data relies on having a trailing LF for the parsing to work correctly
        StringBuilder sb = new(_parser.GetTextAssetString(_orderedLeaves));
        if (original.text.EndsWith("\n"))
            sb.Append('\n');

        string text = sb.ToString();
        if (_logger.IsEnabled(LogLevel.Trace))
            _textAssetDumper.DumpTextAssetContent(path, text);

        return new TextAsset(text);
    }
}