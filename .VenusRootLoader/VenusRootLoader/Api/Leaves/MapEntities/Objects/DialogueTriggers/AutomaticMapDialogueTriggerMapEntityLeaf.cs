using CommunityToolkit.Diagnostics;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.DialogueTriggers;

public sealed class AutomaticMapDialogueTriggerMapEntityLeaf : DialogueTriggerMapEntityLeaf
{
    internal AutomaticMapDialogueTriggerMapEntityLeaf(int gameId, string creatorId, string namedId)
        : base(gameId, creatorId, namedId)
    {
    }

    public Branch<MapDialogueLeaf> MapDialogueLineIdToImmediatelyProcessOnMapLoad
    {
        get;
        set
        {
            if (value.Resolve().AssociatedMap != Map)
                ThrowHelper.ThrowInvalidOperationException($"This map dialogue must be in the {Map.NamedId} map");

            InternalData[0].Value = value.Resolve().GameId;
            field = value;
        }
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(Branch<MapDialogueLeaf> mapDialogueLineIdToImmediatelyProcessOnMapLoad)
    {
        InternalData.AddRange([new(-1), new(0), new(1)]);
        MapDialogueLineIdToImmediatelyProcessOnMapLoad = mapDialogueLineIdToImmediatelyProcessOnMapLoad;
        EntityStartingPosition = new(0f, -999f, 0f);
    }

    internal override void InitializeFromExisting()
    {
        MapDialogueLineIdToImmediatelyProcessOnMapLoad =
            Map.Resolve().DialoguesRegistry.GetByGameId(InternalData[0].Value);
    }
}