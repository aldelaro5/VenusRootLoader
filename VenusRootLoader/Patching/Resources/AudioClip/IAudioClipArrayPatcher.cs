namespace VenusRootLoader.Patching.Resources.AudioClip;

internal interface IAudioClipArrayPatcher
{
    string[] SubPaths { get; }
    UnityEngine.AudioClip[] PatchAudioClipArray(string path, UnityEngine.AudioClip[] original);
}