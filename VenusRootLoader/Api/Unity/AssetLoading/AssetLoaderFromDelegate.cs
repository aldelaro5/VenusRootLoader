using Object = UnityEngine.Object;

namespace VenusRootLoader.Api.Unity.AssetLoading;

/// <summary>
/// An <see cref="IAssetLoader{TObject}"/> that loads the asset by creating it from a provided delegate.
/// </summary>
/// <typeparam name="TObject"><inheritdoc/></typeparam>
public sealed class AssetLoaderFromDelegate<TObject> : IAssetLoader<TObject>
    where TObject : Object
{
    private readonly Func<TObject> _loader;

    /// <summary>
    /// Creates a lazily loaded asset that will be loaded by calling a provided delegate.
    /// </summary>
    /// <param name="loader">The delegate that will create and return the asset when <see cref="LoadAsset"/> is called.</param>
    public AssetLoaderFromDelegate(Func<TObject> loader)
    {
        _loader = loader;
    }

    public TObject LoadAsset() => _loader();
}