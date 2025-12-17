using System.Reflection;

namespace VenusRootLoader.BudLoading;

public interface IAssemblyLoader
{
    Assembly LoadFromPath(string path);
}

public sealed class AssemblyLoader : IAssemblyLoader
{
    public Assembly LoadFromPath(string path) => Assembly.LoadFrom(path);
}