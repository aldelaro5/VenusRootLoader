using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.VenusInternals;

namespace VenusRootLoader.Patching.Resources.TextAsset;

internal interface ILocalizedTextAssetPatcher
{
    UnityEngine.TextAsset PatchResource(int languageId, string subpath, UnityEngine.TextAsset original);
}

internal sealed class LocalizedTextAssetPatcher<T, U> : ILocalizedTextAssetPatcher
    where T : ILeaf<U>
{
    private readonly ILeavesRegistry<T, U> _registry;
    private readonly ILogger<LocalizedTextAssetPatcher<T, U>> _logger;
    private readonly ILocalizedTextAssetSerializable<T, U> _serializable;

    public LocalizedTextAssetPatcher(
        string subpath,
        ILogger<LocalizedTextAssetPatcher<T, U>> logger,
        RootTextAssetPatcher rootTextAssetPatcher,
        ILeavesRegistry<T, U> registry,
        ILocalizedTextAssetSerializable<T, U> serializable)
    {
        _registry = registry;
        _logger = logger;
        _serializable = serializable;
        rootTextAssetPatcher.RegisterLocalizedTextAssetPatcher(subpath, this);
    }

    public UnityEngine.TextAsset PatchResource(int languageId, string subpath, UnityEngine.TextAsset original)
    {
        bool registryHasData = _registry.Items.Count > 0;

        if (!registryHasData)
            return original;

        IEnumerable<string> newLines = _registry.Items.Values
            .OrderBy(i => i.GameId)
            .Select(customLine => _serializable.GetTextAssetSerializedString(languageId, customLine));

        string text = string.Join("\n", newLines);
        _logger.LogTrace("Patching {path} for language {language}:\n{text}", subpath, languageId, text);
        return new UnityEngine.TextAsset(text);
    }
}