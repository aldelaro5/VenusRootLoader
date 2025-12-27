namespace VenusRootLoader;

internal interface IAppDomainEvents
{
    event UnhandledExceptionEventHandler UnhandledException;
    event ResolveEventHandler AssemblyResolve;
}

internal sealed class AppDomainEvents : IAppDomainEvents
{
    public event UnhandledExceptionEventHandler? UnhandledException
    {
        add => AppDomain.CurrentDomain.UnhandledException += value;
        remove => AppDomain.CurrentDomain.UnhandledException -= value;
    }

    public event ResolveEventHandler? AssemblyResolve
    {
        add => AppDomain.CurrentDomain.AssemblyResolve += value;
        remove => AppDomain.CurrentDomain.AssemblyResolve -= value;
    }
}