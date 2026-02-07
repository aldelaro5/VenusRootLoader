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
        internal IServiceCollection AddTextAssetPatcher<TLeaf, TTextAssetParser>(
            string[] textAssetResourcesPath)
            where TLeaf : ILeaf
            where TTextAssetParser : class, ITextAssetParser<TLeaf>
        {
            collection.AddSingleton<ITextAssetParser<TLeaf>, TTextAssetParser>();
            collection.AddSingleton<ITextAssetPatcher, TextAssetPatcher<TLeaf>>(provider => new(
                textAssetResourcesPath,
                provider.GetRequiredService<ILogger<TextAssetPatcher<TLeaf>>>(),
                provider.GetRequiredService<ILeavesRegistry<TLeaf>>(),
                provider.GetRequiredService<ITextAssetParser<TLeaf>>()));
            return collection;
        }

        internal IServiceCollection AddLocalizedTextAssetPatcher<TLeaf, TTextAssetParser>(
            string[] textAssetResourcesSubpath)
            where TLeaf : ILeaf
            where TTextAssetParser : class, ILocalizedTextAssetParser<TLeaf>
        {
            collection.AddSingleton<ILocalizedTextAssetParser<TLeaf>, TTextAssetParser>();
            collection.AddSingleton<ILocalizedTextAssetPatcher, LocalizedTextAssetPatcher<TLeaf>>(provider =>
                new(
                textAssetResourcesSubpath,
                provider.GetRequiredService<ILogger<LocalizedTextAssetPatcher<TLeaf>>>(),
                provider.GetRequiredService<ILeavesRegistry<TLeaf>>(),
                provider.GetRequiredService<ILocalizedTextAssetParser<TLeaf>>()));
            return collection;
        }
    }
}