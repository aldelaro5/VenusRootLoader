using HarmonyLib;
using Microsoft.Extensions.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using System.Reflection;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.Unity.AssetLoading;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity.AssetLoading;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class MusicsCollector : IBaseGameCollector
{
    private const string AudioMusicResourcesPath =
        $"{TextAssetPaths.RootAudioPathPrefix}{TextAssetPaths.AudioMusicDirectory}";

    private readonly string[] _loopPointsData =
        RootCollector.ReadTextAssetLines(TextAssetPaths.DataMusicLoopPointsPath);

    private readonly HashSet<string> _musicAudioClipsByName = Resources
        .LoadAll<AudioClip>(AudioMusicResourcesPath)
        .Select(a => a.name)
        .ToHashSet();

    private readonly Dictionary<int, string[]> _musicsLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(TextAssetPaths.DataLocalizedMusicNamesPathSuffix);

    private readonly string[] _musicNamedIds = Enum.GetNames(typeof(MainManager.Musics)).ToArray();

    private readonly ILogger<MusicsCollector> _logger;
    private readonly ILeavesRegistry<MusicLeaf> _musicRegistry;
    private readonly ITextAssetParser<MusicLeaf> _musicTextAssetParser;
    private readonly ILocalizedTextAssetParser<MusicLeaf> _musicLocalizedTextAssetParser;

    public MusicsCollector(
        ILogger<MusicsCollector> logger,
        ILeavesRegistry<MusicLeaf> musicRegistry,
        ITextAssetParser<MusicLeaf> musicTextAssetParser,
        ILocalizedTextAssetParser<MusicLeaf> musicLocalizedTextAssetParser)
    {
        _logger = logger;
        _musicRegistry = musicRegistry;
        _musicTextAssetParser = musicTextAssetParser;
        _musicLocalizedTextAssetParser = musicLocalizedTextAssetParser;
    }

    public void CollectBaseGameData()
    {
        // The game contains specific music that technically exists as music, but cannot be purchased from Samira.
        // This is enforced in FixSamira where should any music ends up being unlocked, it will be removed from the list.
        // We can use this to collect those excluded music.
        MethodInfo setVariableMethod =
            AccessTools.DeclaredMethod(typeof(MainManager), nameof(MainManager.FixSamira))!;
        using DynamicMethodDefinition dmd = new(setVariableMethod);
        ILContext context = new(dmd.Definition);
        ILCursor cursor = new(context);

        List<int> nonPurchasableMusicGameIds = new();
        while (cursor.TryGotoNext(i => i.Match(OpCodes.Ldc_I4_S)))
        {
            nonPurchasableMusicGameIds.Add((sbyte)cursor.Instrs[cursor.Index].Operand);
            cursor.Index++;
        }

        for (int i = 0; i < _musicNamedIds.Length; i++)
        {
            MusicLeaf musicLeaf = _musicRegistry.RegisterExisting(i, _musicNamedIds[i]);
            _musicTextAssetParser.FromTextAssetSerializedString(
                TextAssetPaths.DataMusicLoopPointsPath,
                _loopPointsData[i],
                musicLeaf);
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                _musicLocalizedTextAssetParser.FromTextAssetSerializedString(
                    TextAssetPaths.DataLocalizedMusicNamesPathSuffix,
                    j,
                    _musicsLanguageData[j][i],
                    musicLeaf);
            }

            // Some music have an enum value so they technically exist, but they don't have an actual AudioClip to back them.
            // Those are considered unused and should also be excluded from Samira as the game implicitly does it.
            bool hasBackingAudioClip = _musicAudioClipsByName.Contains(musicLeaf.NamedId);
            if (hasBackingAudioClip)
            {
                musicLeaf.Music =
                    new AssetLoaderFromResources<AudioClip>($"{AudioMusicResourcesPath}/{musicLeaf.NamedId}");
            }

            musicLeaf.CanBePurchasedFromSamira = hasBackingAudioClip && !nonPurchasableMusicGameIds.Contains(i);
        }

        RootCollector.LogCollectedAmount(_logger, _musicRegistry, _musicNamedIds.Length);
    }
}