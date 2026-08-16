using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.SpritesPatchers;

internal sealed class GrassSpritesArrayPatcher : ISpriteArrayPatcher
{
    public string[] SubPaths { get; }

    private readonly ILeavesRegistry<CuttableGrassLeaf> _cuttableGrassRegistry;

    public GrassSpritesArrayPatcher(string[] subPaths, ILeavesRegistry<CuttableGrassLeaf> cuttableGrassRegistry)
    {
        SubPaths = subPaths;
        _cuttableGrassRegistry = cuttableGrassRegistry;
    }

    public Sprite[] PatchSpriteArray(string path, Sprite[] original)
    {
        List<Sprite> grassSprites = new();

        foreach (CuttableGrassLeaf leaf in _cuttableGrassRegistry)
        {
            grassSprites.Add(leaf.GrassSpriteWhenUncut.LoadAsset());
            grassSprites.Add(leaf.BaseGrassSpriteWhenCut.LoadAsset());
            grassSprites.Add(leaf.GrassSpriteWhenCutFromBase.LoadAsset());
        }

        return grassSprites.ToArray();
    }
}