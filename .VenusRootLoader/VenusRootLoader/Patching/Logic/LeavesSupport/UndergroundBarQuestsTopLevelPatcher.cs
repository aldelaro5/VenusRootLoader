using Mono.Cecil.Cil;
using MonoDetour.Cil;
using MonoMod.Cil;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic.LeavesSupport;

/// <summary>
/// This patcher adds support for allowing <see cref="QuestLeaf"/> to decide if they should only be visible on the board
/// at the UndergroundBar.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="MainManager.GetQuestsBoard"/>: Changes the list of quests game ids that are only visible at the
/// UndergroundBar's baord.</item>
/// <item><see cref="MainManager.ChangeBoardQuest(int, int)"/>: Prevents UndergroundBar exclusive quests from setting
/// the all quests checked flag to false (flag 2).</item>
/// </list>
/// </p>
/// </summary>
internal sealed class UndergroundBarQuestsTopLevelPatcher : ITopLevelPatcher
{
    private readonly ILeavesRegistry<QuestLeaf> _questsRegistry;

    private static UndergroundBarQuestsTopLevelPatcher _instance = null!;

    public UndergroundBarQuestsTopLevelPatcher(ILeavesRegistry<QuestLeaf> questsRegistry)
    {
        _instance = this;
        _questsRegistry = questsRegistry;
    }

    public void Patch()
    {
        Md.MainManager.GetQuestsBoard.ILHook(PatchBountyQuests);
        Md.MainManager.ChangeBoardQuest_System_Int32_System_Int32.ILHook(FixUtterNewQuestsFlag);
    }

    private static void PatchBountyQuests(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        // We are getting into the parts of the long if block that handles bounty quests inside the for loop on each quest game id.
        cursor.GotoNext(x => x.MatchBrtrue(out _));
        cursor.Index++;
        ILLabel questAllowedLabel = null!;
        cursor.GotoNext(x => x.MatchBrtrue(out questAllowedLabel!));
        cursor.Index++;

        // This will completely override the bounty quests checks to check the QuestLeaf's field we have from the registry.
        cursor.GotoNext(x => x.MatchLdelemI4());
        cursor.Index++;
        cursor.SetToStaticDelegateCallAndAdvance(IsQuestUndergroundBarExclusive);
        cursor.EmitBrfalse(questAllowedLabel);
        cursor.Current.OpCode = OpCodes.Br;
    }

    // In the base game, there is a silent game bug where Utter will have a ! emoticon if an UndergroundBar exclusive quest is
    // added to the open board. This normally doesn't affect the base game because those are always added alongside other
    // quests from the start of the game so they don't actually have a meaningful impact. The issue is we have to patch
    // a fix for this because buds might want to unlock their UndergroundBar exclusive quests after the fact.
    // In such a case, these buds would run into this issue which would be visually illogical because the quest can't be
    // taken at Utter's board.
    private static void FixUtterNewQuestsFlag(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        ILLabel returnLabel = null!;
        cursor.GotoNext(x => x.MatchBranch(out returnLabel!));

        // We want to get into the inner if, but because the method is so short, it's more efficient to find it from the end.
        cursor.Goto(cursor.Instrs.Count - 1);
        cursor.GotoPrev(x => x.MatchStelemI1());
        cursor.Index--;
        cursor.GotoPrev(x => x.MatchBranch(out _));
        cursor.Index++;
        // This simply adds an "and" condition to the if that guards setting the flag.
        cursor.EmitLdarg0();
        cursor.EmitDelegate(IsQuestUndergroundBarExclusive);
        cursor.EmitBrtrue(returnLabel);
    }

    private static bool IsQuestUndergroundBarExclusive(int questGameId) =>
        _instance._questsRegistry.GetByGameId(questGameId).CanOnlyBeTakenAtUndergroundBar;
}