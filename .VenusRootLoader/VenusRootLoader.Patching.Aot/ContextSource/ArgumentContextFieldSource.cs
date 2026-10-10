using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.ContextSource;

/// <summary>
/// A <see cref="IContextFieldSource"/> sourced from a method argument.
/// </summary>
public sealed class ArgumentContextFieldSource : IContextFieldSource
{
    private readonly Parameter _parameter;

    public List<CilInstruction> GetLoadIl() => [new(Ldarg, _parameter)];
    public List<CilInstruction> GetStoreIl() => [new(Starg, _parameter)];

    public object Key { get; }
    public TypeSignature TypeSignature => _parameter.ParameterType;

    public ArgumentContextFieldSource(Parameter parameter)
    {
        _parameter = parameter;
        Key = parameter.MethodSignatureIndex;
    }
}