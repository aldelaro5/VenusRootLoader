using HarmonyLib;
using VenusRootLoader.Logging;

namespace VenusRootLoader.Patching;

internal interface IHarmonyTypePatcher
{
    void PatchAll(Type type);
}

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