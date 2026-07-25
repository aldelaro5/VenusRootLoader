using HarmonyLib;
using VenusRootLoader.Logging;

namespace VenusRootLoader.Patching;

/// <summary>
/// A general HarmonyX service that allows to patch everything contained in a type. This is mostly there to make working
/// HarmonyX more friendly for dependency injection purposes so the management of the Harmony instance is abstracted out. 
/// </summary>
internal interface IHarmonyTypePatcher
{
    /// <summary>
    /// Tells the <see cref="VenusRootLoader"/>'s <see cref="Harmony"/> instance to process every patches declared on a type.
    /// </summary>
    /// <param name="type">The type the patches are contained in.</param>
    void PatchAll(Type type);
}

/// <inheritdoc/>
internal sealed class HarmonyTypePatcher : IHarmonyTypePatcher
{
    private readonly Harmony _harmonyInstance;

    public HarmonyTypePatcher(HarmonyLogger harmonyLogger)
    {
        harmonyLogger.InstallHarmonyLogging();
        _harmonyInstance = new Harmony(nameof(VenusRootLoader));
    }

    public void PatchAll(Type type) => _harmonyInstance.PatchAll(type);
}