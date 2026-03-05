using Microsoft.Extensions.Logging;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity;
using VenusRootLoader.Unity.CustomAudioClip;
using Venus = VenusRootLoader.Api.Venus;

namespace VenusRootLoader.BudLoading;

internal interface IVenusFactory
{
    Venus CreateVenusForBud(string budId);
}

internal sealed class VenusFactory : IVenusFactory
{
    private readonly IRegistryResolver _registryResolver;
    private readonly IGlobalMonoBehaviourExecution _globalMonoBehaviourExecution;
    private readonly ICustomAudioClipProvider _customAudioClipProvider;
    private readonly ILogger<Venus> _logger;

    public VenusFactory(
        IRegistryResolver registryResolver,
        IGlobalMonoBehaviourExecution globalMonoBehaviourExecution,
        ICustomAudioClipProvider customAudioClipProvider,
        ILogger<Venus> logger)
    {
        _registryResolver = registryResolver;
        _globalMonoBehaviourExecution = globalMonoBehaviourExecution;
        _customAudioClipProvider = customAudioClipProvider;
        _logger = logger;
    }

    public Venus CreateVenusForBud(string budId) => new(
        budId,
        _registryResolver,
        _globalMonoBehaviourExecution,
        _customAudioClipProvider,
        _logger);
}