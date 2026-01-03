using Microsoft.Extensions.DependencyInjection;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal sealed class RegistryResolver
{
    private readonly IServiceProvider _serviceProvider;

    public RegistryResolver(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    internal ILeavesRegistry<T> Resolve<T>()
        where T : ILeaf
    {
        return _serviceProvider.GetRequiredService<ILeavesRegistry<T>>();
    }
}