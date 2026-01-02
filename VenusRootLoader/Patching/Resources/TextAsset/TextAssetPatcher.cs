using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAsset;

internal sealed class TextAssetPatcher<T, U> : IResourcesTypePatcher<UnityEngine.TextAsset>
    where T : ILeaf<U>
{
    internal string[] SubPaths { get; }

    private readonly ILeavesRegistry<T, U> _registry;
    private readonly ILogger<TextAssetPatcher<T, U>> _logger;
    private readonly ITextAssetSerializable<T, U> _serializable;

    public TextAssetPatcher(
        string[] subPaths,
        ILogger<TextAssetPatcher<T, U>> logger,
        ILeavesRegistry<T, U> registry,
        ITextAssetSerializable<T, U> serializable)
    {
        SubPaths = subPaths;
        _logger = logger;
        _serializable = serializable;
        _registry = registry;
    }

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