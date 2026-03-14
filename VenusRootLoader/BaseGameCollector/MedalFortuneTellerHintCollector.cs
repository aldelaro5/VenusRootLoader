using HarmonyLib;
using Microsoft.Extensions.Logging;
using Mono.Cecil;
using MonoMod.Cil;
using MonoMod.Utils;
using System.Collections;
using System.Reflection;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class MedalFortuneTellerHintCollector : IBaseGameCollector
{
    private static readonly Dictionary<int, string[]> MedalFortuneTellerHintsLanguageData = new();

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

        for (int i = 0; i < RootCollector.LanguageDisplayNames.Length; i++)
        {
            string[] medalFortuneTellerHints = Resources.Load<TextAsset>($"Data/Dialogues{i}/FortuneTeller2").text
                .Trim(Utility.StringUtils.NewlineSplitDelimiter)
                .Split(Utility.StringUtils.NewlineSplitDelimiter, StringSplitOptions.RemoveEmptyEntries);
            MedalFortuneTellerHintsLanguageData.Add(i, medalFortuneTellerHints);
        }
    }

    public void CollectBaseGameData(string baseGameId)
    {
        IMetadataTokenProvider tokenFlags = null!;

        Type event71EnumeratorType = typeof(EventControl).InnerTypes().Single(x => x.Name.Contains("<Event71>"));
        MethodInfo setVariableMethod =
            AccessTools.DeclaredMethod(event71EnumeratorType, nameof(IEnumerator.MoveNext))!;
        using DynamicMethodDefinition dmd = new(setVariableMethod);
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
        cursor.GotoPrev(i => i.MatchLdtoken(out tokenFlags!));

        FieldInfo flagsArrayField = ((FieldReference)tokenFlags).ResolveReflection();
        int[] flags =
            _assemblyCSharpDataCollector.ReadIntArrayFromPrivateImplementationDetailField(flagsArrayField);

        int medalFortuneTellerHintsAmount = MedalFortuneTellerHintsLanguageData.Values.First().Length;
        for (int i = 0; i < medalFortuneTellerHintsAmount; i++)
        {
            MedalFortuneTellerHintLeaf medalFortuneTellerHintLeaf =
                _medalFortuneTellerHintsRegistry.RegisterExisting(i, i.ToString(), baseGameId);
            medalFortuneTellerHintLeaf.MedalObtainedFlag = new(_flagsRegistry.LeavesByGameIds[flags[i]]);
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                _localizedTextAssetParser.FromTextAssetSerializedString(
                    "FortuneTeller2",
                    j,
                    MedalFortuneTellerHintsLanguageData[j][i],
                    medalFortuneTellerHintLeaf);
            }
        }

        _logger.LogInformation(
            "Collected and registered {MedalFortuneTellerHintsAmount} base game medal fortune teller hints",
            medalFortuneTellerHintsAmount);
    }
}