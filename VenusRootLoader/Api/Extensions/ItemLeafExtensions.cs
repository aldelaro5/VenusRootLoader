using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.TextAssetData;

namespace VenusRootLoader.Api.Extensions;

public static class ItemLeafExtensions
{
    extension(ItemLeaf leaf)
    {
        public ItemLeaf WithBuyingPrice(int buyingPrice)
        {
            leaf.ItemData.BuyingPrice = buyingPrice;
            return leaf;
        }

        public ItemLeaf WithEffect(MainManager.ItemUsage itemUsage, int value)
        {
            leaf.ItemData.Effects.Add(
                new()
                {
                    UseType = itemUsage,
                    Value = value
                });
            return leaf;
        }

        public ItemLeaf WithTarget(BattleControl.AttackArea attackArea)
        {
            leaf.ItemData.Target = attackArea;
            return leaf;
        }

        public ItemLeaf WithSprite(Sprite sprite)
        {
            leaf.ItemSprite.Sprite = sprite;
            return leaf;
        }

        public ItemLeaf WithName(int languageId, string name)
        {
            ItemLanguageData itemLanguageData =
                ItemLeaf.GetOrCreateLanguageDataForLanguage(languageId, leaf.ItemLanguageData);
            itemLanguageData.Name = name;
            return leaf;
        }

        public ItemLeaf WithDescription(int languageId, string description)
        {
            ItemLanguageData itemLanguageData =
                ItemLeaf.GetOrCreateLanguageDataForLanguage(languageId, leaf.ItemLanguageData);
            itemLanguageData.Description = description;
            return leaf;
        }

        public ItemLeaf WithPrependerString(int languageId, string prepender)
        {
            ItemLanguageData itemLanguageData =
                ItemLeaf.GetOrCreateLanguageDataForLanguage(languageId, leaf.ItemLanguageData);
            itemLanguageData.Prepender = prepender;
            return leaf;
        }

        private static T GetOrCreateLanguageDataForLanguage<T>(int languageId, Dictionary<int, T> languageData)
            where T : new()
        {
            if (languageData.TryGetValue(languageId, out T? itemLanguageData))
                return itemLanguageData;

            itemLanguageData = new();
            languageData[languageId] = itemLanguageData;
            return itemLanguageData;
        }
    }
}