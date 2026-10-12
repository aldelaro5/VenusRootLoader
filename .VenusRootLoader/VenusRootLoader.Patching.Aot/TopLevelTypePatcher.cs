using AsmResolver.DotNet;

namespace VenusRootLoader.Patching.Aot;

/// <summary>
/// Performs all the patching tasks needed on a top level type of the game assembly.
/// </summary>
public abstract class TopLevelTypePatcher
{
    /// <summary>
    /// The name of the top level type in the game assembly.
    /// </summary>
    public abstract string TypeName { get; }

    /// <summary>
    /// Performs all the patching tasks to patch the type.
    /// </summary>
    /// <param name="gameModuleData">The game module data used for patching.</param>
    /// <param name="type">The type that should be named <see cref="P:TopLevelTypePatcher.TypeName">TypeName</see>.</param>
    public abstract void PatchType(GameModuleData gameModuleData, TypeDefinition type);

    /// <summary>
    /// Build a mapping from enum value to enum name of a particular enum given its name in the game assembly.
    /// </summary>
    /// <param name="gameModuleData">The game module data.</param>
    /// <param name="enumName">The name of the enum in the game assembly.</param>
    /// <returns>A dictionary where the key is each enum value and the value is each matching enum value name.</returns>
    protected static Dictionary<int, string> BuildEnumValueToNameMapping(GameModuleData gameModuleData, string enumName)
    {
        return gameModuleData.Module
            .GetAllTypes()
            .Single(x => x.IsEnum && x.Name == enumName)
            .Fields
            .Where(x => x.Constant is not null)
            .ToDictionary(x => (int)x.Constant!.InterpretData()!, x => x.Name!.Value);
    }
}