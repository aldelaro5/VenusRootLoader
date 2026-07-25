using VenusRootLoader.Patching;
using Object = UnityEngine.Object;

namespace VenusRootLoader.Api.Unity.AssetLoading;

/// <summary>
/// An <see cref="IAssetLoader{TObject}"/> that pulls the asset from the base game's resources.
/// </summary>
/// <typeparam name="TObject"><inheritdoc/></typeparam>
public sealed class AssetLoaderFromResources<TObject> : IAssetLoader<TObject>
    where TObject : Object
{
    private readonly string _resourcesPath;

    /// <summary>
    /// Creates a lazily loaded asset that will be loaded from the base game's resources.
    /// </summary>
    /// <param name="resourcesPath">The path within the base game resources to pull the asset from. If the asset doesn't exist, <see cref="LoadAsset"/> will return null when called.</param>
    public AssetLoaderFromResources(string resourcesPath)
    {
        _resourcesPath = resourcesPath;
    }

    public TObject LoadAsset() => (TObject)UnpatchedMethods.UnpatchedResourcesLoad(_resourcesPath, typeof(TObject));
}