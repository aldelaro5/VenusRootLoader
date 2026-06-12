using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.AndBlocks;

public sealed class AndBlockOnFlagsMapEntityLeaf : AndBlockMapEntityLeaf
{
    internal AndBlockOnFlagsMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
        _flagInputs = new(InternalData, 1, x => new(x.GameId));
    }

    private readonly ListRefWrapper<Branch<FlagLeaf>, int> _flagInputs;
    public IList<Branch<FlagLeaf>> FlagInputs => _flagInputs;

    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<AnimIdLeaf>? animId,
        IList<Branch<FlagLeaf>> flagInputs)
    {
        base.InitializeFromNew(startingPosition, animId);
        InternalData.AddRange([new(-2)]);
        foreach (Branch<FlagLeaf> flagInput in flagInputs)
            FlagInputs.Add(flagInput);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();

        _flagInputs.SynchronizeFromExistingData(
            InternalData
                .Skip(1)
                .Select(x => new Branch<FlagLeaf>(flagsRegistry.LeavesByGameIds[Math.Abs(x.Value)]))
                .ToList());
    }
}