using CommunityToolkit.Diagnostics;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.DialogueTriggers;

public sealed class AutomaticMapDialogueTriggerMapEntityLeaf : DialogueTriggerMapEntityLeaf
{
    internal AutomaticMapDialogueTriggerMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public Branch<MapDialogueLeaf> MapDialogueLineIdToImmediatelyProcessOnMapLoad
    {
        get;
        set
        {
            if (value.Leaf.AssociatedMap != Map)
                ThrowHelper.ThrowInvalidOperationException($"This map dialogue must be in the {Map.NamedId} map");

            InternalData[0].Value = value.GameId;
            field = value;
        }
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([new(-1), new(0), new(1)]);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        MapDialogueLineIdToImmediatelyProcessOnMapLoad =
            Map.Leaf.DialoguesRegistry.LeavesByGameIds[InternalData[0].Value];
    }
}