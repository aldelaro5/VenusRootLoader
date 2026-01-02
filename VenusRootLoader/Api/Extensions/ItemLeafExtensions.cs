using UnityEngine;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Api.Extensions;

public static class ItemLeafExtensions
{
    extension(ItemLeaf leaf)
    {
        public ItemLeaf WithBuyingPrice(int buyingPrice)
        {
            leaf.BuyingPrice = buyingPrice;
            return leaf;
        }

        public ItemLeaf WithEffect(MainManager.ItemUsage itemUsage, int value)
        {
            leaf.Effects.Add(
                new ItemLeaf.ItemUse
                {
                    Effect = itemUsage,
                    Value = value
                });
            return leaf;
        }

        public ItemLeaf WithTarget(BattleControl.AttackArea attackArea)
        {
            leaf.Target = attackArea;
            return leaf;
        }

        public ItemLeaf WithSprite(Sprite sprite)
        {
            leaf.WrappedSprite.Sprite = sprite;
            return leaf;
        }

        public ItemLeaf WithName(int languageId, string name)
        {
            ItemLeaf.ItemLanguageData itemLanguageData =
                ItemLeaf.GetOrCreateLanguageDataForLanguage(languageId, leaf.LanguageData);
            itemLanguageData.Name = name;
            return leaf;
        }

        public ItemLeaf WithDescription(int languageId, string description)
        {
            ItemLeaf.ItemLanguageData itemLanguageData =
                ItemLeaf.GetOrCreateLanguageDataForLanguage(languageId, leaf.LanguageData);
            itemLanguageData.Description = description;
            return leaf;
        }

        public ItemLeaf WithPrependerString(int languageId, string prepender)
        {
            ItemLeaf.ItemLanguageData itemLanguageData =
                ItemLeaf.GetOrCreateLanguageDataForLanguage(languageId, leaf.LanguageData);
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