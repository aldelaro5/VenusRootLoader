using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities.Objects;

public sealed class FlytrapPlatformMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.TempPlatform;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public int TimeInFramesPlayerCanStayOnPlatform { get => InternalData[0]; set => InternalData[0] = value; }

    public float DelayFramesBeforeRespawnWhenFlytrapCloses
    {
        get => InternalVectorData[0].x;
        set => InternalVectorData[0] = new(value, InternalVectorData[0].y, InternalVectorData[0].z);
    }

    internal FlytrapPlatformMapEntity() { }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([60, 1, 1, 1]);
        InternalVectorData.Add(new(60f, 0f, 0f));
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.FlyTrapPlatform - 1;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver) { }
}