using UnityEngine;

namespace VenusRootLoader.Patching.Resources.SpritesPatchers;

internal interface ISpriteArrayPatcher
{
    string[] SubPaths { get; }
    Sprite[] PatchSpriteArray(string path, Sprite[] original);
}