using AsmResolver.DotNet.Signatures;

namespace VenusRootLoader.Patching.Aot.ContextSource;

public interface IContextSource
{
    object Key { get; }
    TypeSignature TypeSignature { get; }
}