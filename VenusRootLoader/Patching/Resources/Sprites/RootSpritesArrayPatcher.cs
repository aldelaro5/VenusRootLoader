using UnityEngine;

namespace VenusRootLoader.Patching.Resources.Sprites;

internal sealed class RootSpritesArrayPatcher : IResourcesArrayTypePatcher<Sprite>
{
    private const string SpritesPrefix = "Sprites/";

    private readonly Dictionary<string, ISpriteArrayPatcher> _spriteArrayPatchers =
        new(StringComparer.OrdinalIgnoreCase);

    public RootSpritesArrayPatcher(
        IEnumerable<ISpriteArrayPatcher> spriteArrayPatchers)
    {
        foreach (ISpriteArrayPatcher textAssetPatcher in spriteArrayPatchers)
        {
            foreach (string subPath in textAssetPatcher.SubPaths)
                _spriteArrayPatchers.Add(subPath, textAssetPatcher);
        }
    }

    public Sprite[] PatchResources(string path, Sprite[] original)
    {
        if (!path.StartsWith(SpritesPrefix, StringComparison.OrdinalIgnoreCase))
            return original;

        string subpath = path[SpritesPrefix.Length..];
        return _spriteArrayPatchers.TryGetValue(subpath, out ISpriteArrayPatcher textAssetPatcher)
            ? textAssetPatcher.PatchSpriteArray(subpath, original)
            : original;
    }
}