using UnityEngine;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.Switches;

public sealed class EventTriggerSwitchMapEntityLeaf : SwitchMapEntityLeaf
{
    internal EventTriggerSwitchMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public Branch<EventLeaf> EventToStartWhenToggled
    {
        get;
        set
        {
            InternalData[1].Value = value.Resolve().GameId;
            field = value;
        }
    }

    public Branch<FlagLeaf>? FlagActivationOverrideOnMapLoad
    {
        get;
        set
        {
            InternalActivationFlagId = value?.Resolve().GameId ?? -1;
            field = value;
        }
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<AnimIdLeaf>? animId,
        Branch<EventLeaf> eventToStartWhenToggled)
    {
        base.InitializeFromNew(startingPosition, animId);
        InternalData.AddRange([new(1), new(1), new(0), new(0), new(0)]);
        EventToStartWhenToggled = eventToStartWhenToggled;
    }

    internal override void InitializeFromExisting()
    {
        base.InitializeFromExisting();
        ILeavesRegistry<EventLeaf> eventsRegistry = RegistryResolver.Resolve<EventLeaf>();
        ILeavesRegistry<FlagLeaf> flagsRegistry = RegistryResolver.Resolve<FlagLeaf>();

        EventToStartWhenToggled = new(eventsRegistry.LeavesByGameIds[InternalData[1].Value]);
        if (InternalActivationFlagId > 0)
            FlagActivationOverrideOnMapLoad = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
    }
}