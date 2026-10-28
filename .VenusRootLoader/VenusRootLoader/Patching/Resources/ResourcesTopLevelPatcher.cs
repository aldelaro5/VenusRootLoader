using MonoDetour.DetourTypes;
using MonoDetour.HookGen;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VenusRootLoader.Patching.Resources;

/// <summary>
/// A patcher that processes all <see cref="IResourcesTypePatcher{T}"/> and <see cref="IResourcesArrayTypePatcher{T}"/>.
/// Notably, it's where <c>Resources.Load</c> and <c>Resources.LoadAll</c> are patched.
/// The expectation is that each resources type patchers handles patching a specific type of resources.
/// </summary>
internal sealed class ResourcesTopLevelPatcher : ITopLevelPatcher
{
    private static ResourcesTopLevelPatcher _instance = null!;

    private readonly IResourcesTypePatcher<TextAsset> _textAssetPatcher;
    private readonly IResourcesTypePatcher<AudioClip> _audioClipPatcher;
    private readonly IResourcesTypePatcher<Object> _prefabPatcher;
    private readonly IResourcesArrayTypePatcher<Sprite> _spriteArrayPatcher;
    private readonly IResourcesArrayTypePatcher<AudioClip> _audioClipArrayPatcher;

    public ResourcesTopLevelPatcher(
        IResourcesTypePatcher<TextAsset> textAssetPatcher,
        IResourcesTypePatcher<AudioClip> audioClipPatcher,
        IResourcesTypePatcher<Object> prefabPatcher,
        IResourcesArrayTypePatcher<Sprite> spriteArrayPatcher,
        IResourcesArrayTypePatcher<AudioClip> audioClipArrayPatcher)
    {
        _instance = this;
        _textAssetPatcher = textAssetPatcher;
        _spriteArrayPatcher = spriteArrayPatcher;
        _prefabPatcher = prefabPatcher;
        _audioClipPatcher = audioClipPatcher;
        _audioClipArrayPatcher = audioClipArrayPatcher;
    }

    public void Patch()
    {
        DefaultMonoDetourManager.Instance.Hook<PostfixDetour>(
            DmdICalls.UnityEngineResourcesLoad.PatchedWrapper,
            PatchResources);
        DefaultMonoDetourManager.Instance.Hook<PostfixDetour>(
            DmdICalls.UnityEngineResourcesLoadAll.PatchedWrapper,
            PatchResourcesArray);
    }

    private static void PatchResources(ref string path, ref Type systemTypeInstance, ref Object result)
    {
        if (systemTypeInstance == typeof(TextAsset))
            result = _instance._textAssetPatcher.PatchResource(path, (TextAsset)result);
        if (systemTypeInstance == typeof(AudioClip))
            result = _instance._audioClipPatcher.PatchResource(path, (AudioClip)result);
        if (systemTypeInstance == typeof(Object))
            result = _instance._prefabPatcher.PatchResource(path, (GameObject)result);
    }

    [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
    private static void PatchResourcesArray(ref string path, ref Type systemTypeInstance, ref Object[] result)
    {
        if (systemTypeInstance == typeof(Sprite))
            result = _instance._spriteArrayPatcher.PatchResources(path, result.Cast<Sprite>().ToArray());
        if (systemTypeInstance == typeof(AudioClip))
            result = _instance._audioClipArrayPatcher.PatchResources(path, result.Cast<AudioClip>().ToArray());
    }
}