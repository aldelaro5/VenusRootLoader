using CommunityToolkit.Diagnostics;
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
/// This patcher allows the <see cref="SkillLeaf"/> registry to function.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="MainManager.RefreshSkills"/>: Replaces the entire method to reimplement it such that all skills are
/// added only if the concerned member can use it and that the <see cref="SkillLeaf"/> registry has a predicate indicating
/// its required conditions for usage are met.</item>
/// <item><see cref="BattleControl.DoAction"/>: Changes the player action to reflect the delegates from the
/// <see cref="SkillLeaf"/> registry.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class SkillsTopLevelPatcher : ITopLevelPatcher
{
    private static SkillsTopLevelPatcher _instance = null!;

    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly IOrderedLeavesRegistry<SkillLeaf> _skillsRegistry;

    public SkillsTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        IOrderedLeavesRegistry<SkillLeaf> skillsRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _skillsRegistry = skillsRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(SkillsTopLevelPatcher));

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MainManager), nameof(MainManager.RefreshSkills))]
    private static bool PatchRefreshSkills()
    {
        IReadOnlyCollection<SkillLeaf> skillLeaves = _instance._skillsRegistry.GetOrderedLeaves();
        for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
        {
            MainManager.RefreshSkillsContext context = new() { playerDataIndex = i };
            MainManager.instance.playerdata[i].skills = [];
            int trueId = MainManager.instance.playerdata[i].trueid;

            foreach (SkillLeaf skillLeaf in skillLeaves)
            {
                if (!IsSkillUsableByPlayerPartyMember(skillLeaf, trueId))
                    continue;
                if (!(skillLeaf.UsabilityPredicate?.Invoke(context) ?? false))
                    continue;

                MainManager.instance.playerdata[i].skills.Add(skillLeaf.GameId);
            }
        }

        return false;
    }

    private static bool IsSkillUsableByPlayerPartyMember(SkillLeaf skillLeaf, int trueId)
    {
        return skillLeaf.UsableBy switch
        {
            SkillLeaf.SkillUsability.AnyBug => true,
            SkillLeaf.SkillUsability.AnyBugWithAtLeastOneValidEnemyTarget => true,
            SkillLeaf.SkillUsability.Bee => trueId == 0,
            SkillLeaf.SkillUsability.Beetle => trueId == 1,
            SkillLeaf.SkillUsability.Moth => trueId == 2,
            SkillLeaf.SkillUsability.BeeAndBeetle => trueId is 0 or 1,
            SkillLeaf.SkillUsability.BeeAndMoth => trueId is 0 or 2,
            SkillLeaf.SkillUsability.BeetleAndMoth => trueId is 1 or 2,
            _ => ThrowHelper.ThrowArgumentException<bool>()
        };
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(BattleControl), nameof(BattleControl.DoAction), MethodType.Enumerator)]
    private static IEnumerable<CodeInstruction> PatchPlayerAction(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator,
        MethodBase method)
    {
        CodeMatcher matcher = new(instructions, generator);
        FieldInfo doActionStateMachinePlayerActionContextField = EnumeratorUtils.GetStateMachineFieldFromUniqueType(
            method.DeclaringType!,
            typeof(BattleControl.DoActionPlayerActionContext));
        FieldInfo thisField = EnumeratorUtils.GetStateMachineThisField(method.DeclaringType!);

        matcher.MatchStartForward(CodeMatch.LoadsConstant("Player"));
        matcher.MatchStartForward(Code.Switch);
        matcher.Advance(1);
        Label switchEndLabel = (Label)matcher.Operand;
        matcher.MatchStartBackwards(CodeMatch.IsLdloc());
        int skillGameIdLocalIndex = matcher.Instruction.LocalIndex();
        matcher.CreateLabel(out Label originalActionLabel);
        matcher.InsertAndAdvance(
            new(CodeInstruction.LoadLocal(skillGameIdLocalIndex)),
            Code.Ldc_I4_0,
            new(OpCodes.Blt, originalActionLabel));

        matcher.InsertAndAdvance(
            Transpilers.EmitYieldReturnCoroutine(
                method,
                PlayerActionHook,
                matcher,
                [
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldfld, thisField),
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldfld, doActionStateMachinePlayerActionContextField),
                    new(CodeInstruction.LoadLocal(skillGameIdLocalIndex))
                ],
                switchEndLabel));

        return matcher.Instructions();
    }

    private static IEnumerator PlayerActionHook(
        BattleControl instance,
        BattleControl.DoActionPlayerActionContext context,
        int skillGameId)
    {
        return _instance._skillsRegistry.Registry.GetByGameId(skillGameId).Action?.Invoke(instance, context)
               ?? EnumeratorUtils.EmptyEnumerator;
    }
}