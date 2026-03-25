using HarmonyLib;
using Microsoft.Extensions.Logging;
using Mono.Cecil;
using MonoMod.Cil;
using MonoMod.Utils;
using System.Collections;
using System.Reflection;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class LoreBooksCollector : IBaseGameCollector
{
    private static readonly Dictionary<int, string[]> FortuneTellerHintsLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(TextAssetPaths.DataLocalizedLoreBookFortuneTellerHintsPathSuffix);

    private static readonly Dictionary<int, string[]> LoreTextsLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(TextAssetPaths.DataLocalizedLoreBooksPathSuffix);

    private readonly ILogger<LoreBooksCollector> _logger;
    private readonly IAssemblyCSharpDataCollector _assemblyCSharpDataCollector;
    private readonly ILocalizedTextAssetParser<LoreBookLeaf> _loreBookLocalizedTextAssetParser;
    private readonly ILeavesRegistry<LoreBookLeaf> _loreBooksRegistry;
    private readonly ILeavesRegistry<FlagLeaf> _flagsRegistry;

    public LoreBooksCollector(
        ILogger<LoreBooksCollector> logger,
        IAssemblyCSharpDataCollector assemblyCSharpDataCollector,
        ILocalizedTextAssetParser<LoreBookLeaf> loreBookLocalizedTextAssetParser,
        ILeavesRegistry<LoreBookLeaf> loreBooksRegistry,
        ILeavesRegistry<FlagLeaf> flagsRegistry)
    {
        _logger = logger;
        _assemblyCSharpDataCollector = assemblyCSharpDataCollector;
        _loreBookLocalizedTextAssetParser = loreBookLocalizedTextAssetParser;
        _loreBooksRegistry = loreBooksRegistry;
        _flagsRegistry = flagsRegistry;
    }

    public void CollectBaseGameData(string baseGameId)
    {
        IMetadataTokenProvider tokenFlags = null!;

        Type event71EnumeratorType = typeof(EventControl).InnerTypes().Single(x => x.Name.Contains("<Event71>"));
        MethodInfo event71MoveNextMethod =
            AccessTools.DeclaredMethod(event71EnumeratorType, nameof(IEnumerator.MoveNext))!;
        using DynamicMethodDefinition dmd = new(event71MoveNextMethod);
        ILContext context = new(dmd.Definition);
        ILCursor cursor = new(context);

        MethodInfo resourcesLoadTextAssetMethod = AccessTools.GetDeclaredMethods(typeof(Resources))
            .Single(m => m.Name == nameof(Resources.Load) && m.ContainsGenericParameters)
            .MakeGenericMethod(typeof(TextAsset));
        FieldInfo flagsField = event71EnumeratorType
            .GetRuntimeFields()
            .Single(f => f.Name.Contains("<flags>"));

        cursor.GotoNext(i => i.MatchCall(resourcesLoadTextAssetMethod));
        cursor.GotoNext(i => i.MatchStfld(flagsField));
        cursor.GotoPrev(i => i.MatchLdtoken(out _));
        cursor.Index--;
        cursor.GotoPrev(i => i.MatchLdtoken(out tokenFlags!));

        FieldInfo flagsArrayField = ((FieldReference)tokenFlags).ResolveReflection();
        int[] flags =
            _assemblyCSharpDataCollector.ReadIntArrayFromPrivateImplementationDetailField(flagsArrayField);

        int loreBooksAmount = LoreTextsLanguageData.Values.First().Length;
        for (int i = 0; i < loreBooksAmount; i++)
        {
            LoreBookLeaf loreBookLeaf = _loreBooksRegistry.RegisterExisting(i, i.ToString(), baseGameId);
            loreBookLeaf.LoreBookObtainedFlag = new(_flagsRegistry.LeavesByGameIds[flags[i]]);
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                loreBookLeaf.LocalizedData[j] = new();
                _loreBookLocalizedTextAssetParser.FromTextAssetSerializedString(
                    TextAssetPaths.DataLocalizedLoreBookFortuneTellerHintsPathSuffix,
                    j,
                    FortuneTellerHintsLanguageData[j][i],
                    loreBookLeaf);
                _loreBookLocalizedTextAssetParser.FromTextAssetSerializedString(
                    TextAssetPaths.DataLocalizedLoreBooksPathSuffix,
                    j,
                    LoreTextsLanguageData[j][i],
                    loreBookLeaf);
            }
        }

        _logger.LogInformation(
            "Collected and registered {LoreBooksAmount} base game lore books",
            loreBooksAmount);
    }
}