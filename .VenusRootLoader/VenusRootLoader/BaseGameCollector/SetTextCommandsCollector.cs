using Microsoft.Extensions.Logging;
using System.Reflection;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class SetTextCommandsCollector : IBaseGameCollector
{
    private readonly string[] _setTextCommandNamedIds = Enum.GetNames(typeof(MainManager.Commands)).ToArray();

    private readonly ILogger<SetTextCommandsCollector> _logger;
    private readonly ILeavesRegistry<SetTextCommandLeaf> _setTextCommandsRegistry;

    public SetTextCommandsCollector(
        ILogger<SetTextCommandsCollector> logger,
        ILeavesRegistry<SetTextCommandLeaf> setTextCommandsRegistry)
    {
        _logger = logger;
        _setTextCommandsRegistry = setTextCommandsRegistry;
    }

    public void CollectBaseGameData()
    {
        Dictionary<string, MethodInfo> setTextCommandsHandlers = RootCollector.GetLeavesMethods(
            typeof(MainManager),
            nameof(MainManager.SetText),
            "Command");

        for (int i = 0; i < _setTextCommandNamedIds.Length; i++)
        {
            SetTextCommandLeaf setTextCommandLeaf = _setTextCommandsRegistry.RegisterExisting(
                i,
                _setTextCommandNamedIds[i]);
            if (setTextCommandsHandlers.TryGetValue(setTextCommandLeaf.NamedId, out MethodInfo method))
            {
                setTextCommandLeaf.Handler =
                    RootCollector.CreateOpenDelegateFromMethod<SetTextCommandLeaf.SetTextCommandHandler>(method);
            }
        }

        RootCollector.LogCollectedAmount(_logger, _setTextCommandsRegistry, _setTextCommandNamedIds.Length);
    }
}