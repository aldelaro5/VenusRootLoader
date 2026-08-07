using UnityEngine;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.AndBlocks;

// TODO: Merge with the multi flags one later with a patch to fix its problems
public sealed class AndBlockOnSingleFlagMapEntityLeaf : AndBlockMapEntityLeaf
{
    internal AndBlockOnSingleFlagMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public NegatableFlag FlagInput
    {
        get;
        set
        {
            InternalActivationFlagId = value.EffectiveValue;
            field = value;
        }
    } = null!;

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<AnimIdLeaf> animId,
        NegatableFlag flagInput)
    {
        base.InitializeFromNew(startingPosition, animId);
        InternalData.AddRange([new(0), new(-1)]);
        FlagInput = flagInput;
    }

    internal override void InitializeFromExisting()
    {
        base.InitializeFromExisting();
        ILeavesRegistry<FlagLeaf> flagsRegistry = RegistryResolver.Resolve<FlagLeaf>();
        FlagInput = new()
        {
            Flag = new(flagsRegistry.GetByGameId(Math.Abs(InternalActivationFlagId))),
            IsValueNegated = InternalActivationFlagId < 0
        };
    }
}