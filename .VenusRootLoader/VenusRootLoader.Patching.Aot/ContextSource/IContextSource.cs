using AsmResolver.DotNet.Signatures;

namespace VenusRootLoader.Patching.Aot.ContextSource;

public interface IContextSource
{
    object MappingKey { get; }
    object Key { get; }
    TypeSignature TypeSignature { get; }
}