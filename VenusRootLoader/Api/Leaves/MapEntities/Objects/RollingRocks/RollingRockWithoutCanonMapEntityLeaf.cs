namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.RollingRocks;

public enum RollingRockScheme
{
    RollOnSpawnWithoutImpactEffect = 0,
    RollWhenHittingGroundWithImpactEffect = 1
}

public sealed class RollingRockWithoutCanonMapEntityLeaf : RollingRockMapEntityLeaf
{
    internal RollingRockWithoutCanonMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public RollingRockScheme RollingScheme
    {
        get => (RollingRockScheme)InternalData[0].Value;
        set => InternalData[0].Value = (int)value;
    }

    internal override void InitializeFromNew()
    {
        base.InitializeFromNew();
        InternalData.AddRange([new(1), new(0), new(0), new(-1)]);
        InternalVectorData.AddRange([new(new(10f, 0f, 0f)), new(new(-10f, 0f, 0f)), new(new(0f, 0f, 5f))]);
    }
}