using UnityEngine;
using VenusRootLoader.Api.Leaves.MapEntities.Objects.Enums;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class BreakableRockMapEntityLeaf : ObjectMapEntityLeaf
{
    internal BreakableRockMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.BreakableRock;

    public BreakableRockTintColor TintColor
    {
        get => (BreakableRockTintColor)InternalData[0].Value;
        set => InternalData[0].Value = (int)value;
    }

    public Branch<FlagLeaf>? FlagSetToTrueWhenRockBreaks
    {
        get;
        set
        {
            InternalActivationFlagId = value?.GameId ?? -1;
            field = value;
        }
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(Vector3 startingPosition)
    {
        InternalData.Add(new(0));
        InternalHaxBoxCol = true;
        EntityStartingPosition = startingPosition;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalActivationFlagId > 0)
        {
            ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
            FlagSetToTrueWhenRockBreaks = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
        }
    }
}