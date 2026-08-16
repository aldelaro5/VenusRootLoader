using HarmonyLib;
using Microsoft.Extensions.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using System.Collections;
using System.Reflection;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class BattleEventDialoguesCollector : IBaseGameCollector
{
    private readonly ILogger<BattleEventDialogueLeaf> _logger;
    private readonly ILeavesRegistry<BattleEventDialogueLeaf> _battleEventDialoguesRegistry;

    public BattleEventDialoguesCollector(
        ILogger<BattleEventDialogueLeaf> logger,
        ILeavesRegistry<BattleEventDialogueLeaf> battleEventDialoguesRegistry)
    {
        _logger = logger;
        _battleEventDialoguesRegistry = battleEventDialoguesRegistry;
    }

    public void CollectBaseGameData()
    {
        Type eventDialogueEnumeratorType =
            typeof(BattleControl).InnerTypes().Single(x => x.Name.Contains("<EventDialogue>"));
        MethodInfo moveNextMethod =
            AccessTools.DeclaredMethod(eventDialogueEnumeratorType, nameof(IEnumerator.MoveNext))!;

        using DynamicMethodDefinition dmd = new(moveNextMethod);
        ILContext context = new(dmd.Definition);
        ILCursor cursor = new(context);

        cursor.GotoNext(i => i.Match(OpCodes.Switch));
        cursor.Index++;
        cursor.GotoNext(i => i.Match(OpCodes.Switch));
        Instruction[] switchArmInstructions = (Instruction[])cursor.Instrs[cursor.Index].Operand;

        int eventDialoguesAmount = switchArmInstructions.Length;
        for (int i = 0; i < eventDialoguesAmount; i++)
            _battleEventDialoguesRegistry.RegisterExisting(i, i.ToString());

        RootCollector.LogCollectedAmount(_logger, _battleEventDialoguesRegistry, eventDialoguesAmount);
    }
}