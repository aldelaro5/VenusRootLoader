namespace VenusRootLoader.Patching.Resources.AudioClip;

internal interface IAudioClipPatcher
{
    string[] SubPaths { get; }
    UnityEngine.AudioClip PatchAudioClip(string path, UnityEngine.AudioClip original);
}