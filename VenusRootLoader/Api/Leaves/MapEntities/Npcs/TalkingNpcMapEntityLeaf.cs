using CommunityToolkit.Diagnostics;
using System.Collections.ObjectModel;
using UnityEngine;
using VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class TalkingNpcMapEntityLeaf : MapEntityLeaf
{
    internal TalkingNpcMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
        Behaviors = new(this);
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.NPC;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.None;

    internal override NPCControl.Interaction Interaction => InteractIconIsQuestionMark
        ? NPCControl.Interaction.Check
        : NPCControl.Interaction.Talk;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }

    public Branch<AnimIdLeaf> AnimId
    {
        get;
        set
        {
            InternalAnimIdOrItemId = value.GameId;
            field = value;
        }
    }

    public ReadOnlyCollection<NpcConditionalDialogue> Dialogues { get; private set; } =
        new List<NpcConditionalDialogue>().AsReadOnly();

    // TODO: Change to be split with first element as the fallback and rest which must have a conditional flag
    public ReadOnlyCollection<NpcConditionalEmoticon> Emoticons { get; private set; } =
        new List<NpcConditionalEmoticon>().AsReadOnly();

    public float MovementRadius
    {
        get => InternalRadiusLimit;
        set => InternalRadiusLimit = value;
    }

    public float BehaviorAndInteractRangeRadius
    {
        get => InternalRadius;
        set => InternalRadius = value;
    }

    public MapEntityBehaviors Behaviors { get; }

    public bool InteractIconIsQuestionMark { get; set; }

    internal override void InitializeFromNew()
    {
        InternalAnimIdOrItemId = 0;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        InteractIconIsQuestionMark = OriginalInteraction == NPCControl.Interaction.Check;

        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        ILeavesRegistry<AnimIdLeaf> animIdsRegistry = registryResolver.Resolve<AnimIdLeaf>();
        ILeavesRegistry<CommonDialogueLeaf> commonDialoguesRegistry = registryResolver.Resolve<CommonDialogueLeaf>();

        Behaviors.InitializeBehaviorFromExisting(registryResolver);

        AnimId = new(animIdsRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);

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

        List<NpcConditionalEmoticon> emoticons = InternalEmoticonFlags
            .Select(emoticon => new NpcConditionalEmoticon
            {
                RequiredFlag = emoticon.x < 0 ? null : new(flagsRegistry.LeavesByGameIds[(int)emoticon.x]),
                EmoticonId = emoticon.y >= 0f ? (int)emoticon.y : null,
            })
            .ToList();
        ChangeEmoticons(emoticons);
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

    public void ChangeEmoticons(List<NpcConditionalEmoticon> emoticons)
    {
        InternalEmoticonFlags.Clear();

        foreach (NpcConditionalEmoticon emoticon in emoticons)
        {
            InternalEmoticonFlags.Add(
                new(emoticon.RequiredFlag?.GameId ?? -1, emoticon.EmoticonId ?? -1));
        }

        Emoticons = emoticons.AsReadOnly();
    }
}