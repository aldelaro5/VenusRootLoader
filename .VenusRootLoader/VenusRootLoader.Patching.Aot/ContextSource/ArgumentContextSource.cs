using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;

namespace VenusRootLoader.Patching.Aot.ContextSource;

public sealed class ArgumentContextSource : IContextSource
{
    private readonly Parameter _parameter;
    public object MappingKey => _parameter.MethodSignatureIndex;
    public object Key { get; }
    public TypeSignature TypeSignature => _parameter.ParameterType;

    public ArgumentContextSource(Parameter parameter)
    {
        _parameter = parameter;
        Key = parameter;
    }
}