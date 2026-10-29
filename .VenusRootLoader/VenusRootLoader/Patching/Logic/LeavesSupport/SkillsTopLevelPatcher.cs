using CommunityToolkit.Diagnostics;
using MonoDetour.Cil;
using MonoDetour.DetourTypes;
using MonoMod.Cil;
using System.Collections;
using System.Reflection;
using VenusRootLoader.Api;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.Leaves.RankBonuses;
using VenusRootLoader.Extensions;
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

    private readonly IOrderedLeavesRegistry<SkillLeaf> _skillsRegistry;
    private readonly ILeafList<RankBonusLeaf> _rankBonusesList;

    public SkillsTopLevelPatcher(
        IOrderedLeavesRegistry<SkillLeaf> skillsRegistry,
        ILeafList<RankBonusLeaf> rankBonusesList)
    {
        _instance = this;
        _skillsRegistry = skillsRegistry;
        _rankBonusesList = rankBonusesList;
    }

    public void Patch()
    {
        Md.MainManager.RefreshSkills.ControlFlowPrefix(PatchRefreshSkills);
        Md.BattleControl.DoAction.ILHookMoveNext(PatchPlayerAction);
    }

    private static ReturnFlow PatchRefreshSkills()
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
                if (IsSkillLockedByRankBonus(skillLeaf, trueId))
                    continue;
                if (!(skillLeaf.UsabilityPredicate?.Invoke(context) ?? false))
                    continue;

                MainManager.instance.playerdata[i].skills.Add(skillLeaf.GameId);
            }
        }

        return ReturnFlow.SkipOriginal;
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

    private static bool IsSkillLockedByRankBonus(SkillLeaf skillLeaf, int trueId)
    {
        // In case of ambiguity, we take the one requiring the highest rank.
        RankBonusLeaf? highestRankBonusSkill = _instance._rankBonusesList
            .OfType<SkillLearnRankBonusLeaf>()
            .Where(x => x.Skill == skillLeaf && x.GetSkillLearnerTrueIds().Contains(trueId))
            .OrderByDescending(x => x.RankNeeded)
            .FirstOrDefault();

        return highestRankBonusSkill is not null
               && MainManager.instance.partylevel < highestRankBonusSkill.RankNeeded;
    }

    private static void PatchPlayerAction(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);
        FieldInfo doActionStateMachinePlayerActionContextField = EnumeratorUtils.GetStateMachineFieldFromUniqueType(
            info.Original!.DeclaringType!,
            typeof(BattleControl.DoActionPlayerActionContext));
        FieldInfo thisField = EnumeratorUtils.GetStateMachineThisField(info.Original!.DeclaringType!);

        cursor.GotoNext(x => x.MatchLdstr("Player"));
        cursor.GotoNext(x => x.MatchSwitch(out _));
        cursor.Index++;
        ILLabel switchEndLabel = (ILLabel)cursor.Current.Operand;
        int skillGameIdLocalIndex = 0;
        cursor.GotoPrev(x => x.MatchLdloc(out skillGameIdLocalIndex));
        ILLabel originalActionLabel = cursor.MarkLabel();
        cursor.MoveBeforeLabels();
        cursor.EmitLdloc(skillGameIdLocalIndex);
        cursor.EmitLdcI4(0);
        cursor.EmitBlt(originalActionLabel);

        cursor.EmitYieldReturnCoroutine(
            info.Original,
            PlayerActionHook,
            ilCursor =>
            {
                ilCursor.EmitLdarg0();
                ilCursor.EmitLdfld(thisField);
                ilCursor.EmitLdarg0();
                ilCursor.EmitLdfld(doActionStateMachinePlayerActionContextField);
                ilCursor.EmitLdloc(skillGameIdLocalIndex);
            },
            switchEndLabel);
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