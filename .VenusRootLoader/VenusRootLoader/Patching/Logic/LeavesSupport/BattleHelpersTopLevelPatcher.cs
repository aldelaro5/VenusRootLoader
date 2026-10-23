using Mono.Cecil;
using MonoDetour.Cil;
using MonoDetour.DetourTypes;
using MonoMod.Cil;
using System.Collections;
using System.Reflection;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;
using Random = UnityEngine.Random;

namespace VenusRootLoader.Patching.Logic.LeavesSupport;

/// <summary>
/// This patcher allows the <see cref="BattleHelperLeaf"/> registry to function.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="BattleControl.AIAttack"/>: Changes the attack switch to reflect the delegate from the
/// <see cref="BattleHelperLeaf"/> registry.</item>
/// <item><see cref="BattleControl.StartBattle"/>: Changes helper addition logic to reflect the predicates from the
/// <see cref="BattleHelperLeaf"/> registry.</item>
/// <item><see cref="MapControl.HelperMedalCheck"/>: Completely replace the method such that the conditions required for
/// the <see cref="MainManager.BadgeTypes.Helper"/> medal to appear at Merab's shop reflects the predicates from the
/// <see cref="BattleHelperLeaf"/> registry.</item>
/// <item><see cref="EventControl.Event222"/>: Adds back the logic to apply an infinite attack up and defense up conditions
/// to all enemies after the battle has starter (this logic would have been removed by the <see cref="BattleControl.StartBattle"/>
/// mentioned above).</item>
/// </list>
/// </p>
/// </summary>
internal sealed class BattleHelpersTopLevelPatcher : ITopLevelPatcher
{
    private static BattleHelpersTopLevelPatcher _instance = null!;

    private readonly ILeavesRegistry<BattleHelperLeaf> _battleHelpersRegistry;

    public BattleHelpersTopLevelPatcher(ILeavesRegistry<BattleHelperLeaf> battleHelpersRegistry)
    {
        _instance = this;
        _battleHelpersRegistry = battleHelpersRegistry;
    }

    public void Patch()
    {
        Md.BattleControl.AIAttack.ILHookMoveNext(PatchAiAttack);
        Md.BattleControl.StartBattle.ILHookMoveNext(PatchStartBattle);
        Md.MapControl.HelperMedalCheck.ControlFlowPrefix(PatchHelperMedalCheck);
        Md.EventControl.Event222.ILHookMoveNext(PatchLooseEndsEvent);
    }

    private static void PatchAiAttack(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);
        FieldInfo aiAttackContextField = EnumeratorUtils.GetStateMachineFieldFromUniqueType(
            info.Original!.DeclaringType!,
            typeof(BattleControl.AIAttackContext));
        FieldInfo thisField = EnumeratorUtils.GetStateMachineThisField(info.Original!.DeclaringType!);

        cursor.GotoNext(x => x.MatchSwitch(out _));
        cursor.Index++;
        cursor.GotoNext(x => x.MatchSwitch(out _));
        cursor.Index++;
        ILLabel switchEndLabel = (ILLabel)cursor.Current.Operand;
        cursor.GotoPrev(x => x.MatchStfld(aiAttackContextField));
        cursor.Index++;
        if (!cursor.Current.MatchLdloc(out int aiAttackAnimIdGameIdLocalIndex))
            aiAttackAnimIdGameIdLocalIndex = -1;
        FieldReference? aiAttackAnimIdGameIdField = null;
        if (aiAttackAnimIdGameIdLocalIndex == -1)
        {
            cursor.GotoNext(x => x.MatchLdfld(out aiAttackAnimIdGameIdField));
            cursor.GotoPrev(x => x.MatchStfld(aiAttackContextField));
            cursor.Index++;
        }

