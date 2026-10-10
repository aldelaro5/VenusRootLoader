using AsmResolver.DotNet;

namespace VenusRootLoader.Patching.Aot;

/// <summary>
/// Performs all the patching tasks needed on a top level type of the game assembly.
/// </summary>
public interface ITopLevelTypePatcher
{
    /// <summary>
    /// The name of the top level type in the game assembly.
    /// </summary>
    string TypeName { get; }

    /// <summary>
    /// Performs all the patching tasks to patch the type.
    /// </summary>
    /// <param name="gameModuleData">The game module data used for patching.</param>
    /// <param name="type">The type that should be named <see cref="TypeName"/>.</param>
    void PatchType(GameModuleData gameModuleData, TypeDefinition type);
}