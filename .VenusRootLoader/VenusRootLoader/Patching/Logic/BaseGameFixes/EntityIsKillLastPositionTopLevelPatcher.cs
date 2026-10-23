using MonoDetour.Cil;
using MonoMod.Cil;
using UnityEngine;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Logic.BaseGameFixes;

/// <summary>
/// This patcher fixes a base game issue where the <see cref="EntityControl.lastpos"/> of an entity with <see cref="EntityControl.iskill"/>
/// is not set alongside the transform position to offscreen on <see cref="NPCControl.LateUpdate"/> which causes <see cref="NPCControl.Update"/>
/// to later reset the position to the one the entity had originally before being moved. It particularly affects shelved
/// medals of medals shops located in an inside that are iskilled due to not having anything to display in their slots.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="NPCControl.LateUpdate"/>: Sets the <see cref="EntityControl.lastpos"/> of the entity to its
/// transform position after it gets set.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class EntityIsKillLastPositionTopLevelPatcher : ITopLevelPatcher
{
    public void Patch()
    {
        Md.NPCControl.LateUpdate.ILHook(SetEntityLastPositionWhenIsKilled);
    }

    private static void SetEntityLastPositionWhenIsKilled(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        // Navigate through the control flows to the iskill logic
        cursor.GotoNext(x => x.MatchLdfld<NPCControl>(nameof(NPCControl.dummy)));
        cursor.GotoNext(x => x.MatchLdfld<EntityControl>(nameof(EntityControl.incamera)));
        cursor.GotoNext(x => x.MatchCallOrCallvirt<Time>(
            ReflectionUtils.PropertyNameToGetterName(nameof(Time.frameCount))));

        // This constant happens to be used to check if the transform position was already set
        cursor.GotoNext(x => x.MatchLdcR4(-999f));

        // We want to go after the transform position is set which is the last step of the iskill logic
        cursor.GotoNext(x => x.MatchCallOrCallvirt<Transform>(
            ReflectionUtils.PropertyNameToSetterName(nameof(Transform.position))));
        cursor.Index++;
        cursor.EmitLdarg0();
        cursor.EmitDelegate(OnPostIsKillLogic);
    }

    private static void OnPostIsKillLogic(NPCControl instance)
    {
        instance.entity.lastpos = instance.entity.transform.position;
    }
}