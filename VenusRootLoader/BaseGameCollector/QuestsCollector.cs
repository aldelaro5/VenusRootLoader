using HarmonyLib;
using Microsoft.Extensions.Logging;
using MonoMod.Cil;
using MonoMod.Utils;
using System.Reflection;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Patching.Resources.TextAssetPatchers;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class QuestsCollector : IBaseGameCollector
{
    private static readonly string[] BoardData = Resources.Load<TextAsset>("Data/BoardData").text
        .Trim('\n')
        .Split(['\n'], StringSplitOptions.RemoveEmptyEntries);

    private static readonly string[] ChecksData = Resources.Load<TextAsset>("Data/QuestChecks").text
        .Trim('\n')
        .Split(['\n'], StringSplitOptions.RemoveEmptyEntries);

    private readonly Sprite[] _enemyPortraitsSprites = Resources.LoadAll<Sprite>("Sprites/Items/EnemyPortraits");

    private static readonly Dictionary<int, string[]> QuestsLanguageData = new();

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

        for (int i = 0; i < RootCollector.LanguageDisplayNames.Length; i++)
        {
            string[] questLanguageData = Resources.Load<TextAsset>($"Data/Dialogues{i}/BoardQuests").text
                .Trim('\n')
                .Split(['\n'], StringSplitOptions.RemoveEmptyEntries);
            QuestsLanguageData.Add(i, questLanguageData);
        }
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
            _questTextAssetParser.FromTextAssetSerializedString("BoardData", BoardData[i], questLeaf);
            _questTextAssetParser.FromTextAssetSerializedString("QuestChecks", ChecksData[i], questLeaf);
            IEnemyPortraitSprite enemyPortraitSprite = questLeaf;
            questLeaf.CanOnlyBeTakenAtUndergroundBar = bountyQuestsGameIds.Contains(i);
            enemyPortraitSprite.WrappedSprite.Sprite =
                _enemyPortraitsSprites[enemyPortraitSprite.EnemyPortraitsSpriteIndex!.Value];
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                _questLocalizedTextAssetParser.FromTextAssetSerializedString(
                    "BoardQuests",
                    j,
                    QuestsLanguageData[j][i],
                    questLeaf);
            }
        }

        _logger.LogInformation("Collected and registered {QuestsAmount} base game quests", _questNamedIds.Length);
    }
}