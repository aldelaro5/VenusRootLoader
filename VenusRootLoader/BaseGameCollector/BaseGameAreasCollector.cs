using HarmonyLib;
using Microsoft.Extensions.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using System.Reflection;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class BaseGameAreasCollector : IBaseGameCollector
{
    private static readonly Dictionary<int, string[]> AreaNamesData = new();
    private static readonly Dictionary<int, string[]> AreaDescriptionsData = new();

    private readonly string[] _areasNamedIds = Enum.GetNames(typeof(MainManager.Areas)).ToArray();

    private readonly ILogger<BaseGameAreasCollector> _logger;
    private readonly ILeavesRegistry<AreaLeaf> _areasRegistry;
    private readonly ILocalizedTextAssetParser<AreaLeaf> _areaLocalizedTextAssetParser;

    public BaseGameAreasCollector(
        ILogger<BaseGameAreasCollector> logger,
        ILocalizedTextAssetParser<AreaLeaf> areaLocalizedTextAssetParser,
        ILeavesRegistry<AreaLeaf> areasRegistry)
    {
        _logger = logger;
        _areaLocalizedTextAssetParser = areaLocalizedTextAssetParser;
        _areasRegistry = areasRegistry;

        for (int i = 0; i < RootBaseGameDataCollector.LanguageDisplayNames.Length; i++)
        {
            string[] areaNames = Resources.Load<TextAsset>($"Data/Dialogues{i}/AreaNames").text
                .Trim(Utility.StringUtils.NewlineSplitDelimiter)
                .Split(Utility.StringUtils.NewlineSplitDelimiter, StringSplitOptions.RemoveEmptyEntries);
            AreaNamesData.Add(i, areaNames);
            string[] areaDescriptions = Resources.Load<TextAsset>($"Data/Dialogues{i}/AreaDesc").text
                .Trim(Utility.StringUtils.NewlineSplitDelimiter)
                .Split(Utility.StringUtils.NewlineSplitDelimiter, StringSplitOptions.RemoveEmptyEntries);
            AreaDescriptionsData.Add(i, areaDescriptions);
        }
    }

    public void CollectBaseGameData(string baseGameId)
    {
        int areasAmount = AreaNamesData.Values.First().Length;
        for (int i = 0; i < areasAmount; i++)
        {
            AreaLeaf areaLeaf = _areasRegistry.RegisterExisting(i, _areasNamedIds[i], baseGameId);
            for (int j = 0; j < RootBaseGameDataCollector.LanguageDisplayNames.Length; j++)
            {
                _areaLocalizedTextAssetParser.FromTextAssetSerializedString(
                    "AreaNames",
                    j,
                    AreaNamesData[j][i],
                    areaLeaf);
                _areaLocalizedTextAssetParser.FromTextAssetSerializedString(
                    "AreaDesc",
                    j,
                    AreaDescriptionsData[j][i],
                    areaLeaf);
            }
        }

        MethodInfo setVariableMethod =
            AccessTools.DeclaredMethod(typeof(PauseMenu), nameof(PauseMenu.MapSetup))!;
        using DynamicMethodDefinition dmd = new(setVariableMethod);
        ILContext context = new(dmd.Definition);
        ILCursor cursor = new(context);

        cursor.GotoNext(i => i.Match(OpCodes.Switch));
        Instruction[] switchArmInstructions = (Instruction[])cursor.Instrs[cursor.Index].Operand;

        for (int i = 0; i < switchArmInstructions.Length; i++)
        {
            Instruction switchArmInstruction = switchArmInstructions[i];
            cursor.Goto(switchArmInstruction);
            float x = 0f;
            float z = 0f;
            cursor.GotoNext(inst => inst.MatchLdcR4(out x));
            cursor.GotoNext(inst => inst.MatchLdcR4(out _));
            cursor.GotoNext(inst => inst.MatchLdcR4(out z));

            _areasRegistry.LeavesByGameIds[i].MapPosition = new(-x, -z);
        }

        _logger.LogInformation(
            "Collected and registered {AreasAmount} base game Areas",
            areasAmount);
    }
}