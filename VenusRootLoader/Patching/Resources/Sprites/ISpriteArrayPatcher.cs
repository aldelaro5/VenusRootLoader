using UnityEngine;

namespace VenusRootLoader.Patching.Resources.Sprites;

internal interface ISpriteArrayPatcher
{
    string[] SubPaths { get; }
    Sprite[] PatchSpriteArray(string path, Sprite[] original);
}