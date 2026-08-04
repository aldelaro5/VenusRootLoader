using Microsoft.Extensions.Logging;
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
    internal readonly IGlobalMonoBehaviourExecution GlobalMonoBehaviourExecution;
    internal readonly ICustomAudioClipProvider CustomAudioClipProvider;
    internal readonly ILoggerFactory LoggerFactory;
    internal readonly ILogger<Venus> Logger;

    internal Venus(
        string budId,
        IGlobalMonoBehaviourExecution globalMonoBehaviourExecution,
        ICustomAudioClipProvider customAudioClipProvider,
        ILoggerFactory loggerFactory,
        ILogger<Venus> logger)
    {
        BudId = budId;
        GlobalMonoBehaviourExecution = globalMonoBehaviourExecution;
        CustomAudioClipProvider = customAudioClipProvider;
        LoggerFactory = loggerFactory;
        Logger = logger;
    }
}