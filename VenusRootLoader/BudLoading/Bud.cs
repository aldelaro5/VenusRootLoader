using Microsoft.Extensions.Logging;
using VenusRootLoader.Modding;
using VenusRootLoader.Models;

namespace VenusRootLoader.BudLoading;

public abstract class Bud
{
    protected internal ILogger Logger { get; internal set; } = null!;
    protected internal BudManifest BudInfo { get; internal set; } = null!;
    protected internal string BaseBudPath { get; internal set; } = null!;
    protected internal Venus Venus { get; internal set; } = null!;

    protected internal abstract void Main();
}