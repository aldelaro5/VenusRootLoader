using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching;

namespace VenusRootLoader.Registry;

internal sealed class ItemsRegistry : EnumBasedRegistry<ItemLeaf, MainManager.Items>
{
    public ItemsRegistry(EnumPatcher enumPatcher) : base(enumPatcher) { }
}