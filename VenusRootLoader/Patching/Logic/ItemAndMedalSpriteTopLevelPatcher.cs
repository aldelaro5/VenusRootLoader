using HarmonyLib;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

/// <summary>
/// This patcher adds custom sprites support for <see cref="ItemLeaf"/> and <see cref="MedalLeaf"/>.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="MainManager.LoadItemSprites"/>: Replaces the whole method so <see cref="MainManager.itemsprites"/>
/// reflects the information from the <see cref="ItemLeaf"/> and <see cref="MedalLeaf"/> registries.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class ItemAndMedalSpriteTopLevelPatcher : ITopLevelPatcher
{
    // ReSharper disable once UnusedMember.Local
    private enum SpriteKind
    {
        Item,
        Medal
    }

    private static ItemAndMedalSpriteTopLevelPatcher _instance = null!;

    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<ItemLeaf> _itemLeafRegistry;
    private readonly ILeavesRegistry<MedalLeaf> _medalLeafRegistry;

    public ItemAndMedalSpriteTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<ItemLeaf> itemLeafRegistry,
        ILeavesRegistry<MedalLeaf> medalLeafRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _itemLeafRegistry = itemLeafRegistry;
        _medalLeafRegistry = medalLeafRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(ItemAndMedalSpriteTopLevelPatcher));

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.LoadItemSprites))]
    private static bool InsertCustomItemSpriteHandler()
    {
        Sprite[] itemsSprites = _instance._itemLeafRegistry.LeavesByNamedIds.Values
            .Select(i => i.Sprite)
            .ToArray();
        Sprite[] medalsSprites = _instance._medalLeafRegistry.LeavesByNamedIds.Values
            .Select(i => i.Sprite)
            .ToArray();

        MainManager.itemsprites = new Sprite[2, itemsSprites.Length + medalsSprites.Length];
        for (int i = 0; i < itemsSprites.Length; i++)
        {
            Sprite sprite = itemsSprites[i];
            MainManager.itemsprites[(int)SpriteKind.Item, i] = sprite;
        }

        for (int i = 0; i < medalsSprites.Length; i++)
        {
            Sprite sprite = medalsSprites[i];
            MainManager.itemsprites[(int)SpriteKind.Medal, i] = sprite;
        }

        return false;
    }
}