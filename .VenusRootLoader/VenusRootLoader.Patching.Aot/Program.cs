using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using VenusRootLoader.Patching.Aot.TopLevelPatching;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot;

public static class Program
{
    public static int Main(string[] args)
    {
        AssemblyDefinition assembly = AssemblyDefinition.FromFile(args[0]);
        LocalNetStandardReferenceImporter referenceImporter = new(assembly.ManifestModule!);
        GameModuleData gameModuleData = new(assembly.ManifestModule!, referenceImporter);

        List<TopLevelTypePatcher> patchers =
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
        List<TopLevelTypePatcher> patchers)
    {
        MakeAllPrivateEnumPublic(gameModuleData);
        AddMissingUpdateToMonoBehaviours(gameModuleData);

        foreach (TopLevelTypePatcher topLevelTypePatcher in patchers)
        {
            TypeDefinition type = gameModuleData.Module
                .GetAllTypes()
                .Single(x => x.Name == topLevelTypePatcher.TypeName);
            topLevelTypePatcher.PatchType(gameModuleData, type);
        }
    }

    private static void MakeAllPrivateEnumPublic(GameModuleData gameModuleData)
    {
        foreach (TypeDefinition enumType in gameModuleData.Module.GetAllTypes().Where(x => x.IsEnum))
        {
            if (enumType is { IsNestedPrivate: false, IsNestedFamily: false, IsNotPublic: false })
                continue;

            bool wasNested = enumType.IsNested;
            if (wasNested)
                enumType.IsNestedPublic = true;
            else
                enumType.IsPublic = true;
        }
    }

    private static void AddMissingUpdateToMonoBehaviours(GameModuleData gameModuleData)
    {
        foreach (TypeDefinition topLevelType in gameModuleData.Module.TopLevelTypes
                     .Where(x => x.BaseType is not null && x.BaseType.Name == "MonoBehaviour"))
        {
            bool hasUpdate = topLevelType.Methods.Any(x => x.Name == "Update");
            bool hasLateUpdate = topLevelType.Methods.Any(x => x.Name == "LateUpdate");
            bool hasFixedUpdate = topLevelType.Methods.Any(x => x.Name == "FixedUpdate");
            if (!hasFixedUpdate || hasUpdate || hasLateUpdate)
                continue;

            MethodDefinition newUpdateMethod = new(
                "Update",
                MethodAttributes.Private | MethodAttributes.HideBySig,
                MethodSignature.CreateInstance(gameModuleData.Module.CorLibTypeFactory.Void));
            newUpdateMethod.CilMethodBody = new() { Instructions = { Ret } };
            topLevelType.Methods.Add(newUpdateMethod);
        }
    }
}