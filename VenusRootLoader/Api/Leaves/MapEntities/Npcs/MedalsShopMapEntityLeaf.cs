using CommunityToolkit.Diagnostics;
using System.Collections.ObjectModel;
using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class MedalsShopMapEntityLeaf : NpcMapEntityLeaf
{
    internal MedalsShopMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.Shop;

    public Branch<DialogueLeaf> DialogueWhenInteractingWithShopKeeper
    {
        get;
        set
        {
            if (value.Leaf.AssociatedMap is not null && value.Leaf.AssociatedMap != Map)
                ThrowHelper.ThrowInvalidOperationException($"This map dialogue must be in the {Map.NamedId} map");

            InternalDialogues[0] = new(InternalDialogues[0].x, value.GameId, InternalDialogues[0].z);
            field = value;
        }
    }

    public bool OnlyAcceptsCrystalBerries
    {
        get => Mathf.Approximately(InternalDialogues[1].y, 1f);
        set => InternalDialogues[1] = new(InternalDialogues[1].x, value ? 1f : 0f, InternalDialogues[1].z);
    }

    public Branch<DialogueLeaf> DialogueWhenInteractingWithShelvedItem
    {
        get;
        set
        {
            if (value.Leaf.AssociatedMap is not null && value.Leaf.AssociatedMap != Map)
                ThrowHelper.ThrowInvalidOperationException($"This map dialogue must be in the {Map.NamedId} map");

            InternalDialogues[6] = new(InternalDialogues[6].x, value.GameId, InternalDialogues[6].z);
            field = value;
        }
    }

    public float? ShelvedItemsInteractionRadius
    {
        get => InternalDialogues[8].x > 0.1f ? InternalDialogues[8].x / 10f : null;
        set => InternalDialogues[8] = new(value * 10f ?? 0f, InternalDialogues[8].y, InternalDialogues[8].z);
    }

    public Branch<MedalShopLeaf> AssociatedMedalsShop
    {
        get;
        set
        {
            InternalDialogues[9] = new(value.GameId, InternalDialogues[9].y, InternalDialogues[9].z);
            field = value;
        }
    }

    public Branch<DialogueLeaf>? SpyDialogue
    {
        get;
        set
        {
            InternalSpyDialogueId = value?.GameId ?? -1;
            field = value;
        }
    }

    public ReadOnlyCollection<Vector3> ShelvedMedalPositions { get; private set; } =
        new List<Vector3>().AsReadOnly();

    internal override void InitializeFromNew()
    {
        base.InitializeFromNew();
        InternalDialogues.AddRange(Enumerable.Repeat(Vector3.zero, 10));
        InternalDialogues.Add(new(1f, 0f, 0f));
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<MedalShopLeaf> medalShopsRegistry = registryResolver.Resolve<MedalShopLeaf>();
        ILeavesRegistry<CommonDialogueLeaf> commonDialoguesRegistry = registryResolver.Resolve<CommonDialogueLeaf>();

        if (InternalSpyDialogueId != -1)
        {
            SpyDialogue = InternalSpyDialogueId < 0
                ? commonDialoguesRegistry.LeavesByGameIds[InternalSpyDialogueId]
                : Map.Leaf.DialoguesRegistry.LeavesByGameIds[InternalSpyDialogueId];
        }

        AssociatedMedalsShop = new(medalShopsRegistry.LeavesByGameIds[(int)InternalDialogues[9].x]);

        List<Vector3> shelvedMedalPositions = InternalVectorData.ToList();
        ChangeShelvedMedalPositions(shelvedMedalPositions);

        DialogueWhenInteractingWithShopKeeper = (int)InternalDialogues[0].y < 0
            ? commonDialoguesRegistry.LeavesByGameIds[(int)InternalDialogues[0].y]
            : Map.Leaf.DialoguesRegistry.LeavesByGameIds[(int)InternalDialogues[0].y];
        DialogueWhenInteractingWithShelvedItem = (int)InternalDialogues[6].y < 0
            ? commonDialoguesRegistry.LeavesByGameIds[(int)InternalDialogues[6].y]
            : Map.Leaf.DialoguesRegistry.LeavesByGameIds[(int)InternalDialogues[6].y];
    }

    public void ChangeShelvedMedalPositions(List<Vector3> shelvedMedalPositions)
    {
        InternalVectorData.Clear();
        foreach (Vector3 shelvedMedalPosition in shelvedMedalPositions)
        {
            InternalVectorData.Add(shelvedMedalPosition);
        }

        ShelvedMedalPositions = shelvedMedalPositions.AsReadOnly();
    }
}