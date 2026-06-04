using CommunityToolkit.Diagnostics;

namespace VenusRootLoader.Api.Leaves.MapEntities.Enemies;

public sealed class EnemyEncounterRespawnableMapEntityLeaf : EnemyMapEntityLeaf
{
    internal EnemyEncounterRespawnableMapEntityLeaf(int gameId, string namedId, string creatorId)
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

    internal override void InitializeFromNew()
    {
        base.InitializeFromNew();
        InternalEventId = 30;
    }
}