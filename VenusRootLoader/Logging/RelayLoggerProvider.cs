using Microsoft.Extensions.Logging;

namespace VenusRootLoader.Logging;

internal class RelayLoggerProvider : ILoggerProvider
{
    private readonly BootstrapFunctions _bootstrapFunctions;

    public RelayLoggerProvider(BootstrapFunctions bootstrapFunctions)
    {
        _bootstrapFunctions = bootstrapFunctions;
    }

    public ILogger CreateLogger(string categoryName) => new RelayLogger(_bootstrapFunctions, categoryName);

    public void Dispose() { }
}