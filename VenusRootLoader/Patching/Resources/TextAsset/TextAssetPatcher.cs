using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.VenusInternals;

namespace VenusRootLoader.Patching.Resources.TextAsset;

internal sealed class TextAssetPatcher<T, U> : ResourcesTypePatcher<UnityEngine.TextAsset>
    where T : ILeaf<U>
{
    private readonly ILeavesRegistry<T, U> _registry;
    private readonly ILogger<TextAssetPatcher<T, U>> _logger;
    private readonly ITextAssetSerializable<T, U> _serializable;

    public TextAssetPatcher(
        string path,
        ILogger<TextAssetPatcher<T, U>> logger,
        RootTextAssetPatcher rootTextAssetPatcher,
        ILeavesRegistry<T, U> registry,
        ITextAssetSerializable<T, U> serializable)
    {
        _logger = logger;
        _serializable = serializable;
        _registry = registry;
        rootTextAssetPatcher.RegisterTextAssetPatcher(path, this);
    }

    public override UnityEngine.TextAsset PatchResource(string path, UnityEngine.TextAsset original)
    {
        bool registryHasData = _registry.Items.Count > 0;
        if (!registryHasData)
            return original;

        IEnumerable<string> newLines = _registry.Items.Values
            .OrderBy(i => i.GameId)
            .Select(customLine => _serializable.GetTextAssetSerializedString(customLine));

        string text = string.Join("\n", newLines);
        _logger.LogTrace("Patching {path}:\n{text}", path, text);
        return new UnityEngine.TextAsset(text);
    }
}