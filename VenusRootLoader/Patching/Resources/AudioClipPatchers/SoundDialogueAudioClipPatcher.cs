using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.AudioClipPatchers;

/// <summary>
/// An <see cref="IAudioClipPatcher"/> that handles patching dialogue bleeps from the game.
/// </summary>
internal sealed class SoundDialoguesAudioClipPatcher : IAudioClipPatcher
{
    private readonly ILeavesRegistry<DialogueBleepLeaf> _dialogueBleepsRegistry;

    public SoundDialoguesAudioClipPatcher(
        string[] subPaths,
        ILeavesRegistry<DialogueBleepLeaf> dialogueBleepsRegistry)
    {
        SubPaths = subPaths;
        _dialogueBleepsRegistry = dialogueBleepsRegistry;
    }

    public string[] SubPaths { get; }

    public AudioClip PatchAudioClip(string path, AudioClip original)
    {
        if (!char.IsDigit(path[^1]))
            return original;

        int gameId = int.Parse(path.Replace($"{TextAssetPaths.AudioSoundsDialogueDirectory}/Dialogue", string.Empty));
        AudioClip bleepSound = _dialogueBleepsRegistry.LeavesByGameIds[gameId].BleepSound;
        // This is important because the game may use the name to discover what bleep the AudioClip is playing.
        bleepSound.name = $"Dialogue{gameId}";
        return bleepSound;
    }
}