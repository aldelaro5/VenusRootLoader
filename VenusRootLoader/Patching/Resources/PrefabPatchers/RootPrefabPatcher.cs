using VenusRootLoader.Utility;
using Object = UnityEngine.Object;

namespace VenusRootLoader.Patching.Resources.PrefabPatchers;

internal sealed class RootPrefabPatcher : IResourcesTypePatcher<Object>
{
    private readonly Dictionary<string, IPrefabPatcher> _textAssetPatchers =
        new(StringComparer.OrdinalIgnoreCase);

    public RootPrefabPatcher(IEnumerable<IPrefabPatcher> textAssetPatchers)
    {
        foreach (IPrefabPatcher textAssetPatcher in textAssetPatchers)
        {
            foreach (string subPath in textAssetPatcher.SubPaths)
                _textAssetPatchers.Add(subPath, textAssetPatcher);
        }
    }

    public Object PatchResource(string path, Object original)
    {
        if (!path.StartsWith(TextAssetPaths.RootPrefabsPathPrefix, StringComparison.OrdinalIgnoreCase))
            return original;

        string prefabSubpath = path[TextAssetPaths.RootPrefabsPathPrefix.Length..];
        if (_textAssetPatchers.TryGetValue(prefabSubpath, out IPrefabPatcher specificPrefabPatcher))
            return specificPrefabPatcher.PatchPrefab(prefabSubpath, original);

        int lastIndexSlash = prefabSubpath.LastIndexOf('/');
        if (lastIndexSlash == -1)
            return original;

        string subpath = prefabSubpath[..lastIndexSlash];
        if (_textAssetPatchers.TryGetValue(subpath, out IPrefabPatcher prefabPatcher))
            return prefabPatcher.PatchPrefab(prefabSubpath, original);

        return original;
    }
}