using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching;

namespace VenusRootLoader.Registry;

internal sealed class AreasRegistry : EnumBasedRegistry<AreaLeaf, MainManager.Areas>
{
    public AreasRegistry(EnumPatcher enumPatcher, ILogger<AreasRegistry> logger) : base(enumPatcher, logger) { }
}