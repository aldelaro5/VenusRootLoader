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
        FieldInfo battleControlFirstStrikeField = typeof(BattleControl).FieldByName(nameof(BattleControl.firststrike))!;
        FieldInfo entityControlOnGroundField = typeof(EntityControl).FieldByName(nameof(EntityControl.onground))!;
        FieldInfo doActionStateMachineEnemyActionContextField = EnumeratorUtils.GetStateMachineFieldFromUniqueType(
            method.DeclaringType!,
            typeof(BattleControl.DoActionEnemyActionContext));
        FieldInfo thisField = EnumeratorUtils.GetStateMachineThisField(method.DeclaringType!);

        matcher.MatchStartForward(CodeMatch.LoadsField(battleControlFirstStrikeField));
        matcher.MatchStartForward(CodeMatch.LoadsField(entityControlOnGroundField));
        matcher.MatchStartForward(Code.Switch);
        matcher.Advance(1);
        Label switchEndLabel = (Label)matcher.Operand;
        matcher.MatchStartBackwards(CodeMatch.IsLdloc());
        int enemyGameIdLocalIndex = matcher.Instruction.LocalIndex();

        matcher.InsertAndAdvance(
            Transpilers.EmitYieldReturnCoroutine(
                method,
                EnemyActionHook,
                matcher,
                [
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldfld, thisField),
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldfld, doActionStateMachineEnemyActionContextField),
                    new(CodeInstruction.LoadLocal(enemyGameIdLocalIndex))
                ],
                switchEndLabel));

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