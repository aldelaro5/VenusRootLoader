using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities;

public sealed class DeadLanderOmegaAlertCrystalMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.SavePoint;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public bool HasAnIlluminatedSphere { get => InternalData[0] == 1; set => InternalData[0] = value ? 1 : 0; }
    public int DeadLanderOmegaId { get => InternalData[1] - 10; set => InternalData[1] = value + 10; }

    public Vector3 PositionDeadLanderOmegaLooksAtWhenHit
    {
        get => InternalVectorData[0];
        set => InternalVectorData[0] = value;
    }

    internal DeadLanderOmegaAlertCrystalMapEntity() { }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([1, 10, 0]);
        InternalVectorData.Add(Vector3.zero);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.SavePoint - 1;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver) { }
}