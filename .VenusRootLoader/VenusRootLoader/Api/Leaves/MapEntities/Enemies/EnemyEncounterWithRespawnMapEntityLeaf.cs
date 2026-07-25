using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Enemies;

public sealed class EnemyEncounterWithRespawnMapEntityLeaf : EnemyEncounterMapEntityLeaf
{
    internal EnemyEncounterWithRespawnMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public int FramesAfterDeathBeforeRespawn
    {
        get => InternalEventId;
        set
        {
            Guard.IsGreaterThan(value, 0, nameof(FramesAfterDeathBeforeRespawn));
            InternalEventId = value;
        }
    }

    public bool IsStartingPositionSetToGroundBelowWhenMapIsLoaded
    {
        get => Modifiers.HasFlag(MapEntityModifiers.COG);
        set
        {
            if (value)
                Modifiers |= MapEntityModifiers.COG;
            else
                Modifiers &= ~MapEntityModifiers.COG;
        }
    }

    public bool ForceNotGroundedOnSpawn
    {
        get => Modifiers.HasFlag(MapEntityModifiers.NGS);
        set
        {
            if (value)
                Modifiers |= MapEntityModifiers.NGS;
            else
                Modifiers &= ~MapEntityModifiers.NGS;
        }
    }

    [MapEntityInitializeFromNew]
    internal override void InitializeFromNew(
        Vector3 startingPosition,
        Branch<AnimIdLeaf> animId,
        IList<Branch<EnemyLeaf>> enemiesFormationInBattle)
    {
        base.InitializeFromNew(startingPosition, animId, enemiesFormationInBattle);
        FramesAfterDeathBeforeRespawn = 30;
    }
}