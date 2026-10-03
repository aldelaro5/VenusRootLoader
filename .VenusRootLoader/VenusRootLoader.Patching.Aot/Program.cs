using AsmResolver.DotNet;
using VenusRootLoader.Patching.Aot.TopLevelPatching;

namespace VenusRootLoader.Patching.Aot;

public static class Program
{
    public static int Main(string[] args)
    {
        AssemblyDefinition assembly = AssemblyDefinition.FromFile(args[0]);

        LocalNetStandardReferenceImporter referenceImporter = new(assembly.ManifestModule!);

        List<ITopLevelTypePatcher> patchers =
        [
            new BattleControlPatcher()
        ];

        PatchAssembly(referenceImporter, assembly, patchers);

        string directory = Path.GetDirectoryName(args[1])!;
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        assembly.Write(args[1]);

        return 0;
    }

    private static void PatchAssembly(
        LocalNetStandardReferenceImporter referenceImporter,
        AssemblyDefinition assembly,
        List<ITopLevelTypePatcher> patchers)
    {
        foreach (ITopLevelTypePatcher topLevelTypePatcher in patchers)
        {
            TypeDefinition type = assembly.ManifestModule!
                .GetAllTypes()
                .Single(x => x.Name == topLevelTypePatcher.TypeName);
            topLevelTypePatcher.PatchType(referenceImporter, type);
        }
    }
}