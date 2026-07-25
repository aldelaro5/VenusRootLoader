using System.Reflection;
using VenusRootLoader.Tests;

[assembly: AssemblyFixture(typeof(AssemblyResolver))]

namespace VenusRootLoader.Tests;

public sealed class AssemblyResolver : IDisposable
{
    public AssemblyResolver()
    {
        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
    }

    private Assembly? OnAssemblyResolve(object? sender, ResolveEventArgs args)
    {
        string assemblyName = new AssemblyName(args.Name).Name!;
        foreach (string file in Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.dll"))
        {
            if (assemblyName == Path.GetFileNameWithoutExtension(file))
                return Assembly.LoadFile(file);
        }

        return null;
    }

    public void Dispose() { }
}