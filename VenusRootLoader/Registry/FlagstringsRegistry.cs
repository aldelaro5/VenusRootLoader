using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal sealed class FlagstringsRegistry : AutoIncrementBasedRegistry<FlagstringLeaf>
{
    public FlagstringsRegistry(ILogger<FlagstringsRegistry> logger) : base(logger) { }
}