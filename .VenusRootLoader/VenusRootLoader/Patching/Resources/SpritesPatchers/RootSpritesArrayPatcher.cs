using UnityEngine;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.SpritesPatchers;

/// <summary>
/// An <see cref="IResourcesArrayTypePatcher{TObject}"/> that patches <see cref="Sprite"/> assets arrays.
/// It mainly dispatches the actual patching to any concerned <see cref="ISpriteArrayPatcher"/> depending on the resource path.
/// </summary>
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