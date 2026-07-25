using Microsoft.Extensions.DependencyInjection;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

/// <summary>
/// A convenience service that allows to get the matching <see cref="ILeavesRegistry{TLeaf}"/> or
/// <see cref="IOrderedLeavesRegistry{TLeaf}"/> from its <see cref="Leaf"/> type.
/// </summary>
internal interface IRegistryResolver
{
    /// <summary>
    /// Gets the <see cref="ILeavesRegistry{TLeaf}"/> that managed leaves of type <typeparamref name="TLeaf"/>.
    /// </summary>
    /// <typeparam name="TLeaf">The type of leaves the registry manages.</typeparam>
    /// <returns>The leaf registry.</returns>
    ILeavesRegistry<TLeaf> Resolve<TLeaf>()
        where TLeaf : Leaf;

    /// <summary>
    /// Gets the <see cref="IOrderedLeavesRegistry{TLeaf}"/> that managed leaves of type <typeparamref name="TLeaf"/>.
    /// </summary>
    /// <typeparam name="TLeaf">The type of leaves the registry manages.</typeparam>
    /// <returns>The ordered leaf registry.</returns>
    IOrderedLeavesRegistry<TLeaf> ResolveWithOrdering<TLeaf>()
        where TLeaf : Leaf;
}

/// <inheritdoc/>
internal sealed class RegistryResolver : IRegistryResolver
{
    private readonly IServiceProvider _serviceProvider;

    public RegistryResolver(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public ILeavesRegistry<T> Resolve<T>()
        where T : Leaf
    {
        return _serviceProvider.GetRequiredService<ILeavesRegistry<T>>();
    }

    public IOrderedLeavesRegistry<T> ResolveWithOrdering<T>() where T : Leaf
    {
        return _serviceProvider.GetRequiredService<IOrderedLeavesRegistry<T>>();
    }
}