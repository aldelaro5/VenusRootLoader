using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching;

namespace VenusRootLoader.Registry;

internal sealed class MedalsRegistry : EnumBasedRegistry<MedalLeaf, MainManager.BadgeTypes>
{
    public MedalsRegistry(EnumPatcher enumPatcher, ILogger<MedalsRegistry> logger) : base(enumPatcher, logger) { }
}