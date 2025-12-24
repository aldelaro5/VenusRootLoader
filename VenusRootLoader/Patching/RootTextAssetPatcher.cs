using UnityEngine;

namespace VenusRootLoader.Patching;

internal sealed class RootTextAssetPatcher : IResourcesTypePatcher<TextAsset>
{
    private readonly Dictionary<string, IResourcesTypePatcher<TextAsset>> _textAssetPatchers = new();

    public RootTextAssetPatcher(ResourcesPatcher resourcesPatcher)
    {
        resourcesPatcher.RegisterResourceTypePatcher(typeof(TextAsset), this);
    }

    internal void RegisterTextAssetPatcher(string path, IResourcesTypePatcher<TextAsset> patcher) =>
        _textAssetPatchers.Add(path, patcher);

    public override TextAsset PatchResource(string path, TextAsset original)
    {
        if (!_textAssetPatchers.TryGetValue(path, out IResourcesTypePatcher<TextAsset> patcher))
            return original;

        TextAsset newAsset = patcher.PatchResource(path, original);
        return newAsset;
    }
}