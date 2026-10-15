using HarmonyLib;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Logic.LeavesSupport;

/// <summary>
/// This patcher allows the <see cref="EnemyLeaf"/> registry to have custom logic such as custom enemy actions.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="BattleControl.DoAction"/>: Changes the enemy action to reflect the delegates from the
/// <see cref="EnemyLeaf"/> registry.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class EnemyLogicTopLevelPatcher : ITopLevelPatcher
{
    private static EnemyLogicTopLevelPatcher _instance = null!;

    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<EnemyLeaf> _enemiesRegistry;

    public EnemyLogicTopLevelPatcher(IHarmonyTypePatcher harmonyTypePatcher, ILeavesRegistry<EnemyLeaf> enemiesRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _enemiesRegistry = enemiesRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(EnemyLogicTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(BattleControl), nameof(BattleControl.DoAction), MethodType.Enumerator)]
    private static IEnumerable<CodeInstruction> PatchEnemyAction(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator,
        MethodBase method)
    {
        CodeMatcher matcher = new(instructions, generator);
        FieldInfo battleControlFirstStrikeField = AccessTools.Field(
            typeof(BattleControl),
            nameof(BattleControl.firststrike));
        FieldInfo entityControlOnGroundField = AccessTools.Field(
            typeof(EntityControl),
            nameof(EntityControl.onground));
        FieldInfo doActionStateMachineEnemyActionContextField = AccessTools.Field(
            method.DeclaringType,
            "doActionEnemyActionContext");
        FieldInfo doActionStateMachineCurrentField = AccessTools.Field(
            method.DeclaringType,
            "<>2__current");
        FieldInfo doActionStateMachineStateField = AccessTools.Field(
            method.DeclaringType,
            "<>1__state");
        MethodInfo monoBehaviourStartCoroutineMethod = AccessTools.Method(
            typeof(MonoBehaviour),
            nameof(MonoBehaviour.StartCoroutine),
            [typeof(IEnumerator)]);

        matcher.MatchStartForward(CodeMatch.LoadsField(battleControlFirstStrikeField));
        matcher.MatchStartForward(CodeMatch.LoadsField(entityControlOnGroundField));
        matcher.MatchStartForward(Code.Switch);
        matcher.Advance(1);
        Label switchEndLabel = (Label)matcher.Operand;
        matcher.MatchStartBackwards(CodeMatch.IsLdloc());
        int enemyGameIdLocalIndex = matcher.Instruction.LocalIndex();

        matcher.InsertAndAdvance(
            Code.Ldloc_1,
            Code.Dup,
            Code.Ldarg_0,
            new CodeInstruction(OpCodes.Ldfld, doActionStateMachineEnemyActionContextField));

        // This keeps the enemy game id on the stack.
        matcher.Advance(1);

        // TODO: Simplify this mess.
        matcher.Insert(
            Transpilers.EmitDelegate(EnemyActionHook),
            new CodeInstruction(OpCodes.Call, monoBehaviourStartCoroutineMethod),
            CodeInstruction.StoreLocal(316),
            CodeInstruction.LoadLocal(316),
            new CodeInstruction(OpCodes.Brfalse, switchEndLabel),
            Code.Ldarg_0,
            CodeInstruction.LoadLocal(316),
            new CodeInstruction(OpCodes.Stfld, doActionStateMachineCurrentField),
            Code.Ldarg_0,
            new CodeInstruction(OpCodes.Ldc_I4, 210),
            new CodeInstruction(OpCodes.Stfld, doActionStateMachineStateField),
            Code.Ldc_I4_1,
            Code.Ret,
            // We have to do this to keep the stack balance even if the original switch won't get to execute.
            CodeInstruction.LoadLocal(enemyGameIdLocalIndex)
        );

        return matcher.Instructions();
    }

    private static IEnumerator EnemyActionHook(
        BattleControl instance,
        BattleControl.DoActionEnemyActionContext context,
        int enemyGameId)
    {
        return _instance._enemiesRegistry.GetByGameId(enemyGameId).Action?.Invoke(instance, context)
               ?? EnumeratorUtils.EmptyEnumerator;
    }
}