using UnityEngine;

namespace VenusRootLoader.Patching.Resources.AudioClipPatchers;

internal interface IAudioClipPatcher
{
    string[] SubPaths { get; }
    AudioClip PatchAudioClip(string path, AudioClip original);
}