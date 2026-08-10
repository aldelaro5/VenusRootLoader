using HarmonyLib;
using Object = UnityEngine.Object;

namespace VenusRootLoader.Patching;

/// <summary>
/// Stubs of patched game methods that do not have any patches applied to them.
/// </summary>
internal static class UnpatchedMethods
{
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(UnityEngine.Resources), nameof(UnityEngine.Resources.Load), typeof(string), typeof(Type))]
    internal static Object UnpatchedResourcesLoad(string path, Type systemTypeInstance) =>
        throw new NotImplementedException("This is a stub method");

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(UnityEngine.Resources), nameof(UnityEngine.Resources.LoadAll), typeof(string), typeof(Type))]
    internal static Object[] UnpatchedResourcesLoadAll(string path, Type systemTypeInstance) =>
        throw new NotImplementedException("This is a stub method");
}