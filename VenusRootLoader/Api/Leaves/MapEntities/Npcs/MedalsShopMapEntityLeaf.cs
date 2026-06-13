using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class MedalsShopMapEntityLeaf : SpyableNpcMapEntityLeaf
{
    internal MedalsShopMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
        _shelvedMedalPositions = new(InternalVectorData, 0, x => new(x));
    }

    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.Shop;

    public Branch<MedalShopLeaf> AssociatedMedalsShop
    {
        get;
        set
        {
            InternalDialogues[9].Value.x = value.GameId;
            field = value;
        }
    }

    public Branch<DialogueLeaf> DialogueWhenInteractingWithShopKeeper
    {
        get;
        set
        {
            if (value.Leaf.AssociatedMap is not null && value.Leaf.AssociatedMap != Map)
                ThrowHelper.ThrowInvalidOperationException($"This map dialogue must be in the {Map.NamedId} map");

            InternalDialogues[0].Value.y = value.GameId;
            field = value;
        }
    }

    public bool OnlyAcceptsCrystalBerries
    {
        get => Mathf.Approximately(InternalDialogues[1].Value.y, 1f);
        set => InternalDialogues[1].Value.y = value ? 1f : 0f;
    }

    public Branch<DialogueLeaf> DialogueWhenInteractingWithShelvedMedal
    {
        get;
        set
        {
            if (value.Leaf.AssociatedMap is not null && value.Leaf.AssociatedMap != Map)
                ThrowHelper.ThrowInvalidOperationException($"This map dialogue must be in the {Map.NamedId} map");

            InternalDialogues[6].Value.y = value.GameId;
            field = value;
        }
    }

    public float? ShelvedMedalsInteractionRadius
    {
        get => InternalDialogues[8].Value.x > 0.1f ? InternalDialogues[8].Value.x / 10f : null;
        set => InternalDialogues[8].Value.x = value * 10f ?? 0f;
    }

    private readonly ListRefWrapper<Vector3, Vector3> _shelvedMedalPositions;
    public IList<Vector3> ShelvedMedalPositions => _shelvedMedalPositions;

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<AnimIdLeaf>? animIdLeaf,
        Branch<DialogueLeaf>? spyDialogue,
        Branch<MedalShopLeaf> associatedMedalsShop,
        Branch<DialogueLeaf> dialogueWhenInteractingWithShopKeeper,
        Branch<DialogueLeaf> dialogueWhenInteractingWithShelvedMedal,
        IList<Vector3> shelvedMedalPositions)
    {
        base.InitializeFromNew(startingPosition, animIdLeaf, spyDialogue);
        InternalDialogues.AddRange(Enumerable.Repeat(new Ref<Vector3>(Vector3.zero), 10));
        InternalDialogues.Add(new(new(1f, 0f, 0f)));
        AssociatedMedalsShop = associatedMedalsShop;
        DialogueWhenInteractingWithShopKeeper = dialogueWhenInteractingWithShopKeeper;
        DialogueWhenInteractingWithShelvedMedal = dialogueWhenInteractingWithShelvedMedal;
        foreach (Vector3 shelvedMedalPosition in shelvedMedalPositions)
            ShelvedMedalPositions.Add(shelvedMedalPosition);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<MedalShopLeaf> medalShopsRegistry = registryResolver.Resolve<MedalShopLeaf>();
        ILeavesRegistry<CommonDialogueLeaf> commonDialoguesRegistry = registryResolver.Resolve<CommonDialogueLeaf>();

        _shelvedMedalPositions.SynchronizeFromExistingData(InternalVectorData.Select(x => x.Value).ToList());
        AssociatedMedalsShop = new(medalShopsRegistry.LeavesByGameIds[(int)InternalDialogues[9].Value.x]);

        DialogueWhenInteractingWithShopKeeper = (int)InternalDialogues[0].Value.y < 0
            ? commonDialoguesRegistry.LeavesByGameIds[(int)InternalDialogues[0].Value.y]
            : Map.Leaf.DialoguesRegistry.LeavesByGameIds[(int)InternalDialogues[0].Value.y];
        DialogueWhenInteractingWithShelvedMedal = (int)InternalDialogues[6].Value.y < 0
            ? commonDialoguesRegistry.LeavesByGameIds[(int)InternalDialogues[6].Value.y]
            : Map.Leaf.DialoguesRegistry.LeavesByGameIds[(int)InternalDialogues[6].Value.y];
    }
}