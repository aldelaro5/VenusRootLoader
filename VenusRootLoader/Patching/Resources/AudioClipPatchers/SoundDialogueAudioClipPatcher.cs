using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.AudioClipPatchers;

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
        bleepSound.name = $"Dialogue{gameId}";
        return bleepSound;
    }
}