using Microsoft.Extensions.Logging;
using System.Reflection;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class ItemUsagesCollector : IBaseGameCollector
{
    private readonly string[] _itemUsagesNamedIds = Enum.GetNames(typeof(MainManager.ItemUsage)).ToArray();

    private readonly ILogger<ItemUsagesCollector> _logger;
    private readonly ILeavesRegistry<ItemUsageLeaf> _itemUsagesRegistry;

    public ItemUsagesCollector(
        ILogger<ItemUsagesCollector> logger,
        ILeavesRegistry<ItemUsageLeaf> itemUsagesRegistry)
    {
        _logger = logger;
        _itemUsagesRegistry = itemUsagesRegistry;
    }

    public void CollectBaseGameData()
    {
        Dictionary<string, MethodInfo> doItemEffectHandlers = RootCollector.GetLeavesMethods(
            typeof(MainManager),
            nameof(MainManager.DoItemEffect));
        Dictionary<string, MethodInfo> battleEffectHandlers = RootCollector.GetLeavesMethods(
            typeof(BattleControl),
            nameof(BattleControl.UseItem),
            "BattleEffect");

        for (int i = 0; i < _itemUsagesNamedIds.Length; i++)
        {
            ItemUsageLeaf itemUsageLeaf = _itemUsagesRegistry.RegisterExisting(
                i,
                _itemUsagesNamedIds[i]);
            if (doItemEffectHandlers.TryGetValue(itemUsageLeaf.NamedId, out MethodInfo methodUsage))
            {
                itemUsageLeaf.UsageHandler =
                    RootCollector.CreateOpenDelegateFromMethod<ItemUsageLeaf.ItemUsageHandler>(methodUsage);
            }

            if (battleEffectHandlers.TryGetValue(itemUsageLeaf.NamedId, out MethodInfo methodBattleUsage))
            {
                itemUsageLeaf.BattleUsageHandler =
                    RootCollector.CreateOpenDelegateFromMethod<ItemUsageLeaf.BattleItemUsageHandler>(methodBattleUsage);
            }
        }

        RootCollector.LogCollectedAmount(_logger, _itemUsagesRegistry, _itemUsagesNamedIds.Length);
    }
}