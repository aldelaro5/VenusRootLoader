using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves.MapEntities.Enemies;
using VenusRootLoader.Api.Leaves.MapEntities.Objects.Collectibles;

namespace VenusRootLoader.Patching.Logic.BaseGameFixes;

/// <summary>
/// This patcher fixes a base game issue where an <see cref="EnemyEncounterMapEntityLeaf"/> next to a
/// <see cref="CollectibleMedalMapEntityLeaf"/> can lead to a situation where the player flees from the enemy and collects
/// the medal during the invulnerability window which leads to them collecting the medal many times. This is because the
/// game incorrectly determines that certain medals with specific game ids are berries items. It happens because while
/// the game checks the <see cref="EntityControl"/>'s animstate (which is the item / medal game id for item entities), it
/// does not check the animId which is the item entity's type.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="NPCControl.CheckItem"/>: Refines the conditions the game use to determine if the collectible is a
/// berry item.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class CollectibleMedalNearEnemyEncounterTopLevelPatcher : ITopLevelPatcher
{
    private readonly IHarmonyTypePatcher _harmonyTypePatcher;

    public CollectibleMedalNearEnemyEncounterTopLevelPatcher(IHarmonyTypePatcher harmonyTypePatcher)
    {
        _harmonyTypePatcher = harmonyTypePatcher;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(CollectibleMedalNearEnemyEncounterTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(NPCControl), nameof(NPCControl.CheckItem), MethodType.Enumerator)]
    internal static IEnumerable<CodeInstruction> RefineIsMoneyCheck(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator,
        MethodBase __originalMethod)
    {
        CodeMatcher matcher = new(instructions, generator);

        FieldInfo isMoneyLocalField =
            __originalMethod.DeclaringType.GetDeclaredFields().Single(x => x.Name.Contains("ismoney"));

        matcher.MatchStartForward(CodeMatch.StoresField(isMoneyLocalField));
        matcher.MatchStartBackwards(CodeMatch.LoadsConstant(1));
        // This is always the "this" of the coroutine so the NPCControl here.
        matcher.SetOpcodeAndAdvance(OpCodes.Ldloc_1);
        matcher.Insert(Transpilers.EmitDelegate(IsRegularItemEntity));

        return matcher.Instructions();
    }

    private static bool IsRegularItemEntity(NPCControl npcControl) => npcControl.entity.animid == 0;
}