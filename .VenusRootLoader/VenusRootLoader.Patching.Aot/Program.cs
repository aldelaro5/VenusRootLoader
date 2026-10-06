using AsmResolver.DotNet;
using VenusRootLoader.Patching.Aot.TopLevelPatching;

namespace VenusRootLoader.Patching.Aot;

public static class Program
{
    public static int Main(string[] args)
    {
        AssemblyDefinition assembly = AssemblyDefinition.FromFile(args[0]);
        LocalNetStandardReferenceImporter referenceImporter = new(assembly.ManifestModule!);
        GameModuleData gameModuleData = new(assembly.ManifestModule!, referenceImporter);

        List<ITopLevelTypePatcher> patchers =
        [
            new BattleControlPatcher(),
            new MainManagerPatcher()
        ];

        PatchAssembly(gameModuleData, patchers);

        string directory = Path.GetDirectoryName(args[1])!;
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        assembly.Write(args[1]);

        return 0;
    }

    private static void PatchAssembly(
        GameModuleData gameModuleData,
        List<ITopLevelTypePatcher> patchers)
    {
        foreach (ITopLevelTypePatcher topLevelTypePatcher in patchers)
        {
            TypeDefinition type = gameModuleData.Module
                .GetAllTypes()
                .Single(x => x.Name == topLevelTypePatcher.TypeName);
            topLevelTypePatcher.PatchType(gameModuleData, type);
        }
    }
}