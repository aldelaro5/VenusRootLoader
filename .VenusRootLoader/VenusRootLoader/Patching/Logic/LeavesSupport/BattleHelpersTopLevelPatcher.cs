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
/// <item><see cref="MapControl.HelperMedalCheck"/>: Changes the conditions required for the <see cref="MainManager.BadgeTypes.Helper"/>
/// medal to appear at Merab's shop to reflect the predicates from the <see cref="BattleHelperLeaf"/> registry.</item>
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
        FieldInfo aiAttackContextField = AccessTools.Field(
            method.DeclaringType,
            StringUtils.ToCamelCase(nameof(BattleControl.AIAttackContext)));

        matcher.MatchStartForward(Code.Switch);
        matcher.Advance(1);
        matcher.MatchStartForward(Code.Switch);
        matcher.Advance(1);
        Label switchEndLabel = (Label)matcher.Operand;
        matcher.MatchStartBackwards(CodeMatch.IsLdloc());
        int aiAttackAnimIdGameIdLocalIndex = matcher.Instruction.LocalIndex();

        matcher.InsertAndAdvance(
            Transpilers.EmitYieldReturnCoroutine(
                method,
                AiAttackHook,
                matcher,
                [
                    new(OpCodes.Ldloc_1),
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldfld, aiAttackContextField),
                    new(CodeInstruction.LoadLocal(aiAttackAnimIdGameIdLocalIndex))
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
        FieldInfo battleControlAiPartyField = AccessTools.Field(
            typeof(BattleControl),
            nameof(BattleControl.aiparty));
        MethodInfo unityRandomRangeMethod = AccessTools.Method(
            typeof(UnityEngine.Random),
            nameof(UnityEngine.Random.Range),
            [typeof(int), typeof(int)]);

        matcher.MatchStartForward(CodeMatch.StoresField(battleControlAiPartyField));
        matcher.Advance(1);

        matcher.DefineLabel(out Label labelAfterHelperJoin);
        matcher.Insert(Transpilers.EmitDelegate(StartBattleHelperJoinHook), new(OpCodes.Br, labelAfterHelperJoin));

        matcher.MatchStartForward(CodeMatch.LoadsField(battleControlAiPartyField));
        matcher.Advance(-1);
        matcher.Labels.Add(labelAfterHelperJoin);

        matcher.MatchStartForward(CodeMatch.LoadsField(battleControlAiPartyField));
        matcher.MatchStartForward(CodeMatch.StoresLocal());
        int helperPoolListLocalIndex = matcher.Instruction.LocalIndex();
        matcher.Advance(1);

        matcher.DefineLabel(out Label labelAfterHelperPool);
        matcher.Insert(
            CodeInstruction.LoadLocal(helperPoolListLocalIndex),
            Transpilers.EmitDelegate(StartBattleMedalHelperPoolHook),
            new(OpCodes.Br, labelAfterHelperPool));

        matcher.MatchStartForward(CodeMatch.Calls(unityRandomRangeMethod));
        matcher.MatchStartBackwards(CodeMatch.Branches());
        matcher.MatchStartBackwards(CodeMatch.LoadsLocal());
        matcher.Labels.Add(labelAfterHelperPool);

        return matcher.Instructions();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(MapControl), nameof(MapControl.HelperMedalCheck))]
    private static IEnumerable<CodeInstruction> PatchHelperMedalCheck(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator,
        MethodBase method)
    {
        CodeMatcher matcher = new(instructions, generator);
        FieldInfo mainManagerBadgeShopsField = AccessTools.Field(typeof(MainManager), nameof(MainManager.badgeshops));

        matcher.MatchStartForward(CodeMatch.LoadsField(mainManagerBadgeShopsField));
        matcher.MatchStartBackwards(CodeMatch.Branches());
        int indexStop = matcher.Pos;
        matcher.Start();
        matcher.MatchStartForward(CodeMatch.Branches());
        matcher.Advance(1);
        matcher.RemoveInstructions(indexStop - matcher.Pos);
        matcher.Insert(Transpilers.EmitDelegate(HelperMedalCheckHook));

        return matcher.Instructions();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(EventControl), nameof(EventControl.Event222), MethodType.Enumerator)]
    private static IEnumerable<CodeInstruction> PatchLooseEndsEvent(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator,
        MethodBase method)
    {
        CodeMatcher matcher = new(instructions, generator);
        MethodInfo battleControlStartBattleMethod = AccessTools.Method(
            typeof(BattleControl),
            nameof(BattleControl.StartBattle));

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
            if (battleHelperLeaf.PredicateJoinBattle?.Invoke() ?? false)
                MainManager.battle.AddAI(battleHelperLeaf.GameId, battleHelperLeaf.StartingAnimstate);
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