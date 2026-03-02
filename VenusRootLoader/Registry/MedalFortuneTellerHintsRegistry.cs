using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal sealed class MedalFortuneTellerHintsRegistry : AutoSequentialIdBasedRegistry<MedalFortuneTellerHintLeaf>
{
    public MedalFortuneTellerHintsRegistry(ILogger<MedalFortuneTellerHintsRegistry> logger)
        : base(logger, IdSequenceDirection.Increment)
    {
    }
}