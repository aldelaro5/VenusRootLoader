using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

internal sealed class AreaMapPositionsTopLevelPatcher : ITopLevelPatcher
{
    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<AreaLeaf> _areasRegistry;

    private static AreaMapPositionsTopLevelPatcher _instance = null!;

    private float _yPositionAreas;

    public AreaMapPositionsTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<AreaLeaf> areasRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _areasRegistry = areasRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(AreaMapPositionsTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(PauseMenu), nameof(PauseMenu.MapSetup))]
    internal static IEnumerable<CodeInstruction> RemoveFlagsHardCapSetVariables(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator,
        MethodBase method)
    {
        CodeMatcher matcher = new(instructions, generator);

        matcher.MatchStartForward(CodeMatch.WithOpcodes([OpCodes.Switch]));

        CodeMatcher tempMatcher = matcher.Clone();
        tempMatcher.MatchStartForward(CodeMatch.LoadsLocal(true));
        LocalBuilder localPosition = (LocalBuilder)tempMatcher.Operand;
        tempMatcher.MatchStartForward(CodeMatch.LoadsConstant());
        tempMatcher.Advance(1);
        tempMatcher.MatchStartForward(CodeMatch.LoadsConstant());
        _instance._yPositionAreas = (float)tempMatcher.Operand;

        matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, localPosition));
        matcher.SetInstruction(Transpilers.EmitDelegate(PatchAreaMapPosition));

        return matcher.Instructions();
    }

    private static void PatchAreaMapPosition(MainManager.Areas area, out Vector3 mapPosition)
    {
        Vector2 leafMapPosition = _instance._areasRegistry.LeavesByGameIds[(int)area].MapPosition;
        mapPosition = new(-leafMapPosition.x, _instance._yPositionAreas, -leafMapPosition.y);
    }
}