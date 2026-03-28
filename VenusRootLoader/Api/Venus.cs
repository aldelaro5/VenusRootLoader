using Microsoft.Extensions.Logging;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity;
using VenusRootLoader.Unity.CustomAudioClip;

// ReSharper disable UnusedMember.Global

namespace VenusRootLoader.Api;

/// <summary>
/// A class that allows each <see cref="Bud"/> to access key <see cref="VenusRootLoader"/> APIs. Each instance is tailored
/// to each specific bud for tracking purposes.
/// </summary>
public sealed partial class Venus
{
    internal readonly string BudId;
    internal readonly IRegistryResolver RegistryResolver;
    internal readonly IGlobalMonoBehaviourExecution GlobalMonoBehaviourExecution;
    internal readonly ICustomAudioClipProvider CustomAudioClipProvider;
    internal readonly ILogger<Venus> Logger;

    internal Venus(
        string budId,
        IRegistryResolver registryResolver,
        IGlobalMonoBehaviourExecution globalMonoBehaviourExecution,
        ICustomAudioClipProvider customAudioClipProvider,
        ILogger<Venus> logger)
    {
        BudId = budId;
        RegistryResolver = registryResolver;
        GlobalMonoBehaviourExecution = globalMonoBehaviourExecution;
        CustomAudioClipProvider = customAudioClipProvider;
        Logger = logger;
    }
}