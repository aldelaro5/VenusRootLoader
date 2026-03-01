using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching;

namespace VenusRootLoader.Registry;

internal sealed class EnemiesRegistry : EnumBasedRegistry<EnemyLeaf, MainManager.Enemies>
{
    public EnemiesRegistry(EnumPatcher enumPatcher, ILogger<EnemiesRegistry> logger) : base(enumPatcher, logger) { }
}