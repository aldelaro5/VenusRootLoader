using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.Leaves.MapEntities;
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
    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<MapLeaf> _mapsRegistry;

    private static PathNodesActionBehaviorsTopLevelPatcher _instance = null!;

    public PathNodesActionBehaviorsTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<MapLeaf> mapsRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _mapsRegistry = mapsRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(PathNodesActionBehaviorsTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(
        typeof(NPCControl),
        nameof(NPCControl.DoBehavior),
        [typeof(NPCControl.ActionBehaviors), typeof(float)],
        [ArgumentType.Ref, ArgumentType.Normal])]
    internal static IEnumerable<CodeInstruction> ChangeNodesPositionArrays(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);
        FieldInfo npcControlVectorDataField = AccessTools.Field(typeof(NPCControl), nameof(NPCControl.vectordata));

        matcher.MatchStartForward(Code.Switch);
        Label[] actionBehaviorsLabels = (Label[])matcher.Operand;
        Label setPathActionBehaviorArm = actionBehaviorsLabels[(int)NPCControl.ActionBehaviors.SetPath];
        matcher.MatchStartForward(new CodeMatch(i => i.labels.Contains(setPathActionBehaviorArm)));

        matcher.MatchStartForward(CodeMatch.LoadsField(npcControlVectorDataField));
        while (matcher.IsValid)
        {
            matcher.SetInstructionAndAdvance(Transpilers.EmitDelegate(PatchNewPathNodesArray));
            matcher.MatchStartForward(CodeMatch.LoadsField(npcControlVectorDataField));
        }

        return matcher.Instructions();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(EntityControl), nameof(EntityControl.Death), MethodType.Enumerator, [typeof(bool)])]
    internal static IEnumerable<CodeInstruction> AllowEnemiesItemsDropsOnDeath(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);
        MethodInfo npcControlHasBehaviorMethod = AccessTools.Method(
            typeof(NPCControl),
            nameof(NPCControl.HasBehavior),
            [typeof(NPCControl.ActionBehaviors)]);
        MethodInfo mainManagerBadgeHowManageEquippedMethod = AccessTools.Method(
            typeof(MainManager),
            nameof(MainManager.BadgeHowManyEquipped),
            [typeof(int)]);

        matcher.MatchStartForward(CodeMatch.Calls(npcControlHasBehaviorMethod));
        matcher.Advance(1);
        matcher.MatchStartForward(CodeMatch.Calls(npcControlHasBehaviorMethod));
        matcher.MatchStartForward(CodeMatch.Calls(mainManagerBadgeHowManageEquippedMethod));
        matcher.MatchStartBackwards(CodeMatch.Branches());
        matcher.Advance(1);
        matcher.CreateLabel(out Label itemDropsLogicLabel);

        matcher.MatchStartBackwards(CodeMatch.Calls(npcControlHasBehaviorMethod));
        matcher.Advance(-1);
        matcher.MatchStartBackwards(CodeMatch.Calls(npcControlHasBehaviorMethod));
        matcher.MatchStartBackwards(CodeMatch.Branches());
        matcher.Advance(1);
        matcher.Insert(new CodeInstruction(OpCodes.Br_S, itemDropsLogicLabel));

        return matcher.Instructions();
    }

    private static Vector3[] PatchNewPathNodesArray(NPCControl instance)
    {
        MapLeaf map = _instance._mapsRegistry.GetByGameId((int)MainManager.map.mapid);
        MapEntityLeaf mapEntityLeaf = map.EntitiesRegistry.GetByGameId(instance.mapid);
        return mapEntityLeaf.InternalSecondaryVectorDataArray;
    }
}