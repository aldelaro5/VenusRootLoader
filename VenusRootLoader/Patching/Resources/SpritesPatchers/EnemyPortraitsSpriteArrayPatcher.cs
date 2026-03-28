using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using Object = UnityEngine.Object;

namespace VenusRootLoader.Patching.Resources.SpritesPatchers;

/// <summary>
/// An <see cref="ISpriteArrayPatcher"/> that handles patching the EnemyPortraits sprites at <c>Sprites/Items/EnemyPortraits</c>.
/// This sprite array is special because it has its contents used by 4 different <see cref="ILeavesRegistry{TLeaf}"/>
/// and each of them references them in a similar way. This is problematic for modding because we want to let buds edit
/// existing sprites or assign new ones for their custom <see cref="Leaf"/>.
/// <p>
/// In order to address this issue, this patcher will reindex the entire sprite array. It does this by ensuring all base
/// game sprites have the same indexes in the array, but every new ones will get their index reserved in an arbitrary order.
/// </p>
/// <p>
/// Additionally, the patcher will clone duplicated sprite references among the 4 registries such that each <see cref="Leaf"/>
/// references its own sprite. This avoids unwanted side effects when editing a sprite of a leaf.
/// </p>
/// <p>
/// This reindexing will cause differences in the TextAssets where these indexes appear in, but this is expected and a
/// necessary side effect to achieve this. The goal is to maintain semantic equivalence with the game.
/// </p>
/// </summary>
internal sealed class EnemyPortraitsSpriteArrayPatcher : ISpriteArrayPatcher
{
    private readonly ILeavesRegistry<DiscoveryLeaf> _discoveriesRegistry;
    private readonly ILeavesRegistry<EnemyLeaf> _enemiesRegistry;
    private readonly ILeavesRegistry<RecordLeaf> _recordsRegistry;
    private readonly ILeavesRegistry<QuestLeaf> _questsRegistry;

    public EnemyPortraitsSpriteArrayPatcher(
        string[] subPaths,
        ILeavesRegistry<DiscoveryLeaf> discoveriesRegistry,
        ILeavesRegistry<EnemyLeaf> enemiesRegistry,
        ILeavesRegistry<RecordLeaf> recordsRegistry,
        ILeavesRegistry<QuestLeaf> questsRegistry)
    {
        SubPaths = subPaths;
        _discoveriesRegistry = discoveriesRegistry;
        _enemiesRegistry = enemiesRegistry;
        _recordsRegistry = recordsRegistry;
        _questsRegistry = questsRegistry;
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

        CloneSpriteDuplicates();
        PatchSpritesFromRegistry(sprites, _discoveriesRegistry);
        PatchSpritesFromRegistry(sprites, _enemiesRegistry);
        PatchSpritesFromRegistry(sprites, _recordsRegistry);
        PatchSpritesFromRegistry(sprites, _questsRegistry);

        return sprites.Values.ToArray();
    }

    // In order to clone duplicated references, we simply set the indexes of the duplicated ones to null. This will
    // cause the patcher to act on them the same way then if they were new sprites of a custom leaf so they will automatically
    // get reassigned to a new index.
    private void CloneSpriteDuplicates()
    {
        IEnumerable<IEnemyPortraitSprite> discoveryLeaves = _discoveriesRegistry
            .LeavesByNamedIds
            .Values;
        IEnumerable<IEnemyPortraitSprite> enemiesLeaves = _enemiesRegistry
            .LeavesByNamedIds
            .Values;
        IEnumerable<IEnemyPortraitSprite> recordsLeaves = _recordsRegistry
            .LeavesByNamedIds
            .Values;
        IEnumerable<IEnemyPortraitSprite> questsLeaves = _questsRegistry
            .LeavesByNamedIds
            .Values;
        List<IEnemyPortraitSprite> allLeavesWithPortraitSprite =
            discoveryLeaves
            .Concat(enemiesLeaves)
            .Concat(recordsLeaves)
            .Concat(questsLeaves)
            .Where(l => l.EnemyPortraitsSpriteIndex is not null)
            .ToList();

        HashSet<int> uniqueSpriteIndexes = new();
        foreach (IEnemyPortraitSprite? leaf in allLeavesWithPortraitSprite)
        {
            bool isUniqueSpriteIndex = uniqueSpriteIndexes.Add(leaf.EnemyPortraitsSpriteIndex!.Value);
            if (isUniqueSpriteIndex)
                continue;

            leaf.WrappedSprite.Sprite = Object.Instantiate(leaf.WrappedSprite.Sprite);
            leaf.EnemyPortraitsSpriteIndex = null;
        }
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