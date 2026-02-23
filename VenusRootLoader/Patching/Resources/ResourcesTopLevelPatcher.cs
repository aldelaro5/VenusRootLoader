using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VenusRootLoader.Patching.Resources;

internal sealed class ResourcesTopLevelPatcher : ITopLevelPatcher
{
    private static ResourcesTopLevelPatcher _instance = null!;
    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly IResourcesTypePatcher<UnityEngine.TextAsset> _textAssetPatcher;
    private readonly IResourcesArrayTypePatcher<Sprite> _spriteArrayPatcher;

    public ResourcesTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        IResourcesTypePatcher<UnityEngine.TextAsset> textAssetPatcher,
        IResourcesArrayTypePatcher<Sprite> spriteArrayPatcher)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _textAssetPatcher = textAssetPatcher;
        _spriteArrayPatcher = spriteArrayPatcher;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(ResourcesTopLevelPatcher));

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnityEngine.Resources), nameof(UnityEngine.Resources.Load), [typeof(string), typeof(Type)])]
    private static void PatchResources(string path, Type systemTypeInstance, ref Object __result)
    {
        if (systemTypeInstance == typeof(UnityEngine.TextAsset))
            __result = _instance._textAssetPatcher.PatchResource(path, (UnityEngine.TextAsset)__result);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnityEngine.Resources), nameof(UnityEngine.Resources.LoadAll), [typeof(string), typeof(Type)])]
    private static void PatchResourcesArray(string path, Type systemTypeInstance, ref Object[] __result)
    {
        if (systemTypeInstance == typeof(Sprite))
        {
            // ReSharper disable once CoVariantArrayConversion
            __result = _instance._spriteArrayPatcher.PatchResources(path, __result.Cast<Sprite>().ToArray());
        }
    }
}