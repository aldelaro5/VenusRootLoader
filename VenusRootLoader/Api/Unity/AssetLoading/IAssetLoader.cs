using Object = UnityEngine.Object;

// ReSharper disable once TypeParameterCanBeVariant

namespace VenusRootLoader.Api.Unity.AssetLoading;

/// <summary>
/// A scheme to load an asset lazily so it is only loaded when needed.
/// </summary>
/// <typeparam name="TObject">The type of asset this loader loads.</typeparam>
public interface IAssetLoader<TObject>
    where TObject : Object
{
    /// <summary>
    /// Loads and obtains the asset backed by this loader.
    /// </summary>
    /// <returns>The loaded asset ready for usage.</returns>
    TObject LoadAsset();
}