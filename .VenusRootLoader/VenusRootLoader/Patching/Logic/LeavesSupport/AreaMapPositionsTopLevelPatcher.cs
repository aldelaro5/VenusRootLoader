using MonoDetour.Cil;
using MonoMod.Cil;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic.LeavesSupport;

/// <summary>
/// This patcher adds support for custom map positions for <see cref="AreaLeaf"/>.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="PauseMenu.MapSetup"/>: Replaces a switch to determine an area's position on the mapo with our own
/// method based on the registry.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class AreaMapPositionsTopLevelPatcher : ITopLevelPatcher
{
    private readonly ILeavesRegistry<AreaLeaf> _areasRegistry;

    private static AreaMapPositionsTopLevelPatcher _instance = null!;

    private float _yPositionAreas;

    public AreaMapPositionsTopLevelPatcher(ILeavesRegistry<AreaLeaf> areasRegistry)
    {
        _instance = this;
        _areasRegistry = areasRegistry;
    }

    public void Patch()
    {
        Md.PauseMenu.MapSetup.ILHook(PatchAllMapPositions);
    }

    private static void PatchAllMapPositions(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        cursor.GotoNext(x => x.MatchSwitch(out _));

        // We first need to take a detour to gather the local where the area is stored and the y position map positions
        // always have (which is effectively a z derived component value).
        int cursorIndex = cursor.Index;
        int localPositionIndex = 0;
        cursor.GotoNext(x => x.MatchLdloca(out localPositionIndex));
        cursor.GotoNext(x => x.MatchLdcR4(out _));
        cursor.Index++;
        cursor.GotoNext(x => x.MatchLdcR4(out _instance._yPositionAreas));
        cursor.Index = cursorIndex;

        cursor.EmitLdloca(localPositionIndex);
        cursor.SetToStaticDelegateCall(PatchAreaMapPosition);
    }

    private static void PatchAreaMapPosition(MainManager.Areas area, out Vector3 mapPosition)
    {
        Vector2 leafMapPosition = _instance._areasRegistry.GetByGameId((int)area).MapPosition;
        // The positions in the game are encoded in this strange form. We already decoded them on the collector side so
        // we need to encode them again.
        mapPosition = new(-leafMapPosition.x, _instance._yPositionAreas, -leafMapPosition.y);
    }
}