using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.LogicExtraction.LogicContextSources;

/// <summary>
/// A <see cref="ILogicContextFieldSource"/> sourced from a field of a method's containing type.
/// </summary>
public sealed class FieldLogicContextFieldSource : ILogicContextFieldSource
{
    private readonly FieldDefinition _field;

    public List<CilInstruction> GetLoadIl() => [new(Ldarg_0), new(Ldfld, _field)];
    public List<CilInstruction> GetStoreIl() => [new(Stfld, _field)];

    public object Key { get; }
    public TypeSignature TypeSignature => _field.Signature!.FieldType;

    public FieldLogicContextFieldSource(FieldDefinition field)
    {
        _field = field;
        Key = field;
    }
}