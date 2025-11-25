using Microsoft.Extensions.Logging;

namespace VenusRootLoader.ModLoading;

public abstract class Mod
{
    protected internal ILogger Logger { get; internal set; } = null!;
    protected internal string BaseModPath { get; internal set; } = null!;

    protected internal abstract void Main();
}