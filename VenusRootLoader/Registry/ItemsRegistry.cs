using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching;

namespace VenusRootLoader.Registry;

internal sealed class ItemsRegistry : EnumBasedRegistry<ItemLeaf, MainManager.Items>
{
    public ItemsRegistry(EnumPatcher enumPatcher, ILogger<ItemsRegistry> logger) : base(enumPatcher, logger) { }
}