using System.Collections.ObjectModel;
using UnityEngine;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Api.MapEntities;

public sealed class BeetleGrassMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.BeetleGrass;

    public Vector3 BoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }
    public Vector3 BoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }

    public int GrassSpriteId { get => InternalData[0]; set => InternalData[0] = value; }

    public Branch<CrystalBerryLeaf>? CrystalBerryDroppedWhenCut
    {
        get;
        set
        {
            InternalData[1] = value?.GameId ?? -1;
            field = value;
        }
    }

    public ReadOnlyCollection<Branch<ItemLeaf>?> ItemsDroppedWhenCut { get; private set; } =
        new List<Branch<ItemLeaf>?>().AsReadOnly();

    public int RegionalFlagId { get => InternalRegionalFlagId; set => InternalRegionalFlagId = value; }

    public Branch<FlagLeaf>? ActivationFlag
    {
        get;
        set
        {
            InternalActivationFlagId = value?.GameId ?? -1;
            field = value;
        }
    }

    internal BeetleGrassMapEntity() { }

    protected internal override void InitializeFromNew()
    {
        InternalData.AddRange([0, -1]);
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = false;
        InternalBoxColCenter = new(0f, 10f, 0f);
        InternalBoxColSize = new(1.5f, 20f, 0.75f);
    }

    public void ChangeItemsDroppedWhenCut(List<Branch<ItemLeaf>?> items)
    {
        InternalVectorData.Clear();
        for (int i = 0; i < items.Count; i++)
        {
            int x = items[i]?.GameId ?? -1;
            if (i < OriginalVectorData.Length)
                InternalVectorData.Add(new(x, OriginalVectorData[i].y, OriginalVectorData[i].z));
            else
                InternalVectorData.Add(new(x, 0f, 0f));
        }
        ItemsDroppedWhenCut = items.AsReadOnly();
    }
}