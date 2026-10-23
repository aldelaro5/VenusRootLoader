using MonoDetour.DetourTypes;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic.LeavesSupport;

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

    private readonly ILeavesRegistry<ItemLeaf> _itemLeafRegistry;
    private readonly ILeavesRegistry<MedalLeaf> _medalLeafRegistry;

    public ItemAndMedalSpriteTopLevelPatcher(
        ILeavesRegistry<ItemLeaf> itemLeafRegistry,
        ILeavesRegistry<MedalLeaf> medalLeafRegistry)
    {
        _instance = this;
        _itemLeafRegistry = itemLeafRegistry;
        _medalLeafRegistry = medalLeafRegistry;
    }

    public void Patch()
    {
        Md.MainManager.LoadItemSprites.ControlFlowPrefix(InsertCustomItemSpriteHandler);
    }

    private static ReturnFlow InsertCustomItemSpriteHandler()
    {
        Sprite[] itemsSprites = _instance._itemLeafRegistry
            .Select(i => i.Sprite.LoadAsset())
            .ToArray();
        Sprite[] medalsSprites = _instance._medalLeafRegistry
            .Select(i => i.Sprite.LoadAsset())
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

        return ReturnFlow.SkipOriginal;
    }
}