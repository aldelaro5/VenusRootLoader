using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace VenusRootLoader.Patching.Logic.LimitsRemoval;

/// <summary>
/// This patcher changes the initialization of some array fields of <see cref="NPCControl"/> such that they default to
/// have a length of 0 instead of 1. This allows previously awkward or impossible combinations such as an
/// enemy having no items drops without having any data elements.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="NPCControl()"/>: Changes the default length of <see cref="NPCControl.data"/> and
/// <see cref="NPCControl.vectordata"/> to 0 instead of 1.</item>
/// <item><see cref="EntityControl.CreateItem"/>: Changes the assignment of <see cref="NPCControl.data"/> so it sets the
/// entire array instead of the first element to not assume its presence anymore.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class MapEntitiesArraysLengthZeroTopLevelPatcher : ITopLevelPatcher
{
    private static readonly int[] InitialItemDataArray = [0];

    private readonly IHarmonyTypePatcher _harmonyTypePatcher;

    public MapEntitiesArraysLengthZeroTopLevelPatcher(IHarmonyTypePatcher harmonyTypePatcher)
    {
        _harmonyTypePatcher = harmonyTypePatcher;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(MapEntitiesArraysLengthZeroTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(NPCControl), MethodType.Constructor)]
    private static IEnumerable<CodeInstruction> DefaultArrayToLengthZero(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);

        FieldInfo dataField = AccessTools.Field(typeof(NPCControl), nameof(NPCControl.data));
        FieldInfo vectorDataField = AccessTools.Field(typeof(NPCControl), nameof(NPCControl.vectordata));

        // Patches data default length.
        matcher.MatchStartForward(CodeMatch.StoresField(dataField));
        matcher.MatchStartBackwards(CodeMatch.LoadsConstant());
        matcher.Set(OpCodes.Ldc_I4_0, null);

        // Patches vectordata default length.
        matcher.MatchStartForward(CodeMatch.StoresField(vectorDataField));

        // Need to remove the first element initializer.
        matcher.Advance(-1);
        while (matcher.Instruction.opcode != OpCodes.Newarr)
        {
            matcher.Set(OpCodes.Nop, null);
            matcher.Advance(-1);
        }

        matcher.MatchStartBackwards(CodeMatch.LoadsConstant(1));
        matcher.Set(OpCodes.Ldc_I4_0, null);

        return matcher.Instructions();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(EntityControl), nameof(EntityControl.CreateItem))]
    private static IEnumerable<CodeInstruction> AssignEntireArrayOfData(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);

        FieldInfo dataField = AccessTools.Field(typeof(NPCControl), nameof(NPCControl.data));

        matcher.MatchStartForward(CodeMatch.LoadsField(dataField));
        // The NPCControl should be on the stack by this point so we can just dup it.
        matcher.Insert(Code.Dup, Transpilers.EmitDelegate(SetNpcControlDataArray));

        return matcher.Instructions();
    }

    private static void SetNpcControlDataArray(NPCControl npcControl) => npcControl.data = InitialItemDataArray;
}