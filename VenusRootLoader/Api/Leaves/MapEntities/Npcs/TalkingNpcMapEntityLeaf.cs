using CommunityToolkit.Diagnostics;
using System.Collections.ObjectModel;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class TalkingNpcMapEntityLeaf : NpcMapEntityLeaf
{
    internal TalkingNpcMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.Interaction Interaction => InteractIconIsQuestionMark
        ? NPCControl.Interaction.Check
        : NPCControl.Interaction.Talk;

    public Branch<DialogueLeaf>? SpyDialogue
    {
        get;
        set
        {
            InternalSpyDialogueId = value?.GameId ?? -1;
            field = value;
        }
    }

    public ReadOnlyCollection<NpcConditionalDialogue> Dialogues { get; private set; } =
        new List<NpcConditionalDialogue>().AsReadOnly();

    public bool InteractIconIsQuestionMark { get; set; }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        InteractIconIsQuestionMark = OriginalInteraction == NPCControl.Interaction.Check;
        
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        ILeavesRegistry<CommonDialogueLeaf> commonDialoguesRegistry = registryResolver.Resolve<CommonDialogueLeaf>();

        if (InternalSpyDialogueId != -1)
        {
            SpyDialogue = InternalSpyDialogueId < 0
                ? commonDialoguesRegistry.LeavesByGameIds[InternalSpyDialogueId]
                : Map.Leaf.DialoguesRegistry.LeavesByGameIds[InternalSpyDialogueId];
        }

        List<NpcConditionalDialogue> dialogues = InternalDialogues
            .Select(dialogue => new NpcConditionalDialogue
            {
                Flag = dialogue.x < 0 ? null : new(flagsRegistry.LeavesByGameIds[(int)dialogue.x]),
                Dialogue = (int)dialogue.y < 0
                    ? commonDialoguesRegistry.LeavesByGameIds[(int)dialogue.y]
                    : Map.Leaf.DialoguesRegistry.LeavesByGameIds[(int)dialogue.y],
                DefaultAnimStateWhenSelected = (int)dialogue.z
            })
            .ToList();
        ChangeDialogues(dialogues);
    }

    public void ChangeDialogues(List<NpcConditionalDialogue> dialogues)
    {
        List<string> badMapDialoguesNamedIds = dialogues
            .Where(x => x.Dialogue.Leaf.AssociatedMap is not null && x.Dialogue.Leaf.AssociatedMap != Map)
            .Select(x => x.Dialogue.NamedId)
            .ToList();
        if (badMapDialoguesNamedIds.Count > 0)
        {
            ThrowHelper.ThrowArgumentException(
                nameof(dialogues),
                $"The following map dialogues needs to be in the {Map.NamedId} map, but they are not: " +
                $"{string.Join(", ", badMapDialoguesNamedIds)}");
        }
        
        InternalDialogues.Clear();

        foreach (NpcConditionalDialogue dialogue in dialogues)
        {
            InternalDialogues.Add(
                new(dialogue.Flag?.GameId ?? -1, dialogue.Dialogue.GameId, dialogue.DefaultAnimStateWhenSelected));
        }

        Dialogues = dialogues.AsReadOnly();
    }
}