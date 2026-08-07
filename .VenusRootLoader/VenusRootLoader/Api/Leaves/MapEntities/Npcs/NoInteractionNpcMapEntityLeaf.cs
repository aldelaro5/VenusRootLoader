using UnityEngine;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class NoInteractionNpcMapEntityLeaf : NpcMapEntityLeaf
{
    internal NoInteractionNpcMapEntityLeaf(int gameId, string creatorId, string namedId)
        : base(gameId, creatorId, namedId)
    {
    }

    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    [MapEntityInitializeFromNew]
    internal override void InitializeFromNew(Vector3 startingPosition, Branch<AnimIdLeaf>? animId)
    {
        base.InitializeFromNew(startingPosition, animId);
    }
}