using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching;
using VenusRootLoader.Patching.Resources.TextAssetPatchers;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Extensions;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection collection)
    {
        internal IServiceCollection AddEnumBasedLeavesRegistry<TLeaf, TEnum>(int offsetEnumValueToGameId = 0)
            where TLeaf : Leaf
            where TEnum : Enum
        {
            collection.AddSingleton<ILeavesRegistry<TLeaf>, EnumBasedRegistry<TLeaf, TEnum>>(provider =>
                new(
                    offsetEnumValueToGameId,
                    provider.GetRequiredService<EnumPatcher>(),
                    provider.GetRequiredService<ILoggerFactory>().CreateLogger(
                        IServiceCollection.GenerateRegistryLogCategoryName<TLeaf>())));
            return collection;
        }

        internal IServiceCollection AddEnumBasedLeavesRegistryWithOrdering<TLeaf, TEnum>()
            where TLeaf : Leaf
            where TEnum : Enum
        {
            collection.AddEnumBasedLeavesRegistry<TLeaf, TEnum>();
            collection.AddSingleton<IOrderedLeavesRegistry<TLeaf>, OrderedLeavesRegistry<TLeaf>>();
            return collection;
        }

        internal IServiceCollection AddAutoSequentialIdBasedLeavesRegistry<TLeaf>(
            IdSequenceDirection sequenceDirection = IdSequenceDirection.Increment,
            int firstGameId = 0)
            where TLeaf : Leaf
        {
            collection.AddSingleton<ILeavesRegistry<TLeaf>, AutoSequentialIdBasedRegistry<TLeaf>>(provider =>
                new AutoSequentialIdBasedRegistry<TLeaf>(
                    provider.GetRequiredService<ILoggerFactory>()
                        .CreateLogger(IServiceCollection.GenerateRegistryLogCategoryName<TLeaf>()),
                    sequenceDirection,
                    firstGameId));
            return collection;
        }

        private static string GenerateRegistryLogCategoryName<TLeaf>() where TLeaf : Leaf =>
            $"{nameof(VenusRootLoader)}.{nameof(Registry)}.{typeof(TLeaf).Name}Registry";

        internal IServiceCollection AddAutoSequentialIdBasedLeavesRegistryWithOrdering<TLeaf>(
            IdSequenceDirection sequenceDirection = IdSequenceDirection.Increment,
            int firstGameId = 0)
            where TLeaf : Leaf
        {
            collection.AddAutoSequentialIdBasedLeavesRegistry<TLeaf>(sequenceDirection, firstGameId);
            collection.AddSingleton<IOrderedLeavesRegistry<TLeaf>, OrderedLeavesRegistry<TLeaf>>();
            return collection;
        }

        internal IServiceCollection AddTextAssetPatcher<TLeaf, TTextAssetParser>(
            string[] textAssetResourcesPath,
            Func<ILeavesRegistry<TLeaf>, IEnumerable<TLeaf>>? leavesSorter = null)
            where TLeaf : Leaf
            where TTextAssetParser : class, ITextAssetParser<TLeaf>
        {
            collection.AddSingleton<ITextAssetParser<TLeaf>, TTextAssetParser>();
            collection.AddSingleton<ITextAssetPatcher, TextAssetPatcher<TLeaf>>(provider => new(
                textAssetResourcesPath,
                leavesSorter,
                provider.GetRequiredService<ILogger<TextAssetPatcher<TLeaf>>>(),
                provider.GetRequiredService<ILeavesRegistry<TLeaf>>(),
                provider.GetRequiredService<ITextAssetParser<TLeaf>>()));
            return collection;
        }

        internal IServiceCollection AddOrderingTextAssetPatcher<TLeaf, TTextAssetParser>(
            string textAssetResourcesPath)
            where TLeaf : Leaf
            where TTextAssetParser : class, IOrderingTextAssetParser<TLeaf>
        {
            collection.AddSingleton<IOrderingTextAssetParser<TLeaf>, TTextAssetParser>();
            collection.AddSingleton<IOrderingTextAssetPatcher, OrderingTextAssetPatcher<TLeaf>>(provider => new(
                textAssetResourcesPath,
                provider.GetRequiredService<ILogger<OrderingTextAssetPatcher<TLeaf>>>(),
                provider.GetRequiredService<IOrderedLeavesRegistry<TLeaf>>(),
                provider.GetRequiredService<IOrderingTextAssetParser<TLeaf>>()));
            return collection;
        }

        internal IServiceCollection AddLocalizedTextAssetPatcher<TLeaf, TTextAssetParser>(
            string[] textAssetResourcesSubpath,
            Func<ILeavesRegistry<TLeaf>, IEnumerable<TLeaf>>? leavesSorter = null)
            where TLeaf : Leaf
            where TTextAssetParser : class, ILocalizedTextAssetParser<TLeaf>
        {
            collection.AddSingleton<ILocalizedTextAssetParser<TLeaf>, TTextAssetParser>();
            collection.AddSingleton<ILocalizedTextAssetPatcher, LocalizedTextAssetPatcher<TLeaf>>(provider =>
                new(
                textAssetResourcesSubpath,
                leavesSorter,
                provider.GetRequiredService<ILogger<LocalizedTextAssetPatcher<TLeaf>>>(),
                provider.GetRequiredService<ILeavesRegistry<TLeaf>>(),
                provider.GetRequiredService<ILocalizedTextAssetParser<TLeaf>>()));
            return collection;
        }
    }
}