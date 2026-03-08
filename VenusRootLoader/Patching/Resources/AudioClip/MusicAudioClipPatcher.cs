using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.AudioClip;

internal sealed class MusicAudioClipPatcher : IAudioClipPatcher
{
    private readonly ILeavesRegistry<MusicLeaf> _musicRegistry;

    public MusicAudioClipPatcher(
        string[] subPaths,
        ILeavesRegistry<MusicLeaf> musicRegistry)
    {
        SubPaths = subPaths;
        _musicRegistry = musicRegistry;
    }

    public string[] SubPaths { get; }

    public UnityEngine.AudioClip PatchAudioClip(string path, UnityEngine.AudioClip original)
    {
        string namedId = path.Replace("Music/", string.Empty);
        UnityEngine.AudioClip music = _musicRegistry.LeavesByNamedIds[namedId].Music;
        music.name = namedId;
        return music;
    }
}