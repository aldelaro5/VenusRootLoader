using HarmonyLib;
using Object = UnityEngine.Object;

namespace VenusRootLoader.Patching.Resources;

internal sealed class ResourcesPatcher
{
    private static readonly Dictionary<Type, IResourcesTypePatcher> ResourcesTypePatchers = new();

    public ResourcesPatcher(IHarmonyTypePatcher harmonyTypePatcher)
    {
        harmonyTypePatcher.PatchAll(typeof(ResourcesPatcher));
    }

    internal void RegisterResourceTypePatcher(Type type, IResourcesTypePatcher patcher) =>
        ResourcesTypePatchers.Add(type, patcher);

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnityEngine.Resources), nameof(UnityEngine.Resources.Load), [typeof(string), typeof(Type)])]
    private static void PatchResources(string path, Type systemTypeInstance, ref Object __result)
    {
        if (!ResourcesTypePatchers.TryGetValue(systemTypeInstance, out IResourcesTypePatcher patcher))
            return;

        __result = patcher.PatchResource(path, __result);
    }
}