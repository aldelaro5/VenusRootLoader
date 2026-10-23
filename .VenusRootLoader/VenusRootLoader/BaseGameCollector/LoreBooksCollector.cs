using Microsoft.Extensions.Logging;
using Mono.Cecil;
using MonoMod.Cil;
using MonoMod.Utils;
using System.Reflection;
using UnityEngine;
using VenusRootLoader.Api.Extensions;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class LoreBooksCollector : IBaseGameCollector
{
    private readonly Dictionary<int, string[]> _fortuneTellerHintsLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(
            ResourcesPaths.DataLocalizedLoreBookFortuneTellerHintsPathSuffix);

    private readonly Dictionary<int, string[]> _loreTextsLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(ResourcesPaths.DataLocalizedLoreBooksPathSuffix);

    private readonly ILogger<LoreBooksCollector> _logger;
    private readonly IAssemblyCSharpDataCollector _assemblyCSharpDataCollector;
    private readonly ILocalizedTextAssetParser<LoreBookLeaf> _loreBookLocalizedTextAssetParser;
    private readonly ILeavesRegistry<LoreBookLeaf> _loreBooksRegistry;
    private readonly ILeavesRegistry<LanguageLeaf> _languageRegistry;
    private readonly ILeavesRegistry<FlagLeaf> _flagsRegistry;

    public LoreBooksCollector(
        ILogger<LoreBooksCollector> logger,
        IAssemblyCSharpDataCollector assemblyCSharpDataCollector,
        ILocalizedTextAssetParser<LoreBookLeaf> loreBookLocalizedTextAssetParser,
        ILeavesRegistry<LoreBookLeaf> loreBooksRegistry,
        ILeavesRegistry<LanguageLeaf> languageRegistry,
        ILeavesRegistry<FlagLeaf> flagsRegistry)
    {
        _logger = logger;
        _assemblyCSharpDataCollector = assemblyCSharpDataCollector;
        _loreBookLocalizedTextAssetParser = loreBookLocalizedTextAssetParser;
        _loreBooksRegistry = loreBooksRegistry;
        _flagsRegistry = flagsRegistry;
        _languageRegistry = languageRegistry;
    }

    public void CollectBaseGameData()
    {
        int[] flags = CollectLoreBooksObtainedFlagGameIds();

        int loreBooksAmount = _loreTextsLanguageData.Values.First().Length;
        for (int i = 0; i < loreBooksAmount; i++)
        {
            LoreBookLeaf loreBookLeaf = _loreBooksRegistry.RegisterExisting(i, i.ToString());
            loreBookLeaf.LoreBookObtainedFlag = new(_flagsRegistry.GetByGameId(flags[i]));
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                loreBookLeaf.LocalizedData[_languageRegistry.GetByGameId(j)] = new();
                _loreBookLocalizedTextAssetParser.FromTextAssetSerializedString(
                    ResourcesPaths.DataLocalizedLoreBookFortuneTellerHintsPathSuffix,
                    j,
                    _fortuneTellerHintsLanguageData[j][i],
                    loreBookLeaf);
                _loreBookLocalizedTextAssetParser.FromTextAssetSerializedString(
                    ResourcesPaths.DataLocalizedLoreBooksPathSuffix,
                    j,
                    _loreTextsLanguageData[j][i],
                    loreBookLeaf);
            }
        }

        RootCollector.LogCollectedAmount(_logger, _loreBooksRegistry, loreBooksAmount);
    }

    // The flags are hardcoded in an array in event 71 (Fortune Teller event).
    private int[] CollectLoreBooksObtainedFlagGameIds()
    {
        IMetadataTokenProvider tokenFlags = null!;

        MethodInfo event71EnumeratorMethod = typeof(EventControl).MethodByName(nameof(EventControl.Event71))!;
        MethodInfo event71MoveNextMethod = event71EnumeratorMethod.GetMoveNextForCoroutineMethod();
        using DynamicMethodDefinition dmd = new(event71MoveNextMethod);
        ILContext context = new(dmd.Definition);
        ILCursor cursor = new(context);

        MethodInfo resourcesLoadTextAssetMethod = typeof(Resources)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Single(m => m.Name == nameof(Resources.Load) && m.ContainsGenericParameters)
            .MakeGenericMethod(typeof(TextAsset));
        FieldInfo flagsField = event71MoveNextMethod.DeclaringType
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
        return flags;
    }
}