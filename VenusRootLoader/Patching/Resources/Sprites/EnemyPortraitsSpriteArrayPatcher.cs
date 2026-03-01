using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.Sprites;

internal sealed class EnemyPortraitsSpriteArrayPatcher : ISpriteArrayPatcher
{
    private readonly ILeavesRegistry<DiscoveryLeaf> _discoveriesRegistry;
    private readonly ILeavesRegistry<EnemyLeaf> _enemiesRegistry;
    private readonly ILeavesRegistry<RecordLeaf> _recordsRegistry;

    public EnemyPortraitsSpriteArrayPatcher(
        string[] subPaths,
        ILeavesRegistry<DiscoveryLeaf> discoveriesRegistry,
        ILeavesRegistry<EnemyLeaf> enemiesRegistry,
        ILeavesRegistry<RecordLeaf> recordsRegistry)
    {
        SubPaths = subPaths;
        _discoveriesRegistry = discoveriesRegistry;
        _enemiesRegistry = enemiesRegistry;
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
        PatchSpritesFromRegistry(sprites, _enemiesRegistry);
        PatchSpritesFromRegistry(sprites, _recordsRegistry);

        return sprites.Values.ToArray();
    }

    private void PatchSpritesFromRegistry<T>(SortedDictionary<int, Sprite> sprites, ILeavesRegistry<T> registry)
        where T : Leaf, IEnemyPortraitSprite
    {
        ICollection<T> leaves = registry
            .LeavesByNamedIds
            .Values;
        List<T> leavesWithDefinedSprites = leaves
            .Where(l => l.EnemyPortraitsSpriteIndex is not null)
            .ToList();
        List<T> leavesWithoutDefinedSprites = leaves
            .Except(leavesWithDefinedSprites)
            .ToList();

        foreach (T leaf in leavesWithDefinedSprites)
            sprites[leaf.EnemyPortraitsSpriteIndex!.Value] = leaf.WrappedSprite.Sprite!;

        foreach (T leaf in leavesWithoutDefinedSprites)
        {
            leaf.EnemyPortraitsSpriteIndex = sprites.Count;
            sprites.Add(leaf.EnemyPortraitsSpriteIndex.Value, leaf.WrappedSprite.Sprite!);
        }
    }
}