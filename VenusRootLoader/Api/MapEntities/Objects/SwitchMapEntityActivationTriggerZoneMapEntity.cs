using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities.Objects;

public enum SwitchMapEntityActivationTriggerZoneMode
{
    ToggleWhileInside = -1,
    DeactivateWhileInside = 0,
    ActivateWhileInside = 1
}

public sealed class SwitchMapEntityActivationTriggerZoneMapEntity : MapEntity
{
    internal SwitchMapEntityActivationTriggerZoneMapEntity(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.TriggerSwitch;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public MapEntity MapEntityActivationControlled
    {
        get;
        set
        {
            if (value.Map != Map)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(
                    nameof(MapEntityActivationControlled),
                    $"The entity is not in the {value.Map.NamedId} map which is required");
            }

            if (value.GameId == GameId)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(
                    nameof(MapEntityActivationControlled),
                    "The entity controlled cannot be the entity itself");
            }

            InternalData[0] = value.GameId;
            field = value;
        }
    } = null!;

    public SwitchMapEntityActivationTriggerZoneMode ActivationMode
    {
        get => (SwitchMapEntityActivationTriggerZoneMode)InternalData[1];
        set => InternalData[1] = (int)value;
    }

    public bool DestroysBeemerangWhileInside { get => InternalData[2] == 1; set => InternalData[2] = value ? 1 : 0; }

    public Vector3 TriggerBoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }
    public Vector3 TriggerBoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }

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

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([0, 1, 0]);
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 3)
            InternalData.AddRange(Enumerable.Repeat(0, 3 - InternalData.Count));

        if (InternalData[0] != -1)
        {
            MapLeaf map = registryResolver.Resolve<MapLeaf>().LeavesByGameIds[Map.GameId];
            MapEntityActivationControlled = map.EntitiesRegistry.LeavesByGameIds[Math.Abs(InternalData[0])];
        }

        if (InternalActivationFlagId > 0)
        {
            ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
            ActivationFlag = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
        }
    }
}