using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class DeadLanderOmegaAlertCrystalMapEntityLeaf : MapEntityLeaf
{
    internal DeadLanderOmegaAlertCrystalMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.SavePoint;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public bool HasIlluminatedSphere
    {
        get => InternalData[0].Value == 1;
        set => InternalData[0].Value = value ? 1 : 0;
    }

    public int DeadLanderOmegaId { get => InternalData[1].Value - 10; set => InternalData[1].Value = value + 10; }

    public Vector3 PositionDeadLanderOmegaLooksAtWhenHit
    {
        get => InternalVectorData[0].Value;
        set => InternalVectorData[0].Value = value;
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([new Ref<int>(1), new Ref<int>(10), new Ref<int>(0)]);
        InternalVectorData.Add(new Ref<Vector3>(Vector3.zero));
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.SavePoint - 1;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver) { }
}