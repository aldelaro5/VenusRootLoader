using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.LogicExtraction.LogicContextSources;

/// <summary>
/// A <see cref="ILogicContextFieldSource"/> sourced from a method argument.
/// </summary>
public sealed class ArgumentLogicContextFieldSource : ILogicContextFieldSource
{
    private readonly Parameter _parameter;

    public List<CilInstruction> GetLoadIl() => [new(Ldarg, _parameter)];
    public List<CilInstruction> GetStoreIl() => [new(Starg, _parameter)];

    public object Key { get; }
    public TypeSignature TypeSignature => _parameter.ParameterType;

    public ArgumentLogicContextFieldSource(Parameter parameter)
    {
        _parameter = parameter;
        Key = parameter.MethodSignatureIndex;
    }
}