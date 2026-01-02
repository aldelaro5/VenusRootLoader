using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.Extensions;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection collection)
    {
        internal IServiceCollection AddBoundTextAssetPatcher<T>(string textAssetResourcesPath)
        {
            collection.AddSingleton<TextAssetPatcher<T>>(provider => new(
                textAssetResourcesPath,
                provider.GetRequiredService<RootTextAssetPatcher>(),
                provider.GetRequiredService<ILogger<TextAssetPatcher<T>>>(),
                provider.GetRequiredService<ITextAssetSerializable<T>>()));
            return collection;
        }

        internal IServiceCollection AddBoundLocalizedTextAssetPatcher<T>(string textAssetResourcesSubpath)
        {
            collection.AddSingleton<LocalizedTextAssetPatcher<T>>(provider => new(
                textAssetResourcesSubpath,
                provider.GetRequiredService<RootTextAssetPatcher>(),
                provider.GetRequiredService<ILogger<LocalizedTextAssetPatcher<T>>>(),
                provider.GetRequiredService<ITextAssetSerializable<T>>()));
            return collection;
        }
    }
}