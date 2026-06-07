using CommunityToolkit.Diagnostics;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.TriggerSwitches;

public enum SwitchMapEntityActivationTriggerZoneMode
{
    ToggleWhileInside = -1,
    DeactivateWhileInside = 0,
    ActivateWhileInside = 1
}

public sealed class SwitchMapEntityLeafActivationTriggerZoneMapEntityLeaf : TriggerSwitchMapEntityLeaf
{
    internal SwitchMapEntityLeafActivationTriggerZoneMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public Branch<MapEntityLeaf> MapEntityLeafActivationControlled
    {
        get;
        set
        {
            if (value.Leaf.Map != Map)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(
                    nameof(MapEntityLeafActivationControlled),
                    $"The entity is not in the {Map.NamedId} map which is required");
            }

            if (value.GameId == GameId)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(
                    nameof(MapEntityLeafActivationControlled),
                    "The entity controlled cannot be the entity itself");
            }

            InternalData[0].Value = value.GameId;
            field = value;
        }
    } = null!;

    public SwitchMapEntityActivationTriggerZoneMode ActivationMode
    {
        get => (SwitchMapEntityActivationTriggerZoneMode)InternalData[1].Value;
        set => InternalData[1].Value = (int)value;
    }

    public bool DestroysBeemerangWhileInside
    {
        get => InternalData[2].Value == 1;
        set => InternalData[2].Value = value ? 1 : 0;
    }

    internal override void InitializeFromNew()
    {
        base.InitializeFromNew();
        InternalData.AddRange([new(0), new(1), new(0)]);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        MapEntityLeafActivationControlled = Map.Leaf.EntitiesRegistry.LeavesByGameIds[Math.Abs(InternalData[0].Value)];
    }
}