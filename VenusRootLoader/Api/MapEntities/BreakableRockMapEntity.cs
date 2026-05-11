using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities;

public enum BreakableRockTintColor
{
    White = 0,
    Orange = 1,
    Yellow = 2,
    Gray = 3,
    Green = 4,
    Magenta = 5
}

public sealed class BreakableRockMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.BreakableRock;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public BreakableRockTintColor TintColor
    {
        get => (BreakableRockTintColor)InternalData[0];
        set => InternalData[0] = (int)value;
    }

    public int? RegionalFlagId
    {
        get => InternalRegionalFlagId < 0 ? null : InternalRegionalFlagId;
        set => InternalRegionalFlagId = value ?? -1;
    }

    public Branch<FlagLeaf>? ActivationFlag
    {
        get;
        set
        {
            InternalActivationFlagId = value?.GameId ?? -1;
            field = value;
        }
    }

    internal BreakableRockMapEntity() { }

    internal override void InitializeFromNew()
    {
        InternalData.Add(0);
        InternalHaxBoxCol = true;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalActivationFlagId > 0)
        {
            ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
            ActivationFlag = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
        }
    }
}