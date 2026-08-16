using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity.AssetLoading;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class CuttableGrassCollector : IBaseGameCollector
{
    private const string SpritesObjectsGrassResourcesPath =
        $"{ResourcesPaths.RootSpritesPathPrefix}{ResourcesPaths.SpritesObjectsGrassPath}";

    private readonly int _amountCuttableGrass = Resources.LoadAll<Sprite>(SpritesObjectsGrassResourcesPath).Length / 3;

    private readonly ILogger<CuttableGrassLeaf> _logger;
    private readonly ILeavesRegistry<CuttableGrassLeaf> _leavesRegistry;

    public CuttableGrassCollector(ILogger<CuttableGrassLeaf> logger, ILeavesRegistry<CuttableGrassLeaf> leavesRegistry)
    {
        _logger = logger;
        _leavesRegistry = leavesRegistry;
    }

    public void CollectBaseGameData()
    {
        for (int i = 0; i < _amountCuttableGrass; i++)
        {
            CuttableGrassLeaf leaf = _leavesRegistry.RegisterExisting(i, i.ToString());
            leaf.GrassSpriteWhenUncut =
                new AssetLoaderFromResources<Sprite>(SpritesObjectsGrassResourcesPath, 0 + i * 3);
            leaf.BaseGrassSpriteWhenCut =
                new AssetLoaderFromResources<Sprite>(SpritesObjectsGrassResourcesPath, 1 + i * 3);
            leaf.GrassSpriteWhenCutFromBase =
                new AssetLoaderFromResources<Sprite>(SpritesObjectsGrassResourcesPath, 2 + i * 3);
        }

        RootCollector.LogCollectedAmount(_logger, _leavesRegistry, _amountCuttableGrass);
    }
}