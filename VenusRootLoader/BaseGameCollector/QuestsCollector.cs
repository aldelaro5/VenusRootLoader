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
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class QuestsCollector : IBaseGameCollector
{
    private static readonly string[] BoardData = RootCollector.ReadTextAssetLines(TextAssetPaths.DataQuestsPath);

    private static readonly string[] ChecksData =
        RootCollector.ReadTextAssetLines(TextAssetPaths.DataQuestsRequirementsPath);

    private readonly Sprite[] _enemyPortraitsSprites = Resources.LoadAll<Sprite>(
        $"{TextAssetPaths.RootSpritesPathPrefix}{TextAssetPaths.SpritesEnemyPortraitsPath}");

    private static readonly Dictionary<int, string[]> QuestsLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(TextAssetPaths.DataLocalizedQuestsPathSuffix);

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

    public void CollectBaseGameData(string baseGameId)
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

        for (int i = 0; i < _questNamedIds.Length; i++)
        {
            QuestLeaf questLeaf = _questsRegistry.RegisterExisting(i, _questNamedIds[i], baseGameId);
            _questTextAssetParser.FromTextAssetSerializedString(TextAssetPaths.DataQuestsPath, BoardData[i], questLeaf);
            _questTextAssetParser.FromTextAssetSerializedString(
                TextAssetPaths.DataQuestsRequirementsPath,
                ChecksData[i],
                questLeaf);
            IEnemyPortraitSprite enemyPortraitSprite = questLeaf;
            questLeaf.CanOnlyBeTakenAtUndergroundBar = bountyQuestsGameIds.Contains(i);
            enemyPortraitSprite.WrappedSprite.Sprite =
                _enemyPortraitsSprites[enemyPortraitSprite.EnemyPortraitsSpriteIndex!.Value];
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                _questLocalizedTextAssetParser.FromTextAssetSerializedString(
                    TextAssetPaths.DataLocalizedQuestsPathSuffix,
                    j,
                    QuestsLanguageData[j][i],
                    questLeaf);
            }
        }

        _logger.LogInformation("Collected and registered {QuestsAmount} base game quests", _questNamedIds.Length);
    }
}