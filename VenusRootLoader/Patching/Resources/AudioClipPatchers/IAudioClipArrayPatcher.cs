using UnityEngine;

namespace VenusRootLoader.Patching.Resources.AudioClipPatchers;

internal interface IAudioClipArrayPatcher
{
    string[] SubPaths { get; }
    AudioClip[] PatchAudioClipArray(string path, AudioClip[] original);
}