using Object = UnityEngine.Object;

namespace VenusRootLoader.Patching.Resources.PrefabPatchers;

internal sealed class RootPrefabPatcher : IResourcesTypePatcher<Object>
{
    private const string PrefabsPrefix = "Prefabs/";
    private static readonly char[] PathSeparator = ['/'];

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
        if (!path.StartsWith(PrefabsPrefix, StringComparison.OrdinalIgnoreCase))
            return original;

        string[] pathParts = path[PrefabsPrefix.Length..].Split(PathSeparator);
        string subpath = pathParts[0];

        return _textAssetPatchers.TryGetValue(subpath, out IPrefabPatcher textAssetPatcher)
            ? textAssetPatcher.PatchPrefab(string.Join("/", pathParts), original)
            : original;
    }
}