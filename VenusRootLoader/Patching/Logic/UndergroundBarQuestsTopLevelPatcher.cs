using HarmonyLib;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

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

        matcher.MatchStartForward(CodeMatch.WithOpcodes([OpCodes.Brtrue, OpCodes.Brtrue_S]));
        matcher.Advance(1);
        matcher.MatchStartForward(CodeMatch.WithOpcodes([OpCodes.Brtrue, OpCodes.Brtrue_S]));
        Label questAllowedLabel = (Label)matcher.Operand;
        matcher.Advance(1);
        matcher.MatchStartForward(Code.Ldelem_I4);
        matcher.Advance(1);
        matcher.SetInstructionAndAdvance(Transpilers.EmitDelegate(IsQuestUndergroundBarExclusive));
        matcher.InsertAndAdvance([new(OpCodes.Brfalse, questAllowedLabel)]);
        matcher.Opcode = OpCodes.Br;

        return matcher.Instructions();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.ChangeBoardQuest), typeof(int), typeof(int))]
    internal static IEnumerable<CodeInstruction> FixUtterNewQuestsFlag(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);

        matcher.MatchStartForward(CodeMatch.Branches());
        Label returnLabel = (Label)matcher.Operand;
        matcher.End();
        matcher.MatchStartBackwards(Code.Stelem_I1);
        matcher.Advance(-1);
        matcher.MatchStartBackwards(CodeMatch.Branches());
        matcher.Advance(1);
        matcher.Insert(
            new(OpCodes.Ldarg_0),
            Transpilers.EmitDelegate(IsQuestUndergroundBarExclusive),
            new(OpCodes.Brtrue_S, returnLabel));

        return matcher.Instructions();
    }

    private static bool IsQuestUndergroundBarExclusive(int questGameId) =>
        _instance._questsRegistry.LeavesByGameIds[questGameId].CanOnlyBeTakenAtUndergroundBar;
}