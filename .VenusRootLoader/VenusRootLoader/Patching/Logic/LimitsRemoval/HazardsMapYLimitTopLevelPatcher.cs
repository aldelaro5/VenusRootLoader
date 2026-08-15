using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Logic.LimitsRemoval;

/// <summary>
/// This patcher allows <see cref="MapLeaf"/> to not have their <see cref="MapLeaf.AllEntitiesYPositionLowerBoundLimitBeforeRespawn"/>
/// even if the map contains a <see cref="Hazards"/> of type <see cref="Hazards.Type.Hole"/> This would have normally caused
/// the value to be overriden to -150 on the Hazards's Start, but this patcher removes this. To compensate, the collector
/// already sets the value on the leaves to -150 on affected maps to reflect what would have happened if no buds changes the value.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="Hazards.Start"/>: Removes the logic that sets the current map's ylimit.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class HazardsMapYLimitTopLevelPatcher : ITopLevelPatcher
{
    private readonly IHarmonyTypePatcher _harmonyTypePatcher;

    public HazardsMapYLimitTopLevelPatcher(IHarmonyTypePatcher harmonyTypePatcher)
    {
        _harmonyTypePatcher = harmonyTypePatcher;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(HazardsMapYLimitTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Hazards), nameof(Hazards.Start))]
    private static IEnumerable<CodeInstruction> RemoveMapYLimitOverride(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);

        FieldInfo mapControlYLimitField = AccessTools.Field(typeof(MapControl), nameof(MapControl.ylimit));
        FieldInfo mainManagerMapField = AccessTools.Field(typeof(MainManager), nameof(MainManager.map));

        matcher.MatchStartForward(CodeMatch.StoresField(mapControlYLimitField));
        while (!matcher.Instruction.LoadsField(mainManagerMapField))
        {
            matcher.Set(OpCodes.Nop, null);
            matcher.Advance(-1);
        }

        matcher.Set(OpCodes.Nop, null);

        return matcher.Instructions();
    }
}