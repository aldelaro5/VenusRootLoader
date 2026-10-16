using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Reflection;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class BattleEventDialoguesCollector : IBaseGameCollector
{
    private readonly ILogger<BattleEventDialogueLeaf> _logger;
    private readonly ILeavesRegistry<BattleEventDialogueLeaf> _battleEventDialoguesRegistry;

    public BattleEventDialoguesCollector(
        ILogger<BattleEventDialogueLeaf> logger,
        ILeavesRegistry<BattleEventDialogueLeaf> battleEventDialoguesRegistry)
    {
        _logger = logger;
        _battleEventDialoguesRegistry = battleEventDialoguesRegistry;
    }

    public void CollectBaseGameData()
    {
        Dictionary<int, MethodInfo> eventDialogueHandlers = RootCollector.GetLeavesMethods(
                typeof(BattleControl),
                nameof(BattleControl.EventDialogue))
            .ToDictionary(x => int.Parse(x.Key, CultureInfo.InvariantCulture), x => x.Value);

        foreach (int gameId in eventDialogueHandlers.Keys.OrderBy(x => x))
        {
            BattleEventDialogueLeaf listTypeLeaf = _battleEventDialoguesRegistry.RegisterExisting(
                gameId,
                gameId.ToString(CultureInfo.InvariantCulture));
            if (eventDialogueHandlers.TryGetValue(gameId, out MethodInfo method))
            {
                listTypeLeaf.Handler =
                    RootCollector.CreateOpenDelegateFromMethod<BattleEventDialogueLeaf.BattleEventDialogueHandler>(
                        method);
            }
        }

        RootCollector.LogCollectedAmount(_logger, _battleEventDialoguesRegistry, eventDialogueHandlers.Count);
    }
}