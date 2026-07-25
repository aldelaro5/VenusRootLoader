using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

/// <summary>
/// This patcher adds support for custom map positions for <see cref="AreaLeaf"/>.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="PauseMenu.MapSetup"/>: Replaces a switch to determine an area's position on the mapo with our own method based on the registry.</item>
/// </list>
/// </p>
/// </summary>
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
    internal static IEnumerable<CodeInstruction> PatchAllMapPositions(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator,
        MethodBase method)
    {
        CodeMatcher matcher = new(instructions, generator);

        matcher.MatchStartForward(CodeMatch.WithOpcodes([OpCodes.Switch]));

        // We first need to take a detour to gather the local where the area is stored and the y position map positions
        // always have (which is effectively a z derived component value).
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
        // The positions in the game are encoded in this strange form. We already decoded them on the collector side so
        // we need to encode them again.
        mapPosition = new(-leafMapPosition.x, _instance._yPositionAreas, -leafMapPosition.y);
    }
}