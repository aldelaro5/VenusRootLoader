using UnityEngine;
using VenusRootLoader.Api.Unity;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class CuttableGrassLeaf : Leaf
{
    internal CuttableGrassLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    public IAssetLoader<Sprite> GrassSpriteWhenUncut { get; set; } = null!;
    public IAssetLoader<Sprite> BaseGrassSpriteWhenCut { get; set; } = null!;
    public IAssetLoader<Sprite> GrassSpriteWhenCutFromBase { get; set; } = null!;

    [LeafInitializeFromNew]
    internal void InitializeFromNew(
        IAssetLoader<Sprite> grassSpriteWhenUncut,
        IAssetLoader<Sprite> baseGrassSpriteWhenCut,
        IAssetLoader<Sprite> cutGrassSpriteFromBase)
    {
        GrassSpriteWhenUncut = grassSpriteWhenUncut;
        BaseGrassSpriteWhenCut = baseGrassSpriteWhenCut;
        GrassSpriteWhenCutFromBase = cutGrassSpriteFromBase;
    }
}