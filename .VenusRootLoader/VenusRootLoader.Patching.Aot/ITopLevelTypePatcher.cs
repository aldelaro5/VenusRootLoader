using AsmResolver.DotNet;

namespace VenusRootLoader.Patching.Aot;

public interface ITopLevelTypePatcher
{
    string TypeName { get; }
    void PatchType(LocalNetStandardReferenceImporter referenceImporter, TypeDefinition type);
}