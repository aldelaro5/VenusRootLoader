using HarmonyLib;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using VenusRootLoader.Api.Extensions;
using VenusRootLoader.Api.Leaves;
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

    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<ActionCommandLeaf> _actionCommandsRegistry;

    public ActionCommandsTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<ActionCommandLeaf> actionCommandsRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _actionCommandsRegistry = actionCommandsRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(ActionCommandsTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(BattleControl), nameof(BattleControl.DoCommand), MethodType.Enumerator)]
    private static IEnumerable<CodeInstruction> PatchDoItemEffectSwitch(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator,
        MethodBase method)
    {
        CodeMatcher matcher = new(instructions, generator);

        FieldInfo setupContextField = EnumeratorUtils.GetStateMachineFieldFromUniqueType(
            method.DeclaringType!,
            typeof(BattleControl.DoCommandSetupContext));
        FieldInfo executionContextField = EnumeratorUtils.GetStateMachineFieldFromUniqueType(
            method.DeclaringType!,
            typeof(BattleControl.DoCommandExecutionContext));
        FieldInfo thisField = EnumeratorUtils.GetStateMachineThisField(method.DeclaringType!);

        matcher.MatchStartForward(Code.Switch);
        matcher.Advance(1);
        matcher.MatchStartForward(Code.Switch);
        matcher.Advance(1);
        Label setupSwitchEndLabel = (Label)matcher.Operand;
        matcher.MatchStartBackwards(CodeMatch.IsLdloc());
        int actionCommandGameIdLocalIndex = matcher.Instruction.LocalIndex();
        matcher.InsertAndAdvance(
            new(OpCodes.Ldarg_0),
            new(OpCodes.Ldfld, thisField),
            Code.Ldarg_0,
            new(OpCodes.Ldfld, setupContextField),
            new(OpCodes.Ldloc, actionCommandGameIdLocalIndex),
            Transpilers.EmitDelegate(DoCommandSetupHook),
            new CodeInstruction(OpCodes.Br, setupSwitchEndLabel));

        matcher.MatchStartForward(Code.Switch);
        matcher.Advance(1);
        matcher.MatchStartForward(Code.Switch);
        matcher.Advance(1);

        Label executionSwitchEndLabel = (Label)matcher.Operand;
        matcher.MatchStartBackwards(CodeMatch.IsLdloc());
        actionCommandGameIdLocalIndex = matcher.Instruction.LocalIndex();

        matcher.InsertAndAdvance(
            Transpilers.EmitYieldReturnCoroutine(
                method,
                DoCommandExecutionHook,
                matcher,
                [
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldfld, thisField),
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldfld, executionContextField),
                    new(CodeInstruction.LoadLocal(actionCommandGameIdLocalIndex))
                ],
                executionSwitchEndLabel));

        return matcher.Instructions();
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