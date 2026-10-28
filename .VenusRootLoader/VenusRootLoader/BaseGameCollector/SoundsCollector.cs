using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity.AssetLoading;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class SoundsCollector : IBaseGameCollector
{
    private const string AudioSoundResourcesPath =
        $"{ResourcesPaths.RootAudioPathPrefix}{ResourcesPaths.AudioSoundsDirectory}";

    // We need to take the shorter clip in case of matching names because there's a clip called BeepLoop that exists twice
    // but its longer version is unused in the game and wasn't supposed to be there as it was replaced in earlier versions.
    private readonly List<string> _soundAudioClipsByName = Resources
        .LoadAll<AudioClip>(AudioSoundResourcesPath)
        .Where(x => !x.name.StartsWith("Dialogue"))
        .GroupBy(x => x.name)
        .Select(x => x.OrderBy(y => y.length).First())
        .Select(x => x.name)
        .ToList();

    private readonly ILogger<SoundsCollector> _logger;
    private readonly ILeavesRegistry<SoundLeaf> _soundRegistry;

    public SoundsCollector(
        ILogger<SoundsCollector> logger,
        ILeavesRegistry<SoundLeaf> soundRegistry)
    {
        _logger = logger;
        _soundRegistry = soundRegistry;
    }

    public void CollectBaseGameData()
    {
        int gameId = 0;
        foreach (string soundName in _soundAudioClipsByName)
        {
            SoundLeaf soundLeaf = _soundRegistry.RegisterExisting(gameId, soundName);
            soundLeaf.Sound = new AssetLoaderFromResources<AudioClip>($"{AudioSoundResourcesPath}/{soundLeaf.NamedId}");
            gameId++;
        }

        RootCollector.LogCollectedAmount(_logger, _soundRegistry, _soundAudioClipsByName.Count);
    }
}