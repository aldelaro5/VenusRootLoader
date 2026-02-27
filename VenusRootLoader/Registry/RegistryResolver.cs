using Microsoft.Extensions.DependencyInjection;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal interface IRegistryResolver
{
    ILeavesRegistry<T> Resolve<T>()
        where T : Leaf;

    IOrderedLeavesRegistry<T> ResolveWithOrdering<T>()
        where T : Leaf;
}

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