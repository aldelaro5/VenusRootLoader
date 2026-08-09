using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.Unity.AssetLoading;
using VenusRootLoader.Patching;
using Object = UnityEngine.Object;

namespace VenusRootLoader.Unity.AssetLoading;

/// <summary>
/// An <see cref="IAssetLoader{TObject}"/> that pulls the asset from the base game's resources.
/// </summary>
/// <typeparam name="TObject"><inheritdoc/></typeparam>
internal sealed class AssetLoaderFromResources<TObject> : IAssetLoader<TObject>
    where TObject : Object
{
    private readonly string _resourcesPath;
    private readonly int? _arrayIndex;

    /// <summary>
    /// Creates a lazily loaded asset that will be loaded from the base game's resources.
    /// </summary>
    /// <param name="resourcesPath">The path within the base game resources to pull the asset from. If the asset doesn't exist, <see cref="LoadAsset"/> will throw an exception when called.</param>
    internal AssetLoaderFromResources(string resourcesPath)
    {
        _resourcesPath = resourcesPath;
        _arrayIndex = null;
    }

    /// <summary>
    /// Creates a lazily loaded asset from an array that will be loaded from the base game's resources.
    /// </summary>
    /// <param name="resourcesPath">The path within the base game resources to pull the asset array from.</param>
    /// <param name="arrayIndex">The index of the asset array to pull the asset from.</param>
    internal AssetLoaderFromResources(string resourcesPath, int arrayIndex)
    {
        _resourcesPath = resourcesPath;
        _arrayIndex = arrayIndex;
    }

    public TObject LoadAsset()
    {
        if (_arrayIndex is null)
        {
            TObject? asset = (TObject?)UnpatchedMethods.UnpatchedResourcesLoad(_resourcesPath, typeof(TObject));
            if (asset == null)
                ThrowHelper.ThrowInvalidOperationException($"The resource at {_resourcesPath} does not exist.");
            return asset;
        }

        Object[]? assetArray = (Object[]?)UnpatchedMethods
            .UnpatchedResourcesLoadAll(_resourcesPath, typeof(TObject));
        if (assetArray is null || assetArray.Length <= _arrayIndex)
        {
            ThrowHelper.ThrowInvalidOperationException(
                $"The resource at {_resourcesPath} - array index {_arrayIndex} does not exist.");
        }

        return (TObject)assetArray[_arrayIndex.Value];
    }
}