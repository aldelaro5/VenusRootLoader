using System.Collections.ObjectModel;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities.Objects;

public sealed class AndBlockOnFlagsMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.ANDBlock;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public ReadOnlyCollection<Branch<FlagLeaf>> FlagsInput { get; private set; } =
        new List<Branch<FlagLeaf>>().AsReadOnly();

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public Branch<AnimIdLeaf>? AnimId
    {
        get;
        set
        {
            InternalAnimIdOrItemId = value?.GameId ?? -1;
            field = value;
        }
    }

    public Vector3 LocalPositionWhenActuatedAfterLerp
    {
        get => InternalVectorData[0];
        set => InternalVectorData[0] = value;
    }

    public float LocalPositionLerpFactorWhenActuated
    {
        get => InternalVectorData[1].x;
        set => InternalVectorData[1] = new(value, InternalVectorData[1].y, InternalVectorData[1].z);
    }

    public Vector3? EntityStartScale
    {
        get => InternalVectorData[2].magnitude <= 0.1f ? null : InternalVectorData[2];
        set => InternalVectorData[2] = value ?? Vector3.zero;
    }

    internal AndBlockOnFlagsMapEntity() { }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([-2]);
        InternalVectorData.AddRange([Vector3.down * 6f, Vector3.right * 0.1f, Vector3.zero]);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.PrisonGate - 1;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalVectorData.Count < 3)
            InternalVectorData.AddRange(Enumerable.Repeat(Vector3.zero, 3 - InternalVectorData.Count));

        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        ILeavesRegistry<AnimIdLeaf> animidRegistry = registryResolver.Resolve<AnimIdLeaf>();

        List<Branch<FlagLeaf>> flagsInput = new();
        for (int i = 1; i < InternalData.Count; i++)
        {
            int value = InternalData[i];
            flagsInput.Add(new(flagsRegistry.LeavesByGameIds[Math.Abs(value)]));
        }

        if (InternalAnimIdOrItemId >= 0)
            AnimId = new(animidRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);

        ChangeFlagsInput(flagsInput);
    }

    public void ChangeFlagsInput(List<Branch<FlagLeaf>> flagsInput)
    {
        InternalData.RemoveRange(1, InternalData.Count - 1);

        foreach (Branch<FlagLeaf> negatableFlag in flagsInput)
            InternalData.Add(-negatableFlag.GameId);

        FlagsInput = flagsInput.AsReadOnly();
    }
}