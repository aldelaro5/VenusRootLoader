using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.Sprites;

internal sealed class EnemyPortraitsSpriteArrayPatcher : ISpriteArrayPatcher
{
    private readonly ILeavesRegistry<DiscoveryLeaf> _discoveriesRegistry;

    public EnemyPortraitsSpriteArrayPatcher(string[] subPaths, ILeavesRegistry<DiscoveryLeaf> discoveriesRegistry)
    {
        SubPaths = subPaths;
        _discoveriesRegistry = discoveriesRegistry;
    }

    public string[] SubPaths { get; }

    public Sprite[] PatchSpriteArray(string path, Sprite[] original)
    {
        SortedDictionary<int, Sprite> sprites = new();
        for (int i = 0; i < original.Length; i++)
        {
            Sprite sprite = original[i];
            sprites.Add(i, sprite);
        }

        ICollection<DiscoveryLeaf> allDiscoveries = _discoveriesRegistry
            .LeavesByNamedIds
            .Values;
        List<DiscoveryLeaf> discoveriesWithDefinedSpriteIndex = allDiscoveries
            .Where(l => l.EnemyPortraitsSpriteIndex is not null)
            .ToList();
        List<DiscoveryLeaf> discoveriesWithoutDefinedSpriteIndex = allDiscoveries
            .Except(discoveriesWithDefinedSpriteIndex)
            .ToList();

        foreach (DiscoveryLeaf leaf in discoveriesWithDefinedSpriteIndex) 
            sprites[leaf.EnemyPortraitsSpriteIndex!.Value] = leaf.WrappedSprite.Sprite;

        foreach (DiscoveryLeaf leaf in discoveriesWithoutDefinedSpriteIndex)
        {
            leaf.EnemyPortraitsSpriteIndex = sprites.Count;
            sprites.Add(leaf.EnemyPortraitsSpriteIndex.Value, leaf.WrappedSprite.Sprite);
        }

        return sprites.Values.ToArray();
    }
}