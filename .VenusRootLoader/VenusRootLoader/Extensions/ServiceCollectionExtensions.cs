using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching;
using VenusRootLoader.Patching.Resources.TextAssetPatchers;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Extensions;

// ReSharper disable UnusedMethodReturnValue.Global
/// <summary>
/// Extensions for <see cref="IServiceCollection"/>.
/// </summary>
internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection collection)
    {
        /// <summary>
        /// Adds an <see cref="EnumBasedRegistry{TLeaf,TEnum}"/> to the service collections.
        /// </summary>
        /// <param name="offsetEnumValueToGameId">An offset to convert the backing enum values to the leaf's game id.</param>
        /// <typeparam name="TLeaf"><inheritdoc cref="ILeavesRegistry{TLeaf}"/></typeparam>
        /// <typeparam name="TEnum">The <see cref="Enum"/> type that can identify every leaf in this registry.</typeparam>
        /// <returns>The service collection.</returns>
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

        /// <summary>
        /// Adds an <see cref="IOrderedLeavesRegistry{TLeaf}"/> that wraps an
        /// <see cref="EnumBasedRegistry{TLeaf,TEnum}"/> to the service collections.
        /// </summary>
        /// <typeparam name="TLeaf"><inheritdoc cref="ILeavesRegistry{TLeaf}"/></typeparam>
        /// <typeparam name="TEnum">The <see cref="Enum"/> type that can identify every leaf in this registry.</typeparam>
        /// <returns>The service collection.</returns>
        internal IServiceCollection AddEnumBasedLeavesRegistryWithOrdering<TLeaf, TEnum>()
            where TLeaf : Leaf
            where TEnum : Enum
        {
            collection.AddEnumBasedLeavesRegistry<TLeaf, TEnum>();
            collection.AddSingleton<IOrderedLeavesRegistry<TLeaf>, OrderedLeavesRegistry<TLeaf>>();
            return collection;
        }

        /// <summary>
        /// Adds an <see cref="AutoSequentialIdBasedRegistry{TLeaf}"/> to the service collections.
        /// </summary>
        /// <param name="sequenceDirection">The direction the sequence of the leaves's <see cref="Leaf.GameId"/> will
        /// grow as leaves gets registered.</param>
        /// <param name="firstGameId">The <see cref="Leaf.GameId"/> to use as the first leaf</param>
        /// <typeparam name="TLeaf"><inheritdoc cref="ILeavesRegistry{TLeaf}"/></typeparam>
        /// <returns>The service collection.</returns>
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

        /// <summary>
        /// Adds an <see cref="IOrderedLeavesRegistry{TLeaf}"/> that wraps an
        /// <see cref="AutoSequentialIdBasedRegistry{TLeaf}"/> to the service collections.
        /// </summary>
        /// <param name="sequenceDirection">The direction the sequence of the leaves's <see cref="Leaf.GameId"/> will
        /// grow as leaves gets registered.</param>
        /// <param name="firstGameId">The <see cref="Leaf.GameId"/> to use as the first leaf</param>
        /// <typeparam name="TLeaf"><inheritdoc cref="ILeavesRegistry{TLeaf}"/></typeparam>
        /// <returns>The service collection.</returns>
        internal IServiceCollection AddAutoSequentialIdBasedLeavesRegistryWithOrdering<TLeaf>(
            IdSequenceDirection sequenceDirection = IdSequenceDirection.Increment,
            int firstGameId = 0)
            where TLeaf : Leaf
        {
            collection.AddAutoSequentialIdBasedLeavesRegistry<TLeaf>(sequenceDirection, firstGameId);
            collection.AddSingleton<IOrderedLeavesRegistry<TLeaf>, OrderedLeavesRegistry<TLeaf>>();
            return collection;
        }

        /// <summary>
        /// Adds an <see cref="ITextAssetPatcher"/> and a matching <see cref="ITextAssetParser{T}"/> to the service collection.
        /// </summary>
        /// <param name="textAssetResourcesPath">The resources paths that the patcher and parser can handle.</param>
        /// <param name="leavesSorter">A calback to determine the order of the leaves as they appear in the TextAsset
        /// if they do not follow their <see cref="Leaf.GameId"/> order.</param>
        /// <typeparam name="TLeaf">The type of <see cref="Leaf"/> to patch.</typeparam>
        /// <typeparam name="TTextAssetParser">The matching <see cref="ITextAssetParser{T}"/> that can handle the leaves.</typeparam>
        /// <returns>The service collection.</returns>
        internal IServiceCollection AddTextAssetPatcher<TLeaf, TTextAssetParser>(
            string[] textAssetResourcesPath,
            Func<ILeavesRegistry<TLeaf>, IEnumerable<TLeaf>>? leavesSorter = null)
            where TLeaf : Leaf
            where TTextAssetParser : class, ITextAssetParser<TLeaf>
        {
            collection.AddSingleton<ITextAssetParser<TLeaf>, TTextAssetParser>();
            collection.AddSingleton<ITextAssetPatcher, TextAssetPatcher<TLeaf>>(provider => new(
                textAssetResourcesPath,
                provider.GetRequiredService<ILogger<TextAssetPatcher<TLeaf>>>(),
                provider.GetRequiredService<ITextAssetDumper>(),
                provider.GetRequiredService<ILeavesRegistry<TLeaf>>(),
                provider.GetRequiredService<ITextAssetParser<TLeaf>>(),
                leavesSorter));
            return collection;
        }

        /// <summary>
        /// Adds an <see cref="IOrderingTextAssetPatcher"/> and a matching <see cref="IOrderingTextAssetParser{T}"/> to the service collection.
        /// </summary>
        /// <param name="textAssetResourcesPath">The resources path that the patcher and parser uses.</param>
        /// <typeparam name="TLeaf">The type of <see cref="Leaf"/> to patch.</typeparam>
        /// <typeparam name="TOrderingTextAssetParser">The matching <see cref="IOrderingTextAssetParser{T}"/> that can handle the leaves.</typeparam>
        /// <returns>The service collection.</returns>
        internal IServiceCollection AddOrderingTextAssetPatcher<TLeaf, TOrderingTextAssetParser>(
            string textAssetResourcesPath)
            where TLeaf : Leaf
            where TOrderingTextAssetParser : class, IOrderingTextAssetParser<TLeaf>
        {
            collection.AddSingleton<IOrderingTextAssetParser<TLeaf>, TOrderingTextAssetParser>();
            collection.AddSingleton<IOrderingTextAssetPatcher, OrderingTextAssetPatcher<TLeaf>>(provider => new(
                textAssetResourcesPath,
                provider.GetRequiredService<ILogger<OrderingTextAssetPatcher<TLeaf>>>(),
                provider.GetRequiredService<ITextAssetDumper>(),
                provider.GetRequiredService<IOrderedLeavesRegistry<TLeaf>>(),
                provider.GetRequiredService<IOrderingTextAssetParser<TLeaf>>()));
            return collection;
        }

        /// <summary>
        /// Adds an <see cref="ILocalizedTextAssetPatcher"/> and a matching <see cref="ILocalizedTextAssetParser{T}"/> to the service collection.
        /// </summary>
        /// <param name="textAssetResourcesSubpath">The resources paths suffixes after the Dialogues directory that the
        /// patcher and parser uses.</param>
        /// <param name="leavesSorter">A calback to determine the order of the leaves as they appear in the TextAsset
        /// if they do not follow their <see cref="Leaf.GameId"/> order.</param>
        /// <typeparam name="TLeaf">The type of <see cref="Leaf"/> to patch.</typeparam>
        /// <typeparam name="TLocalizedTextAssetParser">The matching <see cref="ILocalizedTextAssetParser{T}"/> that can handle the leaves.</typeparam>
        /// <returns>The service collection.</returns>
        internal IServiceCollection AddLocalizedTextAssetPatcher<TLeaf, TLocalizedTextAssetParser>(
            string[] textAssetResourcesSubpath,
            Func<ILeavesRegistry<TLeaf>, IEnumerable<TLeaf>>? leavesSorter = null)
            where TLeaf : Leaf
            where TLocalizedTextAssetParser : class, ILocalizedTextAssetParser<TLeaf>
        {
            collection.AddSingleton<ILocalizedTextAssetParser<TLeaf>, TLocalizedTextAssetParser>();
            collection.AddSingleton<ILocalizedTextAssetPatcher, LocalizedTextAssetPatcher<TLeaf>>(provider =>
                new(
                textAssetResourcesSubpath,
                provider.GetRequiredService<ILogger<LocalizedTextAssetPatcher<TLeaf>>>(),
                provider.GetRequiredService<ITextAssetDumper>(),
                provider.GetRequiredService<ILeavesRegistry<TLeaf>>(),
                provider.GetRequiredService<ILocalizedTextAssetParser<TLeaf>>(),
                leavesSorter));
            return collection;
        }

        private static string GenerateRegistryLogCategoryName<TLeaf>() where TLeaf : Leaf =>
            $"{nameof(VenusRootLoader)}.{nameof(Registry)}.{typeof(TLeaf).Name}Registry";
    }
}