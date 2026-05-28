using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities.Objects;

public sealed class SavePointCrystalMapEntity : MapEntity
{
    internal SavePointCrystalMapEntity(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.SavePoint;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public bool HasAnIlluminatedSphere { get => InternalData[0] == 1; set => InternalData[0] = value ? 1 : 0; }
    public bool HealsPartyWhenHit { get => InternalData[2] == 0; set => InternalData[2] = value ? 0 : 1; }
    public Vector3 PositionSavedWhenSaving { get => InternalVectorData[0]; set => InternalVectorData[0] = value; }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([1, 0, 1]);
        InternalVectorData.Add(Vector3.zero);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.SavePoint - 1;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver) { }
}