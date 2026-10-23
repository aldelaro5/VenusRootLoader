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

internal sealed class MedalFortuneTellerHintCollector : IBaseGameCollector
{
    private readonly Dictionary<int, string[]> _medalFortuneTellerHintsLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(ResourcesPaths.DataLocalizedMedalFortuneTellerHintsPathSuffix);

    private readonly ILogger<MedalFortuneTellerHintCollector> _logger;
    private readonly IAssemblyCSharpDataCollector _assemblyCSharpDataCollector;
    private readonly ILocalizedTextAssetParser<MedalFortuneTellerHintLeaf> _localizedTextAssetParser;
    private readonly ILeavesRegistry<MedalFortuneTellerHintLeaf> _medalFortuneTellerHintsRegistry;
    private readonly ILeavesRegistry<FlagLeaf> _flagsRegistry;

    public MedalFortuneTellerHintCollector(
        ILogger<MedalFortuneTellerHintCollector> logger,
        IAssemblyCSharpDataCollector assemblyCSharpDataCollector,
        ILocalizedTextAssetParser<MedalFortuneTellerHintLeaf> localizedTextAssetParser,
        ILeavesRegistry<MedalFortuneTellerHintLeaf> medalFortuneTellerHintsRegistry,
        ILeavesRegistry<FlagLeaf> flagsRegistry)
    {
        _logger = logger;
        _assemblyCSharpDataCollector = assemblyCSharpDataCollector;
        _localizedTextAssetParser = localizedTextAssetParser;
        _medalFortuneTellerHintsRegistry = medalFortuneTellerHintsRegistry;
        _flagsRegistry = flagsRegistry;
    }

    public void CollectBaseGameData()
    {
        int[] flags = CollectMedalObtainedFlagGameIds();

        int medalFortuneTellerHintsAmount = _medalFortuneTellerHintsLanguageData.Values.First().Length;
        for (int i = 0; i < medalFortuneTellerHintsAmount; i++)
        {
            MedalFortuneTellerHintLeaf medalFortuneTellerHintLeaf =
                _medalFortuneTellerHintsRegistry.RegisterExisting(i, i.ToString());
            medalFortuneTellerHintLeaf.MedalObtainedFlag = _flagsRegistry.GetByGameId(flags[i]);
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                _localizedTextAssetParser.FromTextAssetSerializedString(
                    ResourcesPaths.DataLocalizedMedalFortuneTellerHintsPathSuffix,
                    j,
                    _medalFortuneTellerHintsLanguageData[j][i],
                    medalFortuneTellerHintLeaf);
            }
        }

        RootCollector.LogCollectedAmount(_logger, _medalFortuneTellerHintsRegistry, medalFortuneTellerHintsAmount);
    }

    // The flags are hardcoded in an array in event 71 (Fortune Teller event).
    private int[] CollectMedalObtainedFlagGameIds()
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
        cursor.GotoPrev(i => i.MatchLdtoken(out tokenFlags!));

        FieldInfo flagsArrayField = ((FieldReference)tokenFlags).ResolveReflection();
        int[] flags =
            _assemblyCSharpDataCollector.ReadIntArrayFromPrivateImplementationDetailField(flagsArrayField);
        return flags;
    }
}