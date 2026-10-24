using Mono.Cecil.Cil;
using MonoDetour.Cil;
using MonoMod.Cil;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;

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
    public void Patch()
    {
        Md.Hazards.Start.ILHook(RemoveMapYLimitOverride);
    }

    private static void RemoveMapYLimitOverride(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        cursor.GotoNext(x => x.MatchStfld<MapControl>(nameof(MapControl.ylimit)));
        while (!cursor.Current.MatchLdsfld<MainManager>(nameof(MainManager.map)))
        {
            cursor.Set(OpCodes.Nop);
            cursor.Index--;
        }

        cursor.Set(OpCodes.Nop);
    }
}