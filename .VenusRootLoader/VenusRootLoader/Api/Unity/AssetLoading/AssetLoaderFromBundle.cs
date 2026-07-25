using CommunityToolkit.Diagnostics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VenusRootLoader.Api.Unity.AssetLoading;

/// <summary>
/// An <see cref="IAssetLoader{TObject}"/> that pulls the asset from a provided <see cref="AssetBundle"/>.
/// </summary>
/// <typeparam name="TObject"><inheritdoc/></typeparam>
public sealed class AssetLoaderFromBundle<TObject> : IAssetLoader<TObject>
    where TObject : Object
{
    private readonly AssetBundle _assetBundle;
    private readonly string _resourcesPath;

    /// <summary>
    /// Creates a lazily loaded asset that will be loaded from a provided <see cref="AssetBundle"/>.
    /// </summary>
    /// <param name="bundle">The <see cref="AssetBundle"/> to load the asset from.</param>
    /// <param name="assetPathInBundle">The path within <paramref name="bundle"/> to load the asset from. It may be the filename without extension or the full absolute path with extension.</param>
    public AssetLoaderFromBundle(AssetBundle bundle, string assetPathInBundle)
    {
        if (!bundle.Contains(assetPathInBundle))
        {
            ThrowHelper.ThrowArgumentException(
                nameof(assetPathInBundle),
                "No asset exists in the bundle for the given path");
        }

        _assetBundle = bundle;
        _resourcesPath = assetPathInBundle;
    }

    public TObject LoadAsset() => _assetBundle.LoadAsset<TObject>(_resourcesPath);
}