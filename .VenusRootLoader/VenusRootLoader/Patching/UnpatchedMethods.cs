using HarmonyLib;
using Object = UnityEngine.Object;

namespace VenusRootLoader.Patching;

internal static class UnpatchedMethods
{
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(UnityEngine.Resources), nameof(UnityEngine.Resources.Load), typeof(string), typeof(Type))]
    internal static Object UnpatchedResourcesLoad(string path, Type systemTypeInstance) =>
        throw new NotImplementedException("This is a stub method");
}