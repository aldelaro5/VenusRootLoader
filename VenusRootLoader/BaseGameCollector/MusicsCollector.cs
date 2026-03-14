using HarmonyLib;
using Microsoft.Extensions.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using System.Reflection;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class MusicsCollector : IBaseGameCollector
{
    private static readonly string[] LoopPointsData = Resources.Load<TextAsset>("Data/LoopPoints").text
        .Trim('\n')
        .Split(['\n'], StringSplitOptions.RemoveEmptyEntries);

    private static readonly Dictionary<string, AudioClip> MusicAudioClipsByName = Resources
        .LoadAll<AudioClip>("Audio/Music")
        .ToDictionary(a => a.name, a => a);

    private static readonly Dictionary<int, string[]> MusicsLanguageData = new();

    private readonly string[] _musicNamedIds = Enum.GetNames(typeof(MainManager.Musics)).ToArray();

    private readonly ILogger<EnemiesCollector> _logger;
    private readonly ILeavesRegistry<MusicLeaf> _musicRegistry;
    private readonly ITextAssetParser<MusicLeaf> _musicTextAssetParser;
    private readonly ILocalizedTextAssetParser<MusicLeaf> _musicLocalizedTextAssetParser;

    public MusicsCollector(
        ILogger<EnemiesCollector> logger,
        ILeavesRegistry<MusicLeaf> musicRegistry,
        ITextAssetParser<MusicLeaf> musicTextAssetParser,
        ILocalizedTextAssetParser<MusicLeaf> musicLocalizedTextAssetParser)
    {
        _logger = logger;
        _musicRegistry = musicRegistry;
        _musicTextAssetParser = musicTextAssetParser;
        _musicLocalizedTextAssetParser = musicLocalizedTextAssetParser;

        for (int i = 0; i < RootCollector.LanguageDisplayNames.Length; i++)
        {
            string[] musicLanguageData = Resources.Load<TextAsset>($"Data/Dialogues{i}/MusicList").text
                .Trim('\n')
                .Split(['\n'], StringSplitOptions.RemoveEmptyEntries);
            MusicsLanguageData.Add(i, musicLanguageData);
        }
    }

    public void CollectBaseGameData(string baseGameId)
    {
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
            MusicLeaf musicLeaf = _musicRegistry.RegisterExisting(i, _musicNamedIds[i], baseGameId);
            _musicTextAssetParser.FromTextAssetSerializedString("LoopPoints", LoopPointsData[i], musicLeaf);
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                _musicLocalizedTextAssetParser.FromTextAssetSerializedString(
                    "MusicList",
                    j,
                    MusicsLanguageData[j][i],
                    musicLeaf);
            }

            bool hasBackingAudioClip = MusicAudioClipsByName.ContainsKey(musicLeaf.NamedId);
            if (hasBackingAudioClip)
                musicLeaf.Music = MusicAudioClipsByName[musicLeaf.NamedId];
            musicLeaf.CanBePurchasedFromSamira = hasBackingAudioClip && !nonPurchasableMusicGameIds.Contains(i);
        }

        _logger.LogInformation("Collected and registered {MusicsAmount} base game musics", _musicNamedIds.Length);
    }
}