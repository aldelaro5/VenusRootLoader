using UnityEngine;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.SpritesPatchers;

internal sealed class RootSpritesArrayPatcher : IResourcesArrayTypePatcher<Sprite>
{
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
        if (!path.StartsWith(TextAssetPaths.RootSpritesPathPrefix, StringComparison.OrdinalIgnoreCase))
            return original;

        string spritesSubpath = path[TextAssetPaths.RootSpritesPathPrefix.Length..];
        if (_spriteArrayPatchers.TryGetValue(spritesSubpath, out ISpriteArrayPatcher specificSpriteArrayPatcher))
            return specificSpriteArrayPatcher.PatchSpriteArray(spritesSubpath, original);

        int lastIndexSlash = spritesSubpath.LastIndexOf('/');
        if (lastIndexSlash == -1)
            return original;

        string subpath = spritesSubpath[..lastIndexSlash];
        return _spriteArrayPatchers.TryGetValue(subpath, out ISpriteArrayPatcher spriteArrayPatcher)
            ? spriteArrayPatcher.PatchSpriteArray(spritesSubpath, original)
            : original;
    }
}