using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.Sprites;

internal sealed class EnemyPortraitsSpriteArrayPatcher : ISpriteArrayPatcher
{
    private readonly ILeavesRegistry<DiscoveryLeaf> _discoveriesRegistry;
    private readonly ILeavesRegistry<RecordLeaf> _recordsRegistry;

    public EnemyPortraitsSpriteArrayPatcher(
        string[] subPaths,
        ILeavesRegistry<DiscoveryLeaf> discoveriesRegistry,
        ILeavesRegistry<RecordLeaf> recordsRegistry)
    {
        SubPaths = subPaths;
        _discoveriesRegistry = discoveriesRegistry;
        _recordsRegistry = recordsRegistry;
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

        PatchSpritesFromRegistry(sprites, _discoveriesRegistry);
        PatchSpritesFromRegistry(sprites, _recordsRegistry);

        return sprites.Values.ToArray();
    }

    private void PatchSpritesFromRegistry<T>(SortedDictionary<int, Sprite> sprites, ILeavesRegistry<T> registry)
        where T : class, ILeaf, IEnemyPortraitSprite
    {
        ICollection<T> allDiscoveries = registry
            .LeavesByNamedIds
            .Values;
        List<T> discoveriesWithDefinedSpriteIndex = allDiscoveries
            .Where(l => l.EnemyPortraitsSpriteIndex is not null)
            .ToList();
        List<T> discoveriesWithoutDefinedSpriteIndex = allDiscoveries
            .Except(discoveriesWithDefinedSpriteIndex)
            .ToList();

        foreach (T leaf in discoveriesWithDefinedSpriteIndex)
            sprites[leaf.EnemyPortraitsSpriteIndex!.Value] = leaf.WrappedSprite.Sprite;

        foreach (T leaf in discoveriesWithoutDefinedSpriteIndex)
        {
            leaf.EnemyPortraitsSpriteIndex = sprites.Count;
            sprites.Add(leaf.EnemyPortraitsSpriteIndex.Value, leaf.WrappedSprite.Sprite);
        }
    }
}