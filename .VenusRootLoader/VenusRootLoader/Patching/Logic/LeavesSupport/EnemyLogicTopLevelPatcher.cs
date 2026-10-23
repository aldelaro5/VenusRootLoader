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

    private readonly ILeavesRegistry<EnemyLeaf> _enemiesRegistry;

    public EnemyLogicTopLevelPatcher(ILeavesRegistry<EnemyLeaf> enemiesRegistry)
    {
        _instance = this;
        _enemiesRegistry = enemiesRegistry;
    }

    public void Patch()
    {
        Md.BattleControl.DoAction.ILHookMoveNext(PatchEnemyAction);
    }

    private static void PatchEnemyAction(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);
        FieldInfo doActionStateMachineEnemyActionContextField = EnumeratorUtils.GetStateMachineFieldFromUniqueType(
            info.Original!.DeclaringType!,
            typeof(BattleControl.DoActionEnemyActionContext));
        FieldInfo thisField = EnumeratorUtils.GetStateMachineThisField(info.Original!.DeclaringType!);

        cursor.GotoNext(x => x.MatchLdfld<BattleControl>(nameof(BattleControl.firststrike)));
        cursor.GotoNext(x => x.MatchLdfld<EntityControl>(nameof(EntityControl.onground)));
        cursor.GotoNext(x => x.MatchSwitch(out _));
        cursor.Index++;
        ILLabel switchEndLabel = (ILLabel)cursor.Current.Operand;
        int enemyGameIdLocalIndex = 0;
        cursor.GotoPrev(x => x.MatchLdloc(out enemyGameIdLocalIndex));

        cursor.EmitYieldReturnCoroutine(
            info.Original,
            EnemyActionHook,
            ilCursor =>
            {
                ilCursor.EmitLdarg0();
                ilCursor.EmitLdfld(thisField);
                ilCursor.EmitLdarg0();
                ilCursor.EmitLdfld(doActionStateMachineEnemyActionContextField);
                ilCursor.EmitLdloc(enemyGameIdLocalIndex);
            },
            switchEndLabel);
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