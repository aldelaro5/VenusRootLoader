using Microsoft.Extensions.Logging;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity;
using VenusRootLoader.Unity.CustomAudioClip;

// ReSharper disable UnusedMember.Global

namespace VenusRootLoader.Api;

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