using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;

namespace VenusRootLoader.Patching.Aot.ContextSource;

public sealed class FieldContextSource : IContextSource
{
    private readonly FieldDefinition _field;

    public object MappingKey => _field;
    public object Key { get; }
    public TypeSignature TypeSignature => _field.Signature!.FieldType;

    public FieldContextSource(FieldDefinition field)
    {
        _field = field;
        Key = field;
    }
}