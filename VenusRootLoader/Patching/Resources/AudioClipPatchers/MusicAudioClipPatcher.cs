using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.AudioClipPatchers;

/// <summary>
/// An <see cref="IAudioClipPatcher"/> that handles patching musics from the game.
/// </summary>
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

    public AudioClip PatchAudioClip(string path, AudioClip original)
    {
        string clipName = path
            .Replace(TextAssetPaths.AudioMusicDirectory, string.Empty)
            .Replace("/", string.Empty);
        // If the clip is from the base game, it won't have the creator id part in its name
        AudioClip music = clipName.Contains(Constants.LeafEffectiveIdSeparator)
            ? _musicRegistry.LeavesByEffectiveIds[clipName].Music
            : _musicRegistry.LeavesByEffectiveIds[EffectiveLeafId.CreateBaseGameEffectiveId(clipName)].Music;

        // This is important because the game may use the name to discover what musicc the AudioClip is playing.
        music.name = clipName;
        return music;
    }
}