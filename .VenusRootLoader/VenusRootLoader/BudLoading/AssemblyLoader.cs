using System.Reflection;

namespace VenusRootLoader.BudLoading;

internal interface IAssemblyLoader
{
    Assembly LoadFromPath(string path);
}

internal sealed class AssemblyLoader : IAssemblyLoader
{
    public Assembly LoadFromPath(string path) => Assembly.LoadFrom(path);
}