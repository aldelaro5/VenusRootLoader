using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity.AssetLoading;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class DialogueBleepCollector : IBaseGameCollector
{
    private const string AudioSoundsDialogueResourcesPath =
        $"{TextAssetPaths.RootAudioPathPrefix}{TextAssetPaths.AudioSoundsDialogueDirectory}";

    private readonly string[] _dialogueBleepsName = Resources.LoadAll<AudioClip>(AudioSoundsDialogueResourcesPath)
        .Select(a => a.name)
        .ToArray();

    private readonly ILogger<DialogueBleepCollector> _logger;
    private readonly ILeavesRegistry<DialogueBleepLeaf> _dialogueBleepsRegistry;

    public DialogueBleepCollector(
        ILogger<DialogueBleepCollector> logger,
        ILeavesRegistry<DialogueBleepLeaf> dialogueBleepsRegistry)
    {
        _logger = logger;
        _dialogueBleepsRegistry = dialogueBleepsRegistry;
    }

    public void CollectBaseGameData()
    {
        // We need to strip out clips like Dialogue3old which aren't considered bleeps that can be addressed as such.
        // They are effectively unused in base game.
        List<string> dialogueBleeps = _dialogueBleepsName
            .Where(name => char.IsDigit(name[^1]))
            .OrderBy(name => int.Parse(name.Replace("Dialogue", string.Empty)))
            .ToList();
        for (int i = 0; i < dialogueBleeps.Count; i++)
        {
            DialogueBleepLeaf dialogueBleepLeaf = _dialogueBleepsRegistry.RegisterExisting(i, i.ToString());
            dialogueBleepLeaf.BleepSound = new AssetLoaderFromResources<AudioClip>(
                $"{AudioSoundsDialogueResourcesPath}" +
                $"/{dialogueBleeps[i]}");
        }

        RootCollector.LogCollectedAmount(_logger, _dialogueBleepsRegistry, dialogueBleeps.Count);
    }
}