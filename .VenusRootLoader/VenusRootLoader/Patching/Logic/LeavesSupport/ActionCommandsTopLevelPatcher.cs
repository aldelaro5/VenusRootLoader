using MonoDetour.Cil;
using MonoMod.Cil;
using System.Collections;
using System.Reflection;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Logic.LeavesSupport;

/// <summary>
/// This patcher allows the <see cref="ActionCommandLeaf"/> registry to function.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="BattleControl.DoCommand"/>: Changes the setup and execution switch to reflect the delegates from the
/// <see cref="ActionCommandLeaf"/> registry.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class ActionCommandsTopLevelPatcher : ITopLevelPatcher
{
    private static ActionCommandsTopLevelPatcher _instance = null!;

    private readonly ILeavesRegistry<ActionCommandLeaf> _actionCommandsRegistry;

    public ActionCommandsTopLevelPatcher(ILeavesRegistry<ActionCommandLeaf> actionCommandsRegistry)
    {
        _instance = this;
        _actionCommandsRegistry = actionCommandsRegistry;
    }

    public void Patch()
    {
        Md.BattleControl.DoCommand.ILHookMoveNext(PatchDoCommand);
    }

    private static void PatchDoCommand(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);
        FieldInfo setupContextField = EnumeratorUtils.GetStateMachineFieldFromUniqueType(
            info.Original!.DeclaringType!,
            typeof(BattleControl.DoCommandSetupContext));
        FieldInfo executionContextField = EnumeratorUtils.GetStateMachineFieldFromUniqueType(
            info.Original!.DeclaringType!,
            typeof(BattleControl.DoCommandExecutionContext));
        FieldInfo thisField = EnumeratorUtils.GetStateMachineThisField(info.Original!.DeclaringType!);

        cursor.GotoNext(x => x.MatchSwitch(out _));
        cursor.Index++;
        cursor.GotoNext(x => x.MatchSwitch(out _));
        cursor.Index++;
        ILLabel setupSwitchEndLabel = (ILLabel)cursor.Current.Operand;
        int actionCommandGameIdLocalIndex = 0;
        cursor.GotoPrev(x => x.MatchLdloc(out actionCommandGameIdLocalIndex));

        cursor.EmitLdarg0();
        cursor.EmitLdfld(thisField);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(setupContextField);
        cursor.EmitLdloc(actionCommandGameIdLocalIndex);
        cursor.EmitDelegate(DoCommandSetupHook);
        cursor.EmitBr(setupSwitchEndLabel);

        cursor.GotoNext(x => x.MatchSwitch(out _));
        cursor.Index++;
        cursor.GotoNext(x => x.MatchSwitch(out _));
        cursor.Index++;

        ILLabel executionSwitchEndLabel = (ILLabel)cursor.Current.Operand;
        cursor.GotoPrev(x => x.MatchLdloc(out actionCommandGameIdLocalIndex));

        cursor.EmitYieldReturnCoroutine(
            info.Original,
            DoCommandExecutionHook,
            ilCursor =>
            {
                ilCursor.EmitLdarg0();
                ilCursor.EmitLdfld(thisField);
                ilCursor.EmitLdarg0();
                ilCursor.EmitLdfld(executionContextField);
                ilCursor.EmitLdloc(actionCommandGameIdLocalIndex);
            },
            executionSwitchEndLabel);
    }

    private static void DoCommandSetupHook(
        BattleControl instance,
        BattleControl.DoCommandSetupContext context,
        int actionCommandGameId)
    {
        _instance._actionCommandsRegistry.GetByGameId(actionCommandGameId).SetupHandler.Invoke(instance, context);
    }

    private static IEnumerator DoCommandExecutionHook(
        BattleControl instance,
        BattleControl.DoCommandExecutionContext context,
        int actionCommandGameId)
    {
        return _instance._actionCommandsRegistry
            .GetByGameId(actionCommandGameId).ExecutionHandler
            .Invoke(instance, context);
    }
}