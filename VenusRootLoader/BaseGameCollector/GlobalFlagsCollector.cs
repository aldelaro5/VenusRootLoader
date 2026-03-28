using HarmonyLib;
using Microsoft.Extensions.Logging;
using MonoMod.Cil;
using MonoMod.Utils;
using System.Reflection;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class GlobalFlagsCollector : IBaseGameCollector
{
    private readonly ILogger<GlobalFlagsCollector> _logger;
    private readonly ILeavesRegistry<FlagLeaf> _flagsRegistry;
    private readonly ILeavesRegistry<FlagvarLeaf> _flagvarsRegistry;
    private readonly ILeavesRegistry<FlagstringLeaf> _flagstringsRegistry;

    public GlobalFlagsCollector(
        ILogger<GlobalFlagsCollector> logger,
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

        // The amount of flags, flagvars and flagstrings aren't straight forward to figure out because it's not declared
        // in a dedicated way. The best heuristic is to find the length that these arrays gets initalized at in SetVariables.
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