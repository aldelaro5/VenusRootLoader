using HarmonyLib;
using Microsoft.Extensions.Logging;
using MonoMod.Cil;
using MonoMod.Utils;
using System.Reflection;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class BaseGameGlobalFlagsCollector : IBaseGameCollector
{
    private readonly ILogger<BaseGameGlobalFlagsCollector> _logger;
    private readonly ILeavesRegistry<FlagLeaf> _flagsRegistry;
    private readonly ILeavesRegistry<FlagvarLeaf> _flagvarsRegistry;
    private readonly ILeavesRegistry<FlagstringLeaf> _flagstringsRegistry;

    public BaseGameGlobalFlagsCollector(
        ILogger<BaseGameGlobalFlagsCollector> logger,
        ILeavesRegistry<FlagLeaf> flagsRegistry,
        ILeavesRegistry<FlagvarLeaf> flagvarsRegistry,
        ILeavesRegistry<FlagstringLeaf> flagstringsRegistry)
    {
        _logger = logger;
        _flagsRegistry = flagsRegistry;
        _flagvarsRegistry = flagvarsRegistry;
        _flagstringsRegistry = flagstringsRegistry;
    }

    public void CollectBaseGameData(string baseGameId)
    {
        int flagsAmount = 0;
        int flagvarsAmount = 0;
        int flagstringsAmount = 0;

        MethodInfo setVariableMethod =
            AccessTools.DeclaredMethod(typeof(MainManager), nameof(MainManager.SetVariables))!;
        using DynamicMethodDefinition dmd = new(setVariableMethod);
        ILContext context = new(dmd.Definition);
        ILCursor cursor = new(context);

        cursor
            .GotoNext(i => i.MatchStfld<MainManager>(nameof(MainManager.flags)))
            .GotoPrev(i => i.MatchLdcI4(out flagsAmount));
        cursor.Goto(0);
        cursor
            .GotoNext(i => i.MatchStfld<MainManager>(nameof(MainManager.flagvar)))
            .GotoPrev(i => i.MatchLdcI4(out flagvarsAmount));
        cursor.Goto(0);
        cursor
            .GotoNext(i => i.MatchStfld<MainManager>(nameof(MainManager.flagstring)))
            .GotoPrev(i => i.MatchLdcI4(out flagstringsAmount));

        for (int i = 0; i < flagsAmount; i++)
            _flagsRegistry.RegisterExisting(i, i.ToString(), baseGameId);
        _logger.LogInformation(
            "Collected and registered {FlagsAmount} base game flags slots",
            flagsAmount);

        for (int i = 0; i < flagvarsAmount; i++)
            _flagvarsRegistry.RegisterExisting(i, i.ToString(), baseGameId);
        _logger.LogInformation(
            "Collected and registered {FlagvarsAmount} base game flagvars slots",
            flagvarsAmount);

        for (int i = 0; i < flagstringsAmount; i++)
            _flagstringsRegistry.RegisterExisting(i, i.ToString(), baseGameId);
        _logger.LogInformation(
            "Collected and registered {FlagstringAmount} base game flagstrings slots",
            flagstringsAmount);
    }
}