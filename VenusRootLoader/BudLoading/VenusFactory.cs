using Microsoft.Extensions.Logging;
using VenusRootLoader.Api;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity;
using VenusRootLoader.Unity.CustomAudioClip;
using Venus = VenusRootLoader.Api.Venus;

namespace VenusRootLoader.BudLoading;

/// <summary>
/// A service to create <see cref="Venus"/> instances tailored to each <see cref="Bud"/>.
/// </summary>
internal interface IVenusFactory
{
    /// <summary>
    /// Creates a <see cref="Venus"/> instance tailored to a <see cref="Bud"/>.
    /// </summary>
    /// <param name="budId">The <see cref="Bud"/>'s unique identifier.</param>
    /// <returns>The <see cref="Venus"/> instance.</returns>
    Venus CreateVenusForBud(string budId);
}

/// <inheritdoc/>
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