using HarmonyLib;
using Microsoft.Extensions.Logging;
using MonoMod.Cil;
using MonoMod.Utils;
using System.Reflection;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity.AssetLoading;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class QuestsCollector : IBaseGameCollector
{
    private readonly string[] _boardData = RootCollector.ReadTextAssetLines(ResourcesPaths.DataQuestsPath);

    private readonly string[] _checksData =
        RootCollector.ReadTextAssetLines(ResourcesPaths.DataQuestsRequirementsPath);

    private readonly Dictionary<int, string[]> _questsLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(ResourcesPaths.DataLocalizedQuestsPathSuffix);

    private readonly string[] _questNamedIds = Enum.GetNames(typeof(MainManager.BoardQuests)).ToArray();

    private readonly ILogger<QuestsCollector> _logger;
    private readonly ILeavesRegistry<QuestLeaf> _questsRegistry;
    private readonly ITextAssetParser<QuestLeaf> _questTextAssetParser;
    private readonly ILocalizedTextAssetParser<QuestLeaf> _questLocalizedTextAssetParser;

    public QuestsCollector(
        ILogger<QuestsCollector> logger,
        ILeavesRegistry<QuestLeaf> questsRegistry,
        ITextAssetParser<QuestLeaf> questTextAssetParser,
        ILocalizedTextAssetParser<QuestLeaf> questLocalizedTextAssetParser)
    {
        _logger = logger;
        _questsRegistry = questsRegistry;
        _questTextAssetParser = questTextAssetParser;
        _questLocalizedTextAssetParser = questLocalizedTextAssetParser;
    }

    public void CollectBaseGameData()
    {
        List<int> bountyQuestsGameIds = CollectBountyQuestsGameIds();

        for (int i = 0; i < _questNamedIds.Length; i++)
        {
            QuestLeaf questLeaf = _questsRegistry.RegisterExisting(i, _questNamedIds[i]);
            _questTextAssetParser.FromTextAssetSerializedString(
                ResourcesPaths.DataQuestsPath,
                _boardData[i],
                questLeaf);
            _questTextAssetParser.FromTextAssetSerializedString(
                ResourcesPaths.DataQuestsRequirementsPath,
                _checksData[i],
                questLeaf);
            questLeaf.CanOnlyBeTakenAtUndergroundBar = bountyQuestsGameIds.Contains(i);

            IEnemyPortraitSprite enemyPortraitSprite = questLeaf;
            enemyPortraitSprite.PortraitSprite = new AssetLoaderFromResources<Sprite>(
                ResourcesPaths.SpritesItemsEnemyPortraitsResourcesPath,
                enemyPortraitSprite.EnemyPortraitsSpriteIndex!.Value);

            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                _questLocalizedTextAssetParser.FromTextAssetSerializedString(
                    ResourcesPaths.DataLocalizedQuestsPathSuffix,
                    j,
                    _questsLanguageData[j][i],
                    questLeaf);
            }
        }

        RootCollector.LogCollectedAmount(_logger, _questsRegistry, _questNamedIds.Length);
    }

    // Bounty quests have their game ids hardcoded in GetQuestsBoards so we need to collect them from that method.
    private static List<int> CollectBountyQuestsGameIds()
    {
        MethodInfo setVariableMethod =
            AccessTools.DeclaredMethod(typeof(MainManager), nameof(MainManager.GetQuestsBoard))!;
        using DynamicMethodDefinition dmd = new(setVariableMethod);
        using ILContext context = new(dmd.Definition);

        List<int> bountyQuestsGameIds = new();
        context.Invoke(ilc =>
        {
            ILCursor cursor = new(ilc);
            cursor.GotoNext(i => i.MatchBrtrue(out _));
            cursor.GotoNext(i => i.MatchBrtrue(out _));
            cursor.Index++;
            cursor.GotoNext(i => i.MatchLdelemI4());
            cursor.Index++;
            while (cursor.Instrs[cursor.Index].MatchLdcI4(out int questGameId))
            {
                bountyQuestsGameIds.Add(questGameId);
                cursor.Index++;
                cursor.GotoNext(i => i.MatchLdelemI4());
                cursor.Index++;
            }
        });
        return bountyQuestsGameIds;
    }
}