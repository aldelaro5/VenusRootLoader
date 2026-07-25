using UnityEngine;

namespace VenusRootLoader.Patching.Resources.AudioClipPatchers;

/// <summary>
/// A patcher that handles patching specific <see cref="AudioClip"/> arrays given that the resources path
/// starts with any strings among a list.
/// </summary>
internal interface IAudioClipArrayPatcher
{
    /// <summary>
    /// The list of subpaths that this patcher handles. Any resources path excluding <c>Audio/</c> that starts with any
    /// element of this array will be processed by this patcher.
    /// </summary>
    string[] SubPaths { get; }

    /// <summary>
    /// Patches the original <see cref="AudioClip"/> array given that the game requested to load it using the resources path
    /// <paramref name="path"/> excluding the <c>Audio/</c> prefix.
    /// </summary>
    /// <param name="path">The resources path the game requested to load excluding the <c>Audio/</c> prefix.</param>
    /// <param name="original">The original <see cref="AudioClip"/> array that would be returned if the patcher wasn't present.</param>
    /// <returns>The patched <see cref="AudioClip"/> array.</returns>
    AudioClip[] PatchAudioClipArray(string path, AudioClip[] original);
}