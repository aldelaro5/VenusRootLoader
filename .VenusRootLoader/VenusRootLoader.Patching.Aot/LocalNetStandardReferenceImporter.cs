using AsmResolver.DotNet;

namespace VenusRootLoader.Patching.Aot;

/// <summary>
/// A <see cref="ReferenceImporter"/> that will redirect all reference requests of a .NET Core CorLib assembly to the
/// netstandard.dll that is contained in the same directory as the <see cref="ReferenceImporter.TargetModule"/>.
/// </summary>
public sealed class LocalNetStandardReferenceImporter : ReferenceImporter
{
    public LocalNetStandardReferenceImporter(ModuleDefinition module) : base(module) { }

    protected override AssemblyReference ImportAssembly(AssemblyDescriptor assembly)
    {
        if (!assembly.IsCorLib(DotNetRuntimeInfo.NetCoreApp(Environment.Version)))
            return base.ImportAssembly(assembly);

        string localFolder = Path.GetDirectoryName(TargetModule.FilePath)!;
        return AssemblyDefinition.FromFile(Path.Combine(localFolder, "netstandard.dll")).ToAssemblyReference();
    }
}