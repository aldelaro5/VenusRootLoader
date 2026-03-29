using HarmonyLib;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

/// <summary>
/// This patcher adds support for allowing <see cref="QuestLeaf"/> to decide if they should only be visible on the board at the UndergroundBar.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="MainManager.GetQuestsBoard"/>: Changes the list of quests game ids that are only visible at the UndergroundBar's baord.</item>
/// <item><see cref="MainManager.ChangeBoardQuest(int, int)"/>: Prevents UndergroundBar exclusive quests from setting the all quests checked flag to false (flag 2).</item>
/// </list>
/// </p>
/// </summary>
internal sealed class UndergroundBarQuestsTopLevelPatcher : ITopLevelPatcher
{
    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<QuestLeaf> _questsRegistry;

    private static UndergroundBarQuestsTopLevelPatcher _instance = null!;

    public UndergroundBarQuestsTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<QuestLeaf> questsRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _questsRegistry = questsRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(UndergroundBarQuestsTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.GetQuestsBoard))]
    internal static IEnumerable<CodeInstruction> PatchBountyQuests(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);

        // We are getting into the parts of the long if block that handles bounty quests inside the for loop on each quest game id.
        matcher.MatchStartForward(CodeMatch.WithOpcodes([OpCodes.Brtrue, OpCodes.Brtrue_S]));
        matcher.Advance(1);
        matcher.MatchStartForward(CodeMatch.WithOpcodes([OpCodes.Brtrue, OpCodes.Brtrue_S]));
        Label questAllowedLabel = (Label)matcher.Operand;
        matcher.Advance(1);

        // This will completely override the bounty quests checks to check the QuestLeaf's field we have from the registry.
        matcher.MatchStartForward(Code.Ldelem_I4);
        matcher.Advance(1);
        matcher.SetInstructionAndAdvance(Transpilers.EmitDelegate(IsQuestUndergroundBarExclusive));
        matcher.InsertAndAdvance([new(OpCodes.Brfalse, questAllowedLabel)]);
        matcher.Opcode = OpCodes.Br;

        return matcher.Instructions();
    }

    // In the base game, there is a silent game bug where Utter will have a ! emoticon if an UndergroundBar exclusive quest is
    // added to the open board. This normally doesn't affect the base game because those are always added alongside other
    // quests from the start of the game so they don't actually have a meaningful impact. The issue is we have to patch
    // a fix for this because buds might want to unlock their UndergroundBar exclusive quests after the fact.
    // In such a case, these buds would run into this issue which would be visually illogical because the quest can't be
    // taken at Utter's board.
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.ChangeBoardQuest), typeof(int), typeof(int))]
    internal static IEnumerable<CodeInstruction> FixUtterNewQuestsFlag(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);

        matcher.MatchStartForward(CodeMatch.Branches());
        Label returnLabel = (Label)matcher.Operand;

        // We want to get into the inner if, but because the method is so short, it's more efficient to find it from the end.
        matcher.End();
        matcher.MatchStartBackwards(Code.Stelem_I1);
        matcher.Advance(-1);
        matcher.MatchStartBackwards(CodeMatch.Branches());
        matcher.Advance(1);
        // This simply adds an "and" condition to the if that guards setting the flag.
        matcher.Insert(
            new(OpCodes.Ldarg_0),
            Transpilers.EmitDelegate(IsQuestUndergroundBarExclusive),
            new(OpCodes.Brtrue_S, returnLabel));

        return matcher.Instructions();
    }

    private static bool IsQuestUndergroundBarExclusive(int questGameId) =>
        _instance._questsRegistry.LeavesByGameIds[questGameId].CanOnlyBeTakenAtUndergroundBar;
}