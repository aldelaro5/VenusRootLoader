using Microsoft.Extensions.Logging;
using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAsset;

internal interface IOrderingTextAssetPatcher
{
    string SubPath { get; }
    UnityEngine.TextAsset PatchResource(string path, UnityEngine.TextAsset original);
}

internal sealed class OrderingTextAssetPatcher<T> : IOrderingTextAssetPatcher
    where T : ILeaf
{
    private readonly ILeavesRegistry<T> _registry;
    private readonly ILogger<OrderingTextAssetPatcher<T>> _logger;
    private readonly IOrderingTextAssetParser<T> _parser;

    public OrderingTextAssetPatcher(
        string subPaths,
        ILogger<OrderingTextAssetPatcher<T>> logger,
        ILeavesRegistry<T> registry,
        IOrderingTextAssetParser<T> parser)
    {
        SubPath = subPaths;
        _logger = logger;
        _parser = parser;
        _registry = registry;
    }

    public string SubPath { get; }

    public UnityEngine.TextAsset PatchResource(string path, UnityEngine.TextAsset original)
    {
        bool registryHasData = _registry.Leaves.Count > 0;
        if (!registryHasData)
            return original;

        // Some game data relies on having a trailing LF for the parsing to work correctly
        StringBuilder sb = new(_parser.GetTextAssetString(_registry));
        if (original.text.EndsWith("\n"))
            sb.Append('\n');

        string text = sb.ToString();
        _logger.LogTrace("Patching {path}:\n{text}", path, text);
        return new UnityEngine.TextAsset(text);
    }
}