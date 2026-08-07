using UnityEngine;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.Switches;

public sealed class EventTriggerSwitchMapEntityLeaf : SwitchMapEntityLeaf
{
    internal EventTriggerSwitchMapEntityLeaf(int gameId, string creatorId, string namedId)
        : base(gameId, creatorId, namedId)
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
    } = null!;

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

        EventToStartWhenToggled = new(eventsRegistry.GetByGameId(InternalData[1].Value));
        if (InternalActivationFlagId > 0)
            FlagActivationOverrideOnMapLoad = new(flagsRegistry.GetByGameId(InternalActivationFlagId));
    }
}