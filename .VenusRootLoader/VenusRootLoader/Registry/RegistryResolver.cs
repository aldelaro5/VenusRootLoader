using Microsoft.Extensions.DependencyInjection;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

/// <summary>
/// A convenience service that allows to get the matching <see cref="ILeavesRegistry{TLeaf}"/> or
/// <see cref="IOrderedLeavesRegistry{TLeaf}"/> from its <see cref="Leaf"/> type.
/// </summary>
internal static class RegistryResolver
{
    private static IServiceProvider _serviceProvider = null!;

    internal static void Init(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    /// <summary>
    /// Gets the <see cref="ILeavesRegistry{TLeaf}"/> that managed leaves of type <typeparamref name="TLeaf"/>.
    /// </summary>
    /// <typeparam name="TLeaf">The type of leaves the registry manages.</typeparam>
    /// <returns>The leaf registry.</returns>
    internal static ILeavesRegistry<TLeaf> Resolve<TLeaf>()
        where TLeaf : Leaf
    {
        return _serviceProvider.GetRequiredService<ILeavesRegistry<TLeaf>>();
    }

    /// <summary>
    /// Gets the <see cref="IOrderedLeavesRegistry{TLeaf}"/> that managed leaves of type <typeparamref name="TLeaf"/>.
    /// </summary>
    /// <typeparam name="TLeaf">The type of leaves the registry manages.</typeparam>
    /// <returns>The ordered leaf registry.</returns>
    internal static IOrderedLeavesRegistry<TLeaf> ResolveWithOrdering<TLeaf>()
        where TLeaf : Leaf
    {
        return _serviceProvider.GetRequiredService<IOrderedLeavesRegistry<TLeaf>>();
    }
}