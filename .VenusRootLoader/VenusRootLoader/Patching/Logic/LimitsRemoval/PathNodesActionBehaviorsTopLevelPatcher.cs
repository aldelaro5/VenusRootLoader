using MonoDetour.Cil;
using MonoMod.Cil;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.Leaves.MapEntities;
using VenusRootLoader.Extensions;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic.LimitsRemoval;

/// <summary>
/// This patcher redirects the use of <see cref="NPCControl.vectordata"/> for <see cref="NPCControl.ActionBehaviors.SetPath"/>
/// or <see cref="NPCControl.ActionBehaviors.StealthAI"/> to instead use a list we control that's stored on the corresponding
/// <see cref="MapEntityLeaf"/>. This is necessary to support combinations such as an Enemy with SetPath that can also drop items.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="NPCControl.DoBehavior(ref NPCControl.ActionBehaviors, float)"/>: For
/// <see cref="NPCControl.ActionBehaviors.SetPath"/> or <see cref="NPCControl.ActionBehaviors.StealthAI"/> behaviors,
/// pulls the list in the registry instead of using <see cref="NPCControl.vectordata"/>.</item>
/// <item><see cref="EntityControl.Death(bool)"/>: Removes a check that would normally prevent Enemy with
/// <see cref="NPCControl.ActionBehaviors.SetPath"/> to drop items.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class PathNodesActionBehaviorsTopLevelPatcher : ITopLevelPatcher
{
    private readonly ILeavesRegistry<MapLeaf> _mapsRegistry;

    private static PathNodesActionBehaviorsTopLevelPatcher _instance = null!;

    public PathNodesActionBehaviorsTopLevelPatcher(ILeavesRegistry<MapLeaf> mapsRegistry)
    {
        _instance = this;
        _mapsRegistry = mapsRegistry;
    }

    public void Patch()
    {
        Md.NPCControl.DoBehavior_ref_NPCControl_ActionBehaviors_System_Single.ILHook(ChangeNodesPositionArrays);
        Md.EntityControl.Death_System_Boolean.ILHookMoveNext(AllowEnemiesItemsDropsOnDeath);
    }

    private static void ChangeNodesPositionArrays(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        ILLabel[] actionBehaviorsLabels = null!;
        cursor.GotoNext(x => x.MatchSwitch(out actionBehaviorsLabels!));
        ILLabel setPathActionBehaviorArm = actionBehaviorsLabels[(int)NPCControl.ActionBehaviors.SetPath];
        cursor.GotoLabel(setPathActionBehaviorArm);

        while (cursor.TryGotoNext(x => x.MatchLdfld<NPCControl>(nameof(NPCControl.vectordata))))
            cursor.SetToStaticDelegateCallAndAdvance(PatchNewPathNodesArray);
    }

    private static void AllowEnemiesItemsDropsOnDeath(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        cursor.GotoNext(x => x.MatchCallOrCallvirt<NPCControl>(nameof(NPCControl.HasBehavior)));
        cursor.Index++;
        cursor.GotoNext(x => x.MatchCallOrCallvirt<NPCControl>(nameof(NPCControl.HasBehavior)));
        cursor.GotoNext(x => x.MatchCallOrCallvirt<MainManager>(nameof(MainManager.BadgeHowManyEquipped)));
        cursor.GotoPrev(x => x.MatchBranch(out _));
        cursor.Index++;
        ILLabel itemDropsLogicLabel = cursor.MarkLabel();

        cursor.GotoPrev(x => x.MatchCallOrCallvirt<NPCControl>(nameof(NPCControl.HasBehavior)));
        cursor.Index--;
        cursor.GotoPrev(x => x.MatchCallOrCallvirt<NPCControl>(nameof(NPCControl.HasBehavior)));
        cursor.GotoPrev(x => x.MatchBranch(out _));
        cursor.Index++;
        cursor.EmitBr(itemDropsLogicLabel);
    }

    private static Vector3[] PatchNewPathNodesArray(NPCControl instance)
    {
        MapLeaf map = _instance._mapsRegistry.GetByGameId((int)MainManager.map.mapid);
        MapEntityLeaf mapEntityLeaf = map.EntitiesRegistry.GetByGameId(instance.mapid);
        return mapEntityLeaf.InternalSecondaryVectorDataArray;
    }
}