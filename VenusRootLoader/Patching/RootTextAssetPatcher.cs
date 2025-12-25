using UnityEngine;

namespace VenusRootLoader.Patching;

internal sealed class RootTextAssetPatcher : IResourcesTypePatcher<TextAsset>
{
    private const string LocalizedPathPrefix = "Data/Dialogues";
    private static readonly char[] LocalisedPathSeparator = ['/'];

    private readonly Dictionary<string, IResourcesTypePatcher<TextAsset>> _textAssetPatchers =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, ILocalizedTextAssetPatcher> _localizedTextAssetPatchersBySubpath =
        new(StringComparer.OrdinalIgnoreCase);

    public RootTextAssetPatcher(ResourcesPatcher resourcesPatcher)
    {
        resourcesPatcher.RegisterResourceTypePatcher(typeof(TextAsset), this);
    }

    internal void RegisterTextAssetPatcher(string path, IResourcesTypePatcher<TextAsset> patcher) =>
        _textAssetPatchers.Add(path, patcher);

    internal void RegisterLocalizedTextAssetPatcher(string subpath, ILocalizedTextAssetPatcher patcher) =>
        _localizedTextAssetPatchersBySubpath.Add(subpath, patcher);

    public override TextAsset PatchResource(string path, TextAsset original)
    {
        if (_textAssetPatchers.TryGetValue(path, out IResourcesTypePatcher<TextAsset> patcher))
            return patcher.PatchResource(path, original);

        if (!path.StartsWith(LocalizedPathPrefix, StringComparison.OrdinalIgnoreCase))
            return original;

        string[] localizedPathParts = path[LocalizedPathPrefix.Length..].Split(LocalisedPathSeparator, 2);
        int languageId = int.Parse(localizedPathParts[0]);
        string subpath = localizedPathParts[1];
        if (_localizedTextAssetPatchersBySubpath.TryGetValue(subpath, out ILocalizedTextAssetPatcher localizedPatcher))
            return localizedPatcher.PatchResource(languageId, subpath, original);

        return original;
    }
}