using HarmonyLib;
using Object = UnityEngine.Object;

namespace VenusRootLoader.Patching.Resources;

internal sealed class ResourcesTopLevelPatcher : ITopLevelPatcher
{
    private static ResourcesTopLevelPatcher _instance = null!;
    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly IResourcesTypePatcher<UnityEngine.TextAsset> _textAssetPatcher;

    public ResourcesTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        IResourcesTypePatcher<UnityEngine.TextAsset> textAssetPatcher)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _textAssetPatcher = textAssetPatcher;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(ResourcesTopLevelPatcher));

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnityEngine.Resources), nameof(UnityEngine.Resources.Load), [typeof(string), typeof(Type)])]
    private static void PatchResources(string path, Type systemTypeInstance, ref Object __result)
    {
        if (systemTypeInstance == typeof(UnityEngine.TextAsset))
            __result = _instance._textAssetPatcher.PatchResource(path, (UnityEngine.TextAsset)__result);
    }
}