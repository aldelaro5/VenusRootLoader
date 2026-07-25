using CommunityToolkit.Diagnostics;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Mono.Cecil;
using MonoMod.Cil;
using MonoMod.Utils;
using System.Reflection;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class PrizeMedalsCollector : IBaseGameCollector
{
    private readonly ILogger<PrizeMedalsCollector> _logger;
    private readonly ILeavesRegistry<PrizeMedalLeaf> _prizeMedalsRegistry;
    private readonly IAssemblyCSharpDataCollector _assemblyCSharpDataCollector;

    public PrizeMedalsCollector(
        ILogger<PrizeMedalsCollector> logger,
        ILeavesRegistry<PrizeMedalLeaf> prizeMedalsRegistry,
        IAssemblyCSharpDataCollector assemblyCSharpDataCollector)
    {
        _logger = logger;
        _prizeMedalsRegistry = prizeMedalsRegistry;
        _assemblyCSharpDataCollector = assemblyCSharpDataCollector;
    }

    public void CollectBaseGameData(string baseGameId)
    {
        IMetadataTokenProvider tokenPrizeIds = null!;
        IMetadataTokenProvider tokenPrizeFlags = null!;
        IMetadataTokenProvider tokenPrizeEnemyIds = null!;

        MethodInfo setVariableMethod =
            AccessTools.DeclaredMethod(typeof(MainManager), nameof(MainManager.SetVariables))!;
        using DynamicMethodDefinition dmd = new(setVariableMethod);
        ILContext context = new(dmd.Definition);
        ILCursor cursor = new(context);

        cursor
            .GotoNext(i => i.MatchStfld<MainManager>(nameof(MainManager.prizeids)))
            .GotoPrev(i => i.MatchLdtoken(out tokenPrizeIds!))
            .Goto(0)
            .GotoNext(i => i.MatchStfld<MainManager>(nameof(MainManager.prizeflags)))
            .GotoPrev(i => i.MatchLdtoken(out tokenPrizeFlags!))
            .Goto(0)
            .GotoNext(i => i.MatchStfld<MainManager>(nameof(MainManager.prizeenemyids)))
            .GotoPrev(i => i.MatchLdtoken(out tokenPrizeEnemyIds!));

        FieldInfo prizeIdsField = ((FieldReference)tokenPrizeIds).ResolveReflection();
        FieldInfo prizeFlagsField = ((FieldReference)tokenPrizeFlags).ResolveReflection();
        FieldInfo prizeEnemyIdsField = ((FieldReference)tokenPrizeEnemyIds).ResolveReflection();
        // The medal game ids of the prize medals indexed by prize medal game id.
        int[] prizeIds =
            _assemblyCSharpDataCollector.ReadIntArrayFromPrivateImplementationDetailField(prizeIdsField);
        // The bound flag ids of the prize medals indexed by prize medal game id.
        int[] prizeFlags =
            _assemblyCSharpDataCollector.ReadIntArrayFromPrivateImplementationDetailField(prizeFlagsField);
        // The displayed enemy game ids of the prize medals indexed by prize medal game id.
        // NOTE: It's possible this is negative for the special "Explorer Duo" string
        // TODO: Handle custom values like Explorer Duo more gracefully
        int[] prizeEnemyIds =
            _assemblyCSharpDataCollector.ReadIntArrayFromPrivateImplementationDetailField(prizeEnemyIdsField);

        Guard.IsTrue(prizeIds.Length == prizeFlags.Length);
        Guard.IsTrue(prizeFlags.Length == prizeEnemyIds.Length);

        for (int i = 0; i < prizeIds.Length; i++)
        {
            PrizeMedalLeaf prizeMedalLeaf = _prizeMedalsRegistry.RegisterExisting(i, i.ToString(), baseGameId);
            prizeMedalLeaf.MedalGameId = prizeIds[i];
            prizeMedalLeaf.FlagvarGameId = prizeFlags[i];
            prizeMedalLeaf.DisplayedEnemyGameId = prizeEnemyIds[i];
        }

        _logger.LogInformation(
            "Collected and registered {PrizeMedalsAmount} base game prize medals",
            prizeIds.Length);
    }
}