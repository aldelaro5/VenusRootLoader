using HarmonyLib;
using Microsoft.Extensions.Logging;
using MonoMod.Cil;
using MonoMod.Utils;
using System.Reflection;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class BaseGameFlagsCollector : IBaseGameCollector
{
    private readonly ILogger<BaseGameFlagsCollector> _logger;
    private readonly ILeavesRegistry<FlagLeaf> _flagsRegistry;

    public BaseGameFlagsCollector(
        ILeavesRegistry<FlagLeaf> flagsRegistry,
        ILogger<BaseGameFlagsCollector> logger)
    {
        _flagsRegistry = flagsRegistry;
        _logger = logger;
    }

    public void CollectBaseGameData(string baseGameId)
    {
        int flagsAmount = 0;

        MethodInfo setVariableMethod =
            AccessTools.DeclaredMethod(typeof(MainManager), nameof(MainManager.SetVariables))!;
        using DynamicMethodDefinition dmd = new(setVariableMethod);
        ILContext context = new(dmd.Definition);
        ILCursor cursor = new(context);
        cursor
            .GotoNext(i => i.MatchStfld<MainManager>(nameof(MainManager.flags)))
            .GotoPrev(i => i.MatchLdcI4(out flagsAmount));

        for (int i = 0; i < flagsAmount; i++)
            _flagsRegistry.RegisterExisting(i, i.ToString(), baseGameId);

        _logger.LogInformation(
            "Collected and registered {FlagsAmount} base game flags slots",
            flagsAmount);
    }
}