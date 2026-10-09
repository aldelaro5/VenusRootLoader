using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;

namespace VenusRootLoader.Patching.Aot.ContextSource;

public sealed class LocalContextSource : IContextSource
{
    private readonly CilLocalVariable _local;

    public object Key { get; }
    public TypeSignature TypeSignature => _local.VariableType;

    public LocalContextSource(CilLocalVariable local)
    {
        _local = local;
        Key = local;
    }
}