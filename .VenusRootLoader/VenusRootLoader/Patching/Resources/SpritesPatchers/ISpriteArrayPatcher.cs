using UnityEngine;

namespace VenusRootLoader.Patching.Resources.SpritesPatchers;

/// <summary>
/// A patcher that handles patching specific <see cref="Sprite"/> arrays given that the resources path
/// starts with any strings among a list.
/// </summary>
internal interface ISpriteArrayPatcher
{
    /// <summary>
    /// The list of subpaths that this patcher handles. Any resources path excluding <c>Sprites/</c> that starts with any
    /// element of this array will be processed by this patcher.
    /// </summary>
    string[] SubPaths { get; }

    /// <summary>
    /// Patches the original <see cref="Sprite"/> array given that the game requested to load it using the resources path
    /// <paramref name="path"/> excluding the <c>Sprites/</c> prefix.
    /// </summary>
    /// <param name="path">The resources path the game requested to load excluding the <c>Sprites/</c> prefix.</param>
    /// <param name="original">The original <see cref="Sprite"/> array that would be returned if the patcher wasn't present.</param>
    /// <returns>The patched <see cref="Sprite"/> array.</returns>
    Sprite[] PatchSpriteArray(string path, Sprite[] original);
}