        cursor.EmitYieldReturnCoroutine(
            info.Original,
            AiAttackHook,
            ilCursor =>
            {
                ilCursor.EmitLdarg0();
                ilCursor.EmitLdfld(thisField);
                ilCursor.EmitLdarg0();
                ilCursor.EmitLdfld(aiAttackContextField);
                if (aiAttackAnimIdGameIdLocalIndex != -1)
                {
                    ilCursor.EmitLdloc(aiAttackAnimIdGameIdLocalIndex);
                }
                else
                {
                    ilCursor.EmitLdarg0();
                    ilCursor.EmitLdfld(aiAttackAnimIdGameIdField!);
                }
            },
            switchEndLabel);
    }

    private static void PatchStartBattle(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        cursor.GotoNext(x => x.MatchStfld<BattleControl>(nameof(BattleControl.aiparty)));
        cursor.Index++;

        ILLabel labelAfterHelperJoin = cursor.DefineLabel();
        cursor.EmitDelegate(StartBattleHelperJoinHook);
        cursor.EmitBr(labelAfterHelperJoin);

        cursor.GotoNext(x => x.MatchLdfld<BattleControl>(nameof(BattleControl.aiparty)));
        cursor.Index--;
        cursor.MarkLabel(labelAfterHelperJoin);

        cursor.GotoNext(x => x.MatchLdfld<BattleControl>(nameof(BattleControl.aiparty)));
        cursor.GotoNext(x => x.MatchNewobj(out _));
        cursor.Index++;
        if (!cursor.Current.MatchStloc(out int helperPoolListLocalIndex))
            helperPoolListLocalIndex = -1;
        FieldReference? helperPoolListField = helperPoolListLocalIndex == -1
            ? (FieldReference)cursor.Current.Operand
            : null;
        cursor.Index++;

        ILLabel labelAfterHelperPool = cursor.DefineLabel();
        if (helperPoolListLocalIndex != -1)
        {
            cursor.EmitLdloc(helperPoolListLocalIndex);
        }
        else
        {
            cursor.EmitLdarg0();
            cursor.EmitLdfld(helperPoolListField!);
        }

        cursor.EmitDelegate(StartBattleMedalHelperPoolHook);
        cursor.EmitBr(labelAfterHelperPool);

        cursor.GotoNext(x => x.MatchCallOrCallvirt<Random>(nameof(Random.Range)));
        cursor.GotoPrev(x => x.MatchBranch(out _));
        cursor.GotoPrev(x => helperPoolListLocalIndex != -1
            ? x.MatchLdloc(out _)
            : x.MatchLdarg0());
        cursor.MarkLabel(labelAfterHelperPool);
    }

    private static ReturnFlow PatchHelperMedalCheck()
    {
        if (!MainManager.instance.flags[716]
            && _instance._battleHelpersRegistry.Any(x => x.PredicateIsInHelperMedalPool()))
        {
            MainManager.instance.badgeshops[0].Add(85);
            MainManager.instance.flags[716] = true;
        }

        return ReturnFlow.SkipOriginal;
    }

    private static void PatchLooseEndsEvent(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        cursor.GotoNext(x => x.MatchCallOrCallvirt<BattleControl>(nameof(BattleControl.StartBattle)));
        cursor.GotoNext(x => x.MatchRet());
        cursor.Index++;
        ILLabel labelYieldBattle = cursor.IncomingLabels.Single();
        cursor.GotoPrev(x => x.MatchCallOrCallvirt<BattleControl>(nameof(BattleControl.StartBattle)));
        cursor.GotoNext(x => x.MatchLdarg(out _));
        cursor.EmitYieldReturnCoroutine(info.Original!, LooseEndsEventHook, _ => { }, labelYieldBattle);
    }

    private static IEnumerator AiAttackHook(
        BattleControl instance,
        BattleControl.AIAttackContext context,
        int commandGameId)
    {
        return _instance._battleHelpersRegistry.GetByGameId(commandGameId - 1).Attack(instance, context);
    }

    private static void StartBattleHelperJoinHook()
    {
        foreach (BattleHelperLeaf battleHelperLeaf in _instance._battleHelpersRegistry)
        {
            if (!(battleHelperLeaf.PredicateJoinBattle?.Invoke() ?? false))
                continue;

            MainManager.battle.AddAI(battleHelperLeaf.GameId, battleHelperLeaf.StartingAnimstate);
            break;
        }
    }

    private static void StartBattleMedalHelperPoolHook(List<int[]> helperMedalPool)
    {
        foreach (BattleHelperLeaf battleHelperLeaf in _instance._battleHelpersRegistry)
        {
            if (battleHelperLeaf.PredicateIsInHelperMedalPool())
                helperMedalPool.Add([battleHelperLeaf.GameId, battleHelperLeaf.StartingAnimstate]);
        }
    }

    private static IEnumerator LooseEndsEventHook()
    {
        while (!MainManager.battle.halfload)
            yield return null;
        for (int i = 0; i < MainManager.battle.enemydata.Length; i++)
        {
            MainManager.SetCondition(
                MainManager.BattleCondition.AttackUp,
                ref MainManager.battle.enemydata[i],
                999999);
            MainManager.SetCondition(
                MainManager.BattleCondition.DefenseUp,
                ref MainManager.battle.enemydata[i],
                999999);
        }
    }
}