using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Extensions;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection collection)
    {
        internal IServiceCollection AddBoundTextAssetPatcher<T, U>(string textAssetResourcesPath)
            where T : ILeaf<U>
        {
            collection.AddSingleton<TextAssetPatcher<T, U>>(provider => new(
                textAssetResourcesPath,
                provider.GetRequiredService<ILogger<TextAssetPatcher<T, U>>>(),
                provider.GetRequiredService<RootTextAssetPatcher>(),
                provider.GetRequiredService<ILeavesRegistry<T, U>>(),
                provider.GetRequiredService<ITextAssetSerializable<T, U>>()));
            return collection;
        }

        internal IServiceCollection AddBoundLocalizedTextAssetPatcher<T, U>(string textAssetResourcesSubpath)
            where T : ILeaf<U>
        {
            collection.AddSingleton<LocalizedTextAssetPatcher<T, U>>(provider => new(
                textAssetResourcesSubpath,
                provider.GetRequiredService<ILogger<LocalizedTextAssetPatcher<T, U>>>(),
                provider.GetRequiredService<RootTextAssetPatcher>(),
                provider.GetRequiredService<ILeavesRegistry<T, U>>(),
                provider.GetRequiredService<ILocalizedTextAssetSerializable<T, U>>()));
            return collection;
        }
    }
}