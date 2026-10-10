using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.LogicExtraction.LogicContextSources;

/// <summary>
/// A <see cref="ILogicContextFieldSource"/> sourced from a local variable of a method.
/// </summary>
public sealed class LocalLogicContextFieldSource : ILogicContextFieldSource
{
    private readonly CilLocalVariable _local;

    public List<CilInstruction> GetLoadIl() => [new(Ldloc, _local)];
    public List<CilInstruction> GetStoreIl() => [new(Stloc, _local)];

    public object Key { get; }
    public TypeSignature TypeSignature => _local.VariableType;

    public LocalLogicContextFieldSource(CilLocalVariable local)
    {
        _local = local;
        Key = local;
    }
}