using Microsoft.Extensions.Logging;
using System.Reflection;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class ActionCommandsCollector : IBaseGameCollector
{
    private readonly string[] _actionCommandNamedIds = Enum.GetNames(typeof(BattleControl.ActionCommands)).ToArray();

    private readonly ILogger<ActionCommandsCollector> _logger;
    private readonly ILeavesRegistry<ActionCommandLeaf> _actionCommandsRegistry;

    public ActionCommandsCollector(
        ILogger<ActionCommandsCollector> logger,
        ILeavesRegistry<ActionCommandLeaf> actionCommandsRegistry)
    {
        _logger = logger;
        _actionCommandsRegistry = actionCommandsRegistry;
    }

    public void CollectBaseGameData()
    {
        Dictionary<string, MethodInfo> actionCommandsSetupHandlers = RootCollector.GetLeavesMethods(
            typeof(BattleControl),
            nameof(BattleControl.DoCommand),
            "Setup");
        Dictionary<string, MethodInfo> actionCommandsExecutionHandlers = RootCollector.GetLeavesMethods(
            typeof(BattleControl),
            nameof(BattleControl.DoCommand),
            "Execution");

        for (int i = 0; i < _actionCommandNamedIds.Length; i++)
        {
            string namedId = _actionCommandNamedIds[i];
            if (!actionCommandsSetupHandlers.TryGetValue(namedId, out MethodInfo methodSetup)
                || !actionCommandsExecutionHandlers.TryGetValue(namedId, out MethodInfo methodExecution))
            {
                continue;
            }

            ActionCommandLeaf actionCommandLeaf = _actionCommandsRegistry.RegisterExisting(i, namedId);

            actionCommandLeaf.SetupHandler =
                RootCollector.CreateOpenDelegateFromMethod<ActionCommandLeaf.ActionCommandSetupHandler>(methodSetup);
            actionCommandLeaf.ExecutionHandler =
                RootCollector.CreateOpenDelegateFromMethod<ActionCommandLeaf.ActionCommandExecutionHandler>(
                    methodExecution);
        }

        RootCollector.LogCollectedAmount(_logger, _actionCommandsRegistry, _actionCommandsRegistry.Count);
    }
}