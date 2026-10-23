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

    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<BattleHelperLeaf> _battleHelpersRegistry;

    public BattleHelpersTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<BattleHelperLeaf> battleHelpersRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _battleHelpersRegistry = battleHelpersRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(BattleHelpersTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(BattleControl), nameof(BattleControl.AIAttack), MethodType.Enumerator)]
    private static IEnumerable<CodeInstruction> PatchAiAttack(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator,
        MethodBase method)
    {
        CodeMatcher matcher = new(instructions, generator);
        FieldInfo aiAttackContextField = EnumeratorUtils.GetStateMachineFieldFromUniqueType(
            method.DeclaringType!,
            typeof(BattleControl.AIAttackContext));
        FieldInfo thisField = EnumeratorUtils.GetStateMachineThisField(method.DeclaringType!);

        matcher.MatchStartForward(Code.Switch);
        matcher.Advance(1);
        matcher.MatchStartForward(Code.Switch);
        matcher.Advance(1);
        Label switchEndLabel = (Label)matcher.Operand;
        matcher.MatchStartBackwards(CodeMatch.StoresField(aiAttackContextField));
        matcher.Advance(1);
        int? aiAttackAnimIdGameIdLocalIndex = matcher.Instruction.IsLdloc()
            ? matcher.Instruction.LocalIndex()
            : null;
        FieldInfo? aiAttackAnimIdGameIdField = null;
        if (aiAttackAnimIdGameIdLocalIndex is null)
        {
            matcher.MatchStartForward(Code.Ldfld);
            aiAttackAnimIdGameIdField = (FieldInfo?)matcher.Instruction.operand;
            matcher.MatchStartBackwards(CodeMatch.StoresField(aiAttackContextField));
            matcher.Advance(1);
        }

        List<CodeInstruction> ilLoadAnimIdGameId = aiAttackAnimIdGameIdLocalIndex is not null
            ? [CodeInstruction.LoadLocal(aiAttackAnimIdGameIdLocalIndex.Value)]
            : [Code.Ldarg_0, new CodeInstruction(OpCodes.Ldfld, aiAttackAnimIdGameIdField)];

        matcher.InsertAndAdvance(
            Transpilers.EmitYieldReturnCoroutine(
                method,
                AiAttackHook,
                matcher,
                [
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldfld, thisField),
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldfld, aiAttackContextField),
                    .. ilLoadAnimIdGameId
                ],
                switchEndLabel));

        return matcher.Instructions();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(BattleControl), nameof(BattleControl.StartBattle), MethodType.Enumerator)]
    private static IEnumerable<CodeInstruction> PatchStartBattle(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator,
        MethodBase method)
    {
        CodeMatcher matcher = new(instructions, generator);
        FieldInfo battleControlAiPartyField = typeof(BattleControl).FieldByName(nameof(BattleControl.aiparty))!;
        MethodInfo unityRandomRangeMethod = typeof(UnityEngine.Random).MethodByName(
            nameof(UnityEngine.Random.Range),
            [typeof(int), typeof(int)])!;

        matcher.MatchStartForward(CodeMatch.StoresField(battleControlAiPartyField));
        matcher.Advance(1);

        matcher.DefineLabel(out Label labelAfterHelperJoin);
        matcher.Insert(Transpilers.EmitDelegate(StartBattleHelperJoinHook), new(OpCodes.Br, labelAfterHelperJoin));

        matcher.MatchStartForward(CodeMatch.LoadsField(battleControlAiPartyField));
        matcher.Advance(-1);
        matcher.Labels.Add(labelAfterHelperJoin);

        matcher.MatchStartForward(CodeMatch.LoadsField(battleControlAiPartyField));
        matcher.MatchStartForward(Code.Newobj);
        matcher.Advance(1);
        int? helperPoolListLocalIndex = matcher.Instruction.IsStloc() ? matcher.Instruction.LocalIndex() : null;
        FieldInfo? helperPoolListField = helperPoolListLocalIndex is null
            ? (FieldInfo?)matcher.Instruction.operand
            : null;
        matcher.Advance(1);

        List<CodeInstruction> ilLoadHelperPoolList = helperPoolListLocalIndex is not null
            ? [CodeInstruction.LoadLocal(helperPoolListLocalIndex.Value)]
            : [Code.Ldarg_0, new CodeInstruction(OpCodes.Ldfld, helperPoolListField)];

        matcher.DefineLabel(out Label labelAfterHelperPool);
        matcher.Insert(
        [
            .. ilLoadHelperPoolList,
            Transpilers.EmitDelegate(StartBattleMedalHelperPoolHook),
            new(OpCodes.Br, labelAfterHelperPool)
        ]);

        matcher.MatchStartForward(CodeMatch.Calls(unityRandomRangeMethod));
        matcher.MatchStartBackwards(CodeMatch.Branches());
        matcher.MatchStartBackwards(
            helperPoolListLocalIndex is not null
                ? CodeMatch.LoadsLocal()
                : Code.Ldarg_0);
        matcher.Labels.Add(labelAfterHelperPool);

        return matcher.Instructions();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MapControl), nameof(MapControl.HelperMedalCheck))]
    private static bool PatchHelperMedalCheck()
    {
        if (!MainManager.instance.flags[716] && HelperMedalCheckHook())
        {
            MainManager.instance.badgeshops[0].Add(85);
            MainManager.instance.flags[716] = true;
        }

        return false;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(EventControl), nameof(EventControl.Event222), MethodType.Enumerator)]
    private static IEnumerable<CodeInstruction> PatchLooseEndsEvent(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator,
        MethodBase method)
    {
        CodeMatcher matcher = new(instructions, generator);
        MethodInfo battleControlStartBattleMethod =
            typeof(BattleControl).MethodByName(nameof(BattleControl.StartBattle))!;

        matcher.MatchStartForward(CodeMatch.Calls(battleControlStartBattleMethod));
        matcher.MatchStartForward(Code.Ret);
        matcher.Advance(1);
        Label labelYieldBattle = matcher.Labels[0];
        matcher.MatchStartBackwards(CodeMatch.Calls(battleControlStartBattleMethod));
        matcher.MatchStartForward(CodeMatch.LoadsArgument());
        matcher.Insert(Transpilers.EmitYieldReturnCoroutine(method, LooseEndsEventHook, matcher, [], labelYieldBattle));

        return matcher.Instructions();
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

    private static bool HelperMedalCheckHook() =>
        _instance._battleHelpersRegistry.Any(x => x.PredicateIsInHelperMedalPool());

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