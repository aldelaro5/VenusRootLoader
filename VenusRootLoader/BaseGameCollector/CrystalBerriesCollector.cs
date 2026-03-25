using HarmonyLib;
using Microsoft.Extensions.Logging;
using MonoMod.Cil;
using MonoMod.Utils;
using System.Reflection;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class CrystalBerriesCollector : IBaseGameCollector
{
    private static readonly Dictionary<int, string[]> FortuneTeller0LanguageData = new();

    private readonly ILogger<CrystalBerriesCollector> _logger;
    private readonly ILeavesRegistry<CrystalBerryLeaf> _crystalBerriesRegistry;
    private readonly ILocalizedTextAssetParser<CrystalBerryLeaf> _crystalBerryLanguageDataSerializer;

    public CrystalBerriesCollector(
        ILogger<CrystalBerriesCollector> logger,
        ILocalizedTextAssetParser<CrystalBerryLeaf> crystalBerryLanguageDataSerializer,
        ILeavesRegistry<CrystalBerryLeaf> crystalBerriesRegistry)
    {
        _logger = logger;
        _crystalBerryLanguageDataSerializer = crystalBerryLanguageDataSerializer;
        _crystalBerriesRegistry = crystalBerriesRegistry;

        for (int i = 0; i < RootCollector.LanguageDisplayNames.Length; i++)
        {
            string[] fortuneTeller0Hints = Resources
                .Load<TextAsset>(
                    $"{TextAssetPaths.DataSlashDialogues}{i}/{TextAssetPaths.DataLocalizedCrystalBerryFortuneTellerHintsPathSuffix}")
                .text
                .Trim(StringUtils.NewlineSplitDelimiter)
                .Split(StringUtils.NewlineSplitDelimiter);
            FortuneTeller0LanguageData.Add(i, fortuneTeller0Hints);
        }
    }

    public void CollectBaseGameData(string baseGameId)
    {
        int crystalBerriesAmount = 0;

        MethodInfo setVariableMethod =
            AccessTools.DeclaredMethod(typeof(MainManager), nameof(MainManager.SetVariables))!;
        using DynamicMethodDefinition dmd = new(setVariableMethod);
        ILContext context = new(dmd.Definition);
        ILCursor cursor = new(context);

        cursor
            .GotoNext(i => i.MatchStfld<MainManager>(nameof(MainManager.crystalbflags)))
            .GotoPrev(i => i.MatchLdcI4(out crystalBerriesAmount));

        for (int i = 0; i < crystalBerriesAmount; i++)
        {
            CrystalBerryLeaf crystalBerryLeaf = _crystalBerriesRegistry.RegisterExisting(i, i.ToString(), baseGameId);
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                _crystalBerryLanguageDataSerializer.FromTextAssetSerializedString(
                    TextAssetPaths.DataLocalizedCrystalBerryFortuneTellerHintsPathSuffix,
                    j,
                    FortuneTeller0LanguageData[j][i],
                    crystalBerryLeaf);
            }
        }

        _logger.LogInformation(
            "Collected and registered {CrystalBerriesAmount} base game Crystal Berries",
            crystalBerriesAmount);
    }
}