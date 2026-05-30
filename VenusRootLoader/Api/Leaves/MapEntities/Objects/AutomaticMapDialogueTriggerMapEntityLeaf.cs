using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class AutomaticMapDialogueTriggerMapEntityLeaf : MapEntityLeaf
{
    internal AutomaticMapDialogueTriggerMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.DialogueTrigger;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public int MapDialogueLineIdToImmediatelyProcess
    {
        get => InternalData[0];
        set => InternalData[0] = value;
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([-1, 0, 1]);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver) { }
}