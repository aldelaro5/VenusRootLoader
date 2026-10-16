using HarmonyLib;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using VenusRootLoader.Api.Extensions;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic.LeavesSupport;

/// <summary>
/// This patcher allows the <see cref="BattleEventDialogueLeaf"/> registry to function.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="BattleControl.EventDialogue"/>: Changes the main switch to reflect the delegates from the
/// <see cref="BattleEventDialogueLeaf"/> registry.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class EventDialoguesTopLevelPatcher : ITopLevelPatcher
{
    private static EventDialoguesTopLevelPatcher _instance = null!;

    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<BattleEventDialogueLeaf> _battleEventDialoguesRegistry;

    public EventDialoguesTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<BattleEventDialogueLeaf> battleEventDialoguesRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _battleEventDialoguesRegistry = battleEventDialoguesRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(EventDialoguesTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(BattleControl), nameof(BattleControl.EventDialogue), MethodType.Enumerator)]
    private static IEnumerable<CodeInstruction> PatchEventDialogue(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator,
        MethodBase method)
    {
        CodeMatcher matcher = new(instructions, generator);

        matcher.MatchStartForward(Code.Switch);
        matcher.Advance(1);
        matcher.MatchStartForward(Code.Switch);
        matcher.Advance(1);
        Label switchEndLabel = (Label)matcher.Operand;
        matcher.MatchStartBackwards(CodeMatch.IsLdloc());
        int eventDialogueGameIdLocalIndex = matcher.Instruction.LocalIndex();

        matcher.InsertAndAdvance(
            Transpilers.EmitYieldReturnCoroutine(
                method,
                EventDialogueHandlerHook,
                matcher,
                [
                    Code.Ldloc_1,
                    new(CodeInstruction.LoadLocal(eventDialogueGameIdLocalIndex))
                ],
                switchEndLabel));

        return matcher.Instructions();
    }

    private static IEnumerator EventDialogueHandlerHook(BattleControl battle, int eventDialogueGameId)
    {
        return _instance._battleEventDialoguesRegistry.GetByGameId(eventDialogueGameId).Handler(battle);
    }
}