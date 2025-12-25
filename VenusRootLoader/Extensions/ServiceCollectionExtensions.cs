using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VenusRootLoader.Patching;
using VenusRootLoader.Patching.TextAssetData;

namespace VenusRootLoader.Extensions;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection collection)
    {
        internal IServiceCollection AddBoundTextAssetPatcher<T>(string textAssetResourcesPath)
            where T : ITextAssetSerializable
        {
            collection.AddSingleton<TextAssetPatcher<T>>(provider => new(
                textAssetResourcesPath,
                provider.GetRequiredService<RootTextAssetPatcher>(),
                provider.GetRequiredService<ILogger<TextAssetPatcher<T>>>()));
            return collection;
        }

        internal IServiceCollection AddBoundLocalizedTextAssetPatcher<T>(string textAssetResourcesSubpath)
            where T : ITextAssetSerializable
        {
            collection.AddSingleton<LocalizedTextAssetPatcher<T>>(provider => new(
                textAssetResourcesSubpath,
                provider.GetRequiredService<RootTextAssetPatcher>(),
                provider.GetRequiredService<ILogger<LocalizedTextAssetPatcher<T>>>()));
            return collection;
        }
    }
}