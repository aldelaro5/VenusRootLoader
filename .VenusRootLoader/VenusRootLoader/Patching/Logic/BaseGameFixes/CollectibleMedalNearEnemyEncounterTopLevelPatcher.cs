using Mono.Cecil.Cil;
using MonoDetour.Cil;
using MonoMod.Cil;
using System.Reflection;
using VenusRootLoader.Api.Leaves.MapEntities.Enemies;
using VenusRootLoader.Api.Leaves.MapEntities.Objects.Collectibles;
using VenusRootLoader.Extensions;
using VenusRootLoader.Utility;

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
    public void Patch()
    {
        Md.NPCControl.CheckItem.ILHookMoveNext(RefineIsMoneyCheck);
    }

    private static void RefineIsMoneyCheck(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);
        FieldInfo thisLocalField = EnumeratorUtils.GetStateMachineThisField(info.Original!.DeclaringType!);
        FieldInfo isMoneyLocalField = info.Original!.DeclaringType!
            .GetFields(ReflectionUtils.AllDeclared)
            .Single(x => x.Name.Contains("ismoney"));

        cursor.GotoNext(x => x.MatchStfld(isMoneyLocalField));
        cursor.GotoPrev(x => x.MatchLdcI4(1));
        cursor.SetAndAdvance(OpCodes.Ldarg_0);
        cursor.EmitLdfld(thisLocalField);
        cursor.EmitDelegate(IsRegularItemEntity);
    }

    private static bool IsRegularItemEntity(NPCControl npcControl) => npcControl.entity.animid == 0;
}