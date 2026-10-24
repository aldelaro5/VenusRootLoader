using MonoDetour.Cil;
using MonoMod.Cil;
using System.Reflection;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Logic.LimitsRemoval;

/// <summary>
/// This patcher adds support for a variable amount of elements in array data present on map entities to exist in the game.
/// More specifically, it allows custom length for these fields:
/// <list type="bullet">
/// <item><see cref="NPCControl.requires"/></item>
/// <item><see cref="NPCControl.limit"/></item>
/// <item><see cref="NPCControl.data"/></item>
/// <item><see cref="NPCControl.vectordata"/></item>
/// <item><see cref="NPCControl.dialogues"/></item>
/// <item><see cref="NPCControl.battleids"/></item>
/// <item><see cref="NPCControl.emoticonflag"/></item>
/// </list>
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="MapControl.CreateEntities"/>: Changes the hardcoded length of the array fields to accomodate our map entities.</item>
/// <item><see cref="NPCControl.SetBadgeShop"/>: Changes the amount of shelved items to be the length of
/// <see cref="NPCControl.vectordata"/> instead of <see cref="NPCControl.data"/> which is easier to work with because
/// the content of <see cref="NPCControl.data"/> isn't used for a medals shops while <see cref="NPCControl.vectordata"/>
/// is used for both types of shops.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class MapEntitiesArraysLengthTopLevelPatcher : ITopLevelPatcher
{
    private readonly ILeavesRegistry<MapLeaf> _mapsLeafRegistry;

    private static MapEntitiesArraysLengthTopLevelPatcher _instance = null!;

    public MapEntitiesArraysLengthTopLevelPatcher(ILeavesRegistry<MapLeaf> mapsLeafRegistry)
    {
        _instance = this;
        _mapsLeafRegistry = mapsLeafRegistry;
    }

    public void Patch()
    {
        Md.MapControl.CreateEntities.ILHook(RemoveMapEntitiesArraysLengthHardCap);
        Md.NPCControl.SetBadgeShop.ILHook(FixSetBadgeShopPoolMedalsLength);
    }

    private static void RemoveMapEntitiesArraysLengthHardCap(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        int mapIdLocalIndex = 0;
        cursor.GotoNext(x => x.MatchStloc(out mapIdLocalIndex));

        cursor.GotoNext(x => x.MatchLdelemRef());
        int mapEntityIdLocalIndex = 0;
        cursor.GotoPrev(x => x.MatchLdloc(out mapEntityIdLocalIndex));

        // This field is right before the first we need to patch so it's convenient to go to its store first.
        cursor.GotoNext(x => x.MatchStfld<NPCControl>(nameof(NPCControl.eventid)));
        int mapEntityDataFieldIndexLocal = 0;
        cursor.GotoNext(x => x.MatchStloc(out mapEntityDataFieldIndexLocal));
        cursor.Index++;

        cursor.GotoNext(x => x.MatchStloc(mapEntityDataFieldIndexLocal));
        cursor.GotoPrev(x => x.MatchLdcI4(out _));
        cursor.Index++;
        cursor.EmitLdloc(mapEntityIdLocalIndex);
        cursor.EmitLdloc(mapIdLocalIndex);
        cursor.EmitDelegate(PatchNewRequiresLength);

        cursor.GotoNext(x => x.MatchStfld<NPCControl>(nameof(NPCControl.limit)));
        cursor.Index++;
        cursor.GotoNext(x => x.MatchStfld<NPCControl>(nameof(NPCControl.limit)));
        cursor.GotoNext(x => x.MatchLdelemRef());
        cursor.GotoPrev(x => x.MatchStloc(out mapEntityDataFieldIndexLocal));
        cursor.GotoPrev(x => x.MatchLdcI4(out _));
        cursor.Index++;
        cursor.EmitLdloc(mapEntityIdLocalIndex);
        cursor.EmitLdloc(mapIdLocalIndex);
        cursor.EmitDelegate(PatchNewLimitsLength);

        cursor.GotoNext(x => x.MatchLdfld<NPCControl>(nameof(NPCControl.data)));
        cursor.Index++;
        cursor.GotoNext(x => x.MatchLdfld<NPCControl>(nameof(NPCControl.data)));
        cursor.GotoNext(x => x.MatchLdelemRef());
        cursor.GotoPrev(x => x.MatchStloc(out mapEntityDataFieldIndexLocal));
        cursor.GotoPrev(x => x.MatchLdcI4(out _));
        cursor.Index++;
        cursor.EmitLdloc(mapEntityIdLocalIndex);
        cursor.EmitLdloc(mapIdLocalIndex);
        cursor.EmitDelegate(PatchNewDataLength);

        cursor.GotoNext(x => x.MatchLdfld<NPCControl>(nameof(NPCControl.vectordata)));
        cursor.Index++;
        cursor.GotoNext(x => x.MatchLdfld<NPCControl>(nameof(NPCControl.vectordata)));
        cursor.GotoNext(x => x.MatchLdelemRef());
        cursor.GotoPrev(x => x.MatchStloc(out mapEntityDataFieldIndexLocal));
        cursor.GotoPrev(x => x.MatchLdcI4(out _));
        cursor.Index++;
        cursor.EmitLdloc(mapEntityIdLocalIndex);
        cursor.EmitLdloc(mapIdLocalIndex);
        cursor.EmitDelegate(PatchNewVectorDataFieldsLength);

        // This one patches the hardcoded length if the map entity is a shop system
        cursor.GotoNext(x => x.MatchStfld<NPCControl>(nameof(NPCControl.dialogues)));
        cursor.GotoPrev(x => x.MatchLdcI4(out _));
        cursor.Index++;
        cursor.EmitLdloc(mapEntityIdLocalIndex);
        cursor.EmitLdloc(mapIdLocalIndex);
        cursor.EmitDelegate(PatchNewDialoguesLength);

        // Still a shop system, but this one patches the for loop length condition
        ILLabel labelLoopEnd = null!;
        cursor.GotoNext(x => x.MatchBranch(out labelLoopEnd!));
        cursor.GotoLabel(labelLoopEnd);
        cursor.GotoNext(x => x.MatchLdcI4(out _));
        cursor.Index++;
        cursor.EmitLdloc(mapEntityIdLocalIndex);
        cursor.EmitLdloc(mapIdLocalIndex);
        cursor.EmitDelegate(PatchNewDialoguesLength);

        // This one patches the accumulated field index
        cursor.GotoNext(x => x.MatchStfld<NPCControl>(nameof(NPCControl.dialogues)));
        cursor.Index++;
        cursor.GotoNext(x => x.MatchStfld<NPCControl>(nameof(NPCControl.dialogues)));
        cursor.GotoNext(x => x.MatchBranch(out _));
        cursor.GotoPrev(x => x.MatchStloc(out mapEntityDataFieldIndexLocal));
        cursor.GotoPrev(x => x.MatchLdcI4(out _));
        cursor.Index++;
        cursor.EmitLdloc(mapEntityIdLocalIndex);
        cursor.EmitLdloc(mapIdLocalIndex);
        cursor.EmitDelegate(PatchNewDialoguesFieldsLength);

        cursor.GotoNext(x => x.MatchStfld<NPCControl>(nameof(NPCControl.battleids)));
        cursor.Index++;
        cursor.GotoNext(x => x.MatchStfld<NPCControl>(nameof(NPCControl.battleids)));
        cursor.GotoNext(x => x.MatchLdelemRef());
        cursor.GotoPrev(x => x.MatchStloc(out mapEntityDataFieldIndexLocal));
        cursor.GotoPrev(x => x.MatchLdcI4(out _));
        cursor.Index++;
        cursor.EmitLdloc(mapEntityIdLocalIndex);
        cursor.EmitLdloc(mapIdLocalIndex);
        cursor.EmitDelegate(PatchNewBattleIdsLength);

        cursor.GotoNext(x => x.MatchStfld<NPCControl>(nameof(NPCControl.emoticonflag)));
        ILLabel emoticonFlagLoopLabel = null!;
        cursor.GotoNext(x => x.MatchBranch(out emoticonFlagLoopLabel!));
        cursor.GotoLabel(emoticonFlagLoopLabel);
        cursor.GotoNext(x => x.MatchLdcI4(out _));
        cursor.Index++;
        cursor.EmitLdloc(mapEntityIdLocalIndex);
        cursor.EmitLdloc(mapIdLocalIndex);
        cursor.EmitDelegate(PatchNewEmoticonFlagsLength);
    }

    private static void FixSetBadgeShopPoolMedalsLength(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);
        FieldInfo npcControlVectorDataField = typeof(NPCControl).FieldByName(nameof(NPCControl.vectordata))!;

        cursor.GotoNext(x => x.MatchLdfld<NPCControl>(nameof(NPCControl.data)));
        cursor.Current.Operand = npcControlVectorDataField;
        cursor.GotoNext(x => x.MatchLdfld<NPCControl>(nameof(NPCControl.data)));
        cursor.Current.Operand = npcControlVectorDataField;
    }

    private static int PatchNewRequiresLength(int originalLength, int mapEntityId, int mapGameId)
    {
        MapLeaf mapLeaf = _instance._mapsLeafRegistry.GetByGameId(mapGameId);
        return Math.Max(originalLength, mapLeaf.EntitiesRegistry.GetByGameId(mapEntityId).RequiredFlags.Count);
    }

    private static int PatchNewLimitsLength(int originalLength, int mapEntityId, int mapGameId)
    {
        MapLeaf mapLeaf = _instance._mapsLeafRegistry.GetByGameId(mapGameId);
        return Math.Max(originalLength, mapLeaf.EntitiesRegistry.GetByGameId(mapEntityId).LimitedToFlags.Count);
    }

    private static int PatchNewDataLength(int originalLength, int mapEntityId, int mapGameId)
    {
        MapLeaf mapLeaf = _instance._mapsLeafRegistry.GetByGameId(mapGameId);
        return Math.Max(originalLength, mapLeaf.EntitiesRegistry.GetByGameId(mapEntityId).InternalData.Count);
    }

    private static int PatchNewVectorDataFieldsLength(int originalLength, int mapEntityId, int mapGameId)
    {
        MapLeaf mapLeaf = _instance._mapsLeafRegistry.GetByGameId(mapGameId);
        return Math.Max(
            originalLength,
            mapLeaf.EntitiesRegistry.GetByGameId(mapEntityId).InternalVectorData.Count * 3);
    }

    private static int PatchNewDialoguesLength(int originalLength, int mapEntityId, int mapGameId)
    {
        MapLeaf mapLeaf = _instance._mapsLeafRegistry.GetByGameId(mapGameId);
        return Math.Max(originalLength, mapLeaf.EntitiesRegistry.GetByGameId(mapEntityId).InternalDialogues.Count);
    }

    // This is the amount of fields, the one above is the array length.
    private static int PatchNewDialoguesFieldsLength(int originalLength, int mapEntityId, int mapGameId)
    {
        MapLeaf mapLeaf = _instance._mapsLeafRegistry.GetByGameId(mapGameId);
        return Math.Max(
            originalLength,
            mapLeaf.EntitiesRegistry.GetByGameId(mapEntityId).InternalDialogues.Count * 3);
    }

    private static int PatchNewBattleIdsLength(int originalLength, int mapEntityId, int mapGameId)
    {
        MapLeaf mapLeaf = _instance._mapsLeafRegistry.GetByGameId(mapGameId);
        return Math.Max(
            originalLength,
            mapLeaf.EntitiesRegistry.GetByGameId(mapEntityId).InternalBattleEnemyIds.Count);
    }

    private static int PatchNewEmoticonFlagsLength(int originalLength, int mapEntityId, int mapGameId)
    {
        MapLeaf mapLeaf = _instance._mapsLeafRegistry.GetByGameId(mapGameId);
        return Math.Max(
            originalLength,
            mapLeaf.EntitiesRegistry.GetByGameId(mapEntityId).InternalEmoticonFlags.Count);
    }
}