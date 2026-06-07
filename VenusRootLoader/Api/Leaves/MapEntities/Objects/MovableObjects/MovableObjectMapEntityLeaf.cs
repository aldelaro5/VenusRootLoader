using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.MovableObjects;

public abstract class MovableObjectMapEntityLeaf : MapEntityLeaf
{
    protected MovableObjectMapEntityLeaf(int gameId, string namedId, string creatorId) : base(
        gameId,
        namedId,
        creatorId)
    {
    }

    internal sealed override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.PushRock;
    internal sealed override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }

    internal override void InitializeFromNew()
    {
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = false;
    }

    internal sealed override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 4)
            InternalData.AddRange(Enumerable.Repeat(new Ref<int>(0), 4 - InternalData.Count));
        if (InternalVectorData.Count < 2)
        {
            InternalVectorData.AddRange(
                Enumerable.Repeat(new Ref<Vector3>(Vector3.zero), 2 - InternalVectorData.Count));
        }
    }
}