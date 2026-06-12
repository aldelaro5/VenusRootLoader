using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class TalkingNpcMapEntityLeaf : SpyableNpcMapEntityLeaf
{
    internal TalkingNpcMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
        _conditionalDialogues = new(InternalDialogues, 0, x => x.Vector3Ref);
    }

    internal override NPCControl.Interaction Interaction => InteractIconIsQuestionMark
        ? NPCControl.Interaction.Check
        : NPCControl.Interaction.Talk;

    private readonly ListRefWrapper<NpcConditionalDialogue, Vector3> _conditionalDialogues;
    public IList<NpcConditionalDialogue> ConditionalDialogues => _conditionalDialogues;

    public bool InteractIconIsQuestionMark { get; set; }

    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<AnimIdLeaf>? animIdLeaf,
        Branch<DialogueLeaf>? spyDialogue,
        IList<NpcConditionalDialogue> conditionalDialogues)
    {
        base.InitializeFromNew(startingPosition, animIdLeaf, spyDialogue);
        foreach (NpcConditionalDialogue conditionalDialogue in conditionalDialogues)
            ConditionalDialogues.Add(conditionalDialogue);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        InteractIconIsQuestionMark = OriginalInteraction == NPCControl.Interaction.Check;
        
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        ILeavesRegistry<CommonDialogueLeaf> commonDialoguesRegistry = registryResolver.Resolve<CommonDialogueLeaf>();

        _conditionalDialogues.SynchronizeFromExistingData(
            InternalDialogues
            .Select(dialogue => new NpcConditionalDialogue
            {
                Flag = dialogue.Value.x < 0 ? null : new(flagsRegistry.LeavesByGameIds[(int)dialogue.Value.x]),
                Dialogue = (int)dialogue.Value.y < 0
                    ? commonDialoguesRegistry.LeavesByGameIds[(int)dialogue.Value.y]
                    : Map.Leaf.DialoguesRegistry.LeavesByGameIds[(int)dialogue.Value.y],
                DefaultAnimStateWhenSelected = (int)dialogue.Value.z
            })
            .ToList());
    }
}