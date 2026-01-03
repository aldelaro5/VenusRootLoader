using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAsset;

internal interface ITextAssetPatcher
{
    string[] SubPaths { get; }
    UnityEngine.TextAsset PatchResource(string path, UnityEngine.TextAsset original);
}

internal sealed class TextAssetPatcher<T> : ITextAssetPatcher
    where T : ILeaf
{
    private readonly ILeavesRegistry<T> _registry;
    private readonly ILogger<TextAssetPatcher<T>> _logger;
    private readonly ITextAssetSerializable<T> _serializable;

    public TextAssetPatcher(
        string[] subPaths,
        ILogger<TextAssetPatcher<T>> logger,
        ILeavesRegistry<T> registry,
        ITextAssetSerializable<T> serializable)
    {
        SubPaths = subPaths;
        _logger = logger;
        _serializable = serializable;
        _registry = registry;
    }

    public string[] SubPaths { get; }

    public UnityEngine.TextAsset PatchResource(string path, UnityEngine.TextAsset original)
    {
        bool registryHasData = _registry.Leaves.Count > 0;
        if (!registryHasData)
            return original;

        IEnumerable<string> newLines = _registry.Leaves.Values
            .OrderBy(i => i.GameId)
            .Select(customLine => _serializable.GetTextAssetSerializedString(path, customLine));

        string text = string.Join("\n", newLines);
        _logger.LogTrace("Patching {path}:\n{text}", path, text);
        return new UnityEngine.TextAsset(text);
    }
}