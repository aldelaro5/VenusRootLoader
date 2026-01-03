using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAsset;

internal interface ILocalizedTextAssetPatcher
{
    string[] SubPaths { get; }
    UnityEngine.TextAsset PatchResource(int languageId, string subpath, UnityEngine.TextAsset original);
}

internal sealed class LocalizedTextAssetPatcher<T> : ILocalizedTextAssetPatcher
    where T : ILeaf
{
    private readonly ILeavesRegistry<T> _registry;
    private readonly ILogger<LocalizedTextAssetPatcher<T>> _logger;
    private readonly ILocalizedTextAssetSerializable<T> _serializable;

    public LocalizedTextAssetPatcher(
        string[] subPaths,
        ILogger<LocalizedTextAssetPatcher<T>> logger,
        ILeavesRegistry<T> registry,
        ILocalizedTextAssetSerializable<T> serializable)
    {
        SubPaths = subPaths;
        _registry = registry;
        _logger = logger;
        _serializable = serializable;
    }

    public string[] SubPaths { get; }

    public UnityEngine.TextAsset PatchResource(int languageId, string subpath, UnityEngine.TextAsset original)
    {
        bool registryHasData = _registry.Leaves.Count > 0;

        if (!registryHasData)
            return original;

        IEnumerable<string> newLines = _registry.Leaves.Values
            .OrderBy(i => i.GameId)
            .Select(customLine => _serializable.GetTextAssetSerializedString(subpath, languageId, customLine));

        string text = string.Join("\n", newLines);
        _logger.LogTrace("Patching {path} for language {language}:\n{text}", subpath, languageId, text);
        return new UnityEngine.TextAsset(text);
    }
}