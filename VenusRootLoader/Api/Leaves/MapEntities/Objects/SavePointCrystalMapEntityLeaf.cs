using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class SavePointCrystalMapEntityLeaf : MapEntityLeaf
{
    internal SavePointCrystalMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.SavePoint;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public bool HasAnIlluminatedSphere
    {
        get => InternalData[0].Value == 1;
        set => InternalData[0].Value = value ? 1 : 0;
    }

    public bool HealsPartyWhenHit { get => InternalData[2].Value == 0; set => InternalData[2].Value = value ? 0 : 1; }

    public Vector3 PositionSavedWhenSaving
    {
        get => InternalVectorData[0].Value;
        set => InternalVectorData[0].Value = value;
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([new(1), new(0), new(1)]);
        InternalVectorData.Add(new(Vector3.zero));
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.SavePoint - 1;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver) { }
}