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
        internal IServiceCollection AddTextAssetPatcher<TLeaf, TGameId, TTextAssetParser>(
            string[] textAssetResourcesPath)
            where TLeaf : ILeaf<TGameId>
            where TTextAssetParser : class, ITextAssetSerializable<TLeaf, TGameId>
        {
            collection.AddSingleton<ITextAssetSerializable<TLeaf, TGameId>, TTextAssetParser>();
            collection.AddSingleton<ITextAssetPatcher, TextAssetPatcher<TLeaf, TGameId>>(provider => new(
                textAssetResourcesPath,
                provider.GetRequiredService<ILogger<TextAssetPatcher<TLeaf, TGameId>>>(),
                provider.GetRequiredService<ILeavesRegistry<TLeaf, TGameId>>(),
                provider.GetRequiredService<ITextAssetSerializable<TLeaf, TGameId>>()));
            return collection;
        }

        internal IServiceCollection AddLocalizedTextAssetPatcher<TLeaf, TGameId, TTextAssetParser>(
            string[] textAssetResourcesSubpath)
            where TLeaf : ILeaf<TGameId>
            where TTextAssetParser : class, ILocalizedTextAssetSerializable<TLeaf, TGameId>
        {
            collection.AddSingleton<ILocalizedTextAssetSerializable<TLeaf, TGameId>, TTextAssetParser>();
            collection.AddSingleton<ILocalizedTextAssetPatcher, LocalizedTextAssetPatcher<TLeaf, TGameId>>(provider =>
                new(
                textAssetResourcesSubpath,
                provider.GetRequiredService<ILogger<LocalizedTextAssetPatcher<TLeaf, TGameId>>>(),
                provider.GetRequiredService<ILeavesRegistry<TLeaf, TGameId>>(),
                provider.GetRequiredService<ILocalizedTextAssetSerializable<TLeaf, TGameId>>()));
            return collection;
        }
    }
}