using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace VenusRootLoader.Patching.Logic;

/// <summary>
/// This patcher fixes a base game issue where the <see cref="EntityControl.lastpos"/> of an entity with <see cref="EntityControl.iskill"/>
/// is not set alongside the transform position to offscreen on <see cref="NPCControl.LateUpdate"/> which causes <see cref="NPCControl.Update"/>
/// to later reset the position to the one the entity had originally before being moved. It particularly affects shelved
/// medals of medals shops located in an inside that are iskilled due to not having anything to display in their slots.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="NPCControl.LateUpdate"/>: Sets the <see cref="EntityControl.lastpos"/> of the entity to its transform position after it gets set.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class EntityIsKillLastPositionTopLevelPatcher : ITopLevelPatcher
{
    private readonly IHarmonyTypePatcher _harmonyTypePatcher;

    public EntityIsKillLastPositionTopLevelPatcher(IHarmonyTypePatcher harmonyTypePatcher)
    {
        _harmonyTypePatcher = harmonyTypePatcher;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(EntityIsKillLastPositionTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(NPCControl), nameof(MapControl.LateUpdate))]
    internal static IEnumerable<CodeInstruction> SetEntityLastPositionWhenIsKilled(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        MethodInfo transformPositionSetterMethod =
            AccessTools.PropertySetter(typeof(Transform), nameof(Transform.position));
        FieldInfo npcDummyField = AccessTools.Field(typeof(NPCControl), nameof(NPCControl.dummy));
        FieldInfo entityInCameraField = AccessTools.Field(typeof(EntityControl), nameof(EntityControl.incamera));
        MethodInfo timeFrameCountGetterMethod = AccessTools.PropertyGetter(typeof(Time), nameof(Time.frameCount));

        CodeMatcher matcher = new(instructions, generator);

        // Naviguate through the control flows to the iskill logic
        matcher.MatchStartForward(CodeMatch.LoadsField(npcDummyField));
        matcher.MatchStartForward(CodeMatch.LoadsField(entityInCameraField));
        matcher.MatchStartForward(CodeMatch.Calls(timeFrameCountGetterMethod));

        // This constant happens to be used to check if the transform position was already set
        matcher.MatchStartForward(CodeMatch.LoadsConstant(-999f));

        // We want to go after the transform position is set which is the last step of the iskill logic
        matcher.MatchStartForward(CodeMatch.Calls(transformPositionSetterMethod));
        matcher.Advance(1);
        matcher.Insert(CodeInstruction.LoadArgument(0), Transpilers.EmitDelegate(OnPostIsKillLogic));

        return matcher.Instructions();
    }

    private static void OnPostIsKillLogic(NPCControl instance)
    {
        instance.entity.lastpos = instance.entity.transform.position;
    }
